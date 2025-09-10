using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main game manager for the Geoguessr game
/// Integrates with MapInteractionManager for map-based gameplay
/// </summary>
public class GeoguessrGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 5;
    [SerializeField] private float roundTimeLimit = 60f; // seconds
    [SerializeField] private bool enableTimer = true;
    
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
    private int currentRound = 1;
    private int totalScore = 0;
    private int currentRoundScore = 0;
    private bool isGameActive = false;
    private bool isRoundActive = false;
    private Coroutine roundTimerCoroutine;
    
    // Events
    public static event System.Action<int> OnRoundStarted;
    public static event System.Action<int> OnRoundEnded;
    public static event System.Action<int> OnGameEnded;
    public static event System.Action<float> OnTimerUpdate;
    
    // Singleton pattern
    public static GeoguessrGameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Subscribe to map events
        MapInteractionManager.OnGuessSubmitted += OnGuessSubmitted;
        MapInteractionManager.OnScoreCalculated += OnScoreCalculated;
        MapInteractionManager.OnMapOpened += OnMapOpened;
        MapInteractionManager.OnMapClosed += OnMapClosed;
        
        // Initialize game
        InitializeGame();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        MapInteractionManager.OnGuessSubmitted -= OnGuessSubmitted;
        MapInteractionManager.OnScoreCalculated -= OnScoreCalculated;
        MapInteractionManager.OnMapOpened -= OnMapOpened;
        MapInteractionManager.OnMapClosed -= OnMapClosed;
    }
    
    /// <summary>
    /// Initializes the game
    /// </summary>
    private void InitializeGame()
    {
        currentRound = 1;
        totalScore = 0;
        isGameActive = true;
        
        LogDebug("Game initialized");
        
        // Start first round
        StartRound();
    }
    
    /// <summary>
    /// Starts a new round
    /// </summary>
    public void StartRound()
    {
        if (!isGameActive || isRoundActive) return;
        
        isRoundActive = true;
        currentRoundScore = 0;
        
        // Get random location from location manager
        if (locationManager != null)
        {
            locationManager.SelectRandomLocation();
            var location = locationManager.GetCurrentLocation();
            
            if (location != null)
            {
                // Set actual location in map manager
                mapManager.SetActualLocation(location.latitude, location.longitude);
                LogDebug($"Round {currentRound} started - Location: {location.name}");
            }
        }
        
        // Start timer if enabled
        if (enableTimer && roundTimeLimit > 0)
        {
            roundTimerCoroutine = StartCoroutine(RoundTimer());
        }
        
        // Show map
        mapManager.ShowMap();
        
        OnRoundStarted?.Invoke(currentRound);
        LogDebug($"Round {currentRound} started");
    }
    
    /// <summary>
    /// Ends the current round
    /// </summary>
    public void EndRound()
    {
        if (!isRoundActive) return;
        
        isRoundActive = false;
        
        // Stop timer
        if (roundTimerCoroutine != null)
        {
            StopCoroutine(roundTimerCoroutine);
            roundTimerCoroutine = null;
        }
        
        // Hide map
        mapManager.HideMap();
        
        // Add round score to total
        totalScore += currentRoundScore;
        
        OnRoundEnded?.Invoke(currentRoundScore);
        LogDebug($"Round {currentRound} ended - Score: {currentRoundScore}, Total: {totalScore}");
        
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
    /// Ends the entire game
    /// </summary>
    private void EndGame()
    {
        isGameActive = false;
        isRoundActive = false;
        
        // Hide map
        mapManager.HideMap();
        
        OnGameEnded?.Invoke(totalScore);
        LogDebug($"Game ended - Final Score: {totalScore}");
        
        // Show results or return to menu
        ShowResults();
    }
    
    /// <summary>
    /// Shows game results
    /// </summary>
    private void ShowResults()
    {
        // Update score display
        mapManager.UpdateScoreDisplay(totalScore, currentRound);
        
        // Show results UI if available
        if (resultsUI != null)
        {
            resultsUI.SetActive(true);
        }
        
        LogDebug("Results displayed");
    }
    
    /// <summary>
    /// Starts the next round after a delay
    /// </summary>
    private IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(2f); // 2 second delay between rounds
        
        currentRound++;
        StartRound();
    }
    
    /// <summary>
    /// Round timer coroutine
    /// </summary>
    private IEnumerator RoundTimer()
    {
        float timeRemaining = roundTimeLimit;
        
        while (timeRemaining > 0 && isRoundActive)
        {
            OnTimerUpdate?.Invoke(timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }
        
        // Time's up
        if (isRoundActive)
        {
            LogDebug("Round time limit reached");
            EndRound();
        }
    }
    
    /// <summary>
    /// Called when a guess is submitted
    /// </summary>
    /// <param name="guessLocation">The guessed location coordinates</param>
    private void OnGuessSubmitted(Vector2 guessLocation)
    {
        LogDebug($"Guess submitted at: {guessLocation}");
        // The score calculation will be handled by OnScoreCalculated
    }
    
    /// <summary>
    /// Called when score is calculated
    /// </summary>
    /// <param name="score">The calculated score</param>
    private void OnScoreCalculated(int score)
    {
        currentRoundScore = score;
        LogDebug($"Score calculated: {score}");
        
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
    
    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        currentRound = 1;
        totalScore = 0;
        isGameActive = true;
        isRoundActive = false;
        
        // Reset map manager
        mapManager.ResetRound();
        
        // Hide results UI
        if (resultsUI != null)
        {
            resultsUI.SetActive(false);
        }
        
        LogDebug("Game restarted");
        StartRound();
    }
    
    /// <summary>
    /// Returns to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Adjust scene name as needed
    }
    
    // Public getters for UI
    public int GetCurrentRound() => currentRound;
    public int GetTotalRounds() => totalRounds;
    public int GetTotalScore() => totalScore;
    public int GetCurrentRoundScore() => currentRoundScore;
    public bool IsGameActive() => isGameActive;
    public bool IsRoundActive() => isRoundActive;
    public float GetTimeRemaining() => roundTimeLimit; // This would need to be tracked properly
    
    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[GeoguessrGameManager] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[GeoguessrGameManager] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[GeoguessrGameManager] {message}");
    }
}
