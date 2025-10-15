using UnityEngine;
using UnityEngine.UI;

/***
 * 
 * ScoreBar, Responsible for displaying the score bar with a progress bar, 
 * 
 * Written by aleu0007
 * Last Modified: 1/08/2025
 * 
 */
[ExecuteInEditMode()]
public class ScoreBar : MonoBehaviour
{   
    private int maximum = 2500;
    private int current = 1000;
    [SerializeField] private Image mask;

    void Awake() => Refresh();
    void OnValidate() => Refresh();             // so it updates in the Editor

    private void Refresh()
    {
        if (!mask) return;

        float fillAmount = (float)current / maximum;
        mask.fillAmount = fillAmount;
    }    
}
