using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{

    public GameObject partMesh;
    public enum HealthType
    {
        PlayerArm,
        PlayerWholeBody,
        ZombieArm,
        ZombieLeg,
        ZombieTorso,
        ZombieHead
    }
    public GameObject gibZombieUpperArm;
    public GameObject gibZombieLowerArm;
    public GameObject gibZombieUpperLeg;
    public GameObject gibZombieLowerLeg;
    public GameObject gibZombieHead;


    public float maxHealth = 100.0f;
    public float currentHealth;
    public float forceDecay = 5f; // Decay rate of the applied force
    public HealthType healthType = HealthType.ZombieArm;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {

    }

   public void TakeDamage(float damage, Vector3 forceDirection, float forceMagnitude)
    {
        currentHealth -= damage;
        //Debug.Log("Part: " + healthType + " took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth > 0)
        {
            // Simulate force
            // StartCoroutine(ApplySimulatedForce(forceDirection.normalized, forceMagnitude));
        }
        else
        {
            Die();
        }
    }



    void Die()
    {
        Debug.Log("something died");
        
        if(healthType == HealthType.ZombieArm)
        {
            
            partMesh.SetActive(false);
            transform.parent.gameObject.SetActive(false);
            GameObject upperArm = Instantiate(gibZombieUpperArm, transform.position, transform.rotation);
            GameObject lowerArm = Instantiate(gibZombieLowerArm, transform.position, transform.rotation);
            upperArm.AddComponent<Rigidbody>();
            lowerArm.AddComponent<Rigidbody>(); 

            upperArm.GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
            lowerArm.GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
            
        }


        if(healthType == HealthType.ZombieHead)
        {
            
            // partMesh.SetActive(false);
            //transform.parent.gameObject.SetActive(false);
            GameObject head = Instantiate(gibZombieHead, transform.position, transform.rotation);
            head.AddComponent<Rigidbody>();
            head.GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);

            
        }
        
    }
}
