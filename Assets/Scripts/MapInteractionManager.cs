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
#nullable enable
    private LocationData? currentActualLocation;
    private LocationData? currentGuessLocation;
    private bool isMapActive = false;

#nullable disable
    // Events
    public static event Action<LocationData> OnGuessSubmitted; // Event with location data
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
            HideMap();
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
        int zLevelInt = Mathf.RoundToInt(zLevel);
        currentActualLocation = new LocationData
        {
            lat = latitude,
            lng = longitude,
            zLevel = zLevelInt,
            zLevelName = GetZLevelName(zLevelInt)
        };

        LogDebug($"Actual location set to: {latitude}, {longitude}, Level: {zLevelInt} - {GetZLevelName(zLevelInt)} (Instance ID: {this.GetInstanceID()})");
    }

    /// <summary>
    /// Resets the current round data
    /// </summary>
    public void ResetRound()
    {
        LogDebug($"ResetRound called on instance: {this.GetInstanceID()}");
        currentActualLocation = null;
        currentGuessLocation = null;
        LogDebug("Round reset");
    }

    #endregion

    #region JavaScript Communication

    /// <summary>
    /// Called from JavaScript when guess is submitted
    /// Receives LocationPayload and calculates score
    /// </summary>
    /// <param name="jsonData">JSON string containing guess location data: latitude, longitude, zLevel, zLevelName</param>
    public void SubmitGuess(string jsonData)
    {
        try
        {
            LogDebug($"SubmitGuess called on instance: {this.GetInstanceID()}, currentActualLocation is null: {currentActualLocation == null}");

            if (currentActualLocation == null)
            {
                LogWarning($"Cannot calculate score: actual location not set before guess. Ignoring guess. (Instance ID: {this.GetInstanceID()})");
                return;
            }

            // Parse the payload from JavaScript
            var payload = JsonUtility.FromJson<LocationPayload>(jsonData);
            if (payload == null)
            {
                LogError("Failed to parse guess payload from JavaScript");
                return;
            }

            // Store guess location data
            currentGuessLocation = new LocationData
            {
                lat = payload.latitude,
                lng = payload.longitude,
                zLevel = payload.zLevel,
                zLevelName = payload.zLevelName
            };

            LogDebug($"Guess submitted at: {payload.latitude}, {payload.longitude}, Level: {payload.zLevel} - {payload.zLevelName}");

            // Trigger guess submitted event
            if (currentGuessLocation != null)
            {
                OnGuessSubmitted?.Invoke(currentGuessLocation);
            }

            // Calculate score (we already ensured actual is present)
            if (currentGuessLocation != null)
            {
                int score = CalculateScore(currentActualLocation, currentGuessLocation);
                OnScoreCalculated?.Invoke(score);

                // Show actual location on map
                ShowBothLocations();
            }
            else
            {
                LogWarning("Cannot calculate score: guess location missing");
            }
        }
        catch (Exception e)
        {
            LogError($"Error processing guess submission: {e.Message}");
        }
    }

    /// <summary>
    /// Sends actual location data to JavaScript with x, y, z coordinates
    /// </summary>
    /// <param name="latitude">Latitude (x coordinate)</param>
    /// <param name="longitude">Longitude (y coordinate)</param>
    /// <param name="zLevel">Z-level (z coordinate)</param>
    public void SendActualLocationToJavaScript(float latitude, float longitude, int zLevel)
    {
        // Create payload data structure (same format as receiving)
        var locationPayload = new LocationPayload
        {
            latitude = latitude,
            longitude = longitude,
            zLevel = zLevel,
            zLevelName = GetZLevelName(zLevel)
        };

        // Serialize to JSON
        string jsonPayload = JsonUtility.ToJson(locationPayload);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Escape single quotes and backslashes for JavaScript string literal
        string escapedJson = jsonPayload.Replace("\\", "\\\\").Replace("'", "\\'");
        Application.ExternalEval($"addActualLocationFromUnity('{escapedJson}')");
#else
        LogDebug($"Actual location would be sent to JavaScript: ({latitude}, {longitude}), Level: {GetZLevelName(zLevel)}");
#endif
    }

    /// <summary>
    /// Shows actual location on the map by sending it to JavaScript
    /// </summary>
    public void ShowBothLocations()
    {
        if (currentActualLocation != null)
        {
            SendActualLocationToJavaScript(
                currentActualLocation.lat,
                currentActualLocation.lng,
                currentActualLocation.zLevel
            );
        }

        LogDebug("Actual location sent to JavaScript for display");
    }

    #endregion

    #region Scoring System

    /// <summary>
    /// Calculates score based on distance between guess and actual location
    /// </summary>
    /// <param name="actual">Actual location data</param>
    /// <param name="guess">Guess location data</param>
    /// <returns>Score from 0 to maxScore</returns>
    private int CalculateScore(LocationData actual, LocationData guess)
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
    /// <param name="coord1">First location data</param>
    /// <param name="coord2">Second location data</param>
    /// <returns>Distance in meters</returns>
    private float CalculateDistance(LocationData coord1, LocationData coord2)
    {
        // Convert degrees to approximate meters
        // 1 degree latitude ~ 111,000 meters
        // 1 degree longitude ~ 111,000 * cos(latitude) meters
        float latDiff = (coord2.lat - coord1.lat) * 111000f;
        float lngDiff = (coord2.lng - coord1.lng) * 111000f * Mathf.Cos(coord1.lat * Mathf.Deg2Rad);

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

    // Data structure for sending/receiving location payload data (same format for both directions)
    [System.Serializable]
    public class LocationPayload
    {
        public float latitude;
        public float longitude;
        public int zLevel;
        public string zLevelName;
    }

    // Location data structure with lng, lat, zLevel, zLevelName
    [System.Serializable]
    public class LocationData
    {
        public float lat;
        public float lng;
        public int zLevel;
        public string zLevelName;
    }

    #endregion
}