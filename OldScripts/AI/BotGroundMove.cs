using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Characters.FirstPerson;

namespace OldCode {
public class BotGroundMove : MonoBehaviour {
	public Vector3 MyTargetPoint;
	public float MovementSpeed = 1f;
	float LastPositionedSomewhere = 0f;
	public float WanderCoolDown = 4f;
	public float WanderRange = 1f;
	public Behaviour MyBehaviour = Behaviour.Wander;
	public float DistanceToTarget = 2f;
	public Vector3 GravityForce = new Vector3(0,-2,0);
	public bool IsDebugJump = false;
	bool bIsMoving = false;
	float OriginalSpeed;

	// Use this for initialization
	void Start () {
		LastPositionedSomewhere = Time.time;
		Animator MyAnimator = gameObject.transform.GetChild(0).GetComponent<Animator>();
		OriginalSpeed = MyAnimator.speed;
	}
	public void MoveToPosition(Vector3 NewPosition) {
		MyBehaviour = Behaviour.Patrol;
		MyTargetPoint = NewPosition;
	}
	void Update () {
		if (!gameObject.GetComponent<BaseCharacter> ().MyStats.IsDead) {			
			if (MyBehaviour == Behaviour.Wander) {
				if (Time.time - LastPositionedSomewhere > WanderCoolDown) {
					MyTargetPoint = transform.position + transform.forward * Random.Range (-WanderRange, WanderRange) + transform.right * Random.Range (-WanderRange, WanderRange);
					LastPositionedSomewhere = Time.time;
				}
				RandomlyJump ();
			} else if (MyBehaviour == Behaviour.Patrol) {
				MoveToTarget(MyTargetPoint);
				//Debug.DrawLine (transform.position + new Vector3 (0, -0.5f, 0), transform.position + new Vector3 (0, -1f, 0) + transform.forward * 0.5f, Color.black);
			}
		} else {

		}
	}
	
	public void MoveToTarget(Vector3 TargetPosition) {
		MoveToTarget (TargetPosition, TargetPosition, false);
	}

	public void MoveToTarget(Vector3 TargetPosition, bool IsAttack) {
		MoveToTarget (TargetPosition, TargetPosition, IsAttack);
	}

	public void MoveToTarget(Vector3 TargetPosition, Vector3 TargetLookAtPosition, bool IsAttack) {
		MyTargetPoint = TargetPosition;
		float DistanceToTarget2 = Vector3.Distance (transform.position, TargetPosition);
		if (DistanceToTarget2 > 0.1f || IsAttack) {
			transform.LookAt (TargetLookAtPosition);
		}
		Quaternion m_CharacterTargetRot = Quaternion.identity;
		m_CharacterTargetRot *= Quaternion.Euler (0f, transform.eulerAngles.y, 0f);
		transform.localRotation = m_CharacterTargetRot;
		if (DistanceToTarget2 > DistanceToTarget + 0.1f) {
			//Debug.LogError ("Looking at: " + MyTargetPoint.ToString());
			//transform.LookAt (MyTargetPoint);
			TargetPosition.y = transform.position.y;
			MoveTowardsTarget (TargetPosition);
			//Debug.DrawLine(transform.position, MyTargetPoint, Color.blue);
		}
		CheckForJumps();
		ChangeAnimationState ();
	}

	public void CheckForJumps() {
		//if (gameObject.GetComponent<CharacterController> ().isGrounded &&
		//if (!gameObject.GetComponent<CustomController>().m_Jump) 
		{
			//Debug.LogError ("Checking for jumps in: " + gameObject.name);
			Jump (false);
			RaycastHit NewHit;
			float SizeY = (gameObject.GetComponent<CharacterController> ().height) * gameObject.transform.localScale.y;
			float JumpCheckDistance = 0.75f;
			//Debug.LogError ("Checking jump with bounds: " + SizeY);
			for (float i = 0; i < SizeY/2f; i += SizeY/8f) {
				if (Physics.Raycast (transform.position + new Vector3 (0, -i, 0), transform.forward, out NewHit, JumpCheckDistance)) {
					if (IsDebugJump) 
						Debug.DrawLine (transform.position + new Vector3 (0, -i, 0), NewHit.point, Color.red);
					if (NewHit.collider.gameObject.GetComponent<Chunk> () != null) {
						float DistanceToTarget = Vector3.Distance (transform.position, MyTargetPoint);
						float HeightToTarget = Vector3.Distance (new Vector3 (0, transform.position.y, 0), new Vector3 (0, MyTargetPoint.y, 0));
						if (Vector3.Distance (transform.position, NewHit.point) < DistanceToTarget && HeightToTarget <= 2) {
							Jump (true);
						}
						break;
					}
				} else {
					if (IsDebugJump) 
						Debug.DrawLine (transform.position + new Vector3 (0, -i, 0), transform.position + new Vector3 (0, -i, 0) + transform.forward * JumpCheckDistance, Color.cyan);
				}
			}
		}
	}

	public void RandomlyJump() {
		int IsJump = Random.Range (1, 100);
		if (IsJump > 99) {
			Jump (true);
		} else {
			Jump (false);
		}
	}

	public void Jump(bool IsJump) {
		/*var MyCustomController = GetComponent<CustomController> ();
		MyCustomController.m_Jump = IsJump;
		gameObject.GetComponent<CustomController> ().ForceJump = IsJump;
		gameObject.GetComponent<CustomController> ().m_JumpSpeed = gameObject.GetComponent<BotMovement> ().JumpForce;*/
	}

	public bool IsMoving () {
		Vector3 MySpeed = GetSpeed ();
		float flarb = 0.05f;
		if (MySpeed.x >= -flarb && MySpeed.x <= flarb && MySpeed.z >= -flarb && MySpeed.z <= flarb && MySpeed.y >= -flarb && MySpeed.y <= flarb)
			return false;
		else
			return true;
	}

	public Vector3 GetSpeed() {
		CharacterController MyController = GetComponent<CharacterController>();
		//Debug.LogError ("Speed: " + MyController.velocity.ToString ());
		return MyController.velocity;
	}

	void MoveTowardsTarget(Vector3 target) {
		CharacterController MyController = GetComponent<CharacterController>();
		var offset = target - transform.position;
		{
			offset = offset.normalized * MovementSpeed+GravityForce;
			//normalize it and account for movement speed.
			MyController.Move(offset * Time.deltaTime);
		}
	}

	public void ChangeAnimationState() {
		Animator MyAnimator = gameObject.transform.GetChild(0).GetComponent<Animator>();
		AnimatorStateInfo MyAnimationState = MyAnimator.GetCurrentAnimatorStateInfo(0);
		//Debug.LogError(MyAnimationState.IsName("IsWalking").ToString());
		if (MyAnimationState.IsName("Walking")) 
		{
			//Avatar
			//Debug.LogError("Yes, bot be walking. " + OriginalSpeed);
			if (IsMoving() && !bIsMoving) {
				MyAnimator.speed = 2f;
				bIsMoving = true;
			}
			else {
				MyAnimator.speed = 0.1f;
				bIsMoving = false;
			}
		}
	}
}
}