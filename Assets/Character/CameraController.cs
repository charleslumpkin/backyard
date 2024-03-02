using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float sensitivity = 2f;
    private float verticalRotation = 0f;
    private float maxVerticalAngle = 90f;

    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    private bool isThirdPersonViewActive = false;

    // Reference to the first-person controller script, adjust the type as needed
    public MonoBehaviour firstPersonController;

    // Length of the debug ray.
    private float rayLength = 10f; // You can adjust this value as needed

    private void Start()
    {
        ToggleCameraView(isThirdPersonViewActive);
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleCameraRotation();

            // Draw the debug ray
            DrawDebugRay();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            isThirdPersonViewActive = !isThirdPersonViewActive;
            ToggleCameraView(isThirdPersonViewActive);
        }
    }

    void ToggleCameraView(bool isThirdPerson)
    {
        thirdPersonCamera.enabled = isThirdPerson;
        firstPersonCamera.enabled = !isThirdPerson;

        // Optionally, enable/disable the first-person controller or components that should only be active in first-person view
        if (firstPersonController != null)
        {
            firstPersonController.enabled = !isThirdPerson;
        }

        // If there are any other components or GameObjects that need to be toggled with the view, do so here
    }

    void HandleCameraRotation()
    {
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity;
        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void DrawDebugRay()
    {
        // The ray starts at the camera's position and goes forward in the direction it's facing
        Debug.DrawRay(transform.position, transform.forward * rayLength, Color.green);
    }
}
