using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActiveStateToggler : MonoBehaviour {
	public Button MyButton;
	private Color32 WindowOpenColor = new Color32(0,0,0,255);
	private Color32 WindowClosedColor = new Color32(155,0,0,255);
	public bool IsDestroyOnClose = false;
	public void CheckForClose() {
		if (IsDestroyOnClose) {
			Destroy (gameObject);
		} else {
			gameObject.SetActive (false);	// set true until animation scales down
		}
	}
	public void ToggleActive () {
			gameObject.SetActive (!gameObject.activeSelf);
			if (gameObject.activeSelf)
				gameObject.transform.SetAsLastSibling ();
			else {
				if (gameObject.GetComponent<GuiScaleAnimator> () != null) {
					gameObject.SetActive (true);	// set true until animation scales down
					gameObject.GetComponent<GuiScaleAnimator> ().ResetAnimationClose();
				}
			}
	}
	public void UpdateTogglerColours() {
		if (MyButton != null) {
			ColorBlock MyColorBlock = MyButton.colors;
			if (!gameObject.activeSelf) {
				MyColorBlock.normalColor = WindowOpenColor;
				MyColorBlock.highlightedColor = new Color32((byte)(WindowOpenColor.r + 25),
				                                            (byte)(WindowOpenColor.g + 25),
				                                            (byte)(WindowOpenColor.b + 25),
				                                            255);
			} else {
				MyColorBlock.normalColor = WindowClosedColor;
				MyColorBlock.highlightedColor = new Color32((byte)(WindowClosedColor.r + 25),
				                                            (byte)(WindowClosedColor.g + 25),
				                                            (byte)(WindowClosedColor.b + 25),
				                                            255);
			}
			MyButton.colors = MyColorBlock;
		}
	}
	public void OnEnable() {
		if (gameObject.GetComponent<GuiScaleAnimator> () != null) {
			gameObject.SetActive (true);	// set true until animation scales down

			gameObject.GetComponent<GuiScaleAnimator> ().MyFinishAnimationAction.AddListener(
			() =>  {
				CheckForClose();
			});
		}
		UpdateTogglerColours ();
	}
	public void OnDisable() {
		UpdateTogglerColours ();
	}
	public void SetOpenCloseColors(Color32 newWindowOpenColor, Color32 newWindowClosedColour) {
		WindowOpenColor = newWindowOpenColor;
		WindowClosedColor = newWindowClosedColour;
	}
}
