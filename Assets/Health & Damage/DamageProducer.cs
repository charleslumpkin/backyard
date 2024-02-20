using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

public class DamageProducer : MonoBehaviour
{
    public enum DamageType
    {
        ZombieBlunt,
        ZombieBite,
        WeaponEdged,
        WeapongBlunt
    }

    public float lowerDamage = 1.0f;
    public float upperDamage = 5.0f;
    public float forceMagnitude = 10.0f; // New: Define the magnitude of the force to apply
    public DamageType damageType = DamageType.ZombieBlunt;
    public FirstPersonController player;
    public Animator animator;

    private bool isDamaging;

    void Start()
    {
        isDamaging = false;
        player = GameObject.Find("PlayerCapsule").GetComponent<FirstPersonController>();
        animator = GameObject.Find("CharacterArms").GetComponent<Animator>();
    }


    private float calculateDamage()
    {
        return Random.Range(lowerDamage, upperDamage);
    }

    private IEnumerator DamageCoroutine(Collider target, float interval)
    {
        while (target.GetComponent<Health>() != null && player.controlModeType == FirstPersonController.ControlMode.Fighting)
        {
            Health targetHealth = target.GetComponent<Health>();
            float damage = calculateDamage();
            Vector3 forceDirection = (target.transform.position - transform.position).normalized;

            targetHealth.TakeDamage(damage, forceDirection, forceMagnitude);

            yield return new WaitForSeconds(interval); // Wait for 'interval' seconds before applying damage again
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Part: " + other.name + " entered the trigger");

        if (other.GetComponent<Health>() != null && !isDamaging)
        {
            Debug.Log("Part: " + other.name + " is damaging");
            isDamaging = true;
            StartCoroutine(DamageCoroutine(other, 1.0f)); // Example: Apply damage every 1 second
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Health>() != null)
        {
            StopAllCoroutines(); // Stop applying damage when the object exits
            isDamaging = false;
        }
    }
}
