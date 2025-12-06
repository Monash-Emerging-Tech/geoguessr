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
    
    [Header("Z-Level Settings")]
    [SerializeField] private int minZLevel = -4; // P4 (Parking Level 4)
    [SerializeField] private int maxZLevel = 12; // 11th Floor
    [SerializeField] private int currentZLevel = 0; // Ground level
    
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
    
    // Enhanced location data with z-level support
    private MarkerData currentActualMarker;
    private MarkerData currentGuessMarker;
    
    // Events
    public static event Action<MarkerData> OnGuessSubmitted; // Enhanced event with z-level support
    public static event Action<int> OnScoreCalculated;
    public static event Action OnMapOpened;
    public static event Action OnMapClosed;
    public static event Action<int> OnZLevelChanged; // New z-level event
    
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
        Application.ExternalEval("showMapFromUnity()");
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
        Application.ExternalEval("hideMapFromUnity()");
        #else
        LogDebug("Map would be hidden (WebGL only)");
        #endif
        
        OnMapClosed?.Invoke();
        LogDebug("Map closed");
    }

    #endregion

    #region Location Management

    /// <summary>
    /// Sets the actual location for the current round with z-level support
    /// </summary>
    /// <param name="latitude">Latitude of actual location</param>
    /// <param name="longitude">Longitude of actual location</param>
    /// <param name="zLevel">Z-level of actual location</param>
    public void SetActualLocation(float latitude, float longitude, float zLevel)
    {
        currentActualLocation = new Vector2(latitude, longitude);
        
        // Convert float zLevel to int for marker data
        int zLevelInt = Mathf.RoundToInt(zLevel);
        
        // Create enhanced marker data
        currentActualMarker = new MarkerData
        {
            id = "actual-location",
            lng = longitude,
            lat = latitude,
            zLevel = zLevelInt,
            zLevelName = GetZLevelName(zLevelInt),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            options = new MarkerOptions
            {
                imgUrl = "images/fat.svg",
                imgScale = 1.7f,
                color = "#9D9DDC",
                size = 60,
                innerCircle = false,
                shape = "marker",
                zLevel = zLevelInt
            },
            markerType = "actual"
        };
        
        LogDebug($"Actual location set to: {latitude}, {longitude}, Level: {GetZLevelName(zLevelInt)}");
    }

    /// <summary>
    /// Resets the current round data
    /// </summary>
    public void ResetRound()
    {
        currentActualLocation = null;
        currentGuessLocation = null;
        currentActualMarker = null;
        currentGuessMarker = null;
        LogDebug("Round reset");
    }

    #endregion

    #region JavaScript Communication

    /// <summary>
    /// Called from JavaScript when guess is submitted
    /// </summary>
    /// <param name="jsonData">JSON string containing guess coordinates</param>
    public void SubmitGuess(string jsonData)
    {
        try
        {
            // Try to parse as enhanced data first
            var enhancedData = JsonUtility.FromJson<EnhancedMapClickData>(jsonData);
            if (enhancedData != null && !string.IsNullOrEmpty(enhancedData.zLevelName))
            {
                // Enhanced data with z-level
                currentGuessLocation = new Vector2(enhancedData.latitude, enhancedData.longitude);
                
                // Create enhanced marker data for guess
                currentGuessMarker = new MarkerData
                {
                    id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    lng = enhancedData.longitude,
                    lat = enhancedData.latitude,
                    zLevel = enhancedData.zLevel,
                    zLevelName = enhancedData.zLevelName,
                    timestamp = enhancedData.timestamp.ToString(),
                    options = new MarkerOptions
                    {
                imgUrl = "images/handthing.svg",
                imgScale = 1.7f,
                color = "white",
                        size = 60,
                        innerCircle = false,
                        shape = "marker",
                        zLevel = enhancedData.zLevel
                    },
                    markerType = "player"
                };
                
                LogDebug($"Guess submitted at: {enhancedData.latitude}, {enhancedData.longitude}, Level: {enhancedData.zLevelName}");
            }
            else
            {
                // Fallback to legacy data
                var guessData = JsonUtility.FromJson<MapClickData>(jsonData);
                currentGuessLocation = new Vector2(guessData.latitude, guessData.longitude);
                
                // Create basic marker data for legacy guess
                currentGuessMarker = new MarkerData
                {
                    id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    lng = guessData.longitude,
                    lat = guessData.latitude,
                    zLevel = currentZLevel, // Use current z-level
                    zLevelName = GetZLevelName(currentZLevel),
                    timestamp = guessData.timestamp.ToString(),
                    options = new MarkerOptions
                    {
                        imgUrl = "images/handthing.svg",
                        imgScale = 1.7f,
                        color = "white",
                        size = 60,
                        innerCircle = false,
                        shape = "marker",
                        zLevel = currentZLevel
                    },
                    markerType = "player"
                };
                
                LogDebug($"Guess submitted at: {guessData.latitude}, {guessData.longitude} (legacy data)");
            }
            
            // Trigger enhanced guess submitted event
            if (currentGuessMarker != null)
            {
                OnGuessSubmitted?.Invoke(currentGuessMarker);
            }
            
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
        Application.ExternalEval($"updateScoreFromUnity({score}, {round})");
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
        Application.ExternalEval($"showLoading({show.ToString().ToLower()})");
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
        Application.ExternalEval($"addMarkerFromUnity({currentActualLocation.Value.x}, {currentActualLocation.Value.y}, 'Actual Location', 'actual')");
        #endif
        
        // Add guess location marker (if not already added)
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval($"addMarkerFromUnity({currentGuessLocation.Value.x}, {currentGuessLocation.Value.y}, 'Your Guess', 'guess')");
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

    #region Z-Level Management

    /// <summary>
    /// Converts z-level number to readable name
    /// </summary>
    /// <param name="zLevel">Z-level number</param>
    /// <returns>Readable level name</returns>
    private string GetZLevelName(int zLevel)
    {
        if (zLevel == -4) return "P4 (Parking Level 4)";
        if (zLevel == -3) return "P3 (Parking Level 3)";
        if (zLevel == -2) return "P2 (Parking Level 2)";
        if (zLevel == -1) return "P1 (Parking Level 1)";
        if (zLevel == 0) return "LG (Lower Ground)";
        if (zLevel == 1) return "G (Ground)";
        if (zLevel == 2) return "1 (First Floor)";
        if (zLevel == 3) return "2 (Second Floor)";
        if (zLevel == 4) return "3 (Third Floor)";
        if (zLevel == 5) return "4 (Fourth Floor)";
        if (zLevel == 6) return "5 (Fifth Floor)";
        if (zLevel == 7) return "6 (Sixth Floor)";
        if (zLevel == 8) return "7 (Seventh Floor)";
        if (zLevel == 9) return "8 (Eighth Floor)";
        if (zLevel == 10) return "9 (Ninth Floor)";
        if (zLevel == 11) return "10 (Tenth Floor)";
        if (zLevel == 12) return "11 (Eleventh Floor)";
        if (zLevel < -4) return $"B{Math.Abs(zLevel)} (Basement {Math.Abs(zLevel)})";
        return $"{zLevel} (Level {zLevel})";
    }

    /// <summary>
    /// Validates if a z-level is within allowed range
    /// </summary>
    /// <param name="zLevel">Z-level to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidZLevel(int zLevel)
    {
        return zLevel >= minZLevel && zLevel <= maxZLevel;
    }

    /// <summary>
    /// Sets the current z-level
    /// </summary>
    /// <param name="zLevel">New z-level</param>
    public void SetZLevel(int zLevel)
    {
        if (IsValidZLevel(zLevel))
        {
            currentZLevel = zLevel;
            OnZLevelChanged?.Invoke(zLevel);
            LogDebug($"Z-level changed to: {GetZLevelName(zLevel)}");
        }
        else
        {
            LogWarning($"Invalid z-level: {zLevel}. Must be between {minZLevel} and {maxZLevel}");
        }
    }

    /// <summary>
    /// Gets the current z-level
    /// </summary>
    /// <returns>Current z-level</returns>
    public int GetCurrentZLevel()
    {
        return currentZLevel;
    }

    /// <summary>
    /// Creates a MarkerData from Vector2 coordinates (for backward compatibility)
    /// </summary>
    /// <param name="coordinates">Latitude and longitude coordinates</param>
    /// <param name="markerType">Type of marker ("player" or "actual")</param>
    /// <returns>MarkerData with current z-level</returns>
    public MarkerData CreateMarkerDataFromVector2(Vector2 coordinates, string markerType = "player")
    {
        return new MarkerData
        {
            id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            lng = coordinates.y, // longitude
            lat = coordinates.x, // latitude
            zLevel = currentZLevel,
            zLevelName = GetZLevelName(currentZLevel),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            options = new MarkerOptions
            {
                imgUrl = markerType == "actual" ? "images/fat.svg" : "images/handthing.svg",
                imgScale = 1.7f,
                color = markerType == "actual" ? "#9D9DDC" : "white",
                size = 60,
                innerCircle = false,
                shape = "marker",
                zLevel = currentZLevel
            },
            markerType = markerType
        };
    }

    #endregion

    #region Data Structures

    // Enhanced data structure for JSON parsing with z-level support
    [System.Serializable]
    public class EnhancedMapClickData
    {
        public float latitude;
        public float longitude;
        public int zLevel;
        public string zLevelName;
        public long timestamp;
    }

    // Custom marker options for visual customization
    [System.Serializable]
    public class MarkerOptions
    {
        public string imgUrl = "";
        public float imgScale = 1.7f;
        public string color = "white";
        public int size = 60;
        public bool innerCircle = false;
        public string shape = "marker";
        public int zLevel = 0;
    }

    // Complete marker data structure
    [System.Serializable]
    public class MarkerData
    {
        public string id;
        public float lng;
        public float lat;
        public int zLevel;
        public string zLevelName;
        public string timestamp;
        public MarkerOptions options;
        public string markerType; // "player" or "actual"
    }

    // Legacy data structure for backward compatibility
    [System.Serializable]
    private class MapClickData
    {
        public float latitude;
        public float longitude;
        public long timestamp;
    }

    #endregion
}