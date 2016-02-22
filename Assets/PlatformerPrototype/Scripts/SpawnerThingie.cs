using UnityEngine;
using System.Collections;

public class SpawnerThingie : MonoBehaviour {
	public GameObject MyThing;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F)) {
			//Instantiate(MyThing, gameObject.transform.position + gameObject.transform.forward*2f, Quaternion.identity);
			bool IsMouse = Screen.lockCursor;
			Debug.LogError("Toggling Cursor to: " + IsMouse);
			Screen.lockCursor = IsMouse;
			Cursor.visible = !IsMouse;
		}
		
		if (Input.GetKeyDown (KeyCode.R)) {
			Application.LoadLevel (Application.loadedLevel);
		}
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit MyHit;
			if (Physics.Raycast(transform.position, transform.GetChild(0).transform.forward, out MyHit, 3f)) {
				MyHit.collider.gameObject.GetComponent<FlattenMesh>().ContinueAnimation(0);
			}
		}
	} 
	void OnControllerColliderExit(ControllerColliderHit hit){
		Debug.LogError("Characters on controller collider OnControllerColliderExit");
	}
	void OnControllerColliderHit(ControllerColliderHit hit){
		//Debug.LogError("Characters on controller collider hit");
		if (hit.transform.gameObject.GetComponent<FlattenMesh> ()) {
			hit.transform.gameObject.GetComponent<FlattenMesh>().TakeHit(hit.collider, -hit.normal);
		}
	}
}
