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
        float mouseY = Input.GetAxis("Mouse Y");


        verticalRotation -= mouseY * sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // Rotate the camera vertically based on mouse Y movement
        transform.localRotation = Quaternion.Euler(verticalRotation, transform.localEulerAngles.y, 0f);
    }

}
