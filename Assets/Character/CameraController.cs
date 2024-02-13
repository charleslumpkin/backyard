using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private float sensitivity = 2f;       
    private float verticalRotation = 0f; // Declare the verticalRotation variable
    private float maxVerticalAngle = 90f; // Declare the maxVerticalAngle variable

    // Start is called before the first frame update
    void Start()
    {

    }
    void OnGUI()
    {
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        GUI.color = Color.red; // Set the GUI color to red
        GUI.DrawTexture(new Rect(center.x, center.y, 3, 3), Texture2D.whiteTexture);
        GUI.color = Color.white; // Reset the GUI color to white
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleCameraRotation(); // Handle vertical rotation based on mouse input
        }

    }

    void HandleCameraRotation()
    {
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity; // Invert the mouseY input
        verticalRotation += mouseY; // Add the mouseY input to the verticalRotation variable
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle); // Clamp the verticalRotation variable between -maxVerticalAngle and maxVerticalAngle
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Rotate the camera around the x-axis based on the verticalRotation variable
    }

}
