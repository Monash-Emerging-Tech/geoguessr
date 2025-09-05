using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LocationManager;


/***
 * 
 * GameLoader, Responsible for the in game events and keeping track of players score. 
 * 
 * Written by O-Bolt
 * Last Modified: 08/08/2025
 * 
 */
public class GameLoader : MonoBehaviour
{

    // Public Variables 
    public bool inGame;
    public bool userGuessing;
    public int currentScore;
    public int currentRound;

    public int GameMode;
    public int mapPackId = 0;

    // Used to find the LocationManager variables
    public LocationManager locationManager = new LocationManager();

    private List<LocationManager.MapPack> mapPacks;


    public static GameLoader Instance; // Global reference


    // Moves to the Game Scene for the game to start, performs all thes start game logic
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        inGame = true;
        currentRound = 0;
        currentScore = 0;
        List<LocationManager.MapPack> mapPacks = locationManager.GetMapPacks(); 
    }


    // Not too sure how this works but this is called after the Awake() function for MonoBehaviours,
    // This calls the OnSceneLoaded Function, Not sure how it really works or anything
    // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html 
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Function is called when the scene is loaded
    // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the scenes is the GameScene, we should now change the Map and let the game start
        if (scene.name == "GameScene") {
            changeMap();
        }
        
        Debug.Log("Scene name: " + scene.name);
    }


    public void nextRound() {
        currentRound++;
        // currentScore += ; // Update the score eventually
        changeMap();

    }


    // Changes the map shown to the player at the start of a new round
    public void changeMap() {
        locationManager.setCurrentMapPack(mapPackId);
        locationManager.SelectRandomLocation();
    }


    // This ensures that there is only one GameLoader in the game at one time, and ensures that the GameObject is not destroyed
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

