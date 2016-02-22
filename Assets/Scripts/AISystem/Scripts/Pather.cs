using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*	Holds a path - bunch of points
 *  
*/

namespace AISystem {
	public class Pather : MonoBehaviour {
		public List<GameObject> PathPoints;
		public int PathIndex = 0;
		private Movement MyMovement;

		// Use this for initialization
		void Awake () {
			MyMovement = gameObject.GetComponent<Movement> ();
			MyMovement.OnReachTarget.AddListener (OnReachTarget);	
			if (PathPoints.Count > 0) {
				MyMovement.MoveToPosition (PathPoints [PathIndex].transform.position);
			}
		}

		public void OnReachTarget() {
			if (PathPoints.Count > 0) {
				Debug.Log(PathIndex + " - Has reached new position: " + Time.time);
				PathIndex++;
				if (PathIndex >= PathPoints.Count) 
					PathIndex = 0;
				MyMovement.MoveToPosition (PathPoints [PathIndex].transform.position);
			}
		}
	}
}
