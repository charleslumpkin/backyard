using UnityEngine;
using System.Collections;

public class SwingRelayer : MonoBehaviour
{
    // Store an array of DamageProducer components
    public DamageProducer[] damageProducers;

    void Awake()
    {
        // Find and store all DamageProducer components in children
        damageProducers = GetComponentsInChildren<DamageProducer>();
    }
    
    public void RelayBeginAttack()
    {
        // Debug.Log("RelayBeginAttack");
        // Iterate through each DamageProducer and call BeginAttack
        foreach (var damageProducer in damageProducers)
        {
            if (damageProducer != null)
            {
                damageProducer.BeginAttack();
            }
        }
    }

    public void RelayEndAttack()
    {  
        // Debug.Log("RelayEndAttack");
        // Iterate through each DamageProducer and call EndAttack
        foreach (var damageProducer in damageProducers)
        {
            if (damageProducer != null)
            {
                damageProducer.EndAttack();
            }
        }
    }
}
