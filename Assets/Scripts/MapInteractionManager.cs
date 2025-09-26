using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages interactions between Unity and the MazeMap JavaScript API
/// Handles map clicks, scoring, and marker placement
/// 
/// Written by aleu0007
/// Last Modified: 24/09/2025
/// </summary>
public class MapInteractionManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private bool enableMapOnStart = false;
    [SerializeField] private float maxGuessDistance = 1000f; // Maximum distance for scoring in meters
    
    [Header("Scoring Settings")]
    [SerializeField] private int maxScore = 5000;
    [SerializeField] private int minScore = 0;
    [SerializeField] private AnimationCurve scoreCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Current game state
    private Vector2? currentActualLocation;
    private Vector2? currentGuessLocation;
    private bool isMapActive = false;
    
    // Events
    public static event Action<Vector2> OnGuessSubmitted;
    public static event Action<int> OnScoreCalculated;
    public static event Action OnMapOpened;
    public static event Action OnMapClosed;
    public static event Action OnPinPlaced;
    
    // Singleton pattern
    public static MapInteractionManager Instance { get; private set; }

    #region Unity Lifecycle & Initialization

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
        if (enableMapOnStart)
        {
            ShowMap();
        }
        
        LogDebug("MapInteractionManager initialized");
    }

    #endregion

    #region Map Control

    /// <summary>
    /// Shows the map interface
    /// </summary>
    public void ShowMap()
    {
        if (isMapActive) return;
        
        isMapActive = true;
        
        // Call JavaScript function to show map
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("showMapFromUnity");
        #else
        LogDebug("Map would be shown (WebGL only)");
        #endif
        
        OnMapOpened?.Invoke();
        LogDebug("Map opened");
    }
    
    /// <summary>
    /// Hides the map interface
    /// </summary>
    public void HideMap()
    {
        if (!isMapActive) return;
        
        isMapActive = false;
        
        // Call JavaScript function to hide map
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("hideMapFromUnity");
        #else
        LogDebug("Map would be hidden (WebGL only)");
        #endif
        
        OnMapClosed?.Invoke();
        LogDebug("Map closed");
    }

    #endregion

    #region Location Management

    /// <summary>
    /// Sets the actual location for the current round
    /// </summary>
    /// <param name="latitude">Latitude of actual location</param>
    /// <param name="longitude">Longitude of actual location</param>
    public void SetActualLocation(float latitude, float longitude)
    {
        currentActualLocation = new Vector2(latitude, longitude);
        LogDebug($"Actual location set to: {latitude}, {longitude}");
    }

    /// <summary>
    /// Resets the current round data
    /// </summary>
    public void ResetRound()
    {
        currentActualLocation = null;
        currentGuessLocation = null;
        LogDebug("Round reset");
    }

    #endregion

    #region JavaScript Communication

    /// <summary>
    /// Called from JavaScript when map is clicked
    /// </summary>
    /// <param name="jsonData">JSON string containing latitude, longitude, and timestamp</param>
    public void OnMapClick(string jsonData)
    {
        try
        {
            var clickData = JsonUtility.FromJson<MapClickData>(jsonData);
            currentGuessLocation = new Vector2(clickData.latitude, clickData.longitude);
            
            LogDebug($"Map clicked at: {clickData.latitude}, {clickData.longitude}");
            
            // Trigger pin placed event
            OnPinPlaced?.Invoke();
        }
        catch (Exception e)
        {
            LogError($"Error parsing map click data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Called from JavaScript when guess is submitted
    /// </summary>
    /// <param name="jsonData">JSON string containing guess coordinates</param>
    public void SubmitGuess(string jsonData)
    {
        try
        {
            var guessData = JsonUtility.FromJson<MapClickData>(jsonData);
            currentGuessLocation = new Vector2(guessData.latitude, guessData.longitude);
            
            LogDebug($"Guess submitted at: {guessData.latitude}, {guessData.longitude}");
            
            // Calculate score if we have both locations
            if (currentActualLocation.HasValue && currentGuessLocation.HasValue)
            {
                int score = CalculateScore(currentActualLocation.Value, currentGuessLocation.Value);
                OnScoreCalculated?.Invoke(score);
                
                // Show both locations on map
                ShowBothLocations();
            }
            else
            {
                LogWarning("Cannot calculate score: missing actual or guess location");
            }
        }
        catch (Exception e)
        {
            LogError($"Error processing guess submission: {e.Message}");
        }
    }

    #endregion

    #region Scoring System

    /// <summary>
    /// Calculates score based on distance between guess and actual location
    /// </summary>
    /// <param name="actual">Actual location coordinates</param>
    /// <param name="guess">Guess location coordinates</param>
    /// <returns>Score from 0 to maxScore</returns>
    private int CalculateScore(Vector2 actual, Vector2 guess)
    {
        float distance = CalculateDistance(actual, guess);
        
        // If guess is beyond max distance, return minimum score
        if (distance > maxGuessDistance)
        {
            LogDebug($"Distance: {distance:F2}m exceeds max distance {maxGuessDistance}m - Score: {minScore}");
            return minScore;
        }
        
        // Linear scoring: maxScore minus proportional distance
        float scoreRatio = 1f - (distance / maxGuessDistance);
        int score = Mathf.RoundToInt(maxScore * scoreRatio);
        
        // Ensure score doesn't go below minimum
        score = Mathf.Max(minScore, score);
        
        LogDebug($"Distance: {distance:F2}m, Max Distance: {maxGuessDistance}m, Score: {score}");
        return score;
    }
    
    /// <summary>
    /// Calculates distance between two coordinates using simple approximation
    /// Accurate enough for campus distances (under 5km)
    /// </summary>
    /// <param name="coord1">First coordinate (lat, lng)</param>
    /// <param name="coord2">Second coordinate (lat, lng)</param>
    /// <returns>Distance in meters</returns>
    private float CalculateDistance(Vector2 coord1, Vector2 coord2)
    {
        // Convert degrees to approximate meters
        // 1 degree latitude ~ 111,000 meters
        // 1 degree longitude ~ 111,000 * cos(latitude) meters
        float latDiff = (coord2.x - coord1.x) * 111000f;
        float lngDiff = (coord2.y - coord1.y) * 111000f * Mathf.Cos(coord1.x * Mathf.Deg2Rad);
        
        return Mathf.Sqrt(latDiff * latDiff + lngDiff * lngDiff);
    }

    #endregion

    #region UI Communication

    /// <summary>
    /// Updates the score display in the web interface
    /// </summary>
    /// <param name="score">Current score</param>
    /// <param name="round">Current round</param>
    public void UpdateScoreDisplay(int score, int round)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("updateScoreFromUnity", score.ToString(), round.ToString());
        #else
        LogDebug($"Score would be updated: {score}, Round: {round}");
        #endif
    }
    
    /// <summary>
    /// Shows loading indicator
    /// </summary>
    /// <param name="show">Whether to show or hide loading</param>
    public void ShowLoading(bool show)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("showLoading", show.ToString().ToLower());
        #else
        LogDebug($"Loading would be {(show ? "shown" : "hidden")}");
        #endif
    }

    #endregion

    #region Map Markers

    /// <summary>
    /// Shows both actual and guess locations on the map
    /// </summary>
    private void ShowBothLocations()
    {
        if (!currentActualLocation.HasValue || !currentGuessLocation.HasValue) return;
        
        // Add actual location marker
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("addMarkerFromUnity", 
            currentActualLocation.Value.x.ToString(), 
            currentActualLocation.Value.y.ToString(), 
            "Actual Location", 
            "actual");
        #endif
        
        // Add guess location marker (if not already added)
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("addMarkerFromUnity", 
            currentGuessLocation.Value.x.ToString(), 
            currentGuessLocation.Value.y.ToString(), 
            "Your Guess", 
            "guess");
        #endif
        
        LogDebug("Both locations displayed on map");
    }

    #endregion

    #region Debug Logging

    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MapInteractionManager] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[MapInteractionManager] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[MapInteractionManager] {message}");
    }

    #endregion

    #region Data Structures

    // Data structure for JSON parsing
    [System.Serializable]
    private class MapClickData
    {
        public float latitude;
        public float longitude;
        public long timestamp;
    }

    #endregion
}