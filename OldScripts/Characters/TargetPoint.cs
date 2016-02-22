using UnityEngine;
using System.Collections;

// each target point has an index
// this is used for what you want to set up patrol paths in a level editor

public class TargetPoint : MonoBehaviour {
	public int TargetPointIndex = 0;
	public bool IsPlayerSpecific = false;
	public int PlayerIndex = 0;			// if I want only a specific character to have these target points

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
