using UnityEngine;
using System.Collections;

namespace OldCode {
public class OnHitText : MonoBehaviour {
	public GameObject PositionAtPlayer;
	public GameObject LookAtPlayer;
	public Vector3 RandomDifference;
	public float DifferenceFromHitPlayer = 0;
	public Vector3 OriginalPosition = new Vector3(0,0,0);
	Color32 OriginalColor;
	Color32 FadeColor;
	float Variation = 0.7f;
	float FadeDuration = 3f;
	float TimeStarted;
	// Use this for initialization
	void Start () {
		RandomDifference = new Vector3 (Random.Range (-Variation, Variation), Random.Range (0.5f, Variation+0.5f), Random.Range (-Variation, Variation));
		OriginalColor = gameObject.GetComponent<TextMesh> ().color;
		FadeColor = OriginalColor;
		FadeColor.a = 0;
		Destroy (gameObject,FadeDuration);
		TimeStarted = Time.time;
		gameObject.GetComponent<MeshRenderer> ().materials[0].shader = GetManager.GetDataManager ().MyTextShader;
	}
	void Awake() {
		//OriginalPosition = PositionAtPlayer.transform.position;
	}
	bool HasInitiated = false;
	// Update is called once per frame
	void Update () {
		if (!HasInitiated) {
			OriginalPosition = transform.position;
			HasInitiated = true;
		}
		if (LookAtPlayer != null) {
			DifferenceFromHitPlayer = Mathf.Lerp(DifferenceFromHitPlayer,4,Time.deltaTime);
			transform.position = OriginalPosition + new Vector3(0,DifferenceFromHitPlayer,0) + RandomDifference;
			transform.eulerAngles = Quaternion.LookRotation ( transform.position-LookAtPlayer.transform.position).eulerAngles; // we get the angle has to be rotated\
			Color32 NewColor = OriginalColor; 
			float lerp = Mathf.PingPong (Time.time-TimeStarted, FadeDuration) / FadeDuration;
			
			OriginalColor.a = (byte)Mathf.RoundToInt(255*Mathf.Lerp(1.0f, 0.0f, (Time.time-TimeStarted))) ;
			//NewColor.a = (byte)(Mathf.RoundToInt(Mathf.Lerp ((int)NewColor.a,0,Time.deltaTime)));
			gameObject.GetComponent<TextMesh>().color = NewColor;
			gameObject.GetComponent<MeshRenderer> ().materials[0].SetColor("_Color", NewColor);
		}
	}
}
}