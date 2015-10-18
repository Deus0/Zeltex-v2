using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Characters.FirstPerson;

// Class:	Camera Manager
// Description:	Handles the cameras, spawning, setting variables
//				Also locks the mouse in the middle of the screen
//				It has 4 different modes, which should be set by the Game Manager
//		*The camera mode should correspond to the player controller class, so the movement of the character corresponds to the camera

// Used to alter the game play
public enum CameraMode {
	FirstPerson,
	ThirdPerson,
	TopDown,
	Side
};

public class CameraManager : MonoBehaviour {
	private bool IsLockedCursor = false;		// keeps the cursor in the middle of screen if true
	public GameObject CameraPrefab;
	// various things
	public bool IsCameraRestrainted = false;
	// Camera manager stuff
	public CameraMode MyCameraMode;
	public Vector3 TopDownPosition;
	//public Vector3 CameraRestraints = new Vector3(150.0f,65.0f,150);
	// hidden states
	// references
	private GameManager MyGameManager;
	private BaseCharacter MyCharacter;		// local player that has a camera

	public GameObject MainMenuCamera;		// by default the scene needs a camera for the canvas to be displayed
	public Camera MyCamera;				// The camera used by LocalPlayer
	public Vector3 TopDownCameraAngle = new Vector3 (45, 45, 0);
	public Vector3 CameraHotSpot = new Vector3 (0, 0.75f, 0.2f);

	// Use this for initialization
	void Start () {
		MyGameManager = GetManager.GetGameManager ();
		ResetCamera ();
	}

	public void CreateMenuCamera() {
		MainMenuCamera = new GameObject ();
		MainMenuCamera.AddComponent<Camera> ();
	}
	// Update is called once per frame
	void Update () {
		Screen.lockCursor = IsLockedCursor;
		if (MyCharacter == null) {
			MyCharacter = GetManager.GetCharacterManager ().GetLocalPlayer ();
			if (MyCharacter)
				ResetCamera ();
		} else {
			//MyCamera.transform.eulerAngles = new Vector3 (45, 45, 0);
			UpdateCamera ();
			if (MyCameraMode == CameraMode.TopDown) {
				if (Input.GetKeyDown (KeyCode.Space)) {
					CentreToCharacter ();
				}
			}
		}
	}
	
	public GameObject SpawnCamera(BaseCharacter LocalPlayer) {
		//BaseCharacter LocalPlayer = GetManager.GetCharacterManager ().GetLocalPlayer ();
		//NetworkView MyNetworkView = LocalPlayer.gameObject.GetComponent <NetworkView>();
	//	bool IsConnected = GetManager.GetNetworkManager ().IsConnected ();
		//if (!IsConnected || (IsConnected && MyNetworkView.isMine)) 
		{
				GameObject MyCameraObject = (GameObject)Instantiate (CameraPrefab, LocalPlayer.gameObject.transform.position, Quaternion.identity);
				Camera MyCamera = MyCameraObject.GetComponent <Camera> ();
				MyCamera.gameObject.transform.parent = LocalPlayer.gameObject.transform;
				MyCamera.gameObject.transform.localPosition = new Vector3 (MyCamera.gameObject.transform.localPosition.x, 
			                                                           MyCamera.gameObject.transform.localPosition.y,
			                                                           MyCamera.gameObject.transform.localPosition.z) + CameraHotSpot;
			return MyCameraObject;
		}
	}
	public void ResetCamera() {
		EnterNewCameraMode (MyCameraMode);
	}

	public void DestroyMainMenuCamera() {
		if (MainMenuCamera != null)
			Destroy (MainMenuCamera);
	}
	public void ToggleTopDown() {
		if (MyCameraMode == CameraMode.TopDown) {
			MyCameraMode = CameraMode.FirstPerson;
			EnterNewCameraMode (MyCameraMode);
		}
		else if (MyCameraMode == CameraMode.FirstPerson) {
			MyCameraMode = CameraMode.TopDown;
			EnterNewCameraMode (MyCameraMode);
		}
	}
	
	public void UpdateCamera() {
		if (MyCamera == null) {
			MyCamera = Camera.main;
			//Debug.LogError ("Updating MyCamera.");
			//Debug.LogError ("No Camera..");
		} else {
			if (MyCameraMode == CameraMode.TopDown) 
				MyCamera.gameObject.transform.eulerAngles = TopDownCameraAngle;
			//Debug.LogError (MyCamera.gameObject.name + " Rotatation:" + MyCamera.transform.eulerAngles.ToString () + " - : " + MyCamera.transform.rotation.ToString ());
		}
	}

	public void EnterNewCameraMode(CameraMode NewCameraMode) {
		MyCameraMode = NewCameraMode;
		if (MyCamera == null) {
			MyCamera = Camera.main;
		}
		if (MyCamera != null && MyCharacter != null) {
			if (MyCameraMode == CameraMode.FirstPerson) {
				// enable first person controls
				//MyCharacter.GetComponent<PlayerMovement>().IsPlayerInput = true;
				MyCharacter.gameObject.GetComponent<PlayerMovement>().EnableFpsControls();
				//MyCharacter.GetComponent<Player>().IsPlayerInput = true;
				// move camera to first person position
				MyCamera.gameObject.transform.localPosition = CameraHotSpot;
				MyCamera.gameObject.transform.Rotate (new Vector3 (0, 0, 0));
				EnableMouseFPS ();
				// reset top down position to player
				TopDownPosition.x = 0;
				TopDownPosition.z = 0;
			} else if (MyCameraMode == CameraMode.ThirdPerson) {
				if (MyCharacter != null) {
					// for our animations, set to visible
					MyCharacter.gameObject.transform.GetChild (1).gameObject.SetActive (true);
					MyCamera.gameObject.transform.localPosition = new Vector3 (0, 2f, -2);
					MyCamera.gameObject.transform.Rotate (new Vector3 (45, 0, 0));
					EnableMouseFPS ();
				}
			} else if (MyCameraMode == CameraMode.TopDown) {
				if (MyCharacter != null) {
					// move camera position
					MyCharacter.gameObject.transform.rotation = Quaternion.identity;
					MyCamera.gameObject.transform.position = new Vector3 (0, 0f, 0);
					MyCamera.gameObject.transform.localPosition = new Vector3 (10f, 10f, 0);
					MyCamera.gameObject.transform.eulerAngles = (TopDownCameraAngle);
					// disable characters movement + restrain mouse movement
					MyCharacter.gameObject.GetComponent<PlayerMovement>().EnableRtsControls();
					EnableMouseRTS ();
				}
			}
		}
	}
	public void CentreToCharacter() {
		MyCharacter.GetComponent<PlayerMovement> ().CentreCamera();
	}
	
	
	public void EnableInGameControls() {
		if (MyCameraMode == CameraMode.FirstPerson)
			EnableMouseFPS ();
		else if (MyCameraMode == CameraMode.TopDown)
			EnableMouseRTS ();
	}
	public void EnableMouseFPS() {
		IsLockedCursor = true;
		Cursor.visible = false;
	}
	
	public void EnableMouseRTS() {
		IsLockedCursor = false;
		Cursor.visible = true;
	}
	
	public void EnableMouseGui() {
		IsLockedCursor = false;
		Cursor.visible = true;
		/*if (MyCharacter != null) {
			Player MyPlayerScript = MyCharacter.gameObject.GetComponent <Player>();
			if (MyPlayerScript != null) {
				MyPlayerScript.PauseGame();
			}
		}*/
	}
}
