using UnityEngine;
using System.Collections;

public class AttachToObject : MonoBehaviour {
	public GameObject AttachToGameObject;
	// Use this for initialization
	void Start () {
		transform.parent.DetachChildren ();
	}
	
	// Update is called once per frame
	void Update () {
		if (AttachToGameObject != null) {
			transform.position = AttachToGameObject.transform.position;
			transform.rotation = AttachToGameObject.transform.rotation;
		}
	}
}
