using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameLogic, Responsible for all game logic including scoring, round management, and game state.
/// Communicates with MapInteractionManager for map interactions.
/// 
/// Written by O-Bolt
/// Modified by aleu0007
/// Last Modified: 24/09/2025
/// </summary>
public class GameLogic : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 5;
    // [SerializeField] private int gameMode = 0; // TODO
    [SerializeField] private string mapPackName = "all";
    [SerializeField]private ScoreDataScriptableObject scoreData; // Scriptable Object to hold score data
    
    [Header("Map Integration")]
    [SerializeField] private MapInteractionManager mapManager;
    [SerializeField] private LocationManager locationManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject mapUI;
    [SerializeField] private GameObject resultsUI;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    
    // Game state
    private int currentScore = 0;
    private int currentRound = 1;
    private bool inGame = false;
    private bool isGuessing = false;
    private bool isGameActive = false;
    private bool isRoundActive = false;
    
    // MapPack management
    private int resolvedMapPackId = -1;
    
    private Dictionary<int, LocationManager.MapPack> allMapPacks;

    // Events
    public static event System.Action<int> OnRoundStarted;
    public static event System.Action<int> OnRoundEnded;
    public static event System.Action<int> OnGameEnded;
    public static event System.Action<int> OnScoreUpdated;
    public static event System.Action<int, int> OnRoundUpdated; // currentRound, totalRounds
    public static event System.Action<string> OnMapPackChanged; // mapPackName
    
    // Singleton pattern
    public static GameLogic Instance { get; private set; }

    #region Unity Lifecycle & Initialization

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize location manager
            if (locationManager != null)
            {
                locationManager.Start();
                allMapPacks = locationManager.GetMapPackDict();
            }
        }
        else
        {
            Destroy(gameObject); // Kill duplicates
            return;
        }
    }
    
    private void Start()
    {
        // Subscribe to map events
        if (mapManager != null)
        {
            MapInteractionManager.OnGuessSubmitted += OnGuessSubmitted;
            MapInteractionManager.OnScoreCalculated += OnScoreCalculated;
            MapInteractionManager.OnMapOpened += OnMapOpened;
            MapInteractionManager.OnMapClosed += OnMapClosed;
        }
        
        // Validate MapPack name at startup
        if (locationManager != null)
        {
            if (!ResolveMapPackId())
            {
                LogError("Invalid MapPack name in Inspector - please check the MapPack Name field");
            }
        }
        
        // Initialize game
        InitializeGame();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (mapManager != null)
        {
            MapInteractionManager.OnGuessSubmitted -= OnGuessSubmitted;
            MapInteractionManager.OnScoreCalculated -= OnScoreCalculated;
            MapInteractionManager.OnMapOpened -= OnMapOpened;
            MapInteractionManager.OnMapClosed -= OnMapClosed;
        }
    }

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// Loads the game scene from main menu
    /// </summary>
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Called when a scene is loaded - initializes game if it's the GameScene
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the scenes is the GameScene, we should now change the Map and let the game start
        if (scene.name == "GameScene") {
            inGame = true;
            currentRound = 0;
            currentScore = 0;
            isGuessing = true;
            nextRound();
        }
        
        Debug.Log("Scene name: " + scene.name);
    }

    #endregion

    #region Game Initialization

    /// <summary>
    /// Initializes the game
    /// </summary>
    private void InitializeGame()
    {
        currentRound = 1;
        currentScore = 0;
        isGameActive = true;
        inGame = true;
        
        LogDebug("Game initialized");
        
        // Start first round
        nextRound();
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        currentRound = 1;
        currentScore = 0;
        isGameActive = true;
        isRoundActive = false;
        inGame = true;
        
        // Reset map manager
        if (mapManager != null)
        {
            mapManager.ResetRound();
        }
        
        // Hide results UI
        if (resultsUI != null)
        {
            resultsUI.SetActive(false);
        }
        
        LogDebug("Game restarted");
        nextRound();
    }

    #endregion

    #region Round Management

    /// <summary>
    /// Starts a new round
    /// </summary>
    public void nextRound()
    {
        if (!isGameActive || isRoundActive) return;
        
        isRoundActive = true;
        isGuessing = true;
        
        // Get random location from location manager
        if (locationManager != null)
        {
            // Resolve MapPack name to ID if not already resolved
            if (resolvedMapPackId == -1 && !ResolveMapPackId())
            {
                LogError("Failed to resolve MapPack - cannot start round");
                return;
            }
            
            locationManager.SetCurrentMapPack(resolvedMapPackId);
            locationManager.SelectRandomLocation();
            var location = locationManager.GetCurrentLocation();
            
            if (!string.IsNullOrEmpty(location.Name))
            {
                // Set actual location in map manager
                if (mapManager != null)
                {
                    mapManager.SetActualLocation(location.x, location.y);
                }
                LogDebug($"Round {currentRound} started - Location: {location.Name}");
            }
        }
        
        // Show map
        if (mapManager != null)
        {
            mapManager.ShowMap();
        }
        
        OnRoundStarted?.Invoke(currentRound);
        OnRoundUpdated?.Invoke(currentRound, totalRounds);
        LogDebug($"Round {currentRound} started");
    }

    /// <summary>
    /// Ends the current round
    /// </summary>
    public void EndRound()
    {
        if (!isRoundActive) return;
        
        isRoundActive = false;
        isGuessing = false;
        
        // Hide map
        if (mapManager != null)
        {
            mapManager.HideMap();
        }
        
        OnRoundEnded?.Invoke(currentScore);
        LogDebug($"Round {currentRound} ended - Score: {currentScore}");
        
        // Check if game is complete
        if (currentRound >= totalRounds)
        {
            EndGame();
        }
        else
        {
            // Start next round after delay
            StartCoroutine(NextRoundDelay());
        }
    }

    /// <summary>
    /// Starts the next round after a delay
    /// </summary>
    private IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(2f); // 2 second delay between rounds
        
        currentRound++;
        OnRoundUpdated?.Invoke(currentRound, totalRounds);
        nextRound();
    }

    /// <summary>
    /// Ends the entire game
    /// </summary>
    private void EndGame()
    {
        isGameActive = false;
        isRoundActive = false;
        inGame = false;
        
        // Hide map
        if (mapManager != null)
        {
            mapManager.HideMap();
        }
        
        OnGameEnded?.Invoke(currentScore);
        LogDebug($"Game ended - Final Score: {currentScore}");
        
        // Show results or return to menu
        ShowResults();
    }

    #endregion

    #region Game Actions

    /// <summary>
    /// Submits guess through MapInteractionManager
    /// </summary>
    public void submitGuess()
    {
        if (mapManager != null)
        {
            // Let MapInteractionManager handle the guess submission
            // This will trigger OnGuessSubmitted and OnScoreCalculated events
            LogDebug("Guess submission delegated to MapInteractionManager");
        }
        else
        {
            LogWarning("MapInteractionManager not assigned - cannot submit guess");
        }
    }

    /// <summary>
    /// changes the map shown to the player at the start of a new round
    /// </summary>
    public void changeMap() {
        if (locationManager != null)
        {
            // Resolve MapPack name to ID if not already resolved
            if (resolvedMapPackId == -1 && !ResolveMapPackId())
            {
                LogError("Failed to resolve MapPack - cannot change map");
                return;
            }
            
            locationManager.SetCurrentMapPack(resolvedMapPackId);
            locationManager.SelectRandomLocation(); // TODO: Make it so the same location can't be selected twice in the same session
            LogDebug("Map changed for new round");
        }
        else
        {
            LogWarning("LocationManager not assigned - cannot change map");
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when a guess is submitted
    /// </summary>
    /// <param name="guessMarker">The guessed location marker data with z-level support</param>
    private void OnGuessSubmitted(MapInteractionManager.MarkerData guessMarker)
    {
        LogDebug($"Guess submitted at: {guessMarker.lat}, {guessMarker.lng}, Level: {guessMarker.zLevelName}");
        // The score calculation will be handled by OnScoreCalculated
    }
    
    /// <summary>
    /// Called when score is calculated
    /// </summary>
    /// <param name="score">The calculated score</param>
    private void OnScoreCalculated(int score)
    {
        currentScore += score;
        OnScoreUpdated?.Invoke(currentScore);
        LogDebug($"Score calculated: {score}, Total: {currentScore}");
        
        //Set current score to shared asset
        if (scoreData != null)
        {
            scoreData.SetScore(currentScore);
        }

        OnScoreUpdated?.Invoke(currentScore);
        // End the round
        EndRound();
    }
    
    /// <summary>
    /// Called when map is opened
    /// </summary>
    private void OnMapOpened()
    {
        LogDebug("Map opened");
        // Update UI to show map is active
    }
    
    /// <summary>
    /// Called when map is closed
    /// </summary>
    private void OnMapClosed()
    {
        LogDebug("Map closed");
        // Update UI to show map is not active
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Shows game results
    /// </summary>
    private void ShowResults()
    {
        // Update score display
        if (mapManager != null)
        {
            mapManager.UpdateScoreDisplay(currentScore, currentRound);
        }
        
        // Show results UI if available
        if (resultsUI != null)
        {
            resultsUI.SetActive(true);
        }
        
        LogDebug("Results displayed");
    }

    #endregion

    #region Public Getters

    // Public getters for UI
    public int GetCurrentRound() => currentRound;
    public int GetTotalRounds() => totalRounds;
    public int GetCurrentScore() => currentScore;
    public int GetMapPackId() => resolvedMapPackId;
    public string GetMapPackName() => mapPackName;
    public string[] GetAllMapPackNames() => locationManager?.GetAllMapPackNames() ?? new string[0];
    public string GetMapPackNameById(int id) => locationManager?.GetMapPackNameById(id) ?? "Unknown";
    public bool IsGameActive() => isGameActive;
    public bool IsRoundActive() => isRoundActive;
    public bool IsGuessing() => isGuessing;
    public bool IsInGame() => inGame;
    public LocationManager GetLocationManager() => locationManager;
    
    /// <summary>
    /// Sets the MapPack by name and resolves it to an ID
    /// </summary>
    /// <param name="name">MapPack name</param>
    /// <returns>True if successful, false if MapPack not found</returns>
    public bool SetMapPackByName(string name)
    {
        if (locationManager == null)
        {
            LogError("LocationManager not assigned - cannot set MapPack");
            return false;
        }
        
        int id = locationManager.GetMapPackIdByName(name);
        if (id == -1)
        {
            LogError($"MapPack '{name}' not found. Available MapPacks: {string.Join(", ", locationManager.GetAllMapPackNames())}");
            return false;
        }
        
        mapPackName = name;
        resolvedMapPackId = id;
        LogDebug($"MapPack set to: {name} (ID: {id})");
        
        // Fire MapPack changed event
        OnMapPackChanged?.Invoke(name);
        
        return true;
    }
    
    /// <summary>
    /// Gets the current MapPack name from the Inspector field
    /// </summary>
    /// <returns>Current MapPack name</returns>
    public string GetCurrentMapPackName() => mapPackName;
    
    /// <summary>
    /// Validates and resolves the current MapPack name to ID
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    private bool ResolveMapPackId()
    {
        if (locationManager == null)
        {
            LogError("LocationManager not assigned - cannot resolve MapPack");
            return false;
        }
        
        int id = locationManager.GetMapPackIdByName(mapPackName);
        if (id == -1)
        {
            LogError($"MapPack '{mapPackName}' not found. Available MapPacks: {string.Join(", ", locationManager.GetAllMapPackNames())}");
            return false;
        }
        
        resolvedMapPackId = id;
        
        // Fire MapPack changed event (only if this is not the initial startup resolution)
        if (mapPackName != "all" || resolvedMapPackId != 0) // Avoid firing for default startup
        {
            OnMapPackChanged?.Invoke(mapPackName);
        }
        
        return true;
    }

    #endregion

    #region Debug Logging

    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[GameLogic] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[GameLogic] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[GameLogic] {message}");
    }

    #endregion
}