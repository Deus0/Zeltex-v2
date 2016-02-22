using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace AnimationUtilities {

	// this just fades the text
	public class OnHitText : MonoBehaviour {
		public static float MoveToHeight = 2f;
		public Vector3 RandomDifference;
		public float DifferenceFromHitPlayer = 0;
		public Vector3 OriginalPosition = new Vector3(0,0,0);
		Color32 OriginalColor;
		Color32 FadeColor;
		float Variation = 0.5f;
		float FadeDuration = 5f;
		float TimeStarted;
		float TimeToStartFade;
		// Use this for initialization
		void Start () {
			RandomDifference = new Vector3 (Random.Range (-Variation, Variation), Random.Range (Variation, Variation), Random.Range (-Variation, Variation));
			OriginalColor = gameObject.GetComponent<TextMesh> ().color;
			FadeColor = OriginalColor;
			FadeColor.a = 0;
			TimeStarted = Time.time;
			TimeToStartFade = Time.time+1f;
			//gameObject.GetComponent<MeshRenderer> ().materials[0].shader = GetManager.GetDataManager ().MyTextShader;
			OriginalPosition = transform.position;
			Destroy (gameObject,FadeDuration);	// for now, make a on finish fade destroy it
		}

		bool HasInitiated = false;
		// Update is called once per frame
		void Update () {
			if (!HasInitiated) {
				OriginalPosition = transform.position;
				HasInitiated = true;
			}
			float LerpPercent = Mathf.Lerp(0.0f, 1.0f, (Time.time-TimeStarted) / (FadeDuration+1f));
			transform.position = OriginalPosition + new Vector3(0,MoveToHeight * LerpPercent,0) + RandomDifference*LerpPercent;

			Color32 NewColor = OriginalColor; 

			float AlphaLerp = Mathf.Lerp(1.0f, 0.0f, (Time.time-TimeToStartFade) / FadeDuration);
			OriginalColor.a = (byte)Mathf.RoundToInt(255*AlphaLerp);
			gameObject.GetComponent<TextMesh>().color = NewColor;
			gameObject.GetComponent<MeshRenderer> ().materials[0].SetColor("_Color", NewColor);
		}
	}
	public static class StatPopUp  {
		/*void CreateDamagePopup(GameObject LookAtPlayer, GameObject SpawnPosition, float Damage) {
			CreateDamagePopup(LookAtPlayer, SpawnPosition, Damage, 120);
		}
		void CreateDamagePopup(GameObject LookAtPlayer, GameObject SpawnPosition, float Damage, int FontSize) {
			GameObject NewText = new GameObject();
			NewText.transform.position = SpawnPosition.transform.position;
			NewText.name = "Damage Popup " + Time.time.ToString ();
			TextMesh MyText = (TextMesh) NewText.AddComponent<TextMesh>();
			MyText.text = Damage.ToString();
			MyText.fontStyle = FontStyle.Bold;
			MyText.fontSize = FontSize/4;
			NewText.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
			if (Damage > 0) {
				MyText.color = Color.red;
			} else {
				MyText.color = Color.green;
			}
			OnHitText MyOnHit = NewText.AddComponent<OnHitText>();
			MyOnHit.LookAtPlayer = Camera.main.gameObject;//LookAtPlayer;
			MyOnHit.PositionAtPlayer = SpawnPosition;
		}*/
		public static void CreateTextPopup(Vector3 SpawnPosition, float Damage) {
			Color32 MyColor = Color.red;
			if (Damage > 0)
				MyColor = Color.cyan;
			CreateTextPopup(SpawnPosition, Damage, 80+Random.Range(0,40), MyColor);
		}
		public static void CreateTextPopup(Vector3 SpawnPosition, float Damage, int FontSize, Color32 DamageColor) {
			GameObject NewText = new GameObject();
			NewText.transform.position = SpawnPosition;
			NewText.name = "Damage Popup " + Time.time.ToString ();
			TextMesh MyText = (TextMesh) NewText.AddComponent<TextMesh>();
			MyText.text = Damage.ToString();
			MyText.fontStyle = FontStyle.Bold;
			MyText.fontSize = FontSize;
			NewText.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
			MyText.color = DamageColor;
			OnHitText MyOnHit = NewText.AddComponent<OnHitText>();
			NewText.AddComponent<GUI3D.Billboard> ().IsLookAtPlayer = true;
		}
}
}
