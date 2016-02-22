using UnityEngine;
using System.Collections;

/*
	Main script for handling a 3d canvas
		- dialog speech bubble
		- characters name/title etc

	Functions:
		Orbits character - at specific position or orbit aroud
		Toggles canvas
		Lerps the movement to follow the position
		Auto Toggle if distance too great
*/
namespace GUI3D {
	public class Billboard : MonoBehaviour {
		public GameObject TargetCharacter;
		public bool IsLookAtPlayer = true;				// will face the camera.main
		public bool IsLockRotationX = false;
		//public bool IsFixedAtCameraAngle = false;		// changes Z rotation to look at camera

		// Update is called once per frame
		void Update () {
			HandleInput ();
			UpdateBillboard ();
		}

		private void HandleInput() {
		}
		private void UpdateBillboard() {
			if (IsLookAtPlayer) {
				if (Camera.main != null) {
					transform.LookAt (Camera.main.transform.position);
					transform.eulerAngles = transform.eulerAngles + new Vector3 (180, 0, 180);
					if (IsLockRotationX) transform.eulerAngles = new Vector3(0,transform.eulerAngles.y, transform.eulerAngles.z);
				}
			} else {
				if (TargetCharacter != null) {
					transform.LookAt (TargetCharacter.transform.position);
					transform.eulerAngles = transform.eulerAngles + new Vector3 (180, 0, 180);
				}
			}
		}

	}
	public class GUI3DUtilities {
		
		
		public static bool IsKeyDown(string MyKey) {
			if (MyKey != " ") {
				KeyCode MyKeyCode =  (KeyCode) System.Enum.Parse(typeof(KeyCode), MyKey); 
				if (Input.GetKeyDown(MyKeyCode))
					return true;
			}
			return false;
		}
	}
}
