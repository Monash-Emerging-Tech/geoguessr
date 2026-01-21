using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Game/Score Data")]
public class ScoreDataScriptableObject : ScriptableObject
{
    [SerializeField] private int currentScore;
    public int CurrentScore => currentScore;

    public void SetScore(int value) => currentScore = value;
    public void ResetScore() => currentScore = 0;
}