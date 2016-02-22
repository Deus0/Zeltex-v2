using UnityEngine;
using System.Collections;

public class AnimateButton : MonoBehaviour {
	private GameObject MyCanvas;
	public Vector3 BeginPosition;
	public Vector3 EndPosition;
	public Vector3 DifferencePosition;

	public Vector3 MoveToPosition;
	public bool IsForward = true;
	public bool IsAnimating;
	public float FramesPerSecond;
	public float Speed;
	public bool IsLoop;
	public bool IsLerp;
	float Threshold = 0.5f;
	public Vector3 MyPosition;
	public bool IsLeftAlligned;
	public bool IsRightAlligned;
	public float RowIndex = 0;
	public float PixelWidth = 100f;
	public float PixelHeight = 25f;
	public float MarginY = 1.5f;

	// Use this for initialization
	void Start () {
		MyCanvas = GameObject.Find ("MyCanvas");
		ResetPositions ();
	}
	public void ResetPositions() {
		//BeginPosition = transform.position;
		if (IsLeftAlligned) {
			BeginPosition.x = PixelWidth;
			MoveToPosition = new Vector3(-PixelWidth*2f,0,0);
		} else if (IsRightAlligned) {
			//BeginPosition.x = Camera.main.pixelWidth+PixelWidth;
			RectTransform MyCanvasRect = MyCanvas.GetComponent<RectTransform>();
			BeginPosition.x = MyCanvasRect.GetWidth()-PixelWidth;
			MoveToPosition = new Vector3(PixelWidth*2f,0,0);
		}
		//Debug.LogError ("Camera.main.pixelWidth: " + Camera.main.pixelWidth);
		BeginPosition.y = -300 - PixelHeight*RowIndex * MarginY;
		EndPosition = BeginPosition + MoveToPosition;
		DifferencePosition = BeginPosition - EndPosition;
		DifferencePosition.x = Mathf.Abs(DifferencePosition.x);

	}
	// Update is called once per frame
	void Update () {
		if (IsAnimating) {
			ResetPositions ();
			RectTransform MyRectTransform = gameObject.GetComponent<RectTransform>();
			MyPosition = MyRectTransform.anchoredPosition;

			Vector3 TargetPosition;
			Vector3 PreviousPosition;
			if (IsForward) {
				TargetPosition = EndPosition;
				PreviousPosition = BeginPosition;
			} else {
				TargetPosition = BeginPosition;
				PreviousPosition = EndPosition;
			}
			if (IsLerp)
				MyPosition = Vector3.Lerp(MyPosition,TargetPosition,Time.deltaTime*Speed);
			else {
				float Direction = 1;
				if (IsForward)
					Direction = -1;
				MyPosition.x += Direction*Speed*Time.deltaTime*(DifferencePosition.x);
				//MyPosition = Vector3.Slerp(MyPosition,TargetPosition,Time.deltaTime*AnimationSpeed);
				if (!IsForward)
					MyPosition.x = Mathf.Clamp (MyPosition.x, PreviousPosition.x, TargetPosition.x);
				else
					MyPosition.x = Mathf.Clamp (MyPosition.x,  TargetPosition.x, PreviousPosition.x);
			}
			if (MyPosition.x > TargetPosition.x-Threshold && MyPosition.x < TargetPosition.x+Threshold)
				MyPosition.x = TargetPosition.x;
			MyRectTransform.anchoredPosition = MyPosition;
			if (MyPosition == TargetPosition) {
				IsForward = !IsForward;
				if (!IsLoop)
					IsAnimating = false;
			}
		}
	}
}
