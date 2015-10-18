using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 
using System.IO;

// To do
//		add a button listener to the no event
// 
// Seperate the 'GuiList' class, from the FileManager
// FileManager should extend GuiList or use it

public class GameEvent {
	public int EventIndex = 0;	// 0 for new patient, 1 for new guard etc
	public string EventText;
	//public float CashBonus;

	public GameEvent () 
	{

	}
};

public class EventsManager : MonoBehaviour {
	public string FileLocation = "";
	public List<GameEvent> MyEvents = new List<GameEvent>();
	public List<GameObject> MyEventGuis = new List<GameObject>();
	public GameObject EventGuiPrefab;
	public float MarginY = 5f;
	public GameObject MyScrollBar;
	public UnityEngine.Events.UnityEvent MyClickAction;
	// file only stuff
	public Text MyLabel;
	public List<Color32> FileColors = new List<Color32> ();
	public List<string> KnownFileExtensions = new List<string> ();
	public Color32 FolderColor = new Color32(5,5,5,255);
	public Color32 FileColor = new Color32(55,5,5,255);

	// Use this for initialization
	void Start () {
		if (IsFileSystem) {
			DefaultFileColours ();
			FileLocation = Path.GetFullPath (".");
			FileLocation += "\\SavedData";
			AddFileSystem ();
		}
	}
	public int SelectedIndex;
	public string GetSelectedText() {
		return MyEvents [SelectedIndex].EventText;
	}
	public void AddFileSystem() {
		ClearAll ();
		//Debug.LogError("Reading from " + FileLocation);
		MyLabel.text = FileLocation;
		//string[] MyFiles = Directory.GetFileSystemEntries(FileLocation); 
		DirectoryInfo info = new DirectoryInfo(FileLocation);
		FileSystemInfo[] MyFiles = info.GetFileSystemInfos();
		//Debug.LogError ("Found : " + MyFiles.Length);
		for (int i = 0; i < MyFiles.Length; i++) {
			if (!IsFilePathAFolder(MyFiles[i].FullName)) {
				NewEvent(Path.GetFileName(MyFiles[i].FullName));
			}
		}
		for (int i = 0; i < MyFiles.Length; i++) {
			if (IsFilePathAFolder(MyFiles[i].FullName)) {
				NewEvent(Path.GetFileName(MyFiles[i].FullName));
			}
		}
		ResetGui ();
	}
	public bool IsFilePathAFolder(string MyFilePath) {
		return System.IO.File.Exists (MyFilePath);
	}
	public bool IsFileSystem = true;
	// Update is called once per frame
	void Update () {
		if (IsFileSystem) {
			if (Input.GetKeyDown (KeyCode.Backspace) && FileLocation != "C:\\"&& FileLocation != "C:") {
				FileLocation = Path.GetDirectoryName(FileLocation);
				AddFileSystem();
			}
		}
		if (HasUpdated) {
			ResetGui();
			HasUpdated = false;
		}
	}

	// Initial cost for employees is one weeks pay
	public void NewEvent(int EventIndex) {
		GameEvent NewEvent = new GameEvent ();
		NewEvent.EventIndex = EventIndex;
		switch (EventIndex) {
			case (0):
				NewEvent.EventText = ".....";
				break;
		}
		MyEvents.Add (NewEvent);
		HasUpdated = true;
	}
	bool HasUpdated = false;
	public void NewEvent(string EventText) {
		GameEvent NewEvent = new GameEvent ();
		NewEvent.EventIndex = MyEvents.Count-1;	// unique identifier
		NewEvent.EventText = EventText;
		MyEvents.Add (NewEvent);
		HasUpdated = true;
	}
	
	// Removes all the EventGuis from the scene
	public void Clear() {
		for (int i = 0; i < MyEventGuis.Count; i++) {
			Destroy (MyEventGuis[i]);
		}
		MyEventGuis.Clear ();
	}
	public void ClearAll() {
		MyEvents.Clear ();
		Clear ();
	}
	// Updates the Gui Rect Transform, the size, and updates the positions of all the events in it
	public void ResetGui() {
		Clear ();
		float SizeY = EventGuiPrefab.GetComponent<RectTransform> ().sizeDelta.y;
		float NewHeight = MyEvents.Count * (SizeY + MarginY)+MarginY;
		float MinHeight = -1;

		MinHeight = gameObject.transform.parent.parent.GetComponent<RectTransform> ().sizeDelta.y;	// size of the actual window

		if (NewHeight < MinHeight) {
			NewHeight = MinHeight;
			MyScrollBar.SetActive(false);
			Debug.LogError("BAM");
		} else {
			MyScrollBar.SetActive(true);
		}

		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector3(gameObject.GetComponent<RectTransform>().sizeDelta.x,NewHeight,0);

		// Now create teh Event Guis
		for (int i = 0; i < MyEvents.Count; i++) 
		{
			GameObject NewEvent = (GameObject) Instantiate(EventGuiPrefab, new Vector3(), Quaternion.identity);
			NewEvent.name = "Event " + i;
			NewEvent.transform.SetParent(gameObject.transform, false);
			NewEvent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,-MarginY-SizeY/2f-i*(SizeY+MarginY),0);
			MyEventGuis.Add (NewEvent);
		}
		ResetButtonListeners ();
	}
	// Adds the functions to the buttons, so the buttons close when yes or no is pressed, and if yes is pressed they do extra things
	public void ResetButtonListeners() {
		for (int i = 0; i < MyEventGuis.Count; i++) 
		{	
			// as it passes by references, it needs its own copy
			MyEventGuis[i].name = "Event " +  i.ToString();
			
			int ClickEventIndex = i;
			UnityEngine.Events.UnityAction YesAction = () =>  
			{
				//Debug.LogError ("Yes Event: " + EventIndex);
				HandleClick(ClickEventIndex);
				MyClickAction.Invoke();
			};
			MyEventGuis[i].GetComponent<Button>().onClick.AddListener(YesAction);

			MyEventGuis[i].transform.FindChild ("EventLabel").GetComponent<Text>().text = MyEvents[i].EventText;
			if (IsFilePathAFolder(FileLocation + "\\" + MyEvents[i].EventText)) {
				string FileExtension = Path.GetExtension(MyEvents[i].EventText);
				MyEventGuis[i].GetComponent<RawImage>().color = GetFileExtensionColor(FileExtension);
			} else {	// else folder
				MyEventGuis[i].GetComponent<RawImage>().color = FolderColor;
			}
		}
	}
	public Color32 GetFileExtensionColor(string MyFileExtension) {
		for (int i = 0; i < FileColors.Count; i++) {
			if (MyFileExtension == KnownFileExtensions[i]) {
				return FileColors[i];
			}
		}
		return FileColor;
	}
	// this just removes the event - activated on both buttons
	public void RemoveEvent(int EventIndex) {
		//Debug.LogError ("Removing: " + EventIndex);
		for (int i = 0; i < MyEventGuis.Count; i++) {
			if (MyEventGuis[i].name == "Event " + EventIndex.ToString()) {
				if (MyEventGuis[i])
					Destroy (MyEventGuis[i]);
				MyEventGuis.RemoveAt (i);
				if (i >= 0 && i < MyEvents.Count)
					MyEvents.RemoveAt (i);
				i = MyEventGuis.Count;		// break the loop
			}
		}
		ResetGui ();
	}
	public Texture2D TestTexture;
	public MyWindow TestWindow;
	
	public void DefaultFileColours (){
		KnownFileExtensions.Clear ();
		FileColors.Clear ();
		// for .chk	- for chunks
		KnownFileExtensions.Add(".chk");
		FileColors.Add (new Color32 (55, 105, 55, 255));
		
		// for .chr	- for characters
		KnownFileExtensions.Add(".chr");
		FileColors.Add (new Color32 (172, 142, 77, 255));
		
		// for .png	- for characters
		KnownFileExtensions.Add(".png");
		FileColors.Add (new Color32 (77, 142, 172, 255));
	}
	// This is what happens when an event is activated
	public void HandleClick(int EventIndex) {
		SelectedIndex = EventIndex;
		if (IsFileSystem) {
			HandleClickFileSystem (EventIndex);
		} else {
			//Network.Connect (GetManager.GetNetworkManager().MyHostData [EventIndex]);
			//GetManager.GetNetworkManager().ConnectToHost();
			MyClickAction.Invoke();
		}
	}
	public void HandleClickFileSystem(int EventIndex) {
		if (EventIndex >= 0 && EventIndex < MyEvents.Count) {
			GameEvent MyEvent = MyEvents [EventIndex];
			string MyFilePath = FileLocation + "\\" + MyEvent.EventText;
			if (!IsFilePathAFolder (MyFilePath)) {
				FileLocation = MyFilePath;
				AddFileSystem ();
			} else {	// if a file
				string FileExtension = Path.GetExtension (MyEvent.EventText);
				switch (FileExtension) {
				case (".png"):
					// open it in a window heree
					TestTexture = new Texture2D (1, 1);
					TestTexture.LoadImage (File.ReadAllBytes (MyFilePath));	// auto resizes it!
					TestWindow = new MyWindow (TestTexture);
					TestWindow.Name = MyEvent.EventText;
					// maybe create a max, and use scrollbars and mask for the images
					GetManager.GetGuiCreator ().CreateWindow (TestWindow, GetManager.GetGuiCreator ().MyWindows.Count);
					TestWindow.WindowReference.transform.GetChild (1).gameObject.AddComponent<TextureEditorHandler> ();
					break;
				case (".vmd"):	// voxel file
					// open it in a window heree
					GameObject.Find ("MyModelEditorManager").GetComponent<ModelEditorManager> ().LoadModel (MyEvent.EventText);	// or maybe create a new window linked to that model?
					// maybe create a max, and use scrollbars and mask for the images
					//GetManager.GetGuiCreator().CreateWindow(TestWindow, GetManager.GetGuiCreator().MyWindows.Count);
					break;
					
				case (".chk"):	// chunk file
					List<string> FolderNames = new List<string> ();
					int NameCount1 = 0;
					
					for (int i = 0; i < FileLocation.Length; i++) {
						if (FileLocation [i] == '\\') {
							FolderNames.Add (FileLocation.Substring (NameCount1, i - NameCount1));
							NameCount1 = i + 1;
						}
					}
					string NewGameName = "";
					if (FolderNames.Count >= 2)
						NewGameName = FolderNames [FolderNames.Count - 2];
					if (NewGameName != "") {
						GetManager.GetGameManager ().GameName = NewGameName;
						GetManager.GetGameManager ().PlayAdventureGame ();
					}
					// open it in a window heree
					//GameObject.Find ("MyModelEditorManager").GetComponent<ModelEditorManager>().LoadModel(MyEvent.EventText);
					// maybe create a max, and use scrollbars and mask for the images
					//GetManager.GetGuiCreator().CreateWindow(TestWindow, GetManager.GetGuiCreator().MyWindows.Count);
					break;
				}
			}
		}
	}
}
