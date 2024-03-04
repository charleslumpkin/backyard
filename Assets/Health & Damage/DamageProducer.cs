using System.Collections;
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
        WeaponBlunt
    }



    public float lowerDamage = 1.0f;
    public float upperDamage = 5.0f;
    public float forceMagnitude = 10.0f; // The magnitude of the force to apply
    public DamageType damageType = DamageType.ZombieBlunt;
    private bool canDealDamage = false; // Tracks if we can deal damage
    private GameObject zombie;
    private SwingRelayer swingRelayer;


    void Awake()
    {
        if (damageType == DamageType.ZombieBlunt || damageType == DamageType.ZombieBite)
        {
            zombie = transform.GetComponentInParent<ZombieController>().gameObject;
            if (zombie != null)
            {
                swingRelayer = zombie.GetComponentInChildren<SwingRelayer>();
            }
        }
    }

    // This method is called via Animation Event at the start of the swing animation
    public void BeginAttack()
    {
        // Debug.Log("DamageProducer.BeginAttack");
        canDealDamage = true; // Enable damage
    }

    // This method is called via Animation Event at the end of the swing animation
    public void EndAttack()
    {
        // Debug.Log("DamageProducer.EndAttack");
        canDealDamage = false; // Disable damage for the rest of the swing
    }


    private void ApplyDamage(Collider target, float damage, Vector3 forceDirection)
    {

        ZombieBodyManager bodyManager = target.GetComponentInParent<ZombieBodyManager>();
        if (bodyManager != null)
        {
            string bodyPartName = target.gameObject.name;
            bodyManager.TakeDamage(bodyPartName, damage, forceDirection);
        }
    }

    private float CalculateDamage()
    {
        return Random.Range(lowerDamage, upperDamage);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (canDealDamage)
        {
            float damage = CalculateDamage();
            Vector3 forceDirection = (other.transform.position - transform.position).normalized * forceMagnitude;

            if ((damageType == DamageType.WeaponBlunt || damageType == DamageType.WeaponEdged) && other.CompareTag("ZombieBodyPart"))
            {
                string bodyPartName = other.gameObject.name;
                ZombieBodyManager bodyManager = other.GetComponentInParent<ZombieBodyManager>();
                if (bodyManager != null)
                {
                    bodyManager.TakeDamage(bodyPartName, damage, forceDirection);
                    canDealDamage = false; // Optionally reset here to ensure only the first hit is registered
                }
            }

            if ((damageType == DamageType.ZombieBlunt || damageType == DamageType.ZombieBite) && other.CompareTag("Player")) 
            {

                other.transform.root.GetComponent<FirstPersonController>().TakeDamage(damage, forceDirection);
                canDealDamage = false; // Optionally reset here to ensure only the first hit is registered
                swingRelayer.RelayEndAttack();
            }

            if(other.CompareTag("BuildingPart"))
            {
                other.GetComponent<BuildingPart>().TakeDamage(damage);
            }


        }
    }

}
