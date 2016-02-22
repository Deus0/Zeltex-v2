using UnityEngine;
using System.Collections;

namespace GUI3D {
	public class Toggler : MonoBehaviour {
		[Tooltip("The active state when the game begins")]
		public bool BeginToggle = false;
		[Tooltip("The target of the auto toggling")]
		public GameObject TargetCharacter;
		[Tooltip("Set to true if the target is the main camera")]
		public bool IsTargetMainCamera;
		[Tooltip("Auto toggles depending on distance from target")]
		public bool IsAutoToggle = false;
		[Tooltip("The distance it auto toggles at")]
		public float DisableDistance = 20;
		[Tooltip("Key to toggle gui on and off")]
		public KeyCode MyToggleKey;
		private bool OverrideActive = true;	// Controls our On state outside of the auto toggling
		private bool LastSwitched = true;
		PhotonView MyPhoton;
		void Start() {
			if (MyPhoton == null) {
				MyPhoton = gameObject.GetComponent<PhotonView> ();
				if (MyPhoton == null) {
					MyPhoton = gameObject.AddComponent<PhotonView> ();
				}
			}
		}
		void Awake() 
		{
			SetActive (BeginToggle);
		}
		// Update is called once per frame
		void Update () {
			UpdateMainCamera ();
			UpdateDistance ();

			if (Input.GetKeyDown(MyToggleKey))
			{
				MyPhoton.RPC("KeyToggle",  PhotonTargets.All);
			}
		}
		[PunRPC]
		private void KeyToggle() {
			IsAutoToggle = !IsAutoToggle;
			Toggle ();
		}
		public bool GetActive() {
			return LastSwitched;
		}
		
		// if we change our main camera in our scene, we will need to update it.. lol
		public void UpdateMainCamera() 
		{
			if (IsTargetMainCamera && Camera.main) {
				if (TargetCharacter == null || TargetCharacter != Camera.main.gameObject) {
					TargetCharacter = Camera.main.gameObject;
				}
			}
		}

		// saves the distance, and turns it on or off depending on distance
		private void UpdateDistance() 
		{
			if (IsAutoToggle && OverrideActive && TargetCharacter) 
			{
				// Find the current distance to the toggle gameobject
				float DistanceToCam = Vector3.Distance (transform.position, TargetCharacter.transform.position);

				if (DistanceToCam > DisableDistance){
					if (false != LastSwitched) 
						SetActive (false);
				}
				else {
					if (true != LastSwitched) 
						SetActive (true);
				}
			}
		}

		public void SetActive(bool IsEnabled) 
		{
			{
				LastSwitched = IsEnabled;
				for (int i = 0; i < transform.childCount; i++) {
					if (!transform.GetChild (i).name.Contains ("Tooltip"))
						transform.GetChild (i).gameObject.SetActive (IsEnabled);
				}
			}
		}

		public void TurnOff() {
			SetActive (false);
			OverrideActive = false;
		}
		
		public void TurnOn() 
		{
			//Debug.LogError ("Turning " + name + " On!");
			SetActive (true);
			OverrideActive = true;
		}
		
		public void Toggle() 
		{
			SetActive (!LastSwitched);
		}
	}
}
