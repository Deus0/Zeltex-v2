using UnityEngine;
using System.Collections;

namespace GUI3D {
	public class Orbitor : MonoBehaviour 
	{
		[Header("Debug")]
		public bool IsDebugMode = false;
		[Header("Options")]
		public GameObject Target;
		public Vector3 TargetOffset;
		public Vector2 GuiPositionOffset;
		public float Speed = 3f;
		public bool IsInstant = false;
		
		[Header("Player Settings")]
		public bool IsTargetMainCamera = false;
		public KeyCode MyFollowPositionKey;
		public KeyCode MyFollowCameraKey;

		public Vector3 MyDirection;
		public float MyDisplayDistance = 2f;
		float TargetAngleX;
		float TargetAngleZ;
		private bool IsActive = true;
		//public bool IsFollowUserPosition = true;		// follows the character, otherwise it stays still
		public bool IsFollowUserAngle = false;			// rotates to position according to users transform.forward angle
		//public bool IsAboveCharacterHead = false;		// will follow player and appear above their head
		public bool IsFollowUserAngleAddition = false;	// will follow camera rotation on an angle addition
		
		[Header("Spinning")]
		bool IsSpinning = false;
		float TimeCount;
		float LastSpunTime = 0f;
		float TimeDifference = 0f;
		public float SpinSpeed = 0.5f;

		public GameObject TargetCharacter2;		// used to make a gui orbit in the direction of one character from another
		public Vector3 TargetCharacter2Offset;	// used to offset the gui 

		private Vector3 TargetPosition;		// where i want to lerp to
		private Vector3 RelativePosition;	// lerped position

		void OnGUI() {
			if (IsDebugMode) {
				GUILayout.Label ("Direction Normal: " + TargetPosition.normalized.ToString());
				GUILayout.Label ("TargetPosition: " + TargetPosition.ToString());
				GUILayout.Label ("RelativePosition: " + RelativePosition.ToString());
			}
		}

		void Start() 
		{
			if (IsTargetMainCamera)
			{
				Target = Camera.main.gameObject;
			}
			MyDirection.Normalize();
			for (int i = 0; i < transform.childCount; i++) 
			{
				TransformChild(transform.GetChild(i).gameObject);
				//if (transform.GetChild(i).GetComponent<RectTransform>())
				//	transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition3D += GuiPositionOffset;
			}
		}
		public Vector3 GetGuiOffset() {
			if (!this.enabled)
				return new Vector3();
			return new Vector3(GuiPositionOffset.x*Screen.width, GuiPositionOffset.y*Screen.height, 0);
		}
		public void TransformChild(GameObject MyChild) {
			if (this.enabled) {
				if (!MyChild.name.Contains("StatBar"))
				if (MyChild.GetComponent<RectTransform> ())
						MyChild.GetComponent<RectTransform> ().anchoredPosition3D += GetGuiOffset();
			}
		}

		// Update is called once per frame
		void Update () 
		{
			if (Input.GetKeyDown (MyFollowPositionKey)) {
				IsActive = !IsActive;
			}
			if (Input.GetKeyDown (MyFollowCameraKey)) {
				IsFollowUserAngleAddition = !IsFollowUserAngleAddition;
			}
			/*if ((Target == null && IsFollowMainCamera) || (IsFollowMainCamera && Target != Camera.main.gameObject)) {
				Target = Camera.main.gameObject;
			}*/
			if (Target != null) 
			{
				CheckOrbitPosition();
				UpdateOrbit ();
			}
		}

		// orbit position is in direction of the camera/target gameobject - multiplied by distance
		private void CheckOrbitPosition() 
		{
			if (IsActive && Target != null)
			{	// is orbitor
				TargetAngleX = MyDirection.x;
				TargetAngleZ = MyDirection.z;
				OrbitPlayer ();
				// first calculate direction
				Vector3 TemporaryDirection = new Vector3 ((TargetAngleX), 0, (TargetAngleZ)).normalized;
				//TemporaryDirection = TargetCharacter.transform.forward*TargetAngleZ + TargetCharacter.transform.right*TargetAngleX;
				//	Debug.LogError("Direction: " + TemporaryDirection.ToString());
				if (IsFollowUserAngle) 
				{
					TemporaryDirection = Target.transform.forward;
					//MyDirection = TargetCharacter.transform.forward;
				}
				else if (IsFollowUserAngleAddition) 
				{
					TemporaryDirection = Target.transform.forward*TargetAngleZ + Target.transform.right*TargetAngleX;
				}
				else if (TargetCharacter2) 
				{
					Vector3 DirectionOf = (TargetCharacter2.transform.position
					                       -(Target.transform.position+TargetOffset)).normalized;
					TemporaryDirection = DirectionOf;//+new Vector3(TargetCharacter2Offset.x*DirectionOf.x,TargetCharacter2Offset.y*DirectionOf.y,TargetCharacter2Offset.z*DirectionOf.z) ;
				}
				
				// then change the direction into a position value
				TargetPosition = TemporaryDirection * MyDisplayDistance;
				
				if (TargetCharacter2) 
				{
					TargetPosition += 	-Target.transform.right*TargetCharacter2Offset.x
						+Target.transform.up*TargetCharacter2Offset.y
							+Target.transform.forward*TargetCharacter2Offset.z;
				}
				//TargetPosition += Target.transform.position+TargetOffset;
			}
		}

		// updates the relative position value of the orbital path
		private void UpdateOrbit() 
		{
			float TimeValue = Time.deltaTime * Speed / (MyDisplayDistance + 1);
			if (IsInstant) 
			{
				RelativePosition =  Vector3.Lerp (RelativePosition, TargetPosition, TimeValue);
				transform.position = Target.transform.position + RelativePosition;
			}
			else 
			{
				transform.position = Vector3.Lerp (transform.position, Target.transform.position+TargetPosition, TimeValue);
			}
		}

		// rotates around player using x and z angles
		private void OrbitPlayer() {
			if (IsSpinning) {
				TimeCount += Time.deltaTime;
				float SpinAngle = (TimeCount -TimeDifference)*SpinSpeed;
				if (!IsSpinning) {
					SpinAngle = LastSpunTime;
				} 
				else {
					LastSpunTime = SpinAngle;
				}
				TargetAngleX += SpinAngle;
				TargetAngleZ += SpinAngle;
			}
		}

	}
}