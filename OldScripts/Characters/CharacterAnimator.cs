using UnityEngine;
using System.Collections;

// this is made just to animate characters
// the main BaseCharacter class will set the states
// this just interfaces with the animation controller

	namespace DialogueSystem {
	public enum AnimationState {
		Idle,
		Walking,
		Jumping,
		Falling,
		Swimming,
		Punching,
		Kicking,
		Waving,
		Dead,
		Shooting
	};

	public class CharacterAnimator : MonoBehaviour {
		// :: Animation stuff ::
		public Animator MyAnimation;
		public Animation MyAnimationSingleClip;
		public bool IsAnimationOneClip = true;
		public string DebugAnimationState;
		public Vector3 ColorTint = new Vector3 (1f, 1f, 1f);
		public float ScaleSize = 1.5f;
		public AnimationState MyAnimationState;	// 0 for idle, 1 for walking, 2 for punching
		private Material MyMaterial;
		private SkinnedMeshRenderer MyMaterialGameObject;
		public float IsMovingThreshold = 0.1f;
		public float BaseFrameRate;	// debugging
		
		string[] AnimationNames = {"Idle", "Walking", "Jumping", "Falling", "Swimming", "Punching", "Kicking", "Waving", "Dead", "Shooting"};	// matches up to the animation state enum

		// Use this for initialization
		void Start () {
			if (MyAnimationSingleClip != null) {
				BaseFrameRate = MyAnimationSingleClip.clip.frameRate;
			}
		}
		public void PlayDefaultAnimation() {
			PlayAnimation ("Idle");
		}
		public bool IsMoving() {
			Rigidbody MyRigidBody = (Rigidbody) gameObject.GetComponent ("Rigidbody");
			if (MyRigidBody != null)
				if (!(MyRigidBody.velocity.x > -IsMovingThreshold && MyRigidBody.velocity.x < IsMovingThreshold && 
				     MyRigidBody.velocity.z > -IsMovingThreshold && MyRigidBody.velocity.z < IsMovingThreshold)) {
				return true;
			}
			return false;
		}
		// Update is called once per frame
		void Update () {
			UpdateAnimations ();
		}
		public void PlayAnimation(string AnimationName) {
			Debug.Log ("Playing Animation: " + AnimationName);
			//if (!IsAnimationOneClip) {
				if (MyAnimation != null) {
					Rigidbody MyRigidBody = (Rigidbody) gameObject.GetComponent ("Rigidbody");
					MyAnimation.Play (AnimationName);
					if (MyRigidBody != null)
						MyAnimation.speed = MyRigidBody.velocity.magnitude/100.0f;
					if (AnimationName != "Walking")
						MyAnimation.speed = 1;
					if (MyAnimation.speed > 2) MyAnimation.speed = 1.2f;
					if (MyAnimation.speed < 0.5f) MyAnimation.speed = 0.3f;
				}
				else 
					Debug.LogError (name + "'s MyAnimation is null! :(");
			//} else {
				/*if (MyAnimationSingleClip != null) {
					Debug.Log ("    Animation: " + MyAnimationState);
					if (AnimationName == "Idle") {
						MyAnimationState = AnimationState.Idle;
						MyAnimationSingleClip.wrapMode  = WrapMode.Loop;
						MyAnimationSingleClip.Play ("Idle");
					} else if (AnimationName == "Walking") {
						MyAnimationState = AnimationState.Walking;
						MyAnimationSingleClip.wrapMode  = WrapMode.Loop;
						MyAnimationSingleClip.Play ("Walking");
					} else if (AnimationName == "Punching" || AnimationName == "Shooting") {
						MyAnimationSingleClip.wrapMode  = WrapMode.Once;
						MyAnimationState = AnimationState.Punching;
						MyAnimationSingleClip.Play ("attack_2");
					} else if (AnimationName == "Dead") {
						MyAnimationSingleClip.wrapMode  = WrapMode.Once;
						MyAnimationState = AnimationState.Dead;
						MyAnimationSingleClip.Play ("death");
					}
					Debug.Log ("    Animation: " + MyAnimationState);
				} else {
					Debug.Log (name + "'s AnimationSingleClip is null! :(");
				}*/
			//}
		}
		// called per frame
		public void UpdateAnimations() {
			// debug string
			if (MyAnimation != null) {
				if (MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
					DebugAnimationState = "Idle";
				else if (MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Walking"))
					DebugAnimationState = "Walking";
				else if (MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Punching"))
					DebugAnimationState = "Punching";
				else if (MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Shooting"))
					DebugAnimationState = "Shooting";
			} //else {	// havn't implemented yet lel
			//PlayAnimation("Jump");
			//MyAnimationState = AnimationState.Falling;
			//}
			
			if (MyAnimationState == AnimationState.Punching) {
				/*// if punching animation has finished
				if (!IsAnimationOneClip) {	// if animations all in one file
					if (!MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Punching") && !MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("Shooting")
						&& !MyAnimation.GetCurrentAnimatorStateInfo (0).IsName ("ShootingOnGoing")) {
						PlayAnimation ("Idle");
					}
				} else {	// if multiple fbx files
					if (MyAnimationSingleClip != null) {
						if (!MyAnimationSingleClip.IsPlaying ("attack_2")) {
							PlayAnimation ("Walking");
						}
					}
				}*/
			} else {
				if (IsOnGround()) {
					if (MyAnimationState == AnimationState.Idle && IsMoving ()) {
						PlayAnimation("Walking");
					} else if (MyAnimationState == AnimationState.Walking && !IsMoving ()) {
						PlayAnimation("Idle");
					}
				}
			}
		}
		public bool IsOnGround() {
			return true;
		}
		public void UpdateColor() {
			//MeshRenderer MyRenderer = (MeshRenderer)gameObject.GetComponent ("MeshRenderer");
			//if (MyRenderer)
			if (MyMaterial == null && MyMaterialGameObject != null)
				MyMaterial = MyMaterialGameObject.material;
			
			//if (MyRenderer.material)
			if (MyMaterial != null)
				MyMaterial.color = new Color(ColorTint.x, ColorTint.y, ColorTint.z);
			//MyRenderer.material.color = new Color(MyRenderer.material.color.r*ColorTint.x, MyRenderer.material.color.g*ColorTint.y, MyRenderer.material.color.b*ColorTint.z);
		}
	}
}
