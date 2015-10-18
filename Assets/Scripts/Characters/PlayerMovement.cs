using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
//using UnityStandardAssets.Characters.FirstPerson;
//using UnityStandardAssets.ImageEffects;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Need to remove the RTS movement stuff from this and attach it to the camera or something lol..


// Modes:
//		Gui mode - where movement keys don't work
//			Fps mdoe - where walking etc works
//			Rts mode - where the keys just move the camera around
//			Free Roamm Mode - Like fps, but disables collisions and gravity

// put all input into this class - from player, gui or whatever make sure all the controls are here

// interfaces with any input device
// converts to states to be used by the other players classes


public class PlayerMovement : MonoBehaviour {
	public bool IsDebug = false;
	public bool IsPlayerMovementInput;
	//public bool IsRtsConrols = true;
	public bool IsFlyMode = false;
	//public bool IsMoveForward = true;
	
	public GameObject MyCamera;
	private Vector3 CameraPosition = new Vector3();

	// Camera Restraining
	public bool IsCameraRestrained = true;
	public Vector3 LimiterMinimum = new Vector3(-76,10,-76);
	public Vector3 LimiterMaximum = new Vector3(76,30,76);
	public float MovementSpeed = 1f;
	public float TouchMovementSpeed = 2f;

	public bool IsPerspective;
	public float BorderLength = 15f;
	float ZoomDistance;
	float CurrentDistance;
	// RTS Camera Movement
	public float MovementSpeedMultiple = 1.0f;	// used for WASD movement
	public float ZoomSpeedMultiple = 45.0f;		// used for zooming in and out with mouse scroll
	public float MoveLerpSpeed = 6f;

	// ----- Touch Input -----
	// used for panning and zooming
	private Vector2 TouchDelta = new Vector2();
	private Vector2 OriginalTouch = new Vector2();
	private Vector2 CurrentTouch = new Vector2();
	// used for pinching
	private Vector2 OriginalTouch2 = new Vector2();
	private Vector2 TouchDelta2 = new Vector2();
	private Vector2 CurrentTouch2 = new Vector2();
	private Vector3 OriginalMovement = new Vector3();
	private bool IsPinching = false;

	public bool IsAutoMoveForward = false;
	public bool IsPaused = false;
	private Vector3 PreviousMCameraPosition;
	private bool IsAboveGround = false;
	public bool DoesBorderAffectPosition = false;

	void Start () {
		//CameraPosition.y = gameObject.transform.position.y;
		//CameraPosition = gameObject.transform.position + new Vector3(0,10,0);
		MyCamera = gameObject.transform.FindChild("Camera(Clone)").gameObject;
	}

	
	public void CheckPlayerInputs() {
		Inventory MyInventory = gameObject.GetComponent<BaseCharacter> ().MyInventory;
		// Essentially pause screen
		if (Input.GetKeyDown (KeyCode.X)) {
			ToggleMenuSystem();
		}
		bool IsInGame = true;
		if (GetManager.GetGuiManager ()) {
			if (GetManager.GetGuiManager ().CurrentScreen != ScreenType.InGame)
				IsInGame = false;
		}
		if (!IsPaused && IsInGame) {	
			Cursor.lockState = CursorLockMode.Locked;
			if (Input.GetKeyDown (KeyCode.Escape)) {
				MyInventory.UnSelectItem();
			}
			if (Input.GetKeyDown (KeyCode.C)) {
				GetManager.GetGameManager().PauseGame();
			}
			//if (Input.GetKeyDown (KeyCode.Tab)) {
			//	if (!GetManager.GetChatBox().IsEnabled())
			//		GetManager.GetGameManager().ToggleScores();
			//}	
			if (Input.GetKeyDown (KeyCode.Z)) {
				if (!GetManager.GetChatBox().IsEnabled()) {
					MyInventory.DropSelectedItem();
				}
			}
			//if (Input.GetKeyDown (KeyCode.V)) {
			//	GetManager.GetCameraManager().ToggleTopDown();
			//}
			if (Input.GetKeyDown (KeyCode.C)) {
				GetManager.GetZoneManager().ToggleZones();
			}
			if (Input.GetKeyDown(KeyCode.T)) {
				GetManager.GetChatBox().EnableChatBox();
				PauseGame();
			}
			if (!IsFlyMode) {
				//if (Input.GetKeyDown (KeyCode.Q)) 
				if (Input.GetAxis ("Mouse ScrollWheel") > 0) 
				{
					if (!GetManager.GetChatBox().IsEnabled())
						MyInventory.SwitchItem (-1);
				}
				//if (Input.GetKeyDown (KeyCode.E)) 
				if (Input.GetAxis ("Mouse ScrollWheel") < 0) 
				{
					if (!GetManager.GetChatBox().IsEnabled())
						MyInventory.SwitchItem (1);
				}
			}
			if ( GetManager.GetCameraManager ().MyCameraMode == CameraMode.TopDown) {
				if (!IsRayHitGui()) {
					if (GetManager.GetGuiManager().CurrentScreen == ScreenType.InGame) {
						MyInventory.IsShoot = (Input.GetMouseButtonDown (0));
						MyInventory.IsShoot2 = (Input.GetMouseButtonDown (1));
						//MyInventory.IsShoot3 = (Input.GetKeyDown (KeyCode.F));
					}
				}
			} else {
				if (IsPlayerMovementInput) {
					//Input.mouse
					// IsShoot, IsShoot2, should be 'triggers' -> and selectedItem should be related
					// so i can hotkey a key to a selected item with a trigger
					//IsShoot = (Input.GetMouseButtonDown (0));
					MyInventory.IsShoot = (Input.GetMouseButton (0));
					MyInventory.IsShoot2 = (Input.GetMouseButtonDown (1));
					//MyInventory.IsShoot3 = (Input.GetKeyDown (KeyCode.F));
				} 
			}
		}
	}
	
	// Checks to see if the mouse is hitting the gui
	public bool IsRayHitGui() {
		var pointer = new PointerEventData(EventSystem.current);
		pointer.position = (Input.mousePosition);
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, raycastResults);
		if (raycastResults.Count > 0) {
			return true;
		}
		return false;
	}

	// disables all input
	public void EnablePlayerInput() 
	{
		IsPaused = false;
		CameraMode MyCameraMode = GetManager.GetCameraManager ().MyCameraMode;
		if (MyCameraMode == CameraMode.TopDown)
			EnableRtsControls ();
		else if (MyCameraMode == CameraMode.FirstPerson)
			EnableFpsControls ();
		TogglePostProcessing ();
	}

	public void ToggleMenuSystem() {
		if (GetManager.GetGameManager ().CanToggleMenu ()) 
		{
			if (GetManager.GetGuiManager ().ToggleInventory ()) 
			{
				EnablePlayerInput ();
			} 
			else 
			{
				PauseGame ();
			}
		}
	}
	public void PauseGame() 
	{
		//CustomController MyCustomController = gameObject.GetComponent <CustomController>();
		//if (MyCustomController)
		//	MyCustomController.enabled = false;
		//PlayerMovement MyMovement = gameObject.GetComponent<PlayerMovement>();
		//MyMovement.enabled = false;
		IsPlayerMovementInput = false;
		IsPaused = true;
		GetManager.GetCameraManager().EnableMouseGui();
		TogglePostProcessing ();
	}
	
	public void ToggleDeathPostProcessing() {
		GameObject MyCamera = Camera.main.gameObject;
		/*if (MyCamera.GetComponent<SepiaTone> ())
			MyCamera.GetComponent<SepiaTone> ().enabled = true;
		if (MyCamera.GetComponent<NoiseAndGrain> ())
			MyCamera.GetComponent<NoiseAndGrain> ().enabled = true;
		if (MyCamera.GetComponent<Grayscale> ())
			MyCamera.GetComponent<Grayscale> ().enabled = true;*/
	}
	public void TogglePostProcessing() {
		GameObject MyCamera = Camera.main.gameObject;
		if (MyCamera) {
			///if (MyCamera.GetComponent<SepiaTone> ())
			//	MyCamera.GetComponent<SepiaTone> ().enabled = IsPaused;
			//if (MyCamera.GetComponent<NoiseAndGrain> ())
			//	MyCamera.GetComponent<NoiseAndGrain> ().enabled = IsPaused;
			//if (MyCamera.GetComponent<Grayscale> ())
			//	MyCamera.GetComponent<Grayscale> ().enabled = IsPaused;
		}
	}

	
	public void EnableFpsControls() {
		//CustomController MyCustomController = gameObject.GetComponent <CustomController>();
		//MyCustomController.enabled = true;
		IsPlayerMovementInput = true;
	}
	public void EnableRtsControls() {
		//CustomController MyCustomController = gameObject.GetComponent <CustomController>();
		//MyCustomController.enabled = false;
		IsPlayerMovementInput = false;
		GetManager.GetCameraManager().EnableMouseGui();
	}
	// Update is called once per frame
	void Update () {
		if (MyCamera == null) {
			MyCamera = gameObject.transform.GetChild (0).gameObject;
		}
		CheckPlayerInputs ();
		/*if (IsPlayerMovementInput && gameObject.GetComponent<CustomController> ())
		{
			if (Input.GetKeyDown (KeyCode.F)) {
				IsFlyMode = !IsFlyMode;
			}
			if (IsFlyMode && Input.GetKeyDown (KeyCode.Numlock)) {
				IsAutoMoveForward = !IsAutoMoveForward;
			}
			if (IsAutoMoveForward) {
				var target = transform.position+transform.forward*5;
				var offset = target - transform.position;
				gameObject.GetComponent<CharacterController> ().Move(offset*Time.deltaTime);
			}
			if (IsFlyMode) {
				if (Input.GetKey (KeyCode.E)) 
				{
					if (!GetManager.GetChatBox ().IsEnabled ())
						gameObject.GetComponent<CustomController> ().fly = 3.0f;
				} else if (Input.GetKey (KeyCode.Q)) {
					if (!GetManager.GetChatBox ().IsEnabled ())
						gameObject.GetComponent<CustomController> ().fly = -3.0f;
				} else
					gameObject.GetComponent<CustomController> ().fly = 0;
			}
			gameObject.GetComponent<CustomController> ().IsFlyMode = IsFlyMode;
			if (!GetManager.GetChatBox ().IsEnabled ()) {
				if (Input.GetKey (KeyCode.W))
				{
					gameObject.GetComponent<CustomController> ().vertical = 1.0f;
				} else if (Input.GetKey (KeyCode.S)) {
					gameObject.GetComponent<CustomController> ().vertical = -1.0f;
				} else {
					gameObject.GetComponent<CustomController> ().vertical = 0f;
				}
				if (Input.GetKey (KeyCode.A)) {
					gameObject.GetComponent<CustomController> ().horizontal = -1.0f;
				} else if (Input.GetKey (KeyCode.D)) {
					gameObject.GetComponent<CustomController> ().horizontal = 1.0f;
				} else {
					gameObject.GetComponent<CustomController> ().horizontal = 0f;
				}
			}
			if (Input.GetKeyDown(KeyCode.Space))
				gameObject.GetComponent<CustomController> ().m_Jump = true;
		} */
		/*else 
		{	// if rts controls
			HandleTouchInput();
			HandleKeyboardInput();
			Move ();	// moves the camera
		}*/
	}

	public void HandleKeyboardInput() {
		if (!Input.touchSupported && Input.touchCount == 0) {
			float ZoomSpeed = ZoomSpeedMultiple * Time.deltaTime;
			if (Input.GetKey (KeyCode.LeftShift)) {
				ZoomSpeed *= 5.0f;
			}
			if (Input.GetKey (KeyCode.W) || IsMouseInTopBorder ()) {
				CameraPosition += new Vector3 (0, 0, MovementSpeed);
			} else if (Input.GetKey (KeyCode.S) || IsMouseInBottomBorder ()) {
				CameraPosition -= new Vector3 (0, 0, MovementSpeed);
			}
			if (Input.GetKey (KeyCode.A) || IsMouseInLeftBorder ()) {
				CameraPosition -= new Vector3 (MovementSpeed, 0, 0);
			} else if (Input.GetKey (KeyCode.D) || IsMouseInRightBorder ()) {
				CameraPosition += new Vector3 (MovementSpeed, 0, 0);
			}
			if (Input.GetAxis ("Mouse ScrollWheel") < 0) 
			{
				ZoomOut (ZoomSpeed);
			}
			if (Input.GetAxis ("Mouse ScrollWheel") > 0) 
			{
				ZoomIn (ZoomSpeed);
			}
		}
	}

	public void CentreCamera() {
			float TempY = CameraPosition.y;
			CameraPosition = transform.position;
			CameraPosition.y = TempY;
	}

	public void SetPlayerInput(bool NewPlayerInput) {
		if (NewPlayerInput == false) 
		{
			CentreCamera ();
		}
		IsPlayerMovementInput = NewPlayerInput;
	}

	public void RestrainPosition() {
		if (IsCameraRestrained) {
			CameraPosition.x = Mathf.Clamp (CameraPosition.x, transform.position.x+LimiterMinimum.x, transform.position.x+LimiterMaximum.x);
			CameraPosition.z = Mathf.Clamp (CameraPosition.z, transform.position.z+LimiterMinimum.z, transform.position.z+LimiterMaximum.z);
			LayerMask OnlyChunksLayer = (1 << 10);	// ignore chunk layer
			//OnlyChunksLayer = ~OnlyChunksLayer;	// only do chunk layer
			RaycastHit MyHit;
			if (Physics.Raycast(new Vector3(MyCamera.transform.position.x, 25f, MyCamera.transform.position.z), new Vector3(0,-1,0), out MyHit, 75f, OnlyChunksLayer)) {
				//Debug.LogError("Found new bottom: " + MyHit.point.ToString());
				float OldYLimit = LimiterMinimum.y;
				if (OldYLimit != MyHit.point.y + 0.5f) {
					LimiterMinimum.y = MyHit.point.y+0.5f;
					LimiterMaximum.y = LimiterMinimum.y + 10f;
					CameraPosition.y += -(OldYLimit - LimiterMinimum.y);
				}
				IsAboveGround = true;
			} else {
				// if ray not hitting ground - restore previous position
				IsAboveGround = false;
				CameraPosition = PreviousMCameraPosition;
			}
			CameraPosition.y = Mathf.Clamp (CameraPosition.y, LimiterMinimum.y, LimiterMaximum.y);
		}
	}

	public void Move() {
		float MovementSpeed = MovementSpeedMultiple * Time.deltaTime;
		if (Input.GetKey (KeyCode.LeftShift)) {
			MovementSpeed *= 5.0f;
		}
		RestrainPosition ();
		// now reposition the gameobject in the scene - the one with the camera component
		Vector3 NewPosition = CameraPosition.x*GetRightDirection() +  CameraPosition.z*GetForwardDirection();
		NewPosition.y = CameraPosition.y;	//MyCamera.transform.position.y;

		MyCamera.transform.position = Vector3.Lerp (MyCamera.transform.position, NewPosition, Time.deltaTime*MoveLerpSpeed);
		PreviousMCameraPosition = CameraPosition;
	}

	public void HandleTouchInput() {
		if (Input.touchCount > 0) {
			if (Input.GetTouch(0).phase == TouchPhase.Began) {
				OriginalTouch = Input.touches[0].position;
				if (Input.touches.Length > 1) {
					OriginalTouch2 = Input.touches[1].position;
					IsPinching = true;
				} else {
					IsPinching = false;
				}
				OriginalMovement = CameraPosition;
			} else if (Input.GetTouch(0).phase == TouchPhase.Moved) {// Get movement of the finger since last frame
				if (!IsPinching && Input.touches.Length > 1 && Input.GetTouch(1).phase == TouchPhase.Began) {
					OriginalTouch = Input.touches[0].position;
					OriginalTouch2 = Input.touches[1].position;
					OriginalMovement = CameraPosition;
					IsPinching = true;
				}
				CurrentTouch = Input.GetTouch(0).position;
				TouchDelta = OriginalTouch-CurrentTouch;
				
				// Move object across XY plane
				if (!IsPinching) {
					CameraPosition = OriginalMovement + new Vector3(TouchMovementSpeed*(TouchDelta.x/100f), 0, TouchMovementSpeed*(TouchDelta.y/100f));
				} else {
					CurrentTouch2 = Input.GetTouch(1).position;
					TouchDelta2 = OriginalTouch2-CurrentTouch2;
					float OriginalDistance = Vector3.Distance (OriginalTouch2,OriginalTouch);
					CurrentDistance = Vector3.Distance (CurrentTouch2,CurrentTouch);
					ZoomDistance = TouchMovementSpeed*((OriginalDistance-CurrentDistance)/250f);
					CameraPosition.y = OriginalMovement.y + ZoomDistance;
				}
			}
			if (Input.GetTouch(0).phase == TouchPhase.Ended && !IsPinching) {
				TouchDelta = new Vector2();
			}
			if (IsPinching && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)) {
				IsPinching = false;
				OriginalMovement = CameraPosition;
				if (Input.GetTouch(0).phase == TouchPhase.Ended) {
					OriginalTouch = Input.touches[1].position;
				} else {
					OriginalTouch = Input.touches[0].position;
				}
			}
		}
	}
	
	public void EnableMovement() {
		IsPlayerMovementInput = true;
	}
	public void DisableMovement() {
		IsPlayerMovementInput = false;
	}

	public void TogglePerspective() {
		IsPerspective = !IsPerspective;
		//if (IsPerspective) 
		{
			MyCamera.GetComponent<Camera>().orthographic = !IsPerspective;
		}
	}	
	
	public Vector3 GetForwardDirection () {
		/*Vector3 ForwardDirection = MyCamera.transform.right;
		ForwardDirection.y = 0;
		ForwardDirection.Normalize ();
		float TempX = ForwardDirection.x;
		ForwardDirection.x = ForwardDirection.z;
		ForwardDirection.z = TempX;
		return ForwardDirection;*/
		
		Vector3 ForwardDirection = MyCamera.transform.forward;
		ForwardDirection.y = 0;
		ForwardDirection.Normalize ();
		return ForwardDirection;
	}
	public Vector3 GetRightDirection () {
		Vector3 ForwardDirection = MyCamera.transform.right;
		ForwardDirection.y = 0;
		ForwardDirection.Normalize ();
		return ForwardDirection;
	}
	public bool IsMouseInRightBorder() {
		if (!DoesBorderAffectPosition)
			return false;
		Vector3 MousePosition = Input.mousePosition;
		Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
		if (MousePosition.x >= ScreenSize.x - BorderLength)
			return true;
		return false;
	}
	public bool IsMouseInLeftBorder() {
		if (!DoesBorderAffectPosition)
			return false;
		Vector3 MousePosition = Input.mousePosition;
		Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
		if (MousePosition.x <= BorderLength)
			return true;
		return false;
	}
	public bool IsMouseInTopBorder() {
		if (!DoesBorderAffectPosition)
			return false;
		Vector3 MousePosition = Input.mousePosition;
		Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
		if (MousePosition.y >= ScreenSize.y - BorderLength)
			return true; 
		
		return false;
	}
	public bool IsMouseInBottomBorder() {
		if (!DoesBorderAffectPosition)
			return false;
		Vector3 MousePosition = Input.mousePosition;
		Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
		if (MousePosition.y <= BorderLength)
			return true;
		return false;
	}
	
	public void ZoomOut(float ZoomSpeed) {
		if (MyCamera.GetComponent<Camera> ().orthographic) {
			MyCamera.GetComponent<Camera> ().orthographicSize += ZoomSpeed;
		} else {
			CameraPosition.y += ZoomSpeed;
		}
	}
	public void ZoomIn(float ZoomSpeed) {
		if (MyCamera.GetComponent<Camera> ().orthographic) {
			MyCamera.GetComponent<Camera> ().orthographicSize -= ZoomSpeed;
		} else {
			CameraPosition.y -= ZoomSpeed;
		}
	}

	// Debug stuff for mobile
	/*public void OnGUI() {
		if (IsDebug) {
			float ButtonSize = 100f;
			//if (gui.Button("+", GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize)))
			//	ZoomIn();
			//if (gui.Button("-", GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize)))
			//	ZoomOut();
			
			GUI.skin.label.fontSize = 40;
			gui.Label ("Is Touch Enabled: " + Input.touchSupported.ToString (), GUILayout.Width(Screen.width));
			gui.Label ("TouchDelta: " + TouchDelta.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Original Touch Position: " + OriginalTouch.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Current Touch Position: " + CurrentTouch.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Original Camera Position: " + OriginalMovement.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Current Camera Position: " + CameraPosition.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Is Pinching? " + IsPinching.ToString(), GUILayout.Width(Screen.width));
			//gui.Label ("ZoomDistance? " + ZoomDistance.ToString(), GUILayout.Width(Screen.width));
			//gui.Label ("CurrentDistance? " + CurrentDistance.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("CameraPosition? " + CameraPosition.ToString(), GUILayout.Width(Screen.width));
			gui.Label ("Is Camera? " + (MyCamera != null).ToString(), GUILayout.Width(Screen.width));
		}
	}*/
}
