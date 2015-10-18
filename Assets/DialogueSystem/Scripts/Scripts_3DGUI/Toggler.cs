using UnityEngine;
using System.Collections;

namespace GUI3D {
	public class Toggler : MonoBehaviour {
		public bool IsTargetMainCamera;
		public GameObject TargetCharacter;
		public bool IsAutoToggle = false;		// auto toggles depending on distance from target
		public float DisableDistance = 20;		// The distance it auto toggles at
		public string MyToggleKey = " ";		// key to toggle gui on and off
		public bool bCanBePaused = true;		// !
		private GameObject MyMainGameObject;	// Uses a sub object called Main for toggling
		
		void Awake() {
			UpdateMainCamera ();
			MyMainGameObject = transform.Find ("Main").gameObject;
			IsAutoToggle = MyMainGameObject.activeSelf;
		}
		
		// Update is called once per frame
		void Update () {
			UpdateDistance ();
			if (GUI3DUtilities.IsKeyDown(MyToggleKey)) {
				IsAutoToggle = !IsAutoToggle;
				Toggle ();
			}
		}
		
		// saves the distance, and turns it on or off depending on distance
		private void UpdateDistance() 
		{
			if (IsAutoToggle) 
			{
				// Find the current distance to the toggle gameobject
				float  DistanceToCam = Vector3.Distance (transform.position, TargetCharacter.transform.position);

				if (DistanceToCam > DisableDistance)
					Toggle (false);
				else
					Toggle (true);
			}
		}
		
		public void Toggle(bool IsEnabled) {
			MyMainGameObject.SetActive (IsEnabled);
		}
		public void Toggle() {
			Toggle (!MyMainGameObject.activeSelf);
		}

		// if we change our main camera in our scene, we will need to update it.. lol
		public void UpdateMainCamera() {
			if (IsTargetMainCamera) {
				TargetCharacter = Camera.main.gameObject;
			}
		}
		/*void OnTriggerEnter(Collider other) {
			if (IsOn)
				Toggle (false);
		}
		
		void OnTriggerExit(Collider other) {
			if (IsOn)
				Toggle (true);
		}*/
	}
}
