using System;
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

    public bool isGuessing;

    public int currentScore;
    public int currentRound;

    public int GameMode = 0;
    public int mapPackId = 0;

    // Used to find the LocationManager variables
    public LocationManager locationManager = new LocationManager();

    private List<LocationManager.MapPack> mapPacks;


    public static GameLoader Instance; // Global reference


    // Moves to the Game Scene for the game to start, performs all thes start game logic
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
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
            inGame = true;
            currentRound = 0;
            currentScore = 0;
            isGuessing = true;
            nextRound();
        }
        
        Debug.Log("Scene name: " + scene.name);
    }


    public void nextRound() {
        currentRound++;
        isGuessing = true;
        // currentScore += ; // Update the score eventually
        changeMap();

    }



    public void enterGuess()
    {
        isGuessing = false;
        int Score = calculateScore(new Vector3(0, 0, 0), locationManager.currentLocation);
        currentScore += Score;

    }


    public int calculateScore(Vector3 guess, Location answer)
    {
        int distance = (int)Math.Sqrt(Math.Pow(guess.x - answer.x, 2) + Math.Pow(guess.y - answer.y, 2));

        int score = 5000 - distance;

        return distance;
    }



    // Changes the map shown to the player at the start of a new round
    public void changeMap() {
        locationManager.setCurrentMapPack(mapPackId);
        locationManager.SelectRandomLocation();
    }
    

    // More Game Control Logic Here





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

