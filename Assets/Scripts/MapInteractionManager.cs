using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages interactions between Unity and the MazeMap JavaScript API
/// Handles map clicks, scoring, and marker placement
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
    
    // Singleton pattern
    public static MapInteractionManager Instance { get; private set; }
    
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
            
            // Trigger guess submitted event
            OnGuessSubmitted?.Invoke(currentGuessLocation.Value);
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
    /// Calculates score based on distance between guess and actual location
    /// </summary>
    /// <param name="actual">Actual location coordinates</param>
    /// <param name="guess">Guess location coordinates</param>
    /// <returns>Score from 0 to maxScore</returns>
    private int CalculateScore(Vector2 actual, Vector2 guess)
    {
        float distance = CalculateDistance(actual, guess);
        
        // Normalize distance (0 = perfect, 1 = max distance)
        float normalizedDistance = Mathf.Clamp01(distance / maxGuessDistance);
        
        // Apply curve to get score multiplier
        float scoreMultiplier = scoreCurve.Evaluate(normalizedDistance);
        
        // Calculate final score
        int score = Mathf.RoundToInt(minScore + (maxScore - minScore) * scoreMultiplier);
        
        LogDebug($"Distance: {distance:F2}m, Normalized: {normalizedDistance:F2}, Score: {score}");
        
        return score;
    }
    
    /// <summary>
    /// Calculates distance between two coordinates using Haversine formula
    /// </summary>
    /// <param name="coord1">First coordinate (lat, lng)</param>
    /// <param name="coord2">Second coordinate (lat, lng)</param>
    /// <returns>Distance in meters</returns>
    private float CalculateDistance(Vector2 coord1, Vector2 coord2)
    {
        const float R = 6371000f; // Earth's radius in meters
        
        float lat1Rad = coord1.x * Mathf.Deg2Rad;
        float lat2Rad = coord2.x * Mathf.Deg2Rad;
        float deltaLatRad = (coord2.x - coord1.x) * Mathf.Deg2Rad;
        float deltaLngRad = (coord2.y - coord1.y) * Mathf.Deg2Rad;
        
        float a = Mathf.Sin(deltaLatRad / 2) * Mathf.Sin(deltaLatRad / 2) +
                  Mathf.Cos(lat1Rad) * Mathf.Cos(lat2Rad) *
                  Mathf.Sin(deltaLngRad / 2) * Mathf.Sin(deltaLngRad / 2);
        
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        
        return R * c;
    }
    
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
    
    /// <summary>
    /// Resets the current round data
    /// </summary>
    public void ResetRound()
    {
        currentActualLocation = null;
        currentGuessLocation = null;
        LogDebug("Round reset");
    }
    
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
    
    // Data structure for JSON parsing
    [System.Serializable]
    private class MapClickData
    {
        public float latitude;
        public float longitude;
        public long timestamp;
    }
}
