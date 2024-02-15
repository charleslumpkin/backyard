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
        controller.Animator.SetFloat("moveSpeed", 0.0f);
    }
}

public class WalkingState : ZombieState
{
    public WalkingState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = controller.WalkingSpeed;
        controller.Animator.SetFloat("moveSpeed", 0.5f);
    }
}

public class RunningState : ZombieState
{
    public RunningState(ZombieController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AIPath.maxSpeed = controller.RunningSpeed;
        controller.Animator.SetFloat("moveSpeed", 1.0f);
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
                controller.Rigidbody.AddForce(Vector3.up * controller.JumpForce, ForceMode.Impulse);
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
    }

    public override void OnExit()
    {
        controller.Animator.SetBool("isAttacking", false);
    }
}

public class StuckState : ZombieState
{
    private float timeInState = 0f;
    private const float checkInterval = 2f;

    public StuckState(ZombieController controller) : base(controller) { }


    public override void OnEnter()
    {
        // controller.transform.LookAt(controller.character.transform);


        // if (UnityEngine.Random.Range(0, 2) == 0 && controller.IsGrounded())
        // {
        controller.TransitionState(new JumpingState(controller));
        // }
        // else
        // {
        //     Debug.Log("Attacking");
        //     controller.TransitionState(new AttackingState(controller));
        // }
    }

    public override void Update()
    {

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

    private CapsuleCollider capsuleCollider; // Reference to the zombie's capsule collider
    public float checkRadius = 3f; // The radius within which to check for building parts
    public LayerMask terrainLayer; // Assign the layer for the terrain in the inspector
    public bool isNearBuildingPart = false; // Tracks whether the zombie is near a building part


    private void Awake()
    {
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody>();
        character = GameObject.Find("PlayerCapsule");
        capsuleCollider = GetComponent<CapsuleCollider>();
        terrainLayer = LayerMask.GetMask("Terrain");
    }

    private void Start()
    {
        lastPosition = transform.position;
        TransitionState(new RunningState(this)); // Default state to Running
    }

    private void Update()
    {
        currentState.Update();
        CheckProximityToBuildingPart();
        CheckGroundedStatus();


        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            TransitionState(new JumpingState(this));
        }

        moveCheckTimer += Time.deltaTime;
        if (moveCheckTimer >= 1.0f)
        {
            // Debug.Log("Checking movement");
            moveCheckTimer = 0f;
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);
            if (distanceToCharacter <= 1.5f)
            {
                // Debug.Log("Attacking");
                TransitionState(new AttackingState(this));
            }
            else if (distanceMoved < 0.1f)
            { // Assuming the zombie is stuck if it moves less than 0.1 units in a second
                // Debug.Log("Stuck");
                TransitionState(new StuckState(this));
            }
            else
            {
                // Debug.Log("Running");
                // Decide whether to keep running or change to jumping
                TransitionState(new RunningState(this)); // or new JumpingState(this) based on additional logic
            }
        }

        if (GetComponent<SphereCollider>())
        {
            GetComponent<SphereCollider>().enabled = true;
        }
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
        if (!isNearBuildingPart) // Only check for grounding if the zombie has left a building part
        {
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2 + 0.1f, terrainLayer);
            if (isGrounded)
            {
                capsuleCollider.excludeLayers = LayerMask.GetMask("Player");
            }
            else
            {
                capsuleCollider.excludeLayers = LayerMask.GetMask("Zombie", "Player");
            }
        }
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
