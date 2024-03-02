using System;
using Pathfinding;
using UnityEngine;

public class RagdollEnabler : MonoBehaviour
{
    [SerializeField]
    private Animator Animator;
    [SerializeField]
    private Transform RagdollRoot;
    [SerializeField]
    private bool StartRagdoll = false;
    public Seeker seeker;
    public AIPath aiPath;
    public AIDestinationSetter aiDestinationSetter;

    // Only public for Ragdoll Runtime GUI for explosive force
    public Rigidbody[] Rigidbodies;
    private CharacterJoint[] Joints;
    private Collider[] Colliders;



    private void Awake()
    {
        Rigidbodies = RagdollRoot.GetComponentsInChildren<Rigidbody>();
        Joints = RagdollRoot.GetComponentsInChildren<CharacterJoint>();
        Colliders = RagdollRoot.GetComponentsInChildren<Collider>();

        if (StartRagdoll)
        {
            EnableRagdoll();
        }
        else
        {
            EnableAnimator();
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Animator.enabled)
            {
                EnableRagdoll();
            }
            else
            {
                EnableAnimator();
            }
        }
    }

    public void EnableRagdoll()
    {
        Animator.enabled = false;
        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = true;
        }
        foreach (Collider collider in Colliders)
        {
            if (collider.gameObject.name == "Hand_L" || collider.gameObject.name == "Hand_R")
            {
                Destroy(collider);

            }
            else
            {
                collider.isTrigger = false;
            }
        }
        foreach (Rigidbody rigidbody in Rigidbodies)
        {
            if (rigidbody.gameObject.name == "Hand_L" || rigidbody.gameObject.name == "Hand_R")
            {
                 Destroy(rigidbody);
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.velocity = Vector3.zero;
                rigidbody.detectCollisions = true;
                rigidbody.useGravity = true;
            }
        }

        foreach (DamageProducer damageProducer in GetComponentsInChildren<DamageProducer>())
        {
            Destroy(damageProducer);
        }


        seeker.enabled = false;
        aiPath.enabled = false;
        aiDestinationSetter.enabled = false;
    }

    public void EnableAnimator()
    {
        Animator.enabled = true;
        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = false;
        }
        foreach (Collider collider in Colliders)
        {
            collider.isTrigger = true;
        }
        foreach (Rigidbody rigidbody in Rigidbodies)
        {
            rigidbody.detectCollisions = true;
            rigidbody.useGravity = false;
        }

        seeker.enabled = true;
        aiPath.enabled = true;
        aiDestinationSetter.enabled = true;

    }
}