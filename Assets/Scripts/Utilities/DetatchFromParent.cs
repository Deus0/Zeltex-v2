using UnityEngine;
using System.Collections;

// used to make fog follow camera
// should probaly lerp to the player later on - using the a list of previous positions - saved every 1 second
// this will make it seem like the fog is moving by itself
public class DetatchFromParent : MonoBehaviour {
	GameObject FollowObject;

	// Use this for initialization
	void Start () {
	
	}
	void Awake() {
		FollowObject = transform.parent.gameObject;
		transform.parent = gameObject.transform;
	}
	// Update is called once per frame
	void Update () {
		transform.position = FollowObject.transform.position;
	}
}
