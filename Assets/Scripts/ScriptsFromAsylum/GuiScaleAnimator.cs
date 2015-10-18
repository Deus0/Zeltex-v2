using UnityEngine;
using System.Collections;

public class GuiScaleAnimator : MonoBehaviour {
	float BeginTime;
	RectTransform MyRectTransform;
	public Vector2 BeginScale = new Vector2(0,0);
	public Vector2 EndScale = new Vector2(1,1);
	public float AnimationSpeed = 1;
	public bool AnimationDirection;
	public Vector2 OriginalPosition;
	public Vector2 OriginalSize;
	public bool IsAnimating = true;
	public UnityEngine.Events.UnityEvent MyBeginAnimationAction;
	public UnityEngine.Events.UnityEvent MyFinishAnimationAction;

	// Use this for initialization
	void Start () {
		MyRectTransform = gameObject.GetComponent<RectTransform> ();
		//ResetAnimationOpen ();
		OriginalSize = MyRectTransform.sizeDelta;
		OriginalPosition = MyRectTransform.anchoredPosition;
		ResetAnimationOpen ();
	}
	
	public void OnEnable() {
		ResetAnimationOpen ();
	}

	public void ResetAnimationOpen() {
		MyRectTransform = gameObject.GetComponent<RectTransform> ();
		BeginTime = Time.time;
		MyRectTransform.localScale = BeginScale;
		AnimationDirection = true;
		IsAnimating = true;
		OriginalPosition = MyRectTransform.anchoredPosition;
	}

	public void ResetAnimationClose() {
		BeginTime = Time.time;
		MyRectTransform.localScale = EndScale;
		AnimationDirection = false;
		IsAnimating = true;
	}

	// Update is called once per frame
	void Update () {
		if (Time.timeScale != 0 && IsAnimating) 
		{
			float TimePassed = Time.time - BeginTime;
			TimePassed *= AnimationSpeed;
			Vector2 NewScale;
			if (AnimationDirection)
			{
				NewScale = new Vector2 (Mathf.Lerp (BeginScale.x, EndScale.x, TimePassed), 
			                               Mathf.Lerp (BeginScale.y, EndScale.y, TimePassed));
			}
			else
			{
				NewScale = new Vector2 (Mathf.Lerp (EndScale.x, BeginScale.x, TimePassed), 
				                        Mathf.Lerp (EndScale.y, BeginScale.y, TimePassed));
			}

			MyRectTransform.localScale = NewScale;
			MyRectTransform.anchoredPosition = OriginalPosition + new Vector2((1f-NewScale.x)*OriginalSize.x/2f,-(1f-NewScale.y)*OriginalSize.y/2f);	// move position by rescaling
			// if has shrunk to nothingness
			if (NewScale == new Vector2(0,0) && !AnimationDirection) {
				MyRectTransform.anchoredPosition = OriginalPosition;
				MyFinishAnimationAction.Invoke();
			} 
			else if (AnimationDirection && NewScale == new Vector2(1,1)) 
			{
				IsAnimating = false;
				MyBeginAnimationAction.Invoke();
			}
		}
	}
}
