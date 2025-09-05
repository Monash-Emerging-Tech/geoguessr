using UnityEngine;

public class OnGuess : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void HideMapControls()
    {
        BroadcastMessage("Hide");
    }
 
}
