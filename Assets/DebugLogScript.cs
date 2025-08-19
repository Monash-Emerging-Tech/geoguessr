using UnityEngine;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;

public class DebugLogScript : MonoBehaviour
{
    private GameObject gameManager;
    private GameLoader gameLoader;
    private TMP_Text text;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameLogic");
        if (gameManager != null)
        {
            Debug.Log("Debug Log Found GameManager!");
            gameLoader = gameManager.GetComponent<GameLoader>();
            text = GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogWarning("GameManager not found!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager != null)
        {
            text.text = "Debug Log:" +
            "\r\ninGame: " + gameLoader.inGame +
            "\r\nCurrent Score: " + gameLoader.currentScore +
            "\r\nRound: " + gameLoader.currentRound +
            "\r\nMapPackId: " + gameLoader.mapPackId +
            "\r\nMode: " + gameLoader.GameMode +
            "\r\nLocation: " + gameLoader.locationManager.currentLocation.Name +
            "\r\nLocation X: " + gameLoader.locationManager.currentLocation.x +
            "\r\nLocation Y: " + gameLoader.locationManager.currentLocation.y +
            "\r\nLocation Z: " + gameLoader.locationManager.currentLocation.z +
            "\r\nLocation POI: " + gameLoader.locationManager.currentLocation.POI;
        }

    }
}
