using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

// need saving / loading
// need proper rotation


// Seperate this class into 3 parts
//		Model editing functions:
//			edit polygons
//			edit voxels
//			edit bons
//			File Opening/closing/saving

//		Interaction with the actual texture, ie ray casting into it etc	- so i can do this with other 2d guis
//		full screening a window

// My Custom Model editor
// ToDo:
//		Select Object (with 2 different objects in scene
//		ObjectList
//		Z to zoom in on object
//		Select Polygon
//		Select Vertex
//		Select Edge
//		Add Bone	- left click for first position, left click for second
//		Select Bone

// button to increase/decrease subdivision

// For bone editing mode, first create a bone with the code..

namespace OldCode {
public class ModelTextureHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,IPointerUpHandler, IDragHandler {
	public bool IsRunning = false; 	// if pointer is inside of the gui
	// enable zoom in/out with mouse - raytracing of model
	public bool IsMousePressed;
	// full screen stuff
	public bool IsFullScreen = false;
	private GameObject TextureOriginalParent;
	private Vector3 TextureOriginalPosition;
	private Vector2 TextureOriginalSize;
	public bool IsMoveToolHit = false;
	public bool IsMoveToolHitX = false;
	public bool IsMoveToolHitY = false;
	public bool IsMoveToolHitZ = false;

	private Vector2 InsideTextureMousePosition;
	public Vector2 MouseOriginalPosition;	// when intiially pressed
	public Vector2 MouseCurrentPosition;
	public GameObject MyCamera;
	//private RaycastHit MyHit;
	public RaycastHit MyRayTraceHit;
	public bool IsRayHitObject = false;
	public Vector3 Original3DPosition;

	public GameObject MyModelEditorManager;

	// Use this for initialization
	void Start () {
		MyModelEditorManager = GameObject.Find ("MyModelEditorManager");
	}

	public Vector3 MultiplyVectors(Vector3 FirstVector, Vector3 SecondVector) {
		return new Vector3 (FirstVector.x * SecondVector.x, FirstVector.y * SecondVector.y, FirstVector.z * SecondVector.z);
	}
	public bool IsMouseHitModelEditorGui = false;
	// Update is called once per frame
	void Update () {
		//if (IsRunning && MyCamera != null) {
		//	RayTraceToModel();
		//}
		if (MyCamera == null) {
			Debug.LogError("Error, MyCamera is null in ModelTextureHandler.");
		}
	}
	
	// Gets the ray inside the camera viewport
	public Ray GetRayInModelTexture() {
		Ray ray = MyCamera.GetComponent<Camera>().ScreenPointToRay(GetViewportPositionInsideModelTexture());
		return ray;
	}
	
	// calculates the stretched value of the texture, into the viewport
	public Vector3 GetViewportPositionInsideModelTexture() {
		Vector2 SizeOfModelTexture = GetStretchedModelTextureSize ();
		
		InsideTextureMousePosition = Input.mousePosition - transform.position;
		InsideTextureMousePosition.y = SizeOfModelTexture.y - Mathf.Abs (InsideTextureMousePosition.y);
		//Debug.LogError ("InsideTextureMousePosition: " + InsideTextureMousePosition.ToString ());
		Vector2 InsideSecondaryViewportPosition = InsideTextureMousePosition;
		
		InsideSecondaryViewportPosition.x /= SizeOfModelTexture.x;
		InsideSecondaryViewportPosition.y /= SizeOfModelTexture.y;
		float MyTextureSize = 2048f;	// the size of texture , need to make this dynamic
		InsideSecondaryViewportPosition.x *= MyTextureSize;
		InsideSecondaryViewportPosition.y *= MyTextureSize;
		
		return InsideSecondaryViewportPosition;
	}

	// checks a raycast on the ui, to see if model editor gui elements are being hit
	public void CheckExtraMouseHits() {
		IsMouseHitModelEditorGui = false;
		var pointer = new PointerEventData(EventSystem.current);
		pointer.position = (Input.mousePosition);
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, raycastResults);
		for (int i = 0; i < raycastResults.Count; i++) {
			if (raycastResults[i].gameObject.tag == "ModelEditorGui") {
				IsMouseHitModelEditorGui = true;
				break;
			}
		}
	}
	public Vector2 GetStretchedModelTextureSize() {
		Vector2 SizeOfModelTexture = gameObject.GetComponent<RectTransform> ().GetSize ();
		SizeOfModelTexture.x = SizeOfModelTexture.x / 1920f;	// this should use the standard canvas size instead, otherwise if i change it, it will cause issues
		SizeOfModelTexture.x *= ((float)Screen.width);
		SizeOfModelTexture.y = SizeOfModelTexture.y / 1080f;
		SizeOfModelTexture.y *= ((float)Screen.height);
		return SizeOfModelTexture;
	}
	public Vector3 GetPositionInViewPortWithDepth(float Depth) {
		Ray MyRay = GetRayInModelTexture ();
		return MyRay.origin + MyRay.direction * Depth;
	}

	public void RayTraceTo3DModelGizmo() 
	{
		RaycastHit hit;
		IsMoveToolHit = false;
		IsMoveToolHitX = false;
		IsMoveToolHitY = false;
		IsMoveToolHitZ = false;
		Ray ray = GetRayInModelTexture ();
		if (Physics.Raycast (ray, out hit, 100f, 1 << 13)) {	// layer
			if (hit.collider.gameObject.tag == "3DModelGizmo") {
				IsMoveToolHit = true;
			}
			else if (hit.collider.gameObject.tag == "3DModelGizmoX") {
				IsMoveToolHitX = true;
			}
			else if (hit.collider.gameObject.tag == "3DModelGizmoY") {
				IsMoveToolHitY = true;
			}
			else if (hit.collider.gameObject.tag == "3DModelGizmoZ") {
				IsMoveToolHitZ = true;
			}
		}
	}

	public void RayTraceToModel() 
	{
		//Debug.LogError ("RayTracing Model");
		Ray ray = GetRayInModelTexture ();
		if (Physics.Raycast (ray, out MyRayTraceHit)) 
		{
			IsRayHitObject = true;
			//Debug.DrawLine(ray.origin, MyRayTraceHit.point, Color.red, 2f);
			//Debug.DrawLine(MyRayTraceHit.point, ray.origin+ray.direction*50f, Color.white, 2f);
			//Debug.DrawLine(MyRayTraceHit.point, MyRayTraceHit.point + MyRayTraceHit.normal*MyRayTraceHit.collider.gameObject.transform.localScale.x/2f, Color.blue, 2f);
		} 
		else 
		{
			IsRayHitObject = false;
			//Debug.DrawLine(ray.origin, ray.origin+ray.direction*50f, new Color32((byte)(Random.Range(0,255)),(byte)(Random.Range(0,255)),(byte)(Random.Range(0,255)),255), 2f);
		}
		//Debug.Break ();
	}

	public void OnPointerUp(PointerEventData data) 
	{
		IsMousePressed = false;
	}

	public void OnPointerDown (PointerEventData data) 
	{
		IsMousePressed = true;
		MouseOriginalPosition = data.position;
		MouseCurrentPosition = data.position;
		//Vector2 StretchedModelTextureSize = GetStretchedModelTextureSize ();
		Ray MyRay = GetRayInModelTexture ();
		Original3DPosition =  MyRay.origin + MyRay.direction * 10.0f;
		MyModelEditorManager.GetComponent<ModelEditorManager> ().HandleFirstClick ();
	}

	// On dragon I can rotate and move the model
	// later on ill make multiple 3d objects in the scene lol
	// bug: rotatation still has the y flip sometimes
	public Vector2 MouseDeltaPosition;
	public void OnDrag (PointerEventData data) {
		MouseCurrentPosition = data.position;
		MouseDeltaPosition = MouseCurrentPosition - MouseOriginalPosition;
		if (IsMousePressed) {
			MyModelEditorManager.GetComponent<ModelEditorManager> ().HandleMouseDrag ();
		}
	}

	public void OnPointerEnter(PointerEventData eventData) {
		IsRunning = true;
		//Debug.LogError ("Mouse Entering MapTexture: " + Time.time);
	}
	public void OnPointerExit(PointerEventData eventData) {
		IsRunning = false;
		//Debug.LogError ("Mouse Exiting MapTexture: " + Time.time);
	}
	public void OnSelect(BaseEventData eventData) {
		//Debug.LogError ("Select MapTexture: " + Time.time);
		// grab mouse position

	}
	public void OnDeselect(BaseEventData eventData) {
		//Debug.LogError ("Unselect MapTexture: " + Time.time);
	}
	
	// This should be seperate and placed into its own class, for full screening windows
	public void ToggleFullScreen() {
		UpdateFullScreen (!IsFullScreen);
	}
	public void UpdateFullScreen(bool IsFullScreen_) {
		if (IsFullScreen != IsFullScreen_) {
			IsFullScreen = IsFullScreen_;
			if (IsFullScreen) {
				TextureOriginalParent = transform.parent.gameObject;
				TextureOriginalSize = gameObject.GetComponent<RectTransform>().sizeDelta;
				TextureOriginalPosition = gameObject.GetComponent<RectTransform>().GetLeftTopPosition();
				// set new parent to the parents parent
				gameObject.transform.SetParent(gameObject.transform.parent.gameObject.transform.parent,false);
				// as it is a render texture, it has to be the ratio of the texture - textures width/height had to be the same
				gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1920);
				//RectTransformExtensions.SetLeftTopPosition(gameObject.GetComponent<RectTransform>(), new Vector2());
				gameObject.GetComponent<RectTransform>().SetLeftTopPosition( new Vector2(-960, 540));
				
			} else {
				gameObject.transform.SetParent(TextureOriginalParent.transform,false);
				gameObject.GetComponent<RectTransform>().sizeDelta = TextureOriginalSize;
				gameObject.GetComponent<RectTransform>().SetLeftTopPosition(TextureOriginalPosition);
			}
		}
	}
}
}
