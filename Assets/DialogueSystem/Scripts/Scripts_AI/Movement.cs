using UnityEngine;
using System.Collections;

namespace AISystem {
	public class Movement : MonoBehaviour {
		public UnityEngine.Events.UnityEvent OnReachTarget = null;
		public Vector3 MyMoveToPosition;
		public Vector3 MyLookTowardsPosition;
		public bool IsMoveTo = false;
		public bool IsLookTowards = false;
		public float Speed = 0.25f;
		public float TurnSpeed = 0.25f;
		float Thresh = 0.5f;
		float MinimumForce = 0.15f;
		public float SlowDownDistance = 10f;	// movement will start slowing down like breaking, which reduces the curving in the pathed movement
		public UnityChanControlScriptWithRgidBody MyControllerU;
		GameObject MyTarget;

		// Use this for initialization
		void Start () {
			MyControllerU = gameObject.GetComponent<UnityChanControlScriptWithRgidBody>();
		}

		public void MoveToPosition(Vector3 NewPosition) {
			IsMoveTo = true;
			IsLookTowards = false;
			MyMoveToPosition = NewPosition;
			MyLookTowardsPosition = NewPosition;
		}

		public void LookAt(Vector3 NewPosition) {
			IsMoveTo = false;
			IsLookTowards = true;
			MyLookTowardsPosition = NewPosition;
			MyTarget = null;
		}
		public void LookAt(GameObject NewTarget) {
			IsMoveTo = false;
			IsLookTowards = true;
			MyTarget = NewTarget;
		}

		void FixedUpdate () {
			Debug.DrawLine(MyMoveToPosition, MyMoveToPosition+new Vector3(0,3,0), Color.red);
			if (MyControllerU) {
				MyControllerU.StopMovement();
				if (IsMoveTo) {
					float DistanceToTarget = Vector3.Distance(transform.position, MyMoveToPosition);
					if (DistanceToTarget > Thresh) {
						Debug.DrawLine(transform.position, new Vector3(MyMoveToPosition.x, transform.position.y, MyMoveToPosition.z), Color.blue);
						LookTowards(MyMoveToPosition);
						MoveForwards(MyMoveToPosition);
					}
					else
					{
						IsMoveTo = false;
						if (OnReachTarget != null) {
							OnReachTarget.Invoke();
						}
					}
				} else if (IsLookTowards) {
					if (MyTarget)
						LookTowards(MyTarget.transform.position);
					else
						LookTowards(MyLookTowardsPosition);
				}
			}
		}

		void MoveForwards(Vector3 Position) {
			Vector3 MyVelocity = gameObject.GetComponent<Rigidbody>().velocity;
			float DistanceToTarget = Vector3.Distance (Position, transform.position);
			float DesiredForce = Speed;
			if (DistanceToTarget < SlowDownDistance)
				DesiredForce *= (DistanceToTarget)/SlowDownDistance;
			if (DesiredForce < MinimumForce)
				DesiredForce = MinimumForce;
			float SteerForce = DesiredForce-MyVelocity.magnitude;
			MyControllerU.MovementZ = SteerForce;
		}

		void LookTowards(Vector3 Position) {
			Vector3 TargetDirection = Position-transform.position;
			Vector3 MyForwardDirection = transform.TransformDirection(Vector3.forward);
			float ForwardDot = Vector3.Dot(MyForwardDirection, TargetDirection);
			Vector3 MyRightDirection = transform.TransformDirection(Vector3.left);
			float RightDot = Vector3.Dot(MyRightDirection,  TargetDirection);
			float TotalDot = Mathf.Abs(ForwardDot)+Mathf.Abs(RightDot);
			if (transform.rotation.eulerAngles.normalized.y < TargetDirection.normalized.y)
				MyControllerU.MovementX = TurnSpeed * (RightDot/TotalDot);
			else if (transform.rotation.eulerAngles.normalized.y > TargetDirection.normalized.y)
				MyControllerU.MovementX = -TurnSpeed * (RightDot/TotalDot);
		}
	}
}
