using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LocationManager;



public class GameLoader : MonoBehaviour
{

    public bool inGame;
    public int currentScore;
    public int currentRound;

    public int GameMode;
    public int mapPackId = 0;


    public LocationManager locationManager = new LocationManager();

    private List<LocationManager.MapPack> mapPacks;


    public static GameLoader Instance; // Global reference

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        inGame = true;
        List<LocationManager.MapPack> mapPacks = locationManager.GetMapPacks(); 
    }

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") {
            locationManager.setCurrentMapPack(mapPackId);
            locationManager.SelectRandomLocation();
        }
        
        Debug.Log("Scene name: " + scene.name);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            locationManager.Start();
            mapPacks = locationManager.GetMapPacks();
        }
        else
        {
            Destroy(gameObject); // Kill duplicates
            return;
        }

    }
}

