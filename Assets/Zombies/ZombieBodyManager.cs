using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBodyManager : MonoBehaviour
{
    [System.Serializable]
    public class BodyPartDetails
    {
        public GameObject bodyPartObject;
        public GameObject meshObject;
        public GameObject gibletPrefab;
        public float health = 100f;
        public float damageMultiplier = 1f;
    }

    [Header("Body Parts Details")]
    public BodyPartDetails hips;
    public BodyPartDetails torso;
    public BodyPartDetails head;
    public BodyPartDetails leftArmUpper;
    public BodyPartDetails leftArmLower;
    public BodyPartDetails rightArmUpper;
    public BodyPartDetails rightArmLower;
    public BodyPartDetails leftLegUpper;
    public BodyPartDetails leftLegLower;
    public BodyPartDetails rightLegUpper;
    public BodyPartDetails rightLegLower;

    public void TakeDamage(string bodyPartName, float damage, Vector3 forceDirection)
    {
        var bodyPartField = GetType().GetField(bodyPartName);
        if (bodyPartField != null)
        {
            BodyPartDetails bodyPart = (BodyPartDetails)bodyPartField.GetValue(this);
            ApplyDamage(bodyPart, damage, forceDirection);
        }
    }

    private void ApplyDamage(BodyPartDetails bodyPart, float damage, Vector3 forceDirection)
    {
        if (bodyPart == null) return;

        bodyPart.health -= damage * bodyPart.damageMultiplier;
        if (bodyPart.health <= 0)
        {
            // Deactivate mesh and collider
            if (bodyPart.meshObject != null) bodyPart.meshObject.SetActive(false);
            if (bodyPart.bodyPartObject != null)
            {
                Collider collider = bodyPart.bodyPartObject.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(head.health > 0){
                TakeDamage("head", 10f, Vector3.one);
                return;
            }
            if(leftArmLower.health > 0){
                TakeDamage("leftArmLower", 10f, Vector3.one);
                return;
            }
            if(leftArmUpper.health > 0){
                TakeDamage("leftArmUpper", 10f, Vector3.one);
                return;
            }
            if(rightArmLower.health > 0){
                TakeDamage("rightArmLower", 10f, Vector3.one);
                return;
            }
            if(rightArmUpper.health > 0){
                TakeDamage("rightArmUpper", 10f, Vector3.one);
                return;
            }
            if(leftLegLower.health > 0){
                TakeDamage("leftLegLower", 10f, Vector3.one);
                return;
            }
            if(leftLegUpper.health > 0){
                TakeDamage("leftLegUpper", 10f, Vector3.one);
                return;
            }
            if(rightLegLower.health > 0){
                TakeDamage("rightLegLower", 10f, Vector3.one);
                return;
            }
            if(rightLegUpper.health > 0){
                TakeDamage("rightLegUpper", 10f, Vector3.one);
                return;
            }
            if(torso.health > 0){
                TakeDamage("torso", 10f, Vector3.one);
                return;
            }


        }
    }

}
