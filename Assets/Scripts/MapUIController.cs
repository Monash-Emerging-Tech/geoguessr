using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the UI elements related to map interaction
/// Handles map button, score display, and round information
/// </summary>
public class MapUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button mapButton;
    [SerializeField] private Button submitGuessButton;
    [SerializeField] private Button closeMapButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI guessInfoText;
    [SerializeField] private GameObject mapOverlay;
    
    [Header("Settings")]
    [SerializeField] private bool showTimer = true;
    [SerializeField] private string scoreFormat = "Score: {0}";
    [SerializeField] private string roundFormat = "Round: {0}/{1}";
    [SerializeField] private string timerFormat = "Time: {0:F0}s";
    [SerializeField] private string guessFormat = "Guess placed at: {0:F6}, {1:F6}";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // State
    private bool isMapVisible = false;
    private Vector2? currentGuess = null;
    
    private void Start()
    {
        // Subscribe to game events
        GeoguessrGameManager.OnRoundStarted += OnRoundStarted;
        GeoguessrGameManager.OnRoundEnded += OnRoundEnded;
        GeoguessrGameManager.OnGameEnded += OnGameEnded;
        GeoguessrGameManager.OnTimerUpdate += OnTimerUpdate;
        
        MapInteractionManager.OnGuessSubmitted += OnGuessSubmitted;
        MapInteractionManager.OnMapOpened += OnMapOpened;
        MapInteractionManager.OnMapClosed += OnMapClosed;
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Initialize UI
        UpdateUI();
        
        LogDebug("MapUIController initialized");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GeoguessrGameManager.OnRoundStarted -= OnRoundStarted;
        GeoguessrGameManager.OnRoundEnded -= OnRoundEnded;
        GeoguessrGameManager.OnGameEnded -= OnGameEnded;
        GeoguessrGameManager.OnTimerUpdate -= OnTimerUpdate;
        
        MapInteractionManager.OnGuessSubmitted -= OnGuessSubmitted;
        MapInteractionManager.OnMapOpened -= OnMapOpened;
        MapInteractionManager.OnMapClosed -= OnMapClosed;
    }
    
    /// <summary>
    /// Sets up button event listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (mapButton != null)
        {
            mapButton.onClick.AddListener(OnMapButtonClicked);
        }
        
        if (submitGuessButton != null)
        {
            submitGuessButton.onClick.AddListener(OnSubmitGuessClicked);
        }
        
        if (closeMapButton != null)
        {
            closeMapButton.onClick.AddListener(OnCloseMapClicked);
        }
    }
    
    /// <summary>
    /// Called when map button is clicked
    /// </summary>
    private void OnMapButtonClicked()
    {
        if (isMapVisible)
        {
            HideMap();
        }
        else
        {
            ShowMap();
        }
    }
    
    /// <summary>
    /// Called when submit guess button is clicked
    /// </summary>
    private void OnSubmitGuessClicked()
    {
        if (MapInteractionManager.Instance != null)
        {
            // This will trigger the JavaScript function to submit the guess
            MapInteractionManager.Instance.HideMap();
        }
    }
    
    /// <summary>
    /// Called when close map button is clicked
    /// </summary>
    private void OnCloseMapClicked()
    {
        HideMap();
    }
    
    /// <summary>
    /// Shows the map interface
    /// </summary>
    private void ShowMap()
    {
        if (MapInteractionManager.Instance != null)
        {
            MapInteractionManager.Instance.ShowMap();
        }
    }
    
    /// <summary>
    /// Hides the map interface
    /// </summary>
    private void HideMap()
    {
        if (MapInteractionManager.Instance != null)
        {
            MapInteractionManager.Instance.HideMap();
        }
    }
    
    /// <summary>
    /// Updates all UI elements
    /// </summary>
    private void UpdateUI()
    {
        UpdateScoreDisplay();
        UpdateRoundDisplay();
        UpdateMapButton();
        UpdateGuessInfo();
    }
    
    /// <summary>
    /// Updates the score display
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null && GeoguessrGameManager.Instance != null)
        {
            int totalScore = GeoguessrGameManager.Instance.GetTotalScore();
            scoreText.text = string.Format(scoreFormat, totalScore);
        }
    }
    
    /// <summary>
    /// Updates the round display
    /// </summary>
    private void UpdateRoundDisplay()
    {
        if (roundText != null && GeoguessrGameManager.Instance != null)
        {
            int currentRound = GeoguessrGameManager.Instance.GetCurrentRound();
            int totalRounds = GeoguessrGameManager.Instance.GetTotalRounds();
            roundText.text = string.Format(roundFormat, currentRound, totalRounds);
        }
    }
    
    /// <summary>
    /// Updates the map button appearance
    /// </summary>
    private void UpdateMapButton()
    {
        if (mapButton != null)
        {
            var buttonText = mapButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isMapVisible ? "Close Map" : "Open Map";
            }
        }
    }
    
    /// <summary>
    /// Updates the guess information display
    /// </summary>
    private void UpdateGuessInfo()
    {
        if (guessInfoText != null)
        {
            if (currentGuess.HasValue)
            {
                guessInfoText.text = string.Format(guessFormat, 
                    currentGuess.Value.x, currentGuess.Value.y);
                guessInfoText.gameObject.SetActive(true);
            }
            else
            {
                guessInfoText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Updates the timer display
    /// </summary>
    /// <param name="timeRemaining">Time remaining in seconds</param>
    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null && showTimer)
        {
            timerText.text = string.Format(timerFormat, timeRemaining);
            timerText.gameObject.SetActive(true);
        }
    }
    
    // Event handlers
    private void OnRoundStarted(int round)
    {
        currentGuess = null;
        UpdateUI();
        LogDebug($"Round {round} started - UI updated");
    }
    
    private void OnRoundEnded(int score)
    {
        UpdateUI();
        LogDebug($"Round ended with score {score} - UI updated");
    }
    
    private void OnGameEnded(int totalScore)
    {
        UpdateUI();
        LogDebug($"Game ended with total score {totalScore} - UI updated");
    }
    
    private void OnTimerUpdate(float timeRemaining)
    {
        UpdateTimerDisplay(timeRemaining);
    }
    
    private void OnGuessSubmitted(Vector2 guessLocation)
    {
        currentGuess = guessLocation;
        UpdateGuessInfo();
        LogDebug($"Guess submitted at {guessLocation} - UI updated");
    }
    
    private void OnMapOpened()
    {
        isMapVisible = true;
        UpdateMapButton();
        
        if (mapOverlay != null)
        {
            mapOverlay.SetActive(true);
        }
        
        LogDebug("Map opened - UI updated");
    }
    
    private void OnMapClosed()
    {
        isMapVisible = false;
        UpdateMapButton();
        
        if (mapOverlay != null)
        {
            mapOverlay.SetActive(false);
        }
        
        LogDebug("Map closed - UI updated");
    }
    
    // Public methods for external access
    public void SetMapButtonInteractable(bool interactable)
    {
        if (mapButton != null)
        {
            mapButton.interactable = interactable;
        }
    }
    
    public void SetSubmitButtonInteractable(bool interactable)
    {
        if (submitGuessButton != null)
        {
            submitGuessButton.interactable = interactable;
        }
    }
    
    public void ShowGuessInfo(bool show)
    {
        if (guessInfoText != null)
        {
            guessInfoText.gameObject.SetActive(show);
        }
    }
    
    public void ShowTimer(bool show)
    {
        showTimer = show;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(show);
        }
    }
    
    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MapUIController] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[MapUIController] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[MapUIController] {message}");
    }
}
