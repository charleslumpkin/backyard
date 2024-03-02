using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBodyManager : MonoBehaviour
{
    [System.Serializable]
    public class BodyPartDetails
    {
        public bool isActive = true;
        public bool isVital = false;
        public GameObject bodyPartObject;
        public GameObject meshObject;
        public GameObject gibletPrefab;
        public float health = 100f;
        public float damageMultiplier = 1f;
    }

    ZombieController zombieController;

    public float maxBodyHealth = 100f;
    public float currentBodyHealth = 100f;
    public GameObject numberPrefab; // Assign your number prefab in the inspector
    public float spurtForce = 0.5f; // Adjust the force of the spurt

    public RagdollEnabler ragdollEnabler;

    [Header("Body Parts Details")]
    public BodyPartDetails Hips;
    public BodyPartDetails Spine_02;
    public BodyPartDetails Head;
    public BodyPartDetails Shoulder_L;
    public BodyPartDetails Elbow_L;
    public BodyPartDetails Shoulder_R;
    public BodyPartDetails Elbow_R;
    public BodyPartDetails UpperLeg_L;
    public BodyPartDetails LowerLeg_L;
    public BodyPartDetails UpperLeg_R;
    public BodyPartDetails LowerLeg_R;

    public bool isTakingDamage = false;


    void Awake()
    {
        ragdollEnabler = GetComponent<RagdollEnabler>();
        currentBodyHealth = maxBodyHealth;
        zombieController = GetComponent<ZombieController>();
    }

    public void CreateDamageEffect(Vector3 position, Vector3 normal, int damage)
    {
        StartCoroutine(CreateDamageEffectCoroutine(position, normal, damage));
    }


    IEnumerator CreateDamageEffectCoroutine(Vector3 position, Vector3 normal, int damage)
    {

        //instantiate a copy of a Blood prefab under "BloodSplatter" game object - choose a randomg child
        GameObject bloodSplatter = GameObject.Find("BloodSplatter");
        GameObject bloodPrefab = bloodSplatter.transform.GetChild(Random.Range(0, bloodSplatter.transform.childCount)).gameObject;
        GameObject bloodInstance = Instantiate(bloodPrefab, position, Quaternion.LookRotation(normal)) as GameObject;



        bloodInstance.SetActive(true);
        Destroy(bloodInstance, 2f); // Destroy the number after 10 seconds

        yield return null;

    }

    public void FinishTakingDamage()
    {
       // Debug.Log("Finish taking damage");
        isTakingDamage = false;
        zombieController.TransitionState(new WalkingState(zombieController));
    }

    public void Die()
    {
        ragdollEnabler.EnableRagdoll();
        Destroy(gameObject, 120f);
    }


    public void TakeDamage(string bodyPartName, float damage, Vector3 forceDirection)
    {
        var bodyPartField = GetType().GetField(bodyPartName);
        if (bodyPartField != null && !isTakingDamage)
        {
            isTakingDamage = true;
            BodyPartDetails bodyPart = (BodyPartDetails)bodyPartField.GetValue(this);
            ApplyDamage(bodyPart, damage, forceDirection);
            CreateDamageEffect(bodyPart.bodyPartObject.transform.position, forceDirection, (int)damage);

            zombieController.TransitionState(new TakeDamageState(zombieController));

            float hitSide = 0.5f;
            
            if(bodyPart.meshObject == Head.meshObject)
            {
                hitSide = 0.5f;
            }
            else if(bodyPart.meshObject == Shoulder_L.meshObject ||  bodyPart.meshObject == UpperLeg_L.meshObject )
            {
                hitSide = 0.0f;
            }
            else if(bodyPart.meshObject == Shoulder_R.meshObject ||  bodyPart.meshObject == UpperLeg_R.meshObject )
            {
                hitSide = 1.0f;
            }
            else if(bodyPart.meshObject == Elbow_L.meshObject ||  bodyPart.meshObject == LowerLeg_L.meshObject )
            {
                hitSide = 0.25f;
            }
            else if(bodyPart.meshObject == Elbow_R.meshObject ||  bodyPart.meshObject == LowerLeg_R.meshObject )
            {
                hitSide = 0.75f;
            }

            SwingRelayer swingRelayer = GetComponentInChildren<SwingRelayer>();
            if (swingRelayer != null)
            {
                swingRelayer.RelayEndAttack();
            }
            

            //Debug.Log("Hit side: " + hitSide);
            zombieController.Animator.SetFloat("hitSide", hitSide);

        }
    }



    private void ApplyDamage(BodyPartDetails bodyPart, float damage, Vector3 forceDirection, bool passDamge = true)
    {
        if (bodyPart == null) return;

        bodyPart.health -= damage;

        if (passDamge)
        {
            currentBodyHealth -= damage * bodyPart.damageMultiplier;
        }


        if (bodyPart.health <= 0)
        {
            bodyPart.isActive = false;

            if (bodyPart.isVital)
            {
                currentBodyHealth = 0;
            }

            if (bodyPart.meshObject == UpperLeg_L.meshObject && LowerLeg_L.isActive)
            {
                LowerLeg_L.health = 0;
                ApplyDamage(LowerLeg_L, 100f, forceDirection, false);
            }

            if (bodyPart.meshObject == UpperLeg_R.meshObject && LowerLeg_R.isActive)
            {
                LowerLeg_R.health = 0;
                ApplyDamage(LowerLeg_R, 100f, forceDirection, false);
            }

            if (bodyPart.meshObject == Shoulder_L.meshObject && Elbow_L.isActive)
            {
                Elbow_L.health = 0;
                ApplyDamage(Elbow_L, 100f, forceDirection, false);
            }

            if (bodyPart.meshObject == Shoulder_R.meshObject && Elbow_R.isActive)
            {
                Elbow_R.health = 0;
                ApplyDamage(Elbow_R, 100f, forceDirection, false);
            }

            // Deactivate mesh and collider if available and it is not Spine_02

            if (bodyPart.meshObject != Spine_02.meshObject)
            {
                if (bodyPart.meshObject != null)
                {
                    bodyPart.meshObject.SetActive(false);
                }
                if (bodyPart.bodyPartObject != null)
                {
                    Collider collider = bodyPart.bodyPartObject.GetComponent<Collider>();
                    if (collider != null) collider.enabled = false;
                }
            }


            if (bodyPart.meshObject == UpperLeg_L.meshObject || bodyPart.meshObject == UpperLeg_R.meshObject || bodyPart.meshObject == LowerLeg_L.meshObject || bodyPart.meshObject == LowerLeg_R.meshObject)
            {
                // transition the state of the zombie in the zombie controller to a new CrawlingState
                zombieController.TransitionState(new CrawlingState(zombieController));
            }


            // Instantiate giblet
            if (bodyPart.gibletPrefab != null)
            {
                GameObject giblet = Instantiate(bodyPart.gibletPrefab, bodyPart.bodyPartObject.transform.position, Quaternion.identity);
                giblet.SetActive(true);
                Rigidbody gibletRb = giblet.GetComponent<Rigidbody>();
                if (gibletRb != null) gibletRb.AddForce(forceDirection, ForceMode.Impulse);
                Destroy(giblet, 120f);
            }
        }


        if (currentBodyHealth <= 0)
        {
            Die();
            return;
        }


    }

    void Update()
    {


    }

}
