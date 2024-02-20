using UnityEngine;

public class CharacterArms : MonoBehaviour
{
    private Animator animator;
    private Vector3 previousPosition;
    public float movementSpeed;

    void Start()
    {
        animator = GetComponent<Animator>(); // Get the Animator component
        previousPosition = transform.position; // Initialize previousPosition
    }

    void Update()
    {
        // Calculate the movement speed based on the distance moved since last frame
        movementSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position; // Update previousPosition for the next frame

        // Update the Animator's Speed parameter
        animator.SetFloat("Speed", movementSpeed);
    }
}
