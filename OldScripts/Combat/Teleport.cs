using UnityEngine;
using System.Collections;

namespace OldCode {
// This will teleport the user
public class Teleport : MonoBehaviour {
	public float MyTeleportDistance = 8;

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.G)) {
			TeleportPlayer(MyTeleportDistance);
		}
	}
	public void TeleportPlayer(float TeleportDistance) {
		RaycastHit hit;
		BaseCharacter MyPlayer = gameObject.GetComponent<BaseCharacter> ();
		//Vector3 MyBounds = new Vector3 (1, 2, 1);
		if (Physics.Raycast (transform.position, MyPlayer.ShootForwardVector, out hit, TeleportDistance)) {
			transform.position = hit.point - MyPlayer.ShootForwardVector*1;
		} else {
			transform.position += MyPlayer.ShootForwardVector*TeleportDistance;
		}
	}
}
}