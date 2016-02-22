using UnityEngine;
using System.Collections;

// A scent will be dropped behind for each character
// an ai will be able to pick up on the scent and follow its trail
// that is the idea, it will walk to the last known scent if it can't see the player in its vision

public class Scent : MonoBehaviour {
	public int LifeTime = 15;
	// Use this for initialization
	void Start () {
		Destroy (gameObject, LifeTime);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
