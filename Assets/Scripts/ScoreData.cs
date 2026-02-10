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


    /* Function for Calculating the Score based on the distance away
    // The Score is based on how close you are to the target, with different 'harshness' applied to certain distances
    // Within 3m is a Perfect Guess
    // Between 4m and 25m inclusive the score will drop from 495 to 450
    // Between 26m and 50m inclusive the score will drop from 447 to 350
     Everything above 700m will result in a score of 0

     Feel Free to play around with the values to make certain distance regions more harsh than others, this does not take into account z level

    */
    public static int CalculateScore(int distance)
    {

        switch (distance)
        {
            case <= 3:
                return 500;

            case <= 25:
                return CalculateInterval(distance, 4, 25, 495, 450);

            case <= 50:
                return CalculateInterval(distance, 26, 50, 447, 350); 

            case <= 250:
                return CalculateInterval(distance, 51, 250, 345, 200); 

            case <= 300:
                return CalculateInterval(distance, 251, 300, 198, 100); 

            case <= 500:
                return CalculateInterval(distance, 301, 500, 98, 10); 

            case <= 700:
                return CalculateInterval(distance, 501, 700, 10, 0); 

            default:
                return 0;
        }

        int CalculateInterval(int distance, int minDis, int maxDis, int maxScore, int minScore) {

            double gradient = (double)(minScore - maxScore) / (maxDis - minDis); // Negative gradient

            int score = (int)Math.Round(maxScore + gradient*(distance - minDis));

            return score;
        }

    }

}