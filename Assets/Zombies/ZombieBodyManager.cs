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



    void Awake()
    {
        ragdollEnabler = GetComponent<RagdollEnabler>();
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

        //change the color of the blood splatter
        // Renderer renderer = bloodInstance.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     // change the color of the blood from green to dark red based on currentbodyhealth
        //     float healthPercentage = currentBodyHealth / maxBodyHealth;
        //     Color numberColor;
        //     if (healthPercentage > 0.5f)
        //     {
        //         // Above 50% health, interpolate between green and orange
        //         numberColor = Color.Lerp(Color.yellow, Color.green, (healthPercentage - 0.5f) * 2);
        //     }
        //     else
        //     {
        //         // Below or equal to 50% health, interpolate between orange and red
        //         numberColor = Color.Lerp(Color.red, Color.yellow, healthPercentage * 2);
        //     }
        //     renderer.material.color = numberColor;
        // }

        bloodInstance.SetActive(true);
        Destroy(bloodInstance, 2f); // Destroy the number after 10 seconds

        yield return null;
        // float healthPercentage = currentBodyHealth / maxBodyHealth;
        // Color numberColor;
        // if (healthPercentage > 0.5f)
        // {
        //     // Above 50% health, interpolate between green and orange
        //     numberColor = Color.Lerp(Color.yellow, Color.green, (healthPercentage - 0.5f) * 2);
        // }
        // else
        // {
        //     // Below or equal to 50% health, interpolate between orange and red
        //     numberColor = Color.Lerp(Color.red, Color.yellow, healthPercentage * 2);
        // }

        // float delay = 1f / damage;
        // for (int i = 0; i < damage; i++)
        // {
        //     GameObject numberInstance = Instantiate(numberPrefab, position, Quaternion.LookRotation(normal)) as GameObject;
        //     numberInstance.SetActive(true);

        //     // Change the scale randomly between .05 and .25
        //     float randomScale = Random.Range(0.02f, 0.08f);
        //     numberInstance.transform.localScale = new Vector3(0.001f, randomScale, randomScale);

        //     // Set the color of the flat sphere. Assuming the sphere has a Renderer component with a material that can be colored.
        //     Renderer renderer = numberInstance.GetComponent<Renderer>();
        //     if (renderer != null)
        //     {
        //         renderer.material.color = numberColor;
        //     }

        //     Rigidbody rb = numberInstance.AddComponent<Rigidbody>();
        //     rb.useGravity = true;
        //     // Add a little randomness to the force
        //     float randomForce = spurtForce + Random.Range(-1f, 1f);
        //     rb.AddForce(normal * randomForce, ForceMode.Impulse);

        //     Destroy(numberInstance, 10f); // Destroy the number after 10 seconds
        //     yield return new WaitForSeconds(delay);
        // }
    }




    public void TakeDamage(string bodyPartName, float damage, Vector3 forceDirection)
    {
        // Debug.Log("Taking damage: " + bodyPartName);
        var bodyPartField = GetType().GetField(bodyPartName);
        if (bodyPartField != null)
        {
            // Debug.Log("Found body part: " + bodyPartName);
            BodyPartDetails bodyPart = (BodyPartDetails)bodyPartField.GetValue(this);
            ApplyDamage(bodyPart, damage, forceDirection);
            CreateDamageEffect(bodyPart.bodyPartObject.transform.position, forceDirection, (int)damage);
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

            if (bodyPart.meshObject == UpperLeg_L.meshObject || bodyPart.meshObject == UpperLeg_R.meshObject || bodyPart.meshObject == LowerLeg_L.meshObject || bodyPart.meshObject == LowerLeg_R.meshObject)
            {
                // transition the state of the zombie in the zombie controller to a new CrawlingState
                ZombieController zombieController = GetComponent<ZombieController>();
                zombieController.TransitionState(new CrawlingState(zombieController));
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

            // Instantiate giblet
            if (bodyPart.gibletPrefab != null)
            {
                GameObject giblet = Instantiate(bodyPart.gibletPrefab, bodyPart.bodyPartObject.transform.position, Quaternion.identity);
                giblet.SetActive(true);
                Rigidbody gibletRb = giblet.GetComponent<Rigidbody>();
                if (gibletRb != null) gibletRb.AddForce(forceDirection, ForceMode.Impulse);
            }
        }


        if (currentBodyHealth <= 0)
        {
            ragdollEnabler.EnableRagdoll();
            return;
        }


    }

    void Update()
    {

    }

}
