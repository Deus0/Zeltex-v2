/*using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof (CharacterController))]
	[RequireComponent(typeof (AudioSource))]
	public class CustomController : MonoBehaviour
	{
		// used to move the character
		public bool IsBot = false;
		public float horizontal;
		public float vertical;
		public float fly;
		public bool IsFlyMode;
		[SerializeField] public bool m_IsWalking;
		[SerializeField] public float m_WalkSpeed=4;
		[SerializeField] public float m_RunSpeed=6;
		[SerializeField] [Range(0f, 1f)] public float m_RunstepLenghten;
		[SerializeField] public float m_JumpSpeed=6.66f;
		[SerializeField] public float m_StickToGroundForce=10;
		[SerializeField] public float m_GravityMultiplier=2;
		[SerializeField] private MouseLook m_MouseLook = new MouseLook();
		[SerializeField] public bool m_UseFovKick;
		[SerializeField] public FOVKick m_FovKick = new FOVKick();
		[SerializeField] public bool m_UseHeadBob;
		[SerializeField] public CurveControlledBob m_HeadBob = new CurveControlledBob();
		[SerializeField] public LerpControlledBob m_JumpBob = new LerpControlledBob();
		[SerializeField] public float m_StepInterval;
		[SerializeField] public AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
		[SerializeField] public AudioClip m_JumpSound;           // the sound played when character leaves the ground.
		[SerializeField] public AudioClip m_LandSound;           // the sound played when character touches back on ground.
		
		[SerializeField] public Camera m_Camera;
		public bool m_Jump;
		private float m_YRotation;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private Vector3 m_OriginalCameraPosition;
		private float m_StepCycle;
		private float m_NextStep;
		private bool m_Jumping;
		public AudioSource m_AudioSource;
		
		// Use this for initialization
		private void Start() {
			m_StepCycle = 0f;
			m_NextStep = m_StepCycle/2f;
			m_Jumping = false;
			m_CharacterController = GetComponent<CharacterController>();
			m_AudioSource = GetComponent<AudioSource>();
			//if (!IsBot && Camera.main != null)
			//	UpdateCamera (Camera.main);
		}

		public void UpdateCamera(Camera NewCamera) {
			if (NewCamera != null && !IsBot) {
				m_Camera = NewCamera;
				m_MouseLook.Init (transform, m_Camera.transform);
				m_OriginalCameraPosition = m_Camera.transform.localPosition;
				m_FovKick.IncreaseCurve = new AnimationCurve ();
				m_FovKick.Setup (m_Camera);
				m_HeadBob.Setup (m_Camera, m_StepInterval);
			} else {
				if (!IsBot)
				Debug.LogError ("Custom Controller does not have a camera! New Camera equal to null!");
			}
		}
		
		// Update is called once per frame
		private void Update()
		{
			RotateView();
			// the jump state needs to read here to make sure it is not missed
			if (!m_Jump)
			{
				//m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
			
			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
			{
				StartCoroutine(m_JumpBob.DoBobCycle());
				PlayLandingSound();
				m_MoveDir.y = 0f;
				m_Jumping = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
			{
				m_MoveDir.y = 0f;
			}
			
			m_PreviouslyGrounded = m_CharacterController.isGrounded;
		}
		
		
		private void PlayLandingSound()
		{
			m_AudioSource.clip = m_LandSound;
			m_AudioSource.Play();
			m_NextStep = m_StepCycle + .5f;
		}
		
		public bool ForceJump = false;
		private void FixedUpdate()
		{
			float speed;
			GetInput(out speed);
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;
			
			// get a normal for the surface that is being touched to move along it

				RaycastHit hitInfo;
				Physics.SphereCast (transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			                   m_CharacterController.height / 2f);
				desiredMove = Vector3.ProjectOnPlane (desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x*speed;
			m_MoveDir.z = desiredMove.z*speed;

			//if (!IsBot) {
				if (m_CharacterController.isGrounded || ForceJump) {
					m_MoveDir.y = -m_StickToGroundForce;
				
					if (m_Jump) {
						m_MoveDir.y = m_JumpSpeed;
						PlayJumpSound ();
						m_Jump = false;
						m_Jumping = true;
					}
				} else {
					m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
				}
				if (IsFlyMode) {
					m_MoveDir.y = fly;
					m_MoveDir.x *= 5f;
					m_MoveDir.z *= 5f;
				}
				m_CollisionFlags = m_CharacterController.Move (m_MoveDir * Time.fixedDeltaTime);
				ProgressStepCycle(speed);
				UpdateCameraPosition(speed);
			//}
		}
		
		
		private void PlayJumpSound()
		{
			m_AudioSource.clip = m_JumpSound;
			m_AudioSource.Play();
		}
		
		
		private void ProgressStepCycle(float speed)
		{
			if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
			{
				m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
					Time.fixedDeltaTime;
			}
			
			if (!(m_StepCycle > m_NextStep))
			{
				return;
			}
			
			m_NextStep = m_StepCycle + m_StepInterval;
			
			PlayFootStepAudio();
		}

		private void PlayFootStepAudio()
		{
			if (!m_CharacterController.isGrounded)
			{
				return;
			}
			// pick & play a random footstep sound from the array,
			// excluding sound at index 0
			if (m_FootstepSounds != null && m_FootstepSounds.Length > 0) {
				int n = Random.Range (1, m_FootstepSounds.Length);
				m_AudioSource.clip = m_FootstepSounds [n];
				m_AudioSource.PlayOneShot (m_AudioSource.clip);
				// move picked sound to index 0 so it's not picked next time
				m_FootstepSounds [n] = m_FootstepSounds [0];
				m_FootstepSounds [0] = m_AudioSource.clip;
			}
		}
		
		
		private void UpdateCameraPosition(float speed)
		{
			Vector3 newCameraPosition;
			if (!m_UseHeadBob)
			{
				return;
			}
			if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
			{
				m_Camera.transform.localPosition =
					m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
					                    (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
			}
			else
			{
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
			}
			m_Camera.transform.localPosition = newCameraPosition;
		}

		private void GetInput(out float speed)
		{
			// Read input
			//float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			//float vertical = CrossPlatformInputManager.GetAxis("Vertical");
			
			bool waswalking = m_IsWalking;
			
			#if !MOBILE_INPUT
			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
			m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
			#endif
			// set the desired speed to be walking or running
			speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			m_Input = new Vector2(horizontal, vertical);
			
			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1)
			{
				m_Input.Normalize();
			}
			
			// handle speed change to give an fov kick
			// only if the player is going to a run, is running and the fovkick is to be used
			if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
			{
				StopAllCoroutines();
				StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
			}
		}
		
		
		private void RotateView()
		{
			//m_MouseLook.LookRotation (transform, m_Camera.transform);
			if (m_Camera != null && m_MouseLook != null) {
				m_MouseLook.LookRotation (transform, m_Camera.transform);
			} else {
				if (IsBot) {
					Quaternion m_CharacterTargetRot = Quaternion.identity;
					float yRot = transform.localRotation.y;
					
					m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
					transform.localRotation = m_CharacterTargetRot;
				} else {
					if (m_MouseLook == null)
						Debug.LogError ("m_MouseLook Not assigned in CustomController.");
					if (m_Camera == null)
						Debug.LogError ("m_Camera Not assigned in CustomController.");
				}
				//m_MouseLook.LookRotation (transform,gameObject.transform);
			}
		}
		
		
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below)
			{
				return;
			}
			
			if (body == null || body.isKinematic)
			{
				return;
			}
			body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
		}
	}
}*/