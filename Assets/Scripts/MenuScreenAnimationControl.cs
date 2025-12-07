using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class MenuScreenAnimationControl : MonoBehaviour
{

    
    // Camera moving after the animation
    public CameraMoving cameraMove;

    public Animator cameraAnimator;

    // UI Sliding in after the animation
    public GameObject canvasToShow;

    public RectTransform uiPanel;
    public float slideDuration = 0.4f;

    // UI slides in and then slides back out a little bit
    public float overshootAmount = 50f;

    // Normal position of UI
    private Vector2 onScreenPos = new Vector2(485, -82);
    
    private Vector2 offScreenPos;
    // How far in total it goes in
    private Vector2 overShootPos;



    void Start()
    {
        // Hide at the start
        canvasToShow.SetActive(false);

        // Initial positions

        offScreenPos = onScreenPos + new Vector2(Screen.width, 0);
        overShootPos = onScreenPos - new Vector2(overshootAmount, 0);

        uiPanel.anchoredPosition = offScreenPos;

        // User cannot use the camera
        cameraMove.enabled = false;

    }


    // Called by your animation event or button
    public void ShowUI()
    {
        canvasToShow.SetActive(true);
        

        // Start UI slide-in animation
        StartCoroutine(SlideInBounce());
    }


    private IEnumerator SlideInBounce()
    {
        float t = 0f;

        // Initial slide to the overshoot
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            // use smoothstep for better accel/deccel
            float smoothTime = Mathf.SmoothStep(0, 1, t / slideDuration); // get next position change
            uiPanel.anchoredPosition = Vector2.Lerp(offScreenPos, overShootPos, smoothTime);

            // Continue on the next frame
            yield return null;
        }

        // Going to the normal position from overshoot
        t = 0f;

        while (t < slideDuration * 0.5f) // only go for half the duration now
        {
            t += Time.unscaledDeltaTime;
            float smoothTime = Mathf.SmoothStep(0, 1, t / (slideDuration * 0.5f));
            uiPanel.anchoredPosition = Vector2.Lerp(overShootPos, onScreenPos, smoothTime);
            yield return null;
        }

        // Double check
        uiPanel.anchoredPosition = onScreenPos;

        // Let user move the camera
        cameraMove.enabled = true;
        cameraAnimator.enabled = false;
    }
}
