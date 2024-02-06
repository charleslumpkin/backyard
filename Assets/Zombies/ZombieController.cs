using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{

    public GameObject player;
    public float zombieSpeed = 1.0f;
    public float zombieAttackDistance = 1.0f;
    public float zombieAttackRate = 1.0f;
    public float zombieAttackDamage = 10.0f;
    public float zombieHealth = 100.0f;
    public float zombieDetectionDistance = 10.0f;
    public float zombieDetectionAngle = 45.0f;
    public bool isAttacking = false;
    public bool isDead = false;
    public bool isChasing = false;

    
}