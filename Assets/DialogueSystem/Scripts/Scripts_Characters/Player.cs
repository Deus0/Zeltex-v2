using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

/*
		Player class
			Any extra things on top of the Characters functionality
			Keyboard input
*/

namespace DialogueSystem {
	public class Player : MonoBehaviour {
		public string ToggleMouseKey = "";
		public string InteractKey = "";
		public bool IsMouseClickInteract = true;
		bool IsMouseEnabled = true;
		public bool CanJump = false;
		public bool CanRun = false;


		// Use this for initialization
		void Awake () {
			if (!CanJump) {
				gameObject.GetComponent<RigidController>().movementSettings.JumpForce = 0;
			}
			if (!CanRun) {
				gameObject.GetComponent<RigidController>().movementSettings.RunMultiplier = 1;
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (IsMouseEnabled) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			} else {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			HandleInput ();
		}
		private void HandleInput() {
			if (ToggleMouseKey != "") {
				KeyCode MyKey =  (KeyCode) System.Enum.Parse(typeof(KeyCode), ToggleMouseKey); 
				if (Input.GetKeyDown(MyKey)) {
					ToggleMouse();
				}
			}
			if (IsMouseClickInteract && Input.GetMouseButtonDown(0)) {
				gameObject.GetComponent<Character>().RayTrace();
			}
			if (InteractKey != "") {
				KeyCode MyKey =  (KeyCode) System.Enum.Parse(typeof(KeyCode), InteractKey); 
				if (Input.GetKeyDown(MyKey)) {
					//Debug.LogError("Pressing Interact Key");
					gameObject.GetComponent<Character>().RayTrace();
				}
			}
		}
		private void ToggleMouse() {
			IsMouseEnabled = !IsMouseEnabled;
			RigidController MyController = gameObject.GetComponent<RigidController> ();
			if (!IsMouseEnabled) {
				MyController.mouseLook.XSensitivity = 0;
				MyController.mouseLook.YSensitivity = 0;
			} else {
				MyController.mouseLook.XSensitivity = 2;
				MyController.mouseLook.YSensitivity = 2;
			}
		}

		/*public bool IsDebugGui = false;
		void OnGUI () {
			if (IsDebugGui) {
				string FileName = "Lotus";
				string CharacterName = FileName;
				if (!FileName.Contains (".txt"))
					FileName += ".txt";
				string TemporaryFileName = Application.dataPath;
			
				// i can see this going bad
				if (TemporaryFileName.Contains ("Assets")) {
					TemporaryFileName = TemporaryFileName.Remove (TemporaryFileName.IndexOf ("Assets"), "Assets".Length);
				} else {
				}
				if (gameObject.GetComponent<Character> ().MySpeechBubble.MyCharacter2) {
					GUI.Label (new Rect (10, 10, 350, 55), "File: " + gameObject.GetComponent<Character> ().MySpeechBubble.MyCharacter2.MySpeechBubble.MyFile.ToString ());
					GUI.Label (new Rect (10, 110, 150, 55), "Loaded: " + gameObject.GetComponent<Character> ().MySpeechBubble.MyCharacter2.MySpeechBubble.HasLoaded.ToString ());

				} else {
					GUI.Label (new Rect (10, 10, 300, 100), TemporaryFileName);
				}
				if (TemporaryFileName [TemporaryFileName.Length - 1] != '/')
					TemporaryFileName += '/';
				TemporaryFileName = TemporaryFileName + "Resources/" + FileName;
				// Make a background box
			}
		}*/
	}
}
