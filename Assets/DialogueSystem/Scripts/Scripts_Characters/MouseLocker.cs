using UnityEngine;
using System.Collections;

public class MouseLocker : MonoBehaviour {
	public string ToggleMouseKey = "";
	bool IsMouseEnabled = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (ToggleMouseKey != "") {
			KeyCode MyKey =  (KeyCode) System.Enum.Parse(typeof(KeyCode), ToggleMouseKey); 
			if (Input.GetKeyDown(MyKey)) {
				IsMouseEnabled = !IsMouseEnabled;
			}
		}
		if (IsMouseEnabled) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		} else {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
