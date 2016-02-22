using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Characters.FirstPerson;

namespace CharacterSystem {
public class CharacterAnimator : MonoBehaviour {
	// animation controller
	public float OverRideSpeed = 1f;
	private Animator anim;							// キャラにアタッチされるアニメーターへの参照
	private AnimatorStateInfo currentBaseState;			// base layerで使われる、アニメーターの現在の状態の参照
	static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");
	Rigidbody MyRigidBody;
	Vector3 LastPosition;
	// Use this for initialization
	void Start () {
		// animation controlling
		anim = GetComponent<Animator>();
		MyRigidBody = GetComponent<Rigidbody>();
		TimeWhenStill = Time.time;
		TimeWhenRan = Time.time;
	}
	float LastTime = 0f;
	float TimeWhenStill = 0f;
	float TimeWhenRan = 0f;
	public float TimeToBlend = 2f;
	public float MaxSpeed = 1f;
	float BlendBegin = 0f;
	float BlendEnd = 0f;
	float BlendTimeBegin = 0f;
	bool IsBlendingRun = false;
	bool IsBlendingIdle = false;
	void Update ()
	{
		if (Time.time - LastTime >= 0.3f) {
			LastTime = Time.time;
			float MySpeed = 0f;
			float Dicplacement = Vector3.Distance(transform.position, LastPosition);
			LastPosition = transform.position;
			if (Dicplacement >= 0.001f){// || (Input.GetKey(KeyCode.W))) {
				if (!IsBlendingRun) {
					TimeWhenRan = Time.time;
					BlendBegin = MySpeed;
					BlendEnd = MaxSpeed;
					//MySpeed = Mathf.Lerp(0f, MaxSpeed, (Time.time-TimeWhenStill)/TimeToBlend);
					BlendTimeBegin = Time.time;
					IsBlendingRun = true;
				}
			} else {
				if (!IsBlendingIdle) {
					TimeWhenStill = Time.time;
					BlendBegin = MySpeed;
					BlendEnd = 0f;
					BlendTimeBegin = Time.time;
					IsBlendingIdle = true;
				}
			}

			//currentBaseState = anim.GetCurrentAnimatorStateInfo (0);	// 参照用のステート変数にBase Layer (0)の現在のステートを設定する
			//anim.SetBool("Rest", (MySpeed == 0));
			//anim.SetBool("Rest", false);
			//anim.SetBool("Jump", false);
		}
		//Camera.main.gameObject.transform.eulerAngles = new Vector3 (Camera.main.gameObject.transform.eulerAngles.x, Camera.main.gameObject.transform.eulerAngles.y, 0);
	}
	float MySpeed;
	void FixedUpdate() {
		MySpeed = Mathf.Lerp(BlendBegin, BlendEnd, (Time.time-BlendTimeBegin)/TimeToBlend);
		if (OverRideSpeed != 0) {
			MySpeed = OverRideSpeed;
		}
		anim.SetFloat ("Speed", MySpeed);
		//current = Mathf.Lerp (current, 0, delayWeight);
		//anim.SetLayerWeight (1, current);
		anim.speed = MySpeed;

		if (Time.time - BlendTimeBegin >= TimeToBlend) {
			IsBlendingRun = false;
			IsBlendingIdle = false;
		}
		//if (MySpeed < 0.1f)
			anim.speed = 1f;
	}
}
}
