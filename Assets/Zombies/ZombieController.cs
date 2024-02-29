using UnityEngine;
using Pathfinding;
using UnityEditor.Experimental.GraphView;
using System.Collections;

public abstract class ZombieState
{
    protected ZombieController controller;

    public ZombieState(ZombieController controller)
    {
        this.controller = controller;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
}

public class IdleState : ZombieState
{
    public IdleState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = 0;
    }
}



public class WalkingState : ZombieState
{
    public WalkingState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = controller.WalkingSpeed;
    }
}

public class RunningState : ZombieState
{
    public RunningState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = controller.RunningSpeed;
    }
}

public class JumpingState : ZombieState
{
    public JumpingState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        if (controller.IsGrounded())
        {
            controller.Animator.SetBool("isJumping", true);
            controller.StartCoroutine(JumpWithDelay());

            IEnumerator JumpWithDelay()
            {
                yield return new WaitForSeconds(0.68f);
                Vector3 jumpDirection = (controller.character.transform.position - controller.transform.position).normalized;
                jumpDirection = (jumpDirection + Vector3.up) * controller.JumpForce;
                controller.Rigidbody.AddForce(jumpDirection, ForceMode.Impulse);
            }
        }
    }

    public override void OnExit()
    {
        controller.Animator.SetBool("isJumping", false);
    }
}

public class AttackingState : ZombieState
{
    public AttackingState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.LookAtCharacter();
        controller.Animator.SetBool("isAttacking", true);
        controller.Animator.SetFloat("attackType", UnityEngine.Random.Range(0.0f, 1.0f));
    }

    public override void OnExit()
    {
        controller.Animator.SetBool("isAttacking", false);
    }
}

public class StuckState : ZombieState
{
    private const float checkInterval = 2f;

    public StuckState(ZombieController controller) : base(controller) { }


    public override void OnEnter()
    {
        controller.TransitionState(new JumpingState(controller));
    }

    public override void Update()
    {

    }
}

public class CrawlingState : ZombieState
{
    public CrawlingState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = controller.WalkingSpeed;
        controller.Animator.SetBool("isCrawling", true);
    }


}

public class ZombieController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Rigidbody rb;
    public GameObject character;

    private ZombieState currentState;
    private Vector3 lastPosition;
    private float moveCheckTimer = 0f;

    public float WalkingSpeed = 2f;
    public float RunningSpeed = 4f;
    public float JumpForce = 1.5f;

    public Animator Animator => animator;
    public AIPath AIPath => aiPath;
    public Rigidbody Rigidbody => rb;

   // private CapsuleCollider capsuleCollider; // Reference to the zombie's capsule collider
    public float checkRadius = 3f; // The radius within which to check for building parts
    public LayerMask terrainLayer; // Assign the layer for the terrain in the inspector
    public bool isNearBuildingPart = false; // Tracks whether the zombie is near a building part
    public float transitionDuration = 0.25f; // Duration over which the posture change should occur

    public float maxHealth = 10f;
    public float currentHealth;
    public bool isDead = false;

    public GameObject rootBone;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody>();
        character = GameObject.Find("PlayerCapsule");
      //  capsuleCollider = GetComponent<CapsuleCollider>();
        terrainLayer = LayerMask.GetMask("Terrain");
        ChangePosture();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        lastPosition = transform.position;
        TransitionState(new RunningState(this)); // Default state to Running
    }

    private void Update()
    {

        if (currentHealth <= 0 && !isDead)
        {

            isDead = true;
            //ActivateRagdoll();
            // Optionally, set a delay before destruction to allow the ragdoll to settle.
            //Destroy(gameObject, 5f); // Adjust the delay as needed.
        }
        if (isDead)
        {
            return;
        }

        currentState.Update();
        CheckProximityToBuildingPart();
        CheckGroundedStatus();

        // Handling state transitions based on keyboard input for jumping
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            TransitionState(new JumpingState(this));
        }

        // Handling automatic transitions based on movement and proximity
        moveCheckTimer += Time.deltaTime;
        if (moveCheckTimer >= 1.0f)
        {
            moveCheckTimer = 0f;
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            // Checking if the zombie has moved less than a threshold distance
            if (distanceMoved < 0.1f && !(currentState is AttackingState))
            {
                TransitionState(new StuckState(this));
                return;
            }
        }

        float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);
        // Transition to attacking state based on proximity to the character
        // Consider adding a flag to ensure this transition happens only once or under certain conditions
        if (distanceToCharacter <= 1.5f && !(currentState is AttackingState))
        {
            TransitionState(new AttackingState(this));
            return;
        }

        if (distanceToCharacter > 1.5f && !(currentState is RunningState))
        {
            TransitionState(new RunningState(this));
            return;
        }
    }


   

    public void ChangePosture()
    {
        StartCoroutine(ChangePostureCoroutine());
    }

    private IEnumerator ChangePostureCoroutine()
    {
        float currentPosture = Animator.GetFloat("posture");
        float targetPosture = UnityEngine.Random.Range(0.5f, 0.85f);

        // Track the time we're interpolating
        float time = 0;

        while (time < transitionDuration)
        {
            // Increment the elapsed time
            time += Time.deltaTime;

            // Calculate the current posture value using Lerp
            float newPosture = Mathf.Lerp(currentPosture, targetPosture, time / transitionDuration);

            // Update the animator's posture parameter
            Animator.SetFloat("posture", newPosture);

            // Wait until the next frame
            yield return null;
        }

        // Ensure the posture is set to the exact target value at the end
        Animator.SetFloat("posture", targetPosture);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

    }




    void CheckProximityToBuildingPart()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius);
        bool foundBuildingPart = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("BuildingPart"))
            {
                foundBuildingPart = true;
                break; // Exit the loop as soon as one building part is found within range
            }
        }

        if (foundBuildingPart && !isNearBuildingPart)
        {
            AdjustPathfindingProperties(true);
            isNearBuildingPart = true;
        }
        else if (!foundBuildingPart && isNearBuildingPart)
        {
            AdjustPathfindingProperties(false);
            isNearBuildingPart = false;
        }
    }

    void AdjustPathfindingProperties(bool nearBuilding)
    {
        if (nearBuilding)
        {
            aiPath.pickNextWaypointDist = 0.3f;
        }
        else
        {
            aiPath.pickNextWaypointDist = 2.0f;
        }
    }

    void CheckGroundedStatus()
    {
        // if (!isNearBuildingPart) // Only check for grounding if the zombie has left a building part
        // {
        //     bool isGrounded = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2 + 0.1f, terrainLayer);
        //     if (isGrounded)
        //     {
        //         capsuleCollider.excludeLayers = LayerMask.GetMask("0"); // Exclude no layers
        //     }
        //     else
        //     {
        //         capsuleCollider.excludeLayers = LayerMask.GetMask("Zombie");//, "Player");
        //     }
        // }
    }

    public void TransitionState(ZombieState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    public bool IsGrounded()
    {
        Vector3 dr = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        // Debug.DrawRay(dr, Vector3.down * 0.2f, Color.red, 1.0f);
        return Physics.Raycast(dr, Vector3.down, 0.2f);
    }

    public void LookAtCharacter()
    {
        Vector3 directionToCharacter = character.transform.position - transform.position;
        directionToCharacter.y = 0; // Remove vertical component of the direction

        if (directionToCharacter != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToCharacter);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }


}
