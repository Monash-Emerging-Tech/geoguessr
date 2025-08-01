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
    public int maximum;
    public int current;
    public Image mask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrenFill();
    }

    void GetCurrenFill() 
    {
        float fillAmount = (float)current/(float)maximum;
        mask.fillAmount = fillAmount;
    }
}
