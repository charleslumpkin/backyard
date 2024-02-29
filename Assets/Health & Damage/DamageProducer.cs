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
            // Use the GameObject name or another identifier to derive the body part name
            string bodyPartName = target.gameObject.name;

            // Optionally, map the GameObject name to the expected body part name if they don't match directly
            // Example: if (bodyPartName == "ZombieHead") bodyPartName = "head";

            // Assuming each body part has a unique identifier (e.g., a tag or name) that matches the field names in ZombieBodyManager
            bodyManager.TakeDamage(bodyPartName, damage, forceDirection);
        }
    }

    private float CalculateDamage()
    {
        return Random.Range(lowerDamage, upperDamage);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Triggered: " + other.name);

        if (other.CompareTag("ZombieBodyPart")) // Ensure body parts are tagged correctly
        {
            float damage = CalculateDamage();
            Vector3 forceDirection = (other.transform.position - transform.position).normalized * forceMagnitude;

            // Assuming the body part's name is the GameObject name
            string bodyPartName = other.gameObject.name; // Adjust this if your identification logic differs

            // Find the ZombieBodyManager in the parent
            ZombieBodyManager bodyManager = other.GetComponentInParent<ZombieBodyManager>();
            if (bodyManager != null)
            {
                // Correctly pass the body part name. You might need to adjust the naming to match.
                bodyManager.TakeDamage(bodyPartName, damage, forceDirection);
            }
        }
    }

}
