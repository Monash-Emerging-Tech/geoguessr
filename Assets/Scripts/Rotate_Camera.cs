using Unity.VisualScripting;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{

    // OriginPoint, set to an object at 0,0,0 not sure if it actual matters where, we just need a reference to it
    public Transform originPoint;          

    // Sensitivity of the mouse, likely to be controlled in the actual in-game settings
    public float mouseSensitivity = 5.0f;


    // Make private after testing
    public float smoothingFactor = 0.1f; 
    public float maxMouseDelta = 1.0f;    // Max allowed mouse movement per frame


    private float xSpeed = 120.0f;     // Speed of horizontal rotation
    private float ySpeed = 120.0f;     // Speed of vertical rotation


    // Limits on the y angle to make sure it doesn't flip
    private float xMinLimit = -80f;    
    private float xMaxLimit = 80f;     

    private float hor = 0.0f;
    private float vert = 0.0f;

    private float smoothx = 0.0f;
    private float smoothy = 0.0f;

    // Not relevant in our case since we're not looking at an object, we're just moving the camera around the world
    private float distance = 10f;   

    void Start()
    {

        if (originPoint == null)
        {
            Debug.LogError("No OriginPoint found for the Camera Rotation");
            enabled = false;
            return;
        }

        // Get camera's current angles
        Vector3 angles = transform.eulerAngles;
        hor = angles.x;
        vert = angles.y;
    }

    void LateUpdate()
    {
        // declare this else where with a function once the user has ability to edit mouse sensitivity
        xSpeed = mouseSensitivity*200;
        ySpeed = mouseSensitivity*200;

        // Left Mouse Button, check if it's down
        if (Input.GetMouseButton(0)) 
        {
            // Get mouse delta input and clamp it to avoid large jumps
            float deltaX = Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            float deltaY = Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            // Ensure the mouse movement speed did not change too rapidly during the frame
            deltaX = Mathf.Clamp(deltaX, -maxMouseDelta, maxMouseDelta);
            deltaY = Mathf.Clamp(deltaY, -maxMouseDelta, maxMouseDelta);



            // Apply smoothing to mouse movement by averaging the current and previous deltas
            // Lerp function ensures that the new rotation speed is no more than 10% of the previous rotation speed (given default values)
            smoothx = Mathf.Lerp(smoothx, deltaX, smoothingFactor);
            smoothy = Mathf.Lerp(smoothy, deltaY, smoothingFactor);

            // Change these to invert controls, controls currently align with google maps

            hor -= smoothx;
            vert += smoothy;

            // Ensure the angle is between it's flipping limits
            vert = Mathf.Clamp(vert, xMinLimit, xMaxLimit);
        }

        // Position the camera now given the new rotation with reference to the originpoint
        Quaternion rotation = Quaternion.Euler(vert, hor, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + originPoint.position;

        transform.rotation = rotation;
        transform.position = position;
    }

}