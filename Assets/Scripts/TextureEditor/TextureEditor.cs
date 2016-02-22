using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// To DO:
// fix weird bug where it clips to other size
//	add in chunking so updates are more efficient for larger sizes

public class TextureEditor : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
	public GameObject MySelectObject;
	public GameObject MyColorShower;
	bool bInputEnabled;
	public Texture2D MyTexture = null;
	public Color32 MyColor = new Color32(0,0,0,255);
	public Vector2 MyBrushSize = new Vector2(50,50);

	// Use this for initialization
	void Start () {
		PaintTexture (new Vector2 (), new Vector2 (), MyColor);
		ChangeSize (MyBrushSize);
		PaintTexture (MySelectObject, MyBrushSize / 2f, MyBrushSize, MyColor);
		bInputEnabled = true;
		MyPhoton = gameObject.GetComponent<PhotonView> ();
		if (MyPhoton == null)
			MyPhoton = gameObject.AddComponent<PhotonView> ();
	}
	public Vector2 GetDrawPosition() {
		Vector2 MousePosition = Input.mousePosition;
		//Vector3 WorldPosition = Camera.main.ScreenToWorldPoint(MousePosition);
		Vector2 MyInRectPosition;
		RectTransform MyRect = GetComponent<RectTransform>();
		/*if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (MyRect, 
		 * 																PositionToDraw,
		                                                        		null, 
		                                                        		out MyInRectPosition))
			return new Vector2 (-1, -1);*/
		
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (MyRect, 
		                                                              MousePosition,
		                                                              Camera.main,
		                                                              out MyInRectPosition))
			return new Vector2 (-1, -1);
		MyInRectPosition = new Vector2 (Mathf.RoundToInt (MyInRectPosition.x), Mathf.RoundToInt (MyInRectPosition.y));	// not rounding it gives major errors
		return MyInRectPosition;
		//return PositionToDraw;
	}
	// Update is called once per frame
	void Update () {
		if (bInputEnabled) {
			HandleKeyboardInput();
			HandleMouseInput();
			//Debug.LogError(GetDrawPosition().ToString());
			//Debug.LogError("Test2: " + (gameObject.GetComponent<RectTransform>().position.y+gameObject.GetComponent<RectTransform>().sizeDelta.y/2f).ToString());
		}
	}

	public void HandleKeyboardInput() {
		if (Input.GetKeyDown (KeyCode.Q)) {

		}
	}

	PhotonView MyPhoton;
	public void HandleMouseInput() {
		MySelectObject.GetComponent<RectTransform>().position = Input.mousePosition;// the paint brush

		if (Input.GetAxis("Mouse ScrollWheel") > 0) {// == 0) { 
			IncreaseSize(new Vector2(5,5));
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {// == 0) { 
			IncreaseSize(new Vector2(-5,-5));
		}
		if (Input.GetKey(KeyCode.D)) {// == 0) { 
			IncreaseColor(1,1,1,0);
		} else if (Input.GetKey(KeyCode.A)) {// == 0) { 
			IncreaseColor(-1,-1,-1,0);
		}
		Vector2 TempSize = new Vector2(MyBrushSize.x/transform.localScale.x, 
		                               MyBrushSize.y/transform.localScale.y);
		Vector2 MyPaintPosition = GetDrawPosition ();
		if (MyPaintPosition == new Vector2 (-1, -1))
			return;
		if (Input.GetMouseButton (0)) {
			if (PhotonNetwork.connected) {
				MyPhoton.RPC("PaintTexture",
				             PhotonTargets.All,
				             MyPaintPosition,
				             TempSize,
				             MyColor.r,
				             MyColor.g, 
				             MyColor.b, 
				             MyColor.a);
			} else {
				PaintTexture(MyPaintPosition,
				             TempSize,
				             MyColor);
			}
		} else if (Input.GetMouseButton (1)) 
		{
			if (PhotonNetwork.connected) {
				MyPhoton.RPC("PaintTexture",
				             PhotonTargets.All,
				             MyPaintPosition,
				             TempSize,
				             ClearColor.r, 
				             ClearColor.g, 
				             ClearColor.b, 
				             ClearColor.a);
			} else {
				PaintTexture(MyPaintPosition, 
				             TempSize,
				             ClearColor);
			}
		}
	}
	public void IncreaseSize(Vector2 SizeAddition) {
		ChangeSize (MyBrushSize + SizeAddition);
	}
	public void ChangeSize(Vector2 NewSize) {
		MyBrushSize = NewSize;
		MySelectObject.GetComponent<RectTransform> ().sizeDelta = MyBrushSize;
	}
	public void ChangeRed(InputField MyInput) {
		ChangeColor(new Color32((byte)int.Parse(MyInput.text), MyColor.g, MyColor.b, MyColor.a));
	}
	public void ChangeBlue(InputField MyInput) {
		ChangeColor(new Color32(MyColor.r, MyColor.g, (byte)int.Parse(MyInput.text), MyColor.a));
	}
	public void ChangeGreen(InputField MyInput) {
		ChangeColor( new Color32(MyColor.r, (byte)int.Parse(MyInput.text), MyColor.b, MyColor.a));
	}
	public void IncreaseColor(int red, int green, int blue, int transparency) {
		Color32 NewColor = MySelectObject.GetComponent<RawImage> ().color;
		int NewRed = (NewColor.r+red); 
		int NewGreen = (green+NewColor.g); 
		int NewBlue = (blue+NewColor.b); 
		int NewAlpha = (transparency+NewColor.a);
		NewRed = Mathf.Clamp (NewRed, 0, 255);
		NewGreen = Mathf.Clamp (NewGreen, 0, 255);
		NewBlue = Mathf.Clamp (NewBlue, 0, 255);
		NewAlpha = Mathf.Clamp (NewAlpha, 0, 255);
		NewColor.r =  (byte)(NewRed);
		NewColor.g =  (byte)(NewGreen);
		NewColor.b =  (byte)(NewBlue);
		NewColor.a =  (byte)(NewAlpha);
		ChangeColor (NewColor);
	}
	public void ChangeColor(Color32 NewColor) {
		MySelectObject.GetComponent<RawImage> ().color = NewColor;
		MyColorShower.GetComponent<RawImage> ().color = NewColor;
		MyColor = NewColor;
	}

	public Color32 ClearColor = new Color32 (255, 255, 255, 0);
	public void Clear() {
		Color[] MyColorData = MyTexture.GetPixels(0);
		for (int i = 0; i < MyColorData.Length; i++) {
			
			MyColorData[i] = ClearColor;
		}
		MyTexture.SetPixels( MyColorData, 0 );
		MyTexture.Apply(true);
		gameObject.GetComponent<RawImage> ().texture = MyTexture;
	}
	[PunRPC]
	public void PaintTexture(Vector2 Position, Vector2 Size, byte PaintColorR, byte PaintColorG, byte PaintColorB, byte PaintColorA) {
		PaintTexture (gameObject, Position, Size, 
		              new Color32(PaintColorR, PaintColorG, PaintColorB, PaintColorA));
	}

	public void PaintTexture(Vector2 Position, Vector2 Size, Color32 PaintColor) {
		PaintTexture (gameObject, Position, Size, PaintColor);
	}
	public void PaintTexture(GameObject MyPaintObject, Vector2 Position, Vector2 Size, Color32 PaintColor) 
	{
		//Debug.LogError ("Painting at " + Position.ToString ());
		MyTexture = MyPaintObject.GetComponent<RawImage> ().texture as Texture2D;

		// duplicate the texture
		if (MyPaintObject.GetComponent<RawImage> ().texture == null || 
		    (MyTexture.width != MyPaintObject.GetComponent<RectTransform>().GetWidth() || 
		 MyTexture.width != MyPaintObject.GetComponent<RectTransform>().GetHeight())
		 ) {
			MyTexture = new Texture2D (Mathf.FloorToInt(MyPaintObject.GetComponent<RectTransform>().GetWidth()), 
			                           Mathf.FloorToInt(MyPaintObject.GetComponent<RectTransform>().GetHeight()));
			//Debug.Log ("Clearing texture: " + Time.time);
			//Debug.Log("Creating New Texture of Size: " + MyTexture.width + ":" + MyTexture.height);
			Color[] MyColorData2 = MyTexture.GetPixels(0);
			for(int i = 0; i < MyColorData2.Length; i++) {
				MyColorData2[i] = ClearColor;
			}
			MyTexture.SetPixels( MyColorData2, 0 );
		}
		bool IsBlend = false;
		//Debug.Log ("Painting Texture: " + Position.ToString() + " - WIth Size: " + Size.ToString());
		Color[] MyColorData = MyTexture.GetPixels(0);
		for (float i = Position.x-Mathf.FloorToInt(Size.x/2f); i < Mathf.CeilToInt(Position.x+Size.x/2f); i++)
			for (float j = Position.y-Mathf.FloorToInt(Size.y/2f); j < Position.y+Mathf.CeilToInt(Size.y/2f); j++) 
			{
			if (i >= 0 && i < MyTexture.width && j >= 0 && j <= MyTexture.height) {
				float DistanceToMid = Vector2.Distance(new Vector2(i,j), Position); 
				if (DistanceToMid <= Size.x/2f) {
					//PaintColor.a = (byte)(DistanceToMid/(Size.x/2f));
					//PaintColor.a = (byte)(255);
					int Index = Mathf.FloorToInt(i+j*MyTexture.width);
						if (Index < MyColorData.Length && Index >= 0) {
							Color32 NewColor = MyColorData[Index];
							if (IsBlend) {
								NewColor.r = (byte)((NewColor.r+PaintColor.r*PaintColor.a)/2f);
							    NewColor.g = (byte)((NewColor.g+PaintColor.g*PaintColor.a)/2f);
							    NewColor.b = (byte)((NewColor.b+PaintColor.b*PaintColor.a)/2f);
							} else {
								NewColor = PaintColor;
							}
							MyColorData[Index] = NewColor;
						}
					}
				}
			}
		//Debug.LogError ("Size: " + MyTexture.texelSize.ToString ());
		MyTexture.SetPixels( MyColorData, 0 );
		MyTexture.Apply(true);
		MyPaintObject.GetComponent<RawImage> ().texture = MyTexture;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		//Debug.Log ("Enabling Texture Editor");
		//bInputEnabled = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		//Debug.Log ("Disabling Texture Editor");
		//bInputEnabled = false;
	}

	public void OnDeselect(BaseEventData eventData) {

	}
	
	public void OnSelect(BaseEventData eventData) {

	}

	public void OnPointerDown (PointerEventData data) 
	{
		//Debug.Log ("Editing Texture");

	}
}


/* scraps
 * 
		//Debug.LogError("ScreenHeight: " + Screen.height);
		//PositionToDraw.y = Mathf.Abs(PositionToDraw.y-Screen.height);
		//PositionToDraw.x = Mathf.Abs(PositionToDraw.x-Screen.width);

		//PositionToDraw = transform.InverseTransformPoint (PositionToDraw);

		//PositionToDraw -= new Vector2(gameObject.GetComponent<RectTransform>().position.x, gameObject.GetComponent<RectTransform>().position.y);
		
		// now get the scaled size
		Vector3[] MyCorners = new Vector3[4];
		gameObject.GetComponent<RectTransform>().GetWorldCorners (MyCorners);
		Vector2 OldSize = gameObject.GetComponent<RectTransform>().sizeDelta;
		Vector2 ScaledSize = new Vector2(MyCorners[0].x,MyCorners[0].y);
		for (int i = 1; i < 4; i++) {
			if (MyCorners[i].x != ScaledSize.x) {
				if (MyCorners[i].x > ScaledSize.x)
					ScaledSize.x = MyCorners[i].x-ScaledSize.x;
				else
					ScaledSize.x = ScaledSize.x-MyCorners[i].x;
				break;
			}
		}
		for (int i = 1; i < 4; i++) {
			if (MyCorners[i].y != ScaledSize.y) {
				if (MyCorners[i].y > ScaledSize.y)
					ScaledSize.y = MyCorners[i].y-ScaledSize.y;
				else
					ScaledSize.y = ScaledSize.y-MyCorners[i].y;
				break;
			}
		}
		Debug.Log("ScaledSize: " +  ScaledSize.ToString() + " -OldSize: " + OldSize.ToString());

		PositionToDraw += ScaledSize/2f;

		PositionToDraw.y = ScaledSize.y-PositionToDraw.y;	// flip y
		PositionToDraw.x = ScaledSize.x-PositionToDraw.x;	// flip y

		PositionToDraw.x = Mathf.RoundToInt ((PositionToDraw.x/ScaledSize.x)*OldSize.x);
		PositionToDraw.y = Mathf.RoundToInt ((PositionToDraw.y/ScaledSize.y)*OldSize.y);
		//Debug.LogError(PositionToDraw.ToString());
*/