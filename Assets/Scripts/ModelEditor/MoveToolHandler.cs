using UnityEngine;
using System.Collections;

public class MoveToolHandler : MonoBehaviour {
	public bool IsLocalRotation;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!IsLocalRotation) {
			transform.eulerAngles = new Vector3 (0, 0, 0);
		} else {
			transform.eulerAngles = transform.parent.transform.eulerAngles;
		}
	}

	//public void On
}
