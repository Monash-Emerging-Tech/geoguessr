using UnityEngine;
using UnityEngine.UI;

/***
 * 
 * ScoreBar, Responsible for displaying the score bar with a progress bar, 
 * 
 * Written by aleu0007
 * Last Modified: 31/01/2026
 * 
 */
[ExecuteInEditMode()]
public class ScoreBar : MonoBehaviour
{
    public int maximum;

    public Image mask;
    // [SerializeField] private string editorText = "Default Text";
    [SerializeField] private ScoreDataScriptableObject scoreData;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Awake() => Refresh();
    void OnValidate() => Refresh();             // so it updates in the Editor

    private void Refresh()
    {
        int current = (scoreData != null) ? scoreData.CurrentScore : 0;
        float fillAmount = (float)current / (float)maximum;
        mask.fillAmount = fillAmount;
    }
}
