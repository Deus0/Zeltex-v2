/*
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Button : MonoBehaviour {
	public Texture StaticButton;
	public Texture MouseOverButton;
	public bool IsMouseOver = false;
	public Button MyButton;

	void Start() {

	}

	void OnGUI() {
		if (Event.current.type.Equals(EventType.Repaint))
			if (!IsMouseOver) 
				Graphics.DrawTexture(new Rect(transform.position.x, transform.position.y, MyBoxCollider.size.x, MyBoxCollider.size.y), StaticButton);
			else
				Graphics.DrawTexture(new Rect(10, 10, 100, 100), MouseOverButton);
	}
	void OnMouseEnter() {
		IsMouseOver = true;
	}
	void OnMouseExit() {
		IsMouseOver = false;
	}
}
*/