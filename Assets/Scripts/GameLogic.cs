using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LocationManager;


/***
 * 
 * GameLogic, Responsible for all game logic including scoring, round management, and game state.
 * Communicates with MapInteractionManager for map interactions.
 * 
 * Written by O-Bolt
 * Last Modified: 08/08/2025
 * 
 */
public class GameLogic : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 5;
    [SerializeField] private int maxScore = 5000;
    [SerializeField] private MapInteractionManager mapManager;

    // Public Variables 
    public bool inGame;
    public bool isGuessing;
    public int currentScore;
    public int currentRound;
    public int GameMode = 0;
    public int mapPackId = 0;

    // Private game state
    private int totalScore = 0;
    private int currentRoundScore = 0;
    private Vector2? currentGuessLocation;
    private Vector2? currentActualLocation;

    // Used to find the LocationManager variables
    public LocationManager locationManager = new LocationManager();

    private Dictionary<int, LocationManager.MapPack> allMapPacks;

    public static GameLogic Instance; // Global reference

    // From Main Menu to the Game Scene
    // Moves to the Game Scene for the game to start, performs all thes start game logic
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
         
    }


    // Not too sure how this works but this is called after the Awake() function for MonoBehaviours,
    // This calls the OnSceneLoaded Function, Not sure how it really works or anything
    // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html 
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Subscribe to map events
        MapInteractionManager.OnMapClicked += OnMapClicked;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        MapInteractionManager.OnMapClicked -= OnMapClicked;
    }

    // Function is called when the scene is loaded
    // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the scenes is the GameScene, we should now change the Map and let the game start
        if (scene.name == "GameScene") {
            inGame = true;
            currentRound = 0;
            currentScore = 0;
            totalScore = 0;
            isGuessing = true;
            nextRound();
        }
        
        Debug.Log("Scene name: " + scene.name);
    }


    public void nextRound() {
        currentRound++;

        if (currentRound > totalRounds) {
            SceneManager.LoadScene("BreakdownScene");
            return;
        }

        isGuessing = true;
        currentRoundScore = 0;
        
        // Change map and start new round
        changeMap();
        
        // Show map for guessing
        if (mapManager != null)
        {
            mapManager.ShowMap();
        }
    }



    public void submitGuess()
    {
        if (!isGuessing || !currentGuessLocation.HasValue) return;
        
        isGuessing = false;

        // Calculate score
        if (currentActualLocation.HasValue)
        {
            currentRoundScore = CalculateScore(currentActualLocation.Value, currentGuessLocation.Value);
            totalScore += currentRoundScore;
            currentScore = totalScore;
            
            Debug.Log($"Round {currentRound} - Distance: {CalculateDistance(currentActualLocation.Value, currentGuessLocation.Value):F2}m, Score: {currentRoundScore}, Total: {totalScore}");
        }
        
        // Show actual location marker on map
        if (mapManager != null && currentActualLocation.HasValue)
        {
            mapManager.RenderMarker(currentActualLocation.Value.x, currentActualLocation.Value.y, "Actual Location", "actual");
        }
        
        // Update score display
        if (mapManager != null)
        {
            mapManager.UpdateScoreDisplay(totalScore, currentRound);
        }
    }

    /// <summary>
    /// Called when map is clicked
    /// </summary>
    /// <param name="guessLocation">The guessed location coordinates</param>
    private void OnMapClicked(Vector2 guessLocation)
    {
        if (!isGuessing) return;
        
        currentGuessLocation = guessLocation;
        Debug.Log($"Guess placed at: {guessLocation}");
        
        // Render guess marker on map
        if (mapManager != null)
        {
            mapManager.RenderMarker(guessLocation.x, guessLocation.y, "Your Guess", "guess");
        }
    }

    /// <summary>
    /// Calculates score based on distance between guess and actual location
    /// </summary>
    /// <param name="actual">Actual location coordinates</param>
    /// <param name="guess">Guess location coordinates</param>
    /// <returns>Score from 0 to maxScore</returns>
    private int CalculateScore(Vector2 actual, Vector2 guess)
    {
        float distance = CalculateDistance(actual, guess);
        
        // Simple scoring: maxScore minus distance (with minimum of 0)
        int score = Mathf.Max(0, maxScore - Mathf.RoundToInt(distance));
        
        return score;
    }
    
    /// <summary>
    /// Calculates distance between two coordinates using simple Euclidean distance
    /// Works directly with lat/lng coordinates for campus-scale distances
    /// </summary>
    /// <param name="coord1">First coordinate (lat, lng)</param>
    /// <param name="coord2">Second coordinate (lat, lng)</param>
    /// <returns>Distance in coordinate units</returns>
    private float CalculateDistance(Vector2 coord1, Vector2 coord2)
    {
        // Simple Euclidean distance using lat/lng directly
        float deltaLat = coord2.x - coord1.x;
        float deltaLng = coord2.y - coord1.y;
        
        return Mathf.Sqrt(deltaLat * deltaLat + deltaLng * deltaLng);
    }


    // Changes the map shown to the player at the start of a new round
    public void changeMap() {
        locationManager.setCurrentMapPack(mapPackId);
        
        //TODO: Make it so the same location can't be selected twice in the same session
        locationManager.SelectRandomLocation();
        
        // Set the actual location for scoring
        var location = locationManager.currentLocation;
        currentActualLocation = new Vector2(location.x, location.y);
        
        // Set actual location in map manager
        if (mapManager != null)
        {
            mapManager.SetActualLocation(location.x, location.y);
        }
        
        Debug.Log($"Round {currentRound} - Location: {location.Name} at {location.x}, {location.y}");
    }
    

    // More Game Control Logic Here





    // This ensures that there is only one GameLoader in the game at one time, and ensures that the GameObject is not destroyed
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            locationManager.Start();
            allMapPacks = locationManager.GetMapPacks();
        }
        else
        {
            Destroy(gameObject); // Kill duplicates
            return;
        }

    }
}

