using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


/***
* Rotate_Camera.cs, used to handle rotating the camera by clicking and dragging the left mouse button 
* Written by: O_Bolt
* Last Modified: 15/07/25
*/



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

    // Important to remember that this is a rotation around the axis, meaning looking up and down is the x-axis
    private float x = 0.0f;
    private float y = 0.0f;

    private float smoothx = 0.0f;
    private float smoothy = 0.0f;

    // Not relevant in our case since we're not looking at an object, we're just moving the camera around the world
    private float distance = 10f;


    public List<GameObject> cameraDisabledUIElements = new List<GameObject>();
    private bool dragStartedOnUI = false;

    void Start()
    {

        // Quick Debug, always have an invisible object set at 0,0,0 so that the camera can rotate/orbit around
        if (originPoint == null)
        {
            Debug.LogError("No OriginPoint found for the Camera Rotation");
            enabled = false;
            return;
        }

        // Get camera's current angles
        Vector3 angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;
    }

    void LateUpdate()
    {

        // declare this else where with a function once the user has ability to edit mouse sensitivity
        xSpeed = mouseSensitivity*200;
        ySpeed = mouseSensitivity*200;



        // Check if the Mouse was clicked, this only happens on the first frame of the mouse being clicked
        if (Input.GetMouseButtonDown(0))
        {
            dragStartedOnUI = IsMouseOverUI();  // Check if over a UI Element
        }

        // Left Mouse Button, check if it's down
        if (Input.GetMouseButton(0)) 
        {
            
            if (dragStartedOnUI)
                return;

            // Get mouse delta input and clamp it to avoid large jumps
            float deltaX = Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            float deltaY = Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            // Ensure the mouse movement speed did not change too rapidly during the frame
            deltaX = Mathf.Clamp(deltaX, -maxMouseDelta, maxMouseDelta);
            deltaY = Mathf.Clamp(deltaY, -maxMouseDelta, maxMouseDelta);



            // Apply smoothing to mouse movement by averaging the current and previous deltas
            // Lerp function ensures that the new rotation speed is no more than a 10% increase of the previous rotation speed (given default values)
            // ^^ above statement may be false actually, I've got no clue what I'm talking about
            smoothx = Mathf.Lerp(smoothx, deltaX, smoothingFactor);
            smoothy = Mathf.Lerp(smoothy, deltaY, smoothingFactor);


            // Change these to invert controls, controls currently align with google maps
            x -= smoothx;
            y += smoothy;

            // Ensure the angle is between it's flipping limits
            y = Mathf.Clamp(y, xMinLimit, xMaxLimit);
        }

        // Position the camera now given the new rotation with reference to the originpoint
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + originPoint.position;

        transform.rotation = rotation;
        transform.position = position;
    }


    // Checks if the mouse is currently over one of the excluded UI that is set in the unity project
    private bool IsMouseOverUI()
    {

        // Prompted, no idea how this works currently, I'll understand it later lol
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (cameraDisabledUIElements.Contains(result.gameObject))
                return true;
        }

        return false;
    }
}