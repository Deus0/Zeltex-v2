using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BarGui : MonoBehaviour {
	int CurrentStat = 0;
	public int Stat = 1;
	public GameObject BarPrefab;
	public List<GameObject> SpawnedBars = new List<GameObject>();

	// Use this for initialization
	void Start () {
		GameObject Label = transform.GetChild (0).gameObject;
		gameObject.transform.DetachChildren ();

		float MaxWidth = gameObject.GetComponent<RectTransform>().sizeDelta.x;
		float BrickBarWidth = MaxWidth / 10f;
		for (int i = 0; i < 10; i++) {
			Vector3 NewBarPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
			NewBarPosition.x -= (BrickBarWidth*5f)-BrickBarWidth/2f;
			NewBarPosition.y = 0f;
			GameObject NewBarBlock = (GameObject)Instantiate(BarPrefab,NewBarPosition + new Vector3(i*BrickBarWidth,0,0), Quaternion.identity);
			NewBarBlock.transform.SetParent(gameObject.transform,false);
			SpawnedBars.Add (NewBarBlock);
		}

		Label.transform.SetParent (gameObject.transform);
		Label.gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 15);
		Label.transform.localScale = new Vector3 (1, 1, 1);
	}
	
	// Update is called once per frame
	void Update () {
		if (CurrentStat != Stat) {
			SetGui();
			CurrentStat = Stat;
		}
	}
	public void SetGui() {
		for (int i = 0; i < Stat; i++) {
			SpawnedBars[i].SetActive(true);
		}
		for (int i = Stat; i < 10; i++) {
			SpawnedBars[i].SetActive(false);
		}
	}
}
