using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		public enum ControlMode
		{
			Building,
			Fighting
		}

		public ControlMode controlModeType = ControlMode.Fighting;

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		private Animator _characterArmsAnimator;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		private bool _isActionPressed = false;

		public float maxBodyHealth = 100f;
		public float currentBodyHealth = 100f;	


#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;
		private WeaponController _weaponController;

		private const float _threshold = 0.01f;
		private Vector3 originalArmsLocalPosition;
		private Quaternion originalArmsLocalRotation;

		private GameObject _characterArms;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Awake()
		{
			currentBodyHealth = maxBodyHealth;

			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			GameObject characterArms = GameObject.Find("CharacterArms");
			if (characterArms != null)
			{
				_characterArmsAnimator = characterArms.GetComponent<Animator>();
				// Store the original local position and rotation
				originalArmsLocalPosition = characterArms.transform.localPosition;
				originalArmsLocalRotation = characterArms.transform.localRotation;
			}
			else
			{
				Debug.LogWarning("CharacterArms object not found. Make sure it is named correctly and present in the hierarchy.");
			}
		}

		private void Start()
		{
			ResetArmsPosition();


			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			controlModeType = ControlMode.Fighting;

#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
			_weaponController = GameObject.Find("Weapons").GetComponent<WeaponController>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{


			changeControlMode();
			JumpAndGravity();
			GroundedCheck();
			Move();
			HandleAction();

			if (Grounded && _characterArmsAnimator.GetBool("isJumping"))
			{
				_characterArmsAnimator.SetBool("isJumping", false);
				ResetArmsPosition();
			}


		}

		void ResetArmsPosition()
		{
			GameObject characterArms = GameObject.Find("CharacterArms");
			if (characterArms != null)
			{
				characterArms.transform.localPosition = originalArmsLocalPosition;
				characterArms.transform.localRotation = originalArmsLocalRotation;
			}
		}


		private void HandleAction()
		{
			// Check the control mode to determine the context of the action
			if (controlModeType == ControlMode.Fighting)
			{
				//Debug.Log("Fighting Mode");
				// When the action button is pressed
				if (_input.action && !_isActionPressed)
				{
					// Debug.Log("Action Pressed");
					// Mark as action pressed
					_isActionPressed = true;
					// Start swinging if not already swinging
					_weaponController.StartSwinging();
					// Debug.Log("Start Swinging");
				}
				else if (!_input.action && _isActionPressed)
				{
					// Debug.Log("Action Released");
					// Mark as action not pressed
					_isActionPressed = false;
					// Stop swinging when the action button is released
					_weaponController.StopSwinging();
					// Debug.Log("Stop Swinging");
				}
			}
			else
			{
				// Handle actions for building mode or other modes here
			}
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set raycast origin at the bottom of the character controller
			Vector3 raycastOrigin = transform.position + Vector3.up * -GroundedOffset;

			// perform a raycast downwards
			RaycastHit hit;
			Grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, 0.5f, GroundLayers, QueryTriggerInteraction.Ignore);

			// adjust the character controller position based on the hit point
			// if (Grounded)
			// {
			// 	transform.position = hit.point + Vector3.up * GroundedOffset;
			// }
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		void OnTriggerEnter(Collider other)
		{
			//Debug.Log("Player hit something with tag: " + other.tag);
		}


		public void TakeDamage(float damage, Vector3 forceDirection)
		{
			currentBodyHealth -= damage;
			GetComponentInChildren<ScreenDamage>().CurrentHealth = currentBodyHealth;
			if (currentBodyHealth <= 0)
			{
				Die();
			}
		}

		//heal over 5 seconds
		public void Heal(float healAmount)
		{
			StartCoroutine(HealOverTime(healAmount, 5.0f));
		}

		IEnumerator HealOverTime(float healAmount, float time)
		{
			float elapsedTime = 0;
			while (elapsedTime < time)
			{
				currentBodyHealth += healAmount * Time.deltaTime;
				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}

		public void Die()
		{
			// Die
			Debug.Log("Player Died");
		}



		private void changeControlMode()
		{
			if (_input.controlMode)
			{
				// Code to toggle the control mode
				controlModeType = controlModeType == ControlMode.Fighting ? ControlMode.Building : ControlMode.Fighting;

				// Immediately reset the controlMode flag after processing the change
				_input.controlMode = false;

				//				Debug.Log($"Control Mode Changed to {controlModeType}");
			}
		}


		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// Calculate normalized speed value between 0 and 1
			float normalizedSpeed = _speed / SprintSpeed; // Normalize speed based on sprint speed as maximum
			normalizedSpeed = Mathf.Clamp(normalizedSpeed, 0f, 1f); // Ensure value is within 0 and 1

			// Update the Animator's MoveSpeed parameter
			if (_characterArmsAnimator != null)
			{
				if (_characterArmsAnimator.GetBool("isSwinging") == false)
				{
					_characterArmsAnimator.SetFloat("moveSpeed", normalizedSpeed);
				}
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{


				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Ensure isJumping is set to false when grounded
				if (_characterArmsAnimator != null)
				{
					_characterArmsAnimator.SetBool("isJumping", false);
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// Set isJumping to true when the jump is initiated
					if (_characterArmsAnimator != null)
					{
						_characterArmsAnimator.SetBool("isJumping", true);
					}
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}


		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}