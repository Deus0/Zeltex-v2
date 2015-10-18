using UnityEngine;
using System.Collections;

public class MiniMapCharacter : MonoBehaviour {
	GameObject MyPlayer;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (MyPlayer == null) {
			MyPlayer = GetManager.GetCharacterManager ().GetLocalPlayer ().gameObject;
		} else {
			float RotY = MyPlayer.transform.eulerAngles.y;
			gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, RotY+180f+60f);
			//gameObject.transform.eulerAngles = new Vector3(0, 0, RotY+180f);
		}
	}
}
