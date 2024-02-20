using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using StarterAssets;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    private FirstPersonController player;
    Animator animator;
    bool _isSwinging = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerCapsule").GetComponent<FirstPersonController>();
        animator = GameObject.Find("CharacterArms").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player.controlModeType == FirstPersonController.ControlMode.Fighting)
        {
            showWeapon();
        }
        else
        {
            hideWeapon();
        }
        
    }

    public void StartSwinging()
    {
        // Start the swinging animation or logic
        if (!_isSwinging)
        {
            _isSwinging = true;
            animator.SetBool("isSwinging", true);
        }
    }

    public void StopSwinging()
    {
        if (_isSwinging)
        {
            _isSwinging = false;
            animator.SetBool("isSwinging", false);
        }
    }

    private void showWeapon()
    {
        //activate the first child
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void  hideWeapon()
    {
        //deactivate the first child
        transform.GetChild(0).gameObject.SetActive(false);
    }

    
}
