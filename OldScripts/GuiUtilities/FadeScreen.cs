using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour {
	public float InitialPause = 3f;
	public float TimeToFade = 6f;
	public string Label = "Day";
	int Index = 0;
	bool IsFading = true;
	float TimeStarted = 0;
	Color32 MyBackgroundColor;
	Color32 MyFontColor;

	// Use this for initialization
	void Start () {
	}
	void Awake() {
		MyBackgroundColor = gameObject.GetComponent<RawImage> ().color;
		MyFontColor = gameObject.transform.GetChild(0).gameObject.GetComponent<Text> ().color;
		ResetFade ();
	}
	// Update is called once per frame
	void Update () {
		if (IsFading) {
			AnimateFade();
		}
	}
	public void ResetFade() {
		Index++;
		IsFading = true;
		TimeStarted =  Time.time + InitialPause;
		gameObject.transform.GetChild (0).gameObject.GetComponent<Text> ().text = Label + " " + Index;
	}
	public void AnimateFade() {
		float TimePassed = (Time.time - TimeStarted) / TimeToFade;
		int FadeValue = Mathf.RoundToInt (Mathf.Lerp (255, 0, TimePassed));
		Color32 NewBackgroundColor = new Color32 (MyBackgroundColor.r, MyBackgroundColor.g, MyBackgroundColor.b,
		                                          (byte)(FadeValue));
		gameObject.GetComponent<RawImage> ().color = NewBackgroundColor;
		Color32 NewFontColor = new Color32 (MyFontColor.r, MyFontColor.g, MyFontColor.b,
		                                    (byte)(FadeValue));
		gameObject.transform.GetChild (0).gameObject.GetComponent<Text> ().color = NewFontColor;
		if (FadeValue == 0) {
			IsFading = false;
			gameObject.SetActive (false);	// purely for raycasting at the moment.. will change this later
		}
	}
	}
	