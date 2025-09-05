using UnityEngine;

public class HideMapControls : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Hide() {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
