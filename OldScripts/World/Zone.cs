using UnityEngine;
using System.Collections;

namespace OldCode {
public class Zone : MonoBehaviour {
	public Color32 NormalColor = new Color32(0,255,255,125);
	public Color32 HighlightedColor = new Color32(255,0,0,125);
	public Color32 SelectedColor;
	bool IsMouseOver;
	bool UpdateMouseOver;
	public Vector3 InBlockPosition;		// gets the position it is in
	public Vector3 Size;

	public bool IsSpawnZone;
	public int ClanOwnerIndex;			// the owner of the zone can put rules on it, like can clan only edit etc

	//public BlockStructure MyStructure;	// structured stored in the zone	-- used to check if entire building has been destroyed
	// Use this for initialization
	void Start () {
		UpdateColor(NormalColor);
	}
	void Awake() {
		if (IsSpawnZone) {
			gameObject.AddComponent<MonsterSpawn>();
		}
	}
	// Update is called once per frame
	void Update () {
		if (UpdateMouseOver != IsMouseOver) {
			IsMouseOver = UpdateMouseOver;
			if (IsMouseOver)
				UpdateColor (HighlightedColor);
			else
				UpdateColor(NormalColor);
		}
		UpdateMouseOver = false;	// every frame set mouseOver to false
	}
	public void MouseOver() {
		UpdateMouseOver = true;
	}
	public void UpdateColor(Color32 NewColor) {
		gameObject.GetComponent<MeshRenderer> ().material.color = NewColor;;
	}
}
}
