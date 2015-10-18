using UnityEngine;
using System.Collections;

namespace GUI3D {
	public class Orbitor : MonoBehaviour {
		public GameObject TargetCharacter;
		public string MyFollowPositionKey = " ";
		public string MyFollowCameraKey = " ";
		public Vector3 MyDirection;
		public float MyDisplayDistance = 2f;
		float TargetAngleX;
		float TargetAngleZ;
		private bool IsActive = true;
		//public bool IsFollowUserPosition = true;		// follows the character, otherwise it stays still
		public bool IsFollowUserAngle = false;			// rotates to position according to users transform.forward angle
		//public bool IsAboveCharacterHead = false;		// will follow player and appear above their head
		public bool IsFollowUserAngleAddition = false;	// will follow camera rotation on an angle addition
		bool IsSpinning = false;
		float TimeCount;
		float LastSpunTime = 0f;
		float TimeDifference = 0f;
		public float Speed = 3f;
		public float SpinSpeed = 0.5f;
		
		void Awake() {
			MyDirection.Normalize();
		}
		
		// Update is called once per frame
		void Update () {
			UpdateOrbit ();
			if (GUI3DUtilities.IsKeyDown (MyFollowPositionKey)) {
				IsActive = !IsActive;
			}
			if (GUI3DUtilities.IsKeyDown (MyFollowCameraKey)) {
				IsFollowUserAngleAddition = !IsFollowUserAngleAddition;
			}
		}

		
		private void UpdateOrbit() {
			if (IsActive && TargetCharacter != null) {	// is orbitor
				TargetAngleX = MyDirection.x;
				TargetAngleZ = MyDirection.z;
				OrbitPlayer ();
				Vector3 TemporaryDirection = new Vector3 ((TargetAngleX), 0, (TargetAngleZ)).normalized;
				//TemporaryDirection = TargetCharacter.transform.forward*TargetAngleZ + TargetCharacter.transform.right*TargetAngleX;
				//	Debug.LogError("Direction: " + TemporaryDirection.ToString());
				if (IsFollowUserAngle) 
				{
					TemporaryDirection = TargetCharacter.transform.forward;
					//MyDirection = TargetCharacter.transform.forward;
				}
				if (IsFollowUserAngleAddition) {
					TemporaryDirection = TargetCharacter.transform.forward*TargetAngleZ + TargetCharacter.transform.right*TargetAngleX;
				}
				Vector3 TargetPosition = TemporaryDirection * MyDisplayDistance;
				TargetPosition += TargetCharacter.transform.position;
				transform.position = Vector3.Lerp (transform.position, TargetPosition, Time.deltaTime * Speed / (MyDisplayDistance+1));
			}
		}
		// rotates around player using x and z angles
		private void OrbitPlayer() {
			if (IsSpinning) {
				TimeCount += Time.deltaTime;
				float SpinAngle = (TimeCount -TimeDifference)*SpinSpeed;
				if (!IsSpinning) {
					SpinAngle = LastSpunTime;
				} 
				else {
					LastSpunTime = SpinAngle;
				}
				TargetAngleX += SpinAngle;
				TargetAngleZ += SpinAngle;
			}
		}

	}
}