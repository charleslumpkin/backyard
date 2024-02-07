using UnityEngine;
using Pathfinding;
using System.Collections;

public class ZombieController : MonoBehaviour
{

    public GameObject player;
    private float zombieSpeedIdle = 0.2f;
    private float zombieSpeedWalk = 1.1f;
    private float zombieSpeedRun = 3.0f;
    private float zombieAnimatorSpeedIdle = 0.4f;
    private float zombieAnimatorSpeedWalk = 0.65f;
    private float zombieAnimatorSpeedRun = 1.0f;

    private float zombieAttackDistance = 1.0f;
    private float zombieAttackRate = 10.0f;
    private float zombieAttackDamage = 10.0f;
    private float zombieHealth = 100.0f;
    private float zombieDetectionDistance = 10.0f;
    private float zombieDetectionAngle = 45.0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isRunning = true;
    private bool isIdle = false;
    private bool isWalking = false;
    private float transitionTime = 0.5f;


    private Animator zombieAnimator;
    private AIPath aiPath;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Character");
        zombieAnimator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
    }

    // Update is called once per frame
    void Update(){
        if (isDead)
        {
            return;
        }
        if (isAttacking)
        {
            return;
        }
        if (Vector3.Distance(transform.position, player.transform.position) <= zombieAttackDistance)
        {
            isAttacking = true;
            StartCoroutine(AttackPlayer());
        }
        else if (CanSeePlayer())
        {
            aiPath.destination = player.transform.position;
            if (isRunning)
            {
                SetRunning();
            }
            else
            {
                SetWalking();
            }
        }
        else
        {
            SetIdle();
        }
    }



    private bool CanSeePlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        if (direction.magnitude < zombieDetectionDistance && angle < zombieDetectionAngle)
        {
            return true;
        }
        return false;
    }
    


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, zombieDetectionDistance);
        Vector3 frontRayPoint = transform.position + (transform.forward * zombieDetectionDistance);
        Vector3 leftRayPoint = frontRayPoint;
        leftRayPoint.x += zombieDetectionAngle;
        Vector3 rightRayPoint = frontRayPoint;
        rightRayPoint.x -= zombieDetectionAngle;
        Gizmos.DrawLine(transform.position, frontRayPoint);
        Gizmos.DrawLine(transform.position, leftRayPoint);
        Gizmos.DrawLine(transform.position, rightRayPoint);
    }

    private IEnumerator AttackPlayer()
    {
        // player.GetComponent<CharacterController>().TakeDamage(zombieAttackDamage);
        zombieAnimator.SetBool("isAttacking", true);
        float attackType = Random.Range(0.0f, 1.0f);
        Debug.Log("Attack Type: " + attackType);
        float startValue = zombieAnimator.GetFloat("attackType");
        float endValue = attackType;
        float elapsedTime = 0.0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float lerpedValue = Mathf.Lerp(startValue, endValue, t);
            zombieAnimator.SetFloat("attackType", lerpedValue);
            yield return null;
        }

        zombieAnimator.SetFloat("attackType", endValue);
        yield return new WaitForSeconds(zombieAttackRate);
        isAttacking = false;
        zombieAnimator.SetBool("isAttacking", false);
    }

    public void TakeDamage(float damage)
    {
        zombieHealth -= damage;
        if (zombieHealth <= 0)
        {
            isDead = true;
            // zombieAnimator.SetTrigger("Die");
            aiPath.canMove = false;
            aiPath.canSearch = false;
            Destroy(gameObject, 5.0f);
        }
    }

    public void SetIdle()
    {
        isRunning = false;
        isWalking = false;
        isIdle = true;
        SetSpeed(zombieSpeedIdle);
        SetAnimatorSpeed(zombieAnimatorSpeedIdle);
    }

    public void SetWalking()
    {
        isRunning = false;
        isWalking = true;
        isIdle = false;
        SetSpeed(zombieSpeedWalk);
        SetAnimatorSpeed(zombieAnimatorSpeedWalk);
    }

    public void SetRunning()
    {
        isRunning = true;
        isWalking = false;
        isIdle = false;
        SetSpeed(zombieSpeedRun);
        SetAnimatorSpeed(zombieAnimatorSpeedRun);
    }

    public void SetSpeed(float speed)
    {
        aiPath.maxSpeed = speed;
    }

    public void SetAnimatorSpeed(float speed)
    {
        zombieAnimator.SetFloat("moveSpeed", speed);
    }


}