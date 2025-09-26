using UnityEngine;
using TMPro;

/// <summary>
/// Controls the ScoreBanner prefab UI elements (score, round display, map pack info)
/// Subscribes to GameLogic events for updates
/// 
/// Written by aleu0007
/// Last Modified: 24/09/2025
/// </summary>
public class ScoreBannerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI mapPackText;
    
    [Header("Settings")]
    [SerializeField] private string scoreFormat = "{0}";
    [SerializeField] private string roundFormat = "{0}/{1}";
    [SerializeField] private string mapPackFormat = "{0}";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Start()
    {
        // Subscribe to GameLogic events
        GameLogic.OnScoreUpdated += OnScoreUpdated;
        GameLogic.OnRoundUpdated += OnRoundUpdated;
        GameLogic.OnGameEnded += OnGameEnded;
        
        // Initialize UI
        UpdateUI();
        
        LogDebug("ScoreBannerController initialized");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from GameLogic events
        GameLogic.OnScoreUpdated -= OnScoreUpdated;
        GameLogic.OnRoundUpdated -= OnRoundUpdated;
        GameLogic.OnGameEnded -= OnGameEnded;
    }
    
    /// <summary>
    /// Updates all UI elements
    /// </summary>
    private void UpdateUI()
    {
        UpdateScoreDisplay();
        UpdateRoundDisplay();
        UpdateMapPackDisplay();
    }
    
    /// <summary>
    /// Updates the score display
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null && GameLogic.Instance != null)
        {
            int totalScore = GameLogic.Instance.GetCurrentScore();
            scoreText.text = string.Format(scoreFormat, totalScore);
        }
    }
    
    /// <summary>
    /// Updates the round display
    /// </summary>
    private void UpdateRoundDisplay()
    {
        if (roundText != null && GameLogic.Instance != null)
        {
            int currentRound = GameLogic.Instance.GetCurrentRound();
            int totalRounds = GameLogic.Instance.GetTotalRounds();
            roundText.text = string.Format(roundFormat, currentRound, totalRounds);
        }
    }
    
    /// <summary>
    /// Updates the map pack display
    /// </summary>
    private void UpdateMapPackDisplay()
    {
        if (mapPackText != null && GameLogic.Instance != null)
        {
            int mapPackId = GameLogic.Instance.GetMapPackId();
            mapPackText.text = string.Format(mapPackFormat, mapPackId);
        }
    }
    
    // Event handlers
    private void OnScoreUpdated(int totalScore)
    {
        UpdateScoreDisplay();
        LogDebug($"Score updated to: {totalScore}");
    }
    
    private void OnRoundUpdated(int currentRound, int totalRounds)
    {
        UpdateRoundDisplay();
        LogDebug($"Round updated to: {currentRound}/{totalRounds}");
    }
    
    private void OnGameEnded(int finalScore)
    {
        UpdateUI();
        LogDebug($"Game ended with final score: {finalScore}");
    }
    
    // Public methods for external access
    public void SetScoreFormat(string format)
    {
        scoreFormat = format;
        UpdateScoreDisplay();
    }
    
    public void SetRoundFormat(string format)
    {
        roundFormat = format;
        UpdateRoundDisplay();
    }
    
    public void SetMapPackFormat(string format)
    {
        mapPackFormat = format;
        UpdateMapPackDisplay();
    }
    
    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ScoreBannerController] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
            Debug.LogWarning($"[ScoreBannerController] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[ScoreBannerController] {message}");
    }
}
