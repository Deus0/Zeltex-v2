using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit MyHit;
			if (Physics.Raycast (transform.position, transform.forward, out MyHit)) {
				Door MyDoor = MyHit.collider.gameObject.GetComponent<Door>();
				if (MyDoor) {
					Debug.LogError("ToggleDoor!");
					MyDoor.ToggleDoor();
				}
			}
		}
	}
}
