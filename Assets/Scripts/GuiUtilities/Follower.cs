using UnityEngine;
using System.Collections;

/*	Use it for A gui, to simply follow a bot with lerp
 * 	
*/

namespace GUI3D 
{
	[ExecuteInEditMode]
	public class Follower : MonoBehaviour 
	{
		[Header("Debug")]
		public bool IsDebugMode = false;
		public bool IsReposition = false;
		public KeyCode MyFollowKey;
		
		[Header("Options")]
		[SerializeField] private Transform TargetCharacter;
		[SerializeField] private Vector3 AboveCharacterHeadOffset = new Vector3(0,0.6f,0);
		private Vector3 DirectionOffset;
		private bool IsActive = false;
		public float Speed = 3f;

		// Use this for initialization
		void Start () {
			transform.position = TargetCharacter.transform.position + AboveCharacterHeadOffset;
			if (name == "Dialogue")
				DirectionOffset = new Vector3 (0, 0, -0.05f);
			#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) 
				{
					IsActive = true;
				}
			#else
				IsActive = true;
			#endif
			if (TargetCharacter == null) {
				UpdateTarget(transform.parent);
			}
		}
		public void SetTargetOffset(Vector3 NewOffset) {
			AboveCharacterHeadOffset = NewOffset;
		}
		public void UpdateTarget(Transform NewTarget) 
		{
			TargetCharacter = NewTarget;
		}
		private void Reposition(float DeltaTime) 
		{
			Vector3 TargetPosition = TargetCharacter.transform.position + AboveCharacterHeadOffset + DirectionOffset.z*transform.forward;
			transform.position = Vector3.Lerp (transform.position, TargetPosition, DeltaTime * Speed);
		}
		// Update is called once per frame
		void Update () {
			if (IsReposition) {
				IsReposition = false;
				Reposition(1);
			}
			if (IsActive && TargetCharacter != null) 
			{
				Reposition(Time.deltaTime);
			}
			if (Input.GetKeyDown(MyFollowKey))
			{
				IsActive = !IsActive;
			}
		}
	}
}
