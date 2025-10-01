using UnityEngine;

/// <summary>
/// Manages bottom bar UI elements that appear at the end of rounds and games.
/// Handles spawning/despawning of round result and play again bars.
/// 
/// Written by Assistant
/// Last Modified: [Current Date]
/// </summary>
public class BottomBarManager : MonoBehaviour
{
    [Header("Bottom Bar Prefabs")]
    [SerializeField] private GameObject roundResultPrefab;
    [SerializeField] private GameObject playAgainPrefab;
    
    [Header("UI Configuration")]
    [SerializeField] private Transform canvasParent;
    [SerializeField] private bool enableDebugLogs = true;
    
    // SpawnPrefab components for each bar type
    private SpawnPrefab roundResultSpawner;
    private SpawnPrefab playAgainSpawner;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Auto-find canvas if not assigned
        if (canvasParent == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvasParent = canvas.transform;
            }
            else
            {
                LogError("No Canvas found! Please assign a Canvas as the parent for bottom bars.");
            }
        }
        
        SetupSpawners();
    }
    
    private void Start()
    {
        // Subscribe to GameLogic events
        if (GameLogic.Instance != null)
        {
            GameLogic.OnRoundStarted += OnRoundStarted;
            GameLogic.OnRoundEnded += OnRoundEnded;
            GameLogic.OnGameEnded += OnGameEnded;
        }
        else
        {
            LogError("GameLogic instance not found! Bottom bars will not work properly.");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GameLogic.OnRoundStarted -= OnRoundStarted;
        GameLogic.OnRoundEnded -= OnRoundEnded;
        GameLogic.OnGameEnded -= OnGameEnded;
    }
    
    #endregion
    
    #region Event Handlers
    
    /// <summary>
    /// Called when a new round starts - hide all bottom bars
    /// </summary>
    private void OnRoundStarted(int roundNumber)
    {
        LogDebug($"Round {roundNumber} started - hiding all bottom bars");
        HideAllBars();
    }
    
    /// <summary>
    /// Called when a round ends - show round result bar
    /// </summary>
    private void OnRoundEnded(int score)
    {
        LogDebug($"Round ended with score {score} - showing round result bar");
        ShowRoundResultBar();
    }
    
    /// <summary>
    /// Called when the game ends - show play again bar
    /// </summary>
    private void OnGameEnded(int finalScore)
    {
        LogDebug($"Game ended with final score {finalScore} - showing play again bar");
        ShowPlayAgainBar();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Shows the round result bottom bar
    /// </summary>
    public void ShowRoundResultBar()
    {
        HideAllBars(); // Hide other bars first
        if (roundResultSpawner != null)
        {
            roundResultSpawner.Spawn();
        }
        else
        {
            LogError("Round result spawner not set up!");
        }
    }
    
    /// <summary>
    /// Shows the play again bottom bar
    /// </summary>
    public void ShowPlayAgainBar()
    {
        HideAllBars(); // Hide other bars first
        if (playAgainSpawner != null)
        {
            playAgainSpawner.Spawn();
        }
        else
        {
            LogError("Play again spawner not set up!");
        }
    }
    
    /// <summary>
    /// Hides all bottom bars
    /// </summary>
    public void HideAllBars()
    {
        if (roundResultSpawner != null)
            roundResultSpawner.Despawn();
            
        if (playAgainSpawner != null)
            playAgainSpawner.Despawn();
    }
    
    /// <summary>
    /// Manually hide round result bar
    /// </summary>
    public void HideRoundResultBar()
    {
        if (roundResultSpawner != null)
            roundResultSpawner.Despawn();
    }
    
    /// <summary>
    /// Manually hide play again bar
    /// </summary>
    public void HidePlayAgainBar()
    {
        if (playAgainSpawner != null)
            playAgainSpawner.Despawn();
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Sets up the SpawnPrefab components for each bar type
    /// </summary>
    private void SetupSpawners()
    {
        // Create round result spawner
        if (roundResultPrefab != null)
        {
            GameObject roundResultSpawnerObj = new GameObject("RoundResultSpawner");
            roundResultSpawnerObj.transform.SetParent(transform);
            roundResultSpawner = roundResultSpawnerObj.AddComponent<SpawnPrefab>();
            
            // Configure the spawner
            var spawnerScript = roundResultSpawnerObj.GetComponent<SpawnPrefab>();
            // Note: We'll need to set prefab and parent via reflection or public setters
            SetSpawnerPrefab(spawnerScript, roundResultPrefab, canvasParent);
        }
        else
        {
            LogError("Round result prefab not assigned!");
        }
        
        // Create play again spawner
        if (playAgainPrefab != null)
        {
            GameObject playAgainSpawnerObj = new GameObject("PlayAgainSpawner");
            playAgainSpawnerObj.transform.SetParent(transform);
            playAgainSpawner = playAgainSpawnerObj.AddComponent<SpawnPrefab>();
            
            // Configure the spawner
            var spawnerScript = playAgainSpawnerObj.GetComponent<SpawnPrefab>();
            SetSpawnerPrefab(spawnerScript, playAgainPrefab, canvasParent);
        }
        else
        {
            LogError("Play again prefab not assigned!");
        }
    }
    
    /// <summary>
    /// Sets the prefab and parent for a SpawnPrefab component using reflection
    /// </summary>
    private void SetSpawnerPrefab(SpawnPrefab spawner, GameObject prefab, Transform parent)
    {
        // Use reflection to set private fields
        var prefabField = typeof(SpawnPrefab).GetField("prefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var parentField = typeof(SpawnPrefab).GetField("parent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (prefabField != null) prefabField.SetValue(spawner, prefab);
        if (parentField != null) parentField.SetValue(spawner, parent);
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[BottomBarManager] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[BottomBarManager] {message}");
    }
    
    #endregion
}
