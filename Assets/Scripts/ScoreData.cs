using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Game/Score Data")]
public class ScoreDataScriptableObject : ScriptableObject
{
    [SerializeField] private int currentScore;
    [SerializeField] private int roundScore;
    [SerializeField] private int distanceScore;

    public int CurrentScore => currentScore;

    public int RoundScore => currentScore;

    public int RoundDistance => currentScore;


    public void SetTotalScore(int value) => currentScore = value;

    public void SetRoundScore(int value) => roundScore = value;

    public void SetDistanceScore(int value) => distanceScore = value;

    public void ResetTotalScore() => currentScore = 0;


    public string GetScoreData(string data) {

        switch (data)
        {
            case "total":
                return currentScore.ToString();
            case "round-score":
                return roundScore.ToString();
            case "round-distance":
                return distanceScore + "m";
            default:
                return currentScore.ToString();
        }

    }

}