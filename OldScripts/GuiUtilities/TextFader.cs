using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFader : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!gameObject.transform.GetChild(0).gameObject.GetComponent<AnimateButton>().IsAnimating) {
			Color32 MyColor = gameObject.GetComponent<Image>().color;
			MyColor = Color32.Lerp(MyColor, new Color32(MyColor.r, MyColor.g, MyColor.b, 0), Time.deltaTime);
			gameObject.GetComponent<Image>().color = MyColor;
		}
	}
}
