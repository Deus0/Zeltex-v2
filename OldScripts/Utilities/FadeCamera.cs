using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityStandardAssets.Characters.FirstPerson;

// should take in 2 game objects
// assuming one or none has a camera attached:
//		Destroy the main camera if it isn't attached to one of them
//		Spawn a camera and attach it to one
//		Attach player controllers to them if they dont have them
//		Destroy the player controller after
//		*Option to disable the other character after - for things like puzzle solving, or teleporting
//		*Option to clone the first character and spawn in another spot, and destroy the real version after teleportation has been done	- after image effect

public class FadeCamera : MonoBehaviour {
	public Vector3 CameraHotSpot = new Vector3 (0, 0.25f, 0);
	public bool IsMultipleControl = false;
	public bool IsTeleportSecondPlayer = false;
	private RawImage MyImage1;
	private RawImage MyImage2;
	public GameObject MyPlayer1;
	public GameObject MyPlayer2;
	public Camera MyCamera1;
	public Camera MyCamera2;
	public RenderTexture MyRenderTexture1;
	public RenderTexture MyRenderTexture2;
	public float TransitionAlpha = 0;
	public Canvas MyCanvas;

	public bool IsAnimating = false;
	public float AnimationSpeed = 0.25f;
	public bool IsForward = true;
	public int MyUILayer;

	GameObject SpawnedMainCamera;	// for the purpose of the canvas

	// Use this for initialization
	void Awake () {
		CheckForObjects ();
		if (MyPlayer1 != null && MyPlayer2 != null)
			SetupImages ();
		else
			gameObject.SetActive (false);
	}
	//atm the only problem is camera angles
	// can be fixed by just using the cameras already attached to the players

	// Update is called once per frame
	void Update () {
		//Screen.lockCursor = true;
		AnimateCamera();
		if (Input.GetKeyDown (KeyCode.B)) {
			BeginAnimation();
		}
	}
	void SetupImages() {
		//MyPlayer1.GetComponent<CustomController>().m_Camera = MyPlayer1.gameObject.transform.GetChild(0).GetComponent<Camera>();
		//MyPlayer2.GetComponent<CustomController>().m_Camera = MyPlayer2.gameObject.transform.GetChild(0).GetComponent<Camera>();
		
		GameObject MyImage1Object = new GameObject ();
		MyImage1Object.AddComponent<RawImage> ();
		MyImage1 = MyImage1Object.GetComponent<RawImage>();
		MyImage1.GetComponent<RawImage> ().texture = MyRenderTexture1;
		RectTransform MyRectTransform1 = MyImage1Object.GetComponent<RectTransform> ();
		MyRectTransform1.anchorMin = new Vector2 (0, 0);
		MyRectTransform1.anchorMax = new  Vector2 (1, 1);
		MyRectTransform1.sizeDelta = new Vector2(0, 0);
		MyRectTransform1.localPosition = new Vector2 (0, 0);
		MyImage1Object.layer = MyUILayer;
		MyImage1Object.name = "MyImage1";
		MyImage1Object.transform.SetParent (MyCanvas.transform, false);
		
		GameObject MyImage2Object = new GameObject ();
		MyImage2Object.AddComponent<RawImage> ();
		MyImage2 = MyImage2Object.GetComponent<RawImage>();
		MyImage2.GetComponent<RawImage> ().texture = MyRenderTexture2;
		RectTransform MyRectTransform2 = MyImage2Object.GetComponent<RectTransform> ();
		MyRectTransform2.anchorMin = new Vector2 (0, 0);
		MyRectTransform2.anchorMax = new  Vector2 (1, 1);
		MyImage2Object.transform.SetParent (MyCanvas.transform, false);
		MyImage2Object.name = "MyImage2";
		MyImage2Object.layer = MyUILayer;
		
		MyImage1Object.SetActive (false);
		MyImage2Object.SetActive (false);
		SpawnedMainCamera = new GameObject ();
		SpawnedMainCamera.AddComponent<Camera> ();
		SpawnedMainCamera.tag = "MainCamera";
		SpawnedMainCamera.SetActive (false);
	}
	void AnimateCamera() {
		if (IsAnimating) {
			TransitionAlpha = Mathf.Lerp (TransitionAlpha, 0, Time.deltaTime*AnimationSpeed);
			int TemporaryAlpha = Mathf.RoundToInt(TransitionAlpha);
			if (IsForward) {
				MyImage1.color = new Color32(255,255,255,(byte)TemporaryAlpha);	//Color32.Lerp (MyImage1.color, new Color32(255,255,255,0), Time.deltaTime*AnimationSpeed);
				//MyImage1.color = new Color32(255,255,255, (byte) Mathf.RoundToInt(Mathf.Lerp (MyImage1.color.a, 0, Time.deltaTime*AnimationSpeed)));
				MyImage2.color = new Color32(255,255,255,255);	// Color32.Lerp (MyImage1.color,new Color32(255,255,255,255), Time.deltaTime*AnimationSpeed);
				//MyImage2.color = Color32.Lerp (MyImage2.color, new Color32(255,255,255,255), Time.deltaTime*AnimationSpeed);
			} else {
				MyImage1.color = new Color32(255,255,255,255);	// Color32.Lerp (MyImage1.color,new Color32(255,255,255,255), Time.deltaTime*AnimationSpeed);
				MyImage2.color = new Color32(255,255,255,(byte)TemporaryAlpha);	//Color32.Lerp (MyImage1.color, new Color32(255,255,255,0), Time.deltaTime*AnimationSpeed);
				//MyImage2.color = Color32.Lerp (MyImage2.color,  new Color32(255,255,255,0) , Time.deltaTime*AnimationSpeed);
				//MyImage2.color = new Color32(255,255,255, (byte) Mathf.RoundToInt(Mathf.Lerp (MyImage2.color.a, 0, Time.deltaTime*AnimationSpeed)));
			}
			if ((MyImage1.color.a == 0 && IsForward) || (MyImage2.color.a == 0 && !IsForward)) 
			{
				FinishAnimation();
			}
		}
	}
	void FinishAnimation() {
		SpawnedMainCamera.SetActive (false);
		if (IsForward)
			MyCamera1.gameObject.SetActive (false);
		else 
			MyCamera2.gameObject.SetActive (false);
		IsAnimating = false;
		// disable images used for blending
		MyImage1.gameObject.SetActive (false);
		MyImage2.gameObject.SetActive (false);
		MyCamera1.targetTexture = null;
		MyCamera2.targetTexture = null;
		IsForward = !IsForward;
	}

	// Should be a check for the cameras and players
	public void CheckForObjects() {
		if (MyPlayer1 == null) {
			
		}
		if (MyPlayer2 == null) {
			
		}
		if (MyCamera1 == null) {
			if (!DoesObjectHaveCamera(MyPlayer1))
			    AttachCameraToObject(MyPlayer1);

			MyCamera1 = GetObjectCamera(MyPlayer1);
		}
		if (MyCamera2 == null) {
			if (!DoesObjectHaveCamera(MyPlayer2))
				AttachCameraToObject(MyPlayer2);
			MyCamera2 = GetObjectCamera(MyPlayer2);
		}
	}
	
	public Camera GetObjectCamera(GameObject MyCharacter) {
		if (MyCharacter == null)
			return null;
		for (int i = 0; i < MyCharacter.transform.childCount; i++) {
			if (MyCharacter.transform.GetChild(i).GetComponent<Camera>() != null) 
				return MyCharacter.transform.GetChild(i).GetComponent<Camera>();
		}
		return null;
	}

	public void AttachCameraToObject(GameObject MyCharacter) {
		GameObject MyCamera = new GameObject ();
		MyCamera.AddComponent<Camera> ();
		MyCamera.AddComponent<AudioListener> ();
		//CustomController MyPlayerController = MyCharacter.GetComponent<CustomController> ();
		//if (MyPlayerController != null) {
		//	MyPlayerController.UpdateCamera (MyCamera.GetComponent<Camera> ());
		//}
		MyCamera.transform.SetParent (MyCharacter.transform, false);
		MyCamera.transform.position = MyCharacter.transform.position + CameraHotSpot;
		MyCamera.name = "FirstPersonCamera";
	}

	public bool DoesObjectHaveCamera(GameObject MyCharacter) {
		if (MyCharacter == null)
			return false;
		for (int i = 0; i < MyCharacter.transform.childCount; i++) {
			if (MyCharacter.transform.GetChild(i).GetComponent<Camera>() != null) 
				return true;
		}
		return false;
	}

	void BeginAnimation() {
		if (!IsAnimating) {
			CheckForObjects();

			SpawnedMainCamera.SetActive (true);
			// i should also disable character movements for this animation
			if (IsForward) {
				MyPlayer2.transform.position = new Vector3(MyPlayer1.transform.position.x-50, MyPlayer1.transform.position.y, MyPlayer1.transform.position.z);
			} else {
				MyPlayer1.transform.position = new Vector3(MyPlayer2.transform.position.x+50, MyPlayer2.transform.position.y, MyPlayer2.transform.position.z);
			}
			IsAnimating = true;
			// Player Settings
			MyPlayer1.SetActive(true);
			MyPlayer2.SetActive(true);
			if (IsForward) {
				if (!IsMultipleControl) {
					//MyPlayer1.GetComponent<CustomController>().enabled = false;
					//MyPlayer2.GetComponent<CustomController>().enabled = true;
				}
			}
			else {
				if (!IsMultipleControl) {
					//MyPlayer1.GetComponent<CustomController>().enabled = true;
					//MyPlayer2.GetComponent<CustomController>().enabled = false;
				}
			}

			MyCamera1.gameObject.SetActive (true);
			MyCamera2.gameObject.SetActive (true);
			MyImage1.gameObject.SetActive(true);
			MyImage2.gameObject.SetActive(true);
			RectTransform MyImage1Transform = MyImage1.GetComponent<RectTransform>();
			RectTransform MyImage2Transform = MyImage2.GetComponent<RectTransform>();
			if (IsForward) {
				GameObject Canvas = MyImage1.transform.parent.gameObject;
				Canvas.transform.DetachChildren();
				//Debug.Break ();
				MyImage2Transform.SetParent(Canvas.GetComponent<RectTransform>(), false);
				MyImage1Transform.SetParent(Canvas.GetComponent<RectTransform>(), false);
				MyImage1Transform.sizeDelta = new Vector2(0, 0);
				MyImage1Transform.localPosition = new Vector2 (0, 0);
				MyImage2Transform.sizeDelta = new Vector2(0, 0);
				MyImage2Transform.localPosition = new Vector2 (0, 0);
				MyImage1Transform.SetDefaultScale();
				MyImage2Transform.SetDefaultScale();
			} else { 
				GameObject Canvas = MyImage1.transform.parent.gameObject;
				Canvas.transform.DetachChildren();
				MyImage1Transform.SetParent(Canvas.GetComponent<RectTransform>(), false);
				MyImage2Transform.SetParent(Canvas.GetComponent<RectTransform>(), false);
				MyImage1Transform.sizeDelta = new Vector2(0, 0);
				MyImage1Transform.localPosition = new Vector2 (0, 0);
				MyImage2Transform.sizeDelta = new Vector2(0, 0);
				MyImage2Transform.localPosition = new Vector2 (0, 0);
				MyImage1Transform.SetDefaultScale();
				MyImage2Transform.SetDefaultScale();
			}

			//MyPlayer1.AddComponent<Camera>();
			MyCamera1.targetTexture = MyRenderTexture1;
			MyImage1.texture = MyRenderTexture1;
			MyImage1.gameObject.SetActive(true);
			//Debug.Break();
			//MyPlayer2.AddComponent<Camera>();
			MyCamera2.targetTexture = MyRenderTexture2;
			MyImage2.texture = MyRenderTexture2;
			MyImage2.gameObject.SetActive(true);
			//Debug.Break();
			MyImage1.color = new Color32(255,255,255,255);	// starting color for texture to blend
			MyImage2.color = new Color32(255,255,255,255);	// starts off as invisible
			TransitionAlpha = 255;
		}
	}
}
