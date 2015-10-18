using UnityEngine;
using System.Collections;

public class RotateAtCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt (Camera.main.transform.position);
		transform.eulerAngles = transform.eulerAngles + new Vector3 (180, 0, 180);
	}
}
