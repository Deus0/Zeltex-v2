using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnimationUtilities {
	public class AnimateLineOnGui : AnimateLine {
		RectTransform MyRect;

		// Use this for initialization
		void Start () {
			MyRect = transform.GetComponent<RectTransform> ();
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
		}
		// Update is called once per frame
		void Update () {
			Vector3[] MyCorners = new Vector3[4];
			MyRect.GetWorldCorners (MyCorners);
			MyTargetPositions.Clear ();
			MyFromPositions.Clear ();
			for (int i = 0; i < MyLines.Count; i++) {
				MyFromPositions.Add(MyCorners[i]);
				MyTargetPositions.Add(Target.transform.position);
			}
			UpdateLinePositionsTotal ();
		}
	}
}