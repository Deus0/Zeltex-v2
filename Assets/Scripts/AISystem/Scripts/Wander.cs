using UnityEngine;
using System.Collections;
/*	Class: Wander
 * 	Purpose: Every x seconds, it will chose a new wander target, based on its current angle, with a random addition to its angle
 *  
 * */
namespace AISystem {
	public class Wander : MonoBehaviour {
		public float WanderCooldown = 3f;
		private float WanderLastTime = 0f;
		private Movement MyMovement;
		public float WanderRange = 3f;
		public float WanderRotateVariation = 1f;
		bool IsWandering = true;
		public bool IsLimitWander = true;
		public Vector3 WanderSize = new Vector3 (4, 4, 4);
		public Vector3 WanderInitialPosition;
		public bool IsRandomJumping = false;

		// Use this for initialization
		void Start () {
			WanderLastTime = Time.time;
			MyMovement = gameObject.GetComponent<Movement> ();
			if (MyMovement == null)
				MyMovement = gameObject.AddComponent<Movement> ();
			WanderInitialPosition = transform.position;
		}
		
		// Update is called once per frame
		void Update () {
			if (IsWandering && Time.time - WanderLastTime >= WanderCooldown) {
				float Randomness = Random.Range(-WanderRotateVariation, WanderRotateVariation);     // Randomly change wander theta

				Vector3 WanderDirection;//n = new Vector3(WanderRange * Mathf.Cos(Randomness), 0f, WanderRange * Mathf.Sin(Randomness));
				WanderDirection =  Mathf.Sin(Randomness)*transform.right +  Mathf.Cos(Randomness)*transform.forward;

				Vector3 WanderTheta = transform.position + WanderDirection*WanderRange;

				WanderTheta.x = Mathf.Clamp(WanderTheta.x, WanderInitialPosition.x-WanderSize.x, WanderInitialPosition.x+WanderSize.x);
				WanderTheta.z = Mathf.Clamp(WanderTheta.z, WanderInitialPosition.z-WanderSize.z, WanderInitialPosition.z+WanderSize.z);
				RaycastHit MyHit;
				if (Physics.Raycast(transform.position, WanderDirection, out MyHit, WanderRange)) {
					Debug.DrawLine(transform.position, MyHit.point, Color.red, WanderCooldown);
					MyMovement.MoveToPosition(MyHit.point-WanderDirection*0.05f);
				} else {
					Debug.DrawLine(transform.position, transform.position+WanderDirection*WanderRange, Color.green, WanderCooldown);
					MyMovement.MoveToPosition(WanderTheta);
				}

				WanderLastTime = Time.time;

				if (IsRandomJumping) {
					float IsJump = Random.Range(1,100);
					if (IsJump > 95)
						MyMovement.MyControllerU.IsJump = true;
				}
			}
		}
		public void ToggleWander(bool NewWander) {
			IsWandering = NewWander;
		}
	}
}
