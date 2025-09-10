using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script showing how to set up the Geoguessr game with MazeMaps integration
/// This script can be attached to a GameObject in your scene to automatically set up the game
/// </summary>
public class ExampleSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createUIElements = true;
    
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 5;
    [SerializeField] private float roundTimeLimit = 60f;
    [SerializeField] private float maxGuessDistance = 1000f;
    [SerializeField] private int maxScore = 5000;
    
    [Header("UI Prefabs (Optional)")]
    [SerializeField] private GameObject mapButtonPrefab;
    [SerializeField] private GameObject scoreTextPrefab;
    [SerializeField] private GameObject roundTextPrefab;
    [SerializeField] private GameObject timerTextPrefab;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGame();
        }
    }
    
    /// <summary>
    /// Sets up the complete Geoguessr game with MazeMaps integration
    /// </summary>
    [ContextMenu("Setup Game")]
    public void SetupGame()
    {
        Debug.Log("Setting up MNET Geoguessr with MazeMaps integration...");
        
        // 1. Create and configure MapInteractionManager
        SetupMapInteractionManager();
        
        // 2. Create and configure GeoguessrGameManager
        SetupGeoguessrGameManager();
        
        // 3. Create and configure MapUIController
        SetupMapUIController();
        
        // 4. Create UI elements if requested
        if (createUIElements)
        {
            CreateUIElements();
        }
        
        Debug.Log("Game setup complete! You can now build for WebGL.");
    }
    
    /// <summary>
    /// Sets up the MapInteractionManager
    /// </summary>
    private void SetupMapInteractionManager()
    {
        GameObject mapManagerGO = GameObject.Find("MapInteractionManager");
        if (mapManagerGO == null)
        {
            mapManagerGO = new GameObject("MapInteractionManager");
        }
        
        MapInteractionManager mapManager = mapManagerGO.GetComponent<MapInteractionManager>();
        if (mapManager == null)
        {
            mapManager = mapManagerGO.AddComponent<MapInteractionManager>();
        }
        
        // Configure settings
        var maxGuessDistanceField = typeof(MapInteractionManager).GetField("maxGuessDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        maxGuessDistanceField?.SetValue(mapManager, maxGuessDistance);
        
        var maxScoreField = typeof(MapInteractionManager).GetField("maxScore", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        maxScoreField?.SetValue(mapManager, maxScore);
        
        Debug.Log("MapInteractionManager configured");
    }
    
    /// <summary>
    /// Sets up the GeoguessrGameManager
    /// </summary>
    private void SetupGeoguessrGameManager()
    {
        GameObject gameManagerGO = GameObject.Find("GeoguessrGameManager");
        if (gameManagerGO == null)
        {
            gameManagerGO = new GameObject("GeoguessrGameManager");
        }
        
        GeoguessrGameManager gameManager = gameManagerGO.GetComponent<GeoguessrGameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerGO.AddComponent<GeoguessrGameManager>();
        }
        
        // Configure settings
        var totalRoundsField = typeof(GeoguessrGameManager).GetField("totalRounds", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        totalRoundsField?.SetValue(gameManager, totalRounds);
        
        var roundTimeLimitField = typeof(GeoguessrGameManager).GetField("roundTimeLimit", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        roundTimeLimitField?.SetValue(gameManager, roundTimeLimit);
        
        // Find and assign MapInteractionManager reference
        var mapManagerField = typeof(GeoguessrGameManager).GetField("mapManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var mapManager = FindObjectOfType<MapInteractionManager>();
        mapManagerField?.SetValue(gameManager, mapManager);
        
        // Find and assign LocationManager reference (if exists)
        var locationManagerField = typeof(GeoguessrGameManager).GetField("locationManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var locationManager = FindObjectOfType<LocationManager>();
        locationManagerField?.SetValue(gameManager, locationManager);
        
        Debug.Log("GeoguessrGameManager configured");
    }
    
    /// <summary>
    /// Sets up the MapUIController
    /// </summary>
    private void SetupMapUIController()
    {
        GameObject uiControllerGO = GameObject.Find("MapUIController");
        if (uiControllerGO == null)
        {
            uiControllerGO = new GameObject("MapUIController");
        }
        
        MapUIController uiController = uiControllerGO.GetComponent<MapUIController>();
        if (uiController == null)
        {
            uiController = uiControllerGO.AddComponent<MapUIController>();
        }
        
        Debug.Log("MapUIController configured");
    }
    
    /// <summary>
    /// Creates basic UI elements for the game
    /// </summary>
    private void CreateUIElements()
    {
        // Create Canvas if it doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create Map Button
        CreateMapButton(canvas);
        
        // Create Score Display
        CreateScoreDisplay(canvas);
        
        // Create Round Display
        CreateRoundDisplay(canvas);
        
        // Create Timer Display
        CreateTimerDisplay(canvas);
        
        // Create Guess Info Display
        CreateGuessInfoDisplay(canvas);
        
        Debug.Log("UI elements created");
    }
    
    /// <summary>
    /// Creates the map button
    /// </summary>
    private void CreateMapButton(Canvas canvas)
    {
        GameObject buttonGO = new GameObject("MapButton");
        buttonGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(100, -50);
        rectTransform.sizeDelta = new Vector2(150, 50);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        
        Button button = buttonGO.AddComponent<Button>();
        
        // Add text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Open Map";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Assign to MapUIController
        var mapUIController = FindObjectOfType<MapUIController>();
        if (mapUIController != null)
        {
            var mapButtonField = typeof(MapUIController).GetField("mapButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mapButtonField?.SetValue(mapUIController, button);
        }
    }
    
    /// <summary>
    /// Creates the score display
    /// </summary>
    private void CreateScoreDisplay(Canvas canvas)
    {
        GameObject scoreGO = new GameObject("ScoreText");
        scoreGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = scoreGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(100, -100);
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        TextMeshProUGUI text = scoreGO.AddComponent<TextMeshProUGUI>();
        text.text = "Score: 0";
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        // Assign to MapUIController
        var mapUIController = FindObjectOfType<MapUIController>();
        if (mapUIController != null)
        {
            var scoreTextField = typeof(MapUIController).GetField("scoreText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scoreTextField?.SetValue(mapUIController, text);
        }
    }
    
    /// <summary>
    /// Creates the round display
    /// </summary>
    private void CreateRoundDisplay(Canvas canvas)
    {
        GameObject roundGO = new GameObject("RoundText");
        roundGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = roundGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(100, -130);
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        TextMeshProUGUI text = roundGO.AddComponent<TextMeshProUGUI>();
        text.text = "Round: 1/5";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        // Assign to MapUIController
        var mapUIController = FindObjectOfType<MapUIController>();
        if (mapUIController != null)
        {
            var roundTextField = typeof(MapUIController).GetField("roundText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            roundTextField?.SetValue(mapUIController, text);
        }
    }
    
    /// <summary>
    /// Creates the timer display
    /// </summary>
    private void CreateTimerDisplay(Canvas canvas)
    {
        GameObject timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = timerGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(100, -160);
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        TextMeshProUGUI text = timerGO.AddComponent<TextMeshProUGUI>();
        text.text = "Time: 60s";
        text.fontSize = 16;
        text.color = Color.yellow;
        text.alignment = TextAlignmentOptions.Left;
        
        // Assign to MapUIController
        var mapUIController = FindObjectOfType<MapUIController>();
        if (mapUIController != null)
        {
            var timerTextField = typeof(MapUIController).GetField("timerText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timerTextField?.SetValue(mapUIController, text);
        }
    }
    
    /// <summary>
    /// Creates the guess info display
    /// </summary>
    private void CreateGuessInfoDisplay(Canvas canvas)
    {
        GameObject guessGO = new GameObject("GuessInfoText");
        guessGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = guessGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = new Vector2(200, 100);
        rectTransform.sizeDelta = new Vector2(400, 60);
        
        TextMeshProUGUI text = guessGO.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 14;
        text.color = Color.cyan;
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = true;
        
        // Initially hidden
        guessGO.SetActive(false);
        
        // Assign to MapUIController
        var mapUIController = FindObjectOfType<MapUIController>();
        if (mapUIController != null)
        {
            var guessInfoTextField = typeof(MapUIController).GetField("guessInfoText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            guessInfoTextField?.SetValue(mapUIController, text);
        }
    }
    
    /// <summary>
    /// Validates the setup
    /// </summary>
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("Validating MNET Geoguessr setup...");
        
        bool isValid = true;
        
        // Check MapInteractionManager
        MapInteractionManager mapManager = FindObjectOfType<MapInteractionManager>();
        if (mapManager == null)
        {
            Debug.LogError("MapInteractionManager not found!");
            isValid = false;
        }
        else
        {
            Debug.Log("MapInteractionManager found");
        }
        
        // Check GeoguessrGameManager
        GeoguessrGameManager gameManager = FindObjectOfType<GeoguessrGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GeoguessrGameManager not found!");
            isValid = false;
        }
        else
        {
            Debug.Log("GeoguessrGameManager found");
        }
        
        // Check MapUIController
        MapUIController uiController = FindObjectOfType<MapUIController>();
        if (uiController == null)
        {
            Debug.LogError("MapUIController not found!");
            isValid = false;
        }
        else
        {
            Debug.Log("MapUIController found");
        }
        
        // Check Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            isValid = false;
        }
        else
        {
            Debug.Log("Canvas found");
        }
        
        if (isValid)
        {
            Debug.Log("Setup validation passed! Ready for WebGL build.");
        }
        else
        {
            Debug.LogError("Setup validation failed. Please fix the issues above.");
        }
    }
}
