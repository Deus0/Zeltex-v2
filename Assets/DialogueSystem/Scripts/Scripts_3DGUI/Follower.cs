using UnityEngine;
using System.Collections;

/*	Use it for A gui, to simply follow a bot with lerp
 * 	
*/

namespace GUI3D {
	public class Follower : MonoBehaviour {
		public GameObject TargetCharacter;
		public Vector3 AboveCharacterHeadOffset = new Vector3(0,1.6f,0);
		public string MyFollowKey = " ";
		private bool IsActive = true;
		public float Speed = 3f;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			if (IsActive && TargetCharacter != null) {
				transform.position = Vector3.Lerp (transform.position, TargetCharacter.transform.position + AboveCharacterHeadOffset, Time.deltaTime * Speed);
			}
			if (GUI3DUtilities.IsKeyDown (MyFollowKey)) {
				IsActive = !IsActive;
			}
		}
	}
}
