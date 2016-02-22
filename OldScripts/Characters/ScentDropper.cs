using UnityEngine;
using System.Collections;

// this is what drops the scent
// a simple script~ if you want a character to smell give them this delicious script

public class ScentDropper : MonoBehaviour {
	// For our ai
	public GameObject ScentClass;
	public float LastScentTime = 0f;
	public float ScentCoolDown = 5f;

	// Update is called once per frame
	void Update () {
		// drop a scent
		DropScent();
	}
	public void DropScent() {
		if (ScentCoolDown < Time.time - LastScentTime && ScentClass) {
			LastScentTime = Time.time;
			if (ScentClass != null) { 
				GameObject scent = (GameObject)Instantiate (ScentClass, transform.position, Quaternion.identity);
			} else {
				Debug.Log (name + " has not got a scent class!, so they won't smell!");
			}
		}
	}
}
