using UnityEngine;

public class CameraMoving : MonoBehaviour
{

    private float maxDegrees = 5f;  // max degrees of rotation allowed
    private float smoothTime = 0.1f;      // smoothing speed

    private Vector3 currentRotation;
    private Vector3 rotationVelocity;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        // Normalise mouse position which is 0,1 value in x and y 
        Vector2 mouseViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector2 centeredMouse = mouseViewport - new Vector2(0.5f, 0.5f);

        // Calculate target rotation angles
        Vector3 targetRotation = new Vector3(
            -centeredMouse.y * maxDegrees,   
            centeredMouse.x * maxDegrees, 
            0f // Don't change z rotation                                 
        );

        // Make the rotation smooth
        currentRotation = Vector3.SmoothDamp(
            currentRotation,
            targetRotation,
            ref rotationVelocity, // gets calculated in here as well, remembers previous velocity
            smoothTime
        );

        transform.localRotation = startRotation * Quaternion.Euler(currentRotation); // Apply the rotation
    }
}
