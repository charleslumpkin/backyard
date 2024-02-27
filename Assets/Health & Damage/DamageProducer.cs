using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProducer : MonoBehaviour
{
    public enum DamageType
    {
        ZombieBlunt,
        ZombieBite,
        WeaponEdged,
        WeaponBlunt
    }

    public float lowerDamage = 1.0f;
    public float upperDamage = 5.0f;
    public float forceMagnitude = 10.0f; // The magnitude of the force to apply
    public DamageType damageType = DamageType.ZombieBlunt;

    private void ApplyDamage(Collider target, float damage, Vector3 forceDirection)
    {
        // Attempt to get the ZombieBodyManager from the parent of the hit collider
        ZombieBodyManager bodyManager = target.GetComponentInParent<ZombieBodyManager>();

        if (bodyManager != null)
        {
            // Assuming each body part has a unique identifier (e.g., a tag or name)
            bodyManager.TakeDamage(target.tag, damage, forceDirection);
        }
    }

    private float CalculateDamage()
    {
        return Random.Range(lowerDamage, upperDamage);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered: " + other.name);  

        if (other.CompareTag("ZombieBodyPart")) // Make sure your body parts have this tag
        {
            float damage = CalculateDamage();
            Vector3 forceDirection = (other.transform.position - transform.position).normalized;
            ApplyDamage(other, damage, forceDirection);
        }
    }
}
