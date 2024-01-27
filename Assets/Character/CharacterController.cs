using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    private float sensitivity = 2f;
    float horizontalRotation = 0f;
    float moveSpeed = 5f;
    float walkSpeed = 5f;
    float runSpeed = 10f;
    float jumpForce = 5f;
    bool isJumping = false;
    
    Animator animator;

    void Start()
    {
        animator = GameObject.Find("CharacterArms").GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        horizontalRotation += mouseX * sensitivity;
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeed = runSpeed;
            animator.SetFloat("moveSpeed", 1.0f, 0.1f, Time.deltaTime);
        }
        else
        {
            moveSpeed = walkSpeed;
            animator.SetFloat("moveSpeed", 0.5f, 0.1f, Time.deltaTime);
        }

        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        if (moveX==0 && moveZ==0)
        {
            moveSpeed = 0f;
            animator.SetFloat("moveSpeed", 0f, 0.1f, Time.deltaTime);
        }

        transform.Translate(moveX, 0f, moveZ);



        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
        } 




        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain") || collision.gameObject.CompareTag("BuildingPart"))
        {
            isJumping = false;
        }
    }
}

        




    

    


