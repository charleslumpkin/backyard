using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float sensitivity = 2f;
    private float verticalRotation = 0f;
    private float maxVerticalAngle = 90f;

    // Length of the debug ray.
    private float rayLength = 10f; // You can adjust this value as needed

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleCameraRotation();

            // Draw the debug ray
            DrawDebugRay();
        }
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
