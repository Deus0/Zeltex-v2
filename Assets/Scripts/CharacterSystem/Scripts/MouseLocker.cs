using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

namespace CharacterSystem {
	public class MouseLocker : MonoBehaviour 
	{
		private Transform MyPlayer;
		private MyController MyPlayerController;
		private Transform MyCrosshair3D;
		public KeyCode ToggleMouseKey = KeyCode.C;
		[SerializeField] private bool IsFPSEnabled = true;
		private float MovementSpeed;
		private float StrafeSpeed;
		private float BackwardSpeed;
		public bool IsCursorVisible = false;
		public Texture2D MyCursorTexture;

		void OnGUI()
		{
			//GUILayout.Label("Is Controller?: " + (MyPlayerController==null));
		}

		void OnDisable() 
		{
			//Debug.LogError ("Disabling MouseLocker");
			SetMouse (false);
		}

		public void SetController(Transform MyCharacter)
		{
			MyPlayer = MyCharacter;
			if (MyCharacter == null) {
				MyPlayerController = null;
				MyCrosshair3D = null;
				return;
			}
			if (MyCharacter.GetComponent<MyController> ()) {
				MyPlayerController = MyCharacter.GetComponent<MyController> ();
			}
			
			if (MyPlayerController) {
				MovementSpeed = MyPlayerController.movementSettings.ForwardSpeed;
				StrafeSpeed = MyPlayerController.movementSettings.StrafeSpeed;
				BackwardSpeed = MyPlayerController.movementSettings.BackwardSpeed;
			}
			SetMouse (true);
			MyCrosshair3D = MyCharacter.FindChild ("Crosshair");
		}

		// Use this for initialization
		void Start () {
			#if UNITY_EDITOR
			if (MyCursorTexture != null)
				Cursor.SetCursor (MyCursorTexture, new Vector2 (32,32), CursorMode.Auto);
			#else
			if (MyCursorTexture != null)
				Cursor.SetCursor (MyCursorTexture, new Vector2 (32,32), CursorMode.Auto);
			#endif
		}
		public void ToggleMouse() {
			SetMouse (!IsFPSEnabled);
		}

		public void SetMouse(bool NewMouse) 
		{
			IsFPSEnabled = NewMouse;
			//Debug.LogError ("Toggling mouse in mouse locker!");
			if (MyPlayer) 
			{
				if (MyPlayer.GetComponent<CharacterSystem.Player> ()) {
					//Debug.Log ("Toggling Player!");
					MyPlayer.GetComponent<CharacterSystem.Player> ().enabled = NewMouse;
				} else {
					Debug.LogError ("Player Null in mouse locker: " + name);
				}
			}
			if (Camera.main && Camera.main.GetComponent<NewMovement.MouseLook> ())
				Camera.main.GetComponent<NewMovement.MouseLook> ().enabled = IsFPSEnabled;

			if (MyCrosshair3D)
			{
				if (IsFPSEnabled)
					MyCrosshair3D.GetComponent<GUI3D.Toggler> ().TurnOn ();
				else
					MyCrosshair3D.GetComponent<GUI3D.Toggler> ().TurnOff ();
			}
			if (IsFPSEnabled) 
			{
				if (MyPlayerController) 
				{
					MyPlayerController.mouseLook.XSensitivity = 2;
					MyPlayerController.mouseLook.YSensitivity = 2;
					MyPlayerController.movementSettings.ForwardSpeed = MovementSpeed;
					MyPlayerController.movementSettings.BackwardSpeed = BackwardSpeed;
					MyPlayerController.movementSettings.StrafeSpeed = StrafeSpeed;
				}
			} 
			else 
			{
				if (MyPlayerController) 
				{
					MyPlayerController.mouseLook.XSensitivity = 0;
					MyPlayerController.mouseLook.YSensitivity = 0;
					MyPlayerController.movementSettings.ForwardSpeed = 0;
					MyPlayerController.movementSettings.BackwardSpeed = 0;
					MyPlayerController.movementSettings.StrafeSpeed = 0;
				}
			}
			MouseState ();
		}
		void MouseState() {
			if (IsFPSEnabled) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = IsCursorVisible;
			} else {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
		// Update is called once per frame
		void Update () {
			if (Input.GetKeyDown(ToggleMouseKey))
			{
				ToggleMouse();
			}
			MouseState ();
		}
	}
}
