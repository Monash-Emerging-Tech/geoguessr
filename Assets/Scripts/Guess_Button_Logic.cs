using System;
using TMPro;
using UnityEngine;

public class Guess_Button_Logic : MonoBehaviour
{


    private GameLoader gameLogic;

    public TMP_Text textReference;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameLogic = GameObject.Find("GameLogic").GetComponent<GameLoader>();
        textReference.text = "GUESS";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick() {

        Debug.Log(gameLogic.isGuessing);


        if (gameLogic.isGuessing) {
            gameLogic.enterGuess();
            textReference.text = "NEXT GUESS!";
        }
        else
        {
            gameLogic.nextRound();
            textReference.text = "GUESS";
        }

    }
}
