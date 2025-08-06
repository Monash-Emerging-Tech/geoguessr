using UnityEngine;

public class OverlayController: MonoBehaviour
{
    // Reference to the overlay UI
    public GameObject overlayUI;

    // Method to show the overlay
    public void ShowOverlay()
    {
        overlayUI.SetActive(true);
    }

    // Method to hide the overlay
    public void HideOverlay()
    {
        overlayUI.SetActive(false);
    }
}
