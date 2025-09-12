using UnityEngine;
using System;

/// <summary>
/// Handles map interactions and marker rendering only
/// Communicates with GameLogic for game state management
/// </summary>

/***
 * 
 * MapInteractionManager, handles map interactions and marker rendering. 
 * Communicates with GameLogic for game state management.
 * 
 * Written by aleu0007
 * Last Modified: 10/09/2025
 * 
 */
public class MapInteractionManager : MonoBehaviour
{
    // Current game state
    private Vector2? currentActualLocation;
    private Vector2? currentGuessLocation;
    private bool isMapActive = false;
    
    // Events
    public static event Action<Vector2> OnMapClicked;
    
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
            
            Debug.Log($"Map clicked at: {clickData.latitude}, {clickData.longitude}");
            
            // Trigger map clicked event for GameLogic to handle
            OnMapClicked?.Invoke(currentGuessLocation.Value);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing map click data: {e.Message}");
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
        Debug.Log("Map would be shown (WebGL only)");
        #endif
        
        Debug.Log("Map opened");
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
        Debug.Log("Map would be hidden (WebGL only)");
        #endif
        
        Debug.Log("Map closed");
    }
    
    /// <summary>
    /// Sets the actual location for the current round
    /// </summary>
    /// <param name="latitude">Latitude of actual location</param>
    /// <param name="longitude">Longitude of actual location</param>
    public void SetActualLocation(float latitude, float longitude)
    {
        currentActualLocation = new Vector2(latitude, longitude);
        Debug.Log($"Actual location set to: {latitude}, {longitude}");
    }
    
    /// <summary>
    /// Renders a marker on the map
    /// </summary>
    /// <param name="latitude">Latitude of the marker</param>
    /// <param name="longitude">Longitude of the marker</param>
    /// <param name="label">Label for the marker</param>
    /// <param name="type">Type of marker (guess, actual, etc.)</param>
    public void RenderMarker(float latitude, float longitude, string label, string type)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("addMarkerFromUnity", 
            latitude.ToString(), 
            longitude.ToString(), 
            label, 
            type);
        #else
        Debug.Log($"Marker would be rendered: {label} at {latitude}, {longitude} (type: {type})");
        #endif
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
        Debug.Log($"Score would be updated: {score}, Round: {round}");
        #endif
    }
    
    /// <summary>
    /// Resets the current round data
    /// </summary>
    public void ResetRound()
    {
        currentActualLocation = null;
        currentGuessLocation = null;
        Debug.Log("Round reset");
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