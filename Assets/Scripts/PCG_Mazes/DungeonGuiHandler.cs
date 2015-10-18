using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// interface class between the dungeon generator class and the gui

public class DungeonGuiHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,IPointerUpHandler {
	public Maze MyMaze;
	//public Texture MyTexture;
	public bool HasGenerated = false;
	public bool IsActive = false;
	public int SelectedMazeIndex = 0;
	public bool IsGenerateOnClick = true;
	// Use this for initialization
	void Start () {
		MyMaze = GetManager.GetDataManager ().MazeList [SelectedMazeIndex];
		gameObject.GetComponent<RawImage> ().texture = MyMaze.GetMazeTexture();
	}
	
	// Update is called once per frame
	void Update () {
		if (IsActive) {
			CheckInput ();
		}
	}

	public void UpdateGuiWithData(GameObject ParentGui) {

	}
	public void ChangeIsPathways(Toggle NewData) {
		MyMaze.bIsPathways = NewData.isOn;
	}
	public void ChangeMirrorX(Toggle NewData) {
		MyMaze.bIsMirrorX = NewData.isOn;
	}
	public void ChangeMirrorY(Toggle NewData) {
		MyMaze.bIsMirrorY = NewData.isOn;
	}
	public void ChangeMirrorZ(Toggle NewData) {
		MyMaze.bIsMirrorZ = NewData.isOn;
	}
	public void ChangeSparceness(InputField NewData) {
		float NewDataFloat = float.Parse(NewData.text);
		NewDataFloat = Mathf.Clamp (NewDataFloat, 0, 100);
		MyMaze.Sparceness = NewDataFloat;
		NewData.text = NewDataFloat.ToString ();
	}
	public void ChangeLinearity(InputField NewData) {
		float NewDataFloat = float.Parse(NewData.text);
		NewDataFloat = Mathf.Clamp (NewDataFloat, 0, 100);
		MyMaze.Linearity = NewDataFloat;
		NewData.text = NewDataFloat.ToString ();
	}
	public void ChangeVariable(Toggle NewData) {
		string VariableName = NewData.gameObject.name;
		switch (VariableName) {
			case ("IsMirrorX"):
				MyMaze.bIsMirrorX = NewData.isOn;
				break;
			case ("IsMirrorY"):
				MyMaze.bIsMirrorY = NewData.isOn;
				break;
			case ("IsMirrorZ"):
				MyMaze.bIsMirrorZ = NewData.isOn;
				break;
			case ("IsHallways"):
			//	MyMaze.IsHallways = NewData.isOn;
				break;
		}

	}
	public void ChangeVariable(InputField NewData) {
		string VariableName = NewData.gameObject.name;
		float NewDataFloat = LimitFloat (NewData.text);
		switch (VariableName) {
		case ("RoomsMinimum"):
			MyMaze.MinimumRooms = (int)NewDataFloat;

			break;
		case ("RoomsMaximum"):
			MyMaze.MaximumRooms = (int)NewDataFloat;
			break;
		case ("PathSize"):
			
			break;
		case ("RoomSizeX"):
			MyMaze.RoomSize.x = NewDataFloat;
			break;
		case ("RoomSizeZ"):
			MyMaze.RoomSize.z = NewDataFloat;
			break;
		}
		NewData.text = NewDataFloat.ToString();
	}
	public float LimitFloat(string MyFloat) {
		float NewDataFloat = float.Parse(MyFloat);
		NewDataFloat = Mathf.Clamp (NewDataFloat, 1, 100);
		return NewDataFloat;
	}
	public void ChangePathSize(InputField NewData) {
		float NewDataFloat = float.Parse(NewData.text);
		NewDataFloat = Mathf.Clamp (NewDataFloat, 1, 100);
		NewData.text = NewDataFloat.ToString();	// our gui +1
		NewDataFloat--;
		MyMaze.PathSizeMinimum = new Vector3(NewDataFloat,0,NewDataFloat);
	}
	public void GenerateMaze() {
		//Debug.LogError ("Select MapTexture: " + Time.time);
		if (MyMaze != null) {
			if (!HasGenerated) {
				//HasGenerated = true;
				Debug.Log ("GeneratedDungeon: " + Time.time);
				MyMaze.GenerateDungeon ();
				gameObject.GetComponent<RawImage> ().texture = MyMaze.GetMazeTexture ();
			}
		}
		gameObject.transform.parent.FindChild ("SparcePercentageLabel").GetChild (0).gameObject.GetComponent<Text> ().text = 
			((int)(MyMaze.GetSparcePercent ()*100f)).ToString() + " %";
	}
	public void CheckInput() {
		if (Input.GetKeyDown(KeyCode.B)) {
			SelectedMazeIndex++;
			if (SelectedMazeIndex >= GetManager.GetDataManager ().MazeList.Count)
				SelectedMazeIndex = 0;

			MyMaze = GetManager.GetDataManager ().MazeList [SelectedMazeIndex];
			gameObject.GetComponent<RawImage> ().texture = MyMaze.GetMazeTexture();
		}
	}

	public void OnPointerUp(PointerEventData data) {

	}
	public void OnPointerDown (PointerEventData data) {
		if (IsGenerateOnClick) {
			GenerateMaze();
		}
	}

	public void OnPointerEnter(PointerEventData eventData) {
		IsActive = true;
		//Debug.LogError ("Mouse Entering MapTexture: " + Time.time);
	}

	public void OnPointerExit(PointerEventData eventData) {
		IsActive = false;
		//Debug.LogError ("Mouse Exiting MapTexture: " + Time.time);
	}

	public void OnSelect(BaseEventData eventData) {
	}
}
