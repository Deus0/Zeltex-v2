using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LevelSelectGui
{
	static public VoxelEngine.World MyWorld = null;
	static private Rect MyListRect;
	// Map Selector GUI
	private static List<string> MyMaps = new List<string>();
	static private GUIContent[] MyMaps2;
	static private GUIStyle MyButtonStyle;
	static private bool showList = false;
	static private int SelectedMap = 0;
	static private GUIStyle MyDropDownStyle;
	public static string NewWorldName = "";
	// state machine for the menus
	public static bool IsCreatingNewMap = false;
	private static bool IsRenaming = false;
	public static bool IsEditMode = false;
	private static bool CanRename = true;	// false when the new name already has a folder
	
	private static float ButtonHeight = 15;
	// Character List
	private static int SelectedCharacter = 0;
	private static List<string> MyCharacters = new List<string>();
	static private GUIContent[] MyCharacters2 = new GUIContent[0];
	private static Rect MyCharactersRect;
	private static bool IsShowCharactersList = false;

	//static private bool picked = false;
	static LevelSelectGui() 
	{
		MyWorld = GameObject.FindObjectOfType<VoxelEngine.World>();
		RefreshMapsList ();
		InitiateVariables ();
	}
	private static VoxelEngine.World GetWorld() {
		if (MyWorld == null)
			MyWorld = GameObject.FindObjectOfType<VoxelEngine.World>();
		return MyWorld;
	}
	
	public static Camera GetMainCamera() 
	{
		#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying) 
		{
			return Camera.main;
		}
		else 
		{
			return UnityEditor.SceneView.currentDrawingSceneView.camera;
			//return Camera.current;
			//return UnityEditor.SceneView.GetAllSceneCameras()[0];
		}
		#else
		return Camera.main;
		#endif
	}
	static void RefreshMapsList() 
	{
		MyMaps = GuiSystem.MapSelectGui.GetMapsList ();
		MyMaps2 = new GUIContent[MyMaps.Count];
		for (int i = 0; i < MyMaps.Count; i++)
		{
			MyMaps2 [i] = new GUIContent (MyMaps [i]);
		}

		NewWorldName = GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().SaveFileName;
		for (int i = 0; i < MyMaps.Count; i++) 
		{
			if (NewWorldName == MyMaps[i])
			{
				SelectedMap = i;
				i = MyMaps.Count;
				break;
			}
		}
	}
	
	private static void LoadCharactersList()
	{
		if (MyMaps.Count == 0)
			return;
		if (SelectedMap < 0)
			SelectedMap = 0;
		if (SelectedMap >= MyMaps.Count)
			SelectedMap = MyMaps.Count;
		string MyMapName = MyMaps [SelectedMap];
		MyCharacters = GuiSystem.MapSelectGui.GetCharactersList (MyMapName);
		if (MyCharacters.Count == 0)
			return;

		MyCharacters2 = new GUIContent[MyCharacters.Count];
		for (int i = 0; i < MyCharacters.Count; i++)
		{
			MyCharacters2 [i] = new GUIContent (MyCharacters [i]);
		}

		// Make some content for the popup list

	}

	// for the guistyles
	static void InitiateVariables()
	{
		// Make a GUIStyle that has a solid white hover/onHover background to indicate highlighted items
		MyDropDownStyle = new GUIStyle ();
		MyDropDownStyle.normal.textColor = Color.white;
		var WhiteTexture = new Texture2D (2, 2);
		var colors = new Color[4];
		for (int i = 0; i < colors.Length; i++) {
			colors [i] = Color.white;
		}
		WhiteTexture.SetPixels (colors);
		WhiteTexture.Apply ();
		MyDropDownStyle.hover.background = WhiteTexture;
		MyDropDownStyle.onHover.background = WhiteTexture;
		MyDropDownStyle.padding.left = MyDropDownStyle.padding.right = MyDropDownStyle.padding.top = MyDropDownStyle.padding.bottom = 4;
		
		var BlackTexture = new Texture2D (2, 2);
		for (int i = 0; i < colors.Length; i++) {
			colors [i] = Color.black;
		}
		BlackTexture.SetPixels (colors);
		BlackTexture.Apply ();
		MyDropDownStyle.normal.background = BlackTexture;
		
		MyButtonStyle = new GUIStyle ();
		MyButtonStyle.normal.background = BlackTexture;
		MyButtonStyle.normal.textColor = Color.white;
		MyButtonStyle.hover.background = WhiteTexture;
		MyButtonStyle.hover.textColor = Color.black;
	}
	private static bool HasLoadedCharacters = false;
	public static void MapSelectionGui()
	{
		if (!HasLoadedCharacters) 
		{
			HasLoadedCharacters = true;
			LoadCharactersList ();
		}
/*#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			GUILayout.Label ("");
			LevelSelectGui.MyWorld = (VoxelEngine.World)UnityEditor.EditorGUI.ObjectField (new Rect (3, GUILayoutUtility.GetLastRect ().y, 300, 18), 
			                                                                               "World", GetWorld(), typeof(VoxelEngine.World)) as VoxelEngine.World;
		}
#endif*/
		GUILayout.Label ("--Map Selection--", MyButtonStyle, GUILayout.Width(280));
		if (MyWorld == null) 
		{
			InitiateVariables ();
		}
		if (GetWorld () == null)
			return;
		if (IsCreatingNewMap)
		{
			CreateMapGui ();
		} else if (IsRenaming) 
		{
			WorldNameGui ();
		} else if (GetWorld().GetComponent<VoxelEngine.ChunkUpdater> () 
		           && GetWorld().GetComponent<VoxelEngine.ChunkUpdater> ().IsUpdating) {
			LoadGui ();
		} else if (IsEditMode) 
		{
			EditModeGui();
		}
		else
		{
			MainMenu();
		} 
		//GUILayout.Label ("");
	}

	public static void EditModeGui() 
	{
		if (GUILayout.Button("Back", MyButtonStyle, GUILayout.Width(80))) 
		{
			IsEditMode = false;
		}
		Vector2 Meh = GUILayoutUtility.GetLastRect ().position;
		if (GUI.Button (new Rect (Meh.x + 80, Meh.y, 80, ButtonHeight), "Save", MyButtonStyle)) 
		{
			//MyWorld.GetComponent<VoxelEngine.VoxelSaver>().LoadFromFile(MyMaps[SelectedMap]);
			GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().SaveToFile ();
		}
	}

	public static void LoadGui()
	{
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "", new GUIStyle ());
		int ChunkCount = GetWorld().GetComponent<VoxelEngine.ChunkUpdater> ().UpdateList.Count;
		if (ChunkCount != 0)
			GUILayout.Label ("Updating:\n" + 
				(ChunkCount)
				+ " Chunks Remaining.");
		else
			GUILayout.Label ("Updating Lights:\n" + 
			                 (GetWorld().GetComponent<VoxelEngine.ChunkUpdater> ().LightsUpdateList.Count)
				+ " Chunks Remaining.");
	}
	public static bool IsPlaying() {
#if UNITY_EDITOR
		return (UnityEditor.EditorApplication.isPlaying);
#else 
		return true;
#endif
	}
	public static void MainMenu() 
	{
		GUILayout.Label ("Loaded: " + GetWorld().GetComponent<VoxelEngine.VoxelSaver>().SaveFileName, MyButtonStyle, GUILayout.Width(200));
		GUILayout.Label ("Select Map:", MyButtonStyle, GUILayout.Width(100));
		GUILayout.Label ("");
		MyListRect = new Rect (GUILayoutUtility.GetLastRect ().x + 5, GUILayoutUtility.GetLastRect ().y, 300, 20);
		if (MyMaps2.Length > 0) 
		{
			Popup.ListLabel (MyListRect,
			                 MyMaps2 [SelectedMap],
			                 MyButtonStyle);
		}
		
		//GUILayout.Label ("");
		if (GUILayout.Button ("Load", MyButtonStyle, GUILayout.Width (80))) {
			string NewMapName = MyMaps [SelectedMap];
			GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().LoadFromFile (NewMapName);
			NewWorldName = NewMapName;
			LoadCharactersList();
		}
		Vector2 Meh = GUILayoutUtility.GetLastRect ().position;
		if (GUI.Button (new Rect (Meh.x + 120, Meh.y, 80, ButtonHeight), "Delete", MyButtonStyle)) {
			GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().DeleteFile ();
			RefreshMapsList ();
		}
		if (GUI.Button (new Rect (Meh.x + 240, Meh.y, 80, ButtonHeight), "Clone", MyButtonStyle)) {
			GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().CloneFile ();
			RefreshMapsList ();
		}
		
		if (GUILayout.Button ("New", MyButtonStyle, GUILayout.Width (80)))
		{
			IsCreatingNewMap = true;
		}
		Meh = GUILayoutUtility.GetLastRect ().position;
		if (GUI.Button (new Rect (Meh.x + 120, Meh.y, 80, ButtonHeight), "Rename", MyButtonStyle)) 
		{
			NewWorldName = MyMaps[SelectedMap];
			GetWorld().GetComponent<VoxelEngine.VoxelSaver>().SaveFileName = NewWorldName;
			//NewWorldName = ;
			IsRenaming = true;
		}
		if (GUI.Button (new Rect (Meh.x + 240, Meh.y, 80, ButtonHeight), "Edit", MyButtonStyle)) 
		{
			IsEditMode = true;
		}
		GUILayout.Label ("", MyButtonStyle, GUILayout.Width(1));
		GUILayout.Label ("Characters", MyButtonStyle, GUILayout.Width (100));
		if (GUILayout.Button ("Spawn", MyButtonStyle, GUILayout.Width (100)))
		{

		}
		Meh = GUILayoutUtility.GetLastRect ().position;
		if (GUI.Button (new Rect (Meh.x + 100, Meh.y, 100, ButtonHeight), "Delete", MyButtonStyle)) 
		{

		}
		if (GUI.Button (new Rect (Meh.x + 200, Meh.y, 100, ButtonHeight), "Clone", MyButtonStyle)) 
		{
			
		}

		if (MyCharacters2.Length > 0) 
		{
			GUILayout.Label("", GUILayout.Width(1));
			MyCharactersRect = new Rect (GUILayoutUtility.GetLastRect ().x + 5, GUILayoutUtility.GetLastRect ().y, 300, 20);
			Popup.ListLabel (MyCharactersRect,
			                 MyCharacters2 [SelectedCharacter],
			                 MyButtonStyle);
			if (GUILayout.Button("Zoom", MyButtonStyle, GUILayout.Width(100))) 
			{
				Transform MyCharacter = GetSelectedCharacter();
				if (MyCharacter)
				{
					Vector3 Direction = (GetMainCamera().transform.position-MyCharacter.transform.position).normalized;
					GetMainCamera().transform.LookAt(MyCharacter.transform.position);
					GetMainCamera().transform.position = MyCharacter.transform.position+Direction*3f;
				}
			}
			if (IsPlaying())
			if (GUILayout.Button ("Play As", MyButtonStyle, GUILayout.Width (100)))
			{
				PlayerSpawner MySpawner = GameObject.FindObjectOfType<PlayerSpawner>();
				if (MySpawner) 
				{
					MySpawner.UpdateSpawnedPlayer(GetSelectedCharacter());
				}
			}
			Meh = GUILayoutUtility.GetLastRect ().position;
			if (GUI.Button (new Rect (Meh.x + 200, Meh.y, 100, ButtonHeight), "New Character", MyButtonStyle))
			{
				PlayerSpawner MySpawner = GameObject.FindObjectOfType<PlayerSpawner>();
				if (MySpawner) 
				{
					MySpawner.SpawnPlayer();
				}
			}

			if (Popup.List (	MyCharactersRect, 
			               		ref IsShowCharactersList,
			                	ref SelectedCharacter, 
			               		 MyCharacters2 [SelectedCharacter], 
			               		 MyCharacters2,
				                MyButtonStyle,
				                MyDropDownStyle,
				                MyDropDownStyle)) 
				{
					
				}
		}
	}
	public static Transform GetSelectedCharacter() {
		GameObject MyCharacter = GameObject.Find(MyCharacters[SelectedCharacter]);
		if (MyCharacter)
			return MyCharacter.transform;
		else
			return null;
	}
	public static void WorldNameGui() 
	{
		if (!IsRenaming)
			return;
		string OldName = NewWorldName;
		NewWorldName = GUILayout.TextField (NewWorldName, GUILayout.Width (240));
		if (NewWorldName != OldName) 
		{
			CanRename = true;
			for (int i = 0; i < MyMaps.Count; i++) 
			{
				if (NewWorldName == MyMaps [i])
				{
					CanRename = false;
					i = MyMaps.Count;
				}
			}
		}
		if (GUILayout.Button("Cancel", MyButtonStyle, GUILayout.Width(80))) 
		{
			IsRenaming = false;
		}
		if (CanRename) {
			if (GUI.Button (new Rect (GUILayoutUtility.GetLastRect ().x + 240, GUILayoutUtility.GetLastRect ().y, 80, 12),
		               "Confirm", MyButtonStyle)) {
				GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().Rename (NewWorldName);
				RefreshMapsList ();
				IsRenaming = false;
			}
		} else {
			GUI.Label (new Rect (GUILayoutUtility.GetLastRect ().x + 120, GUILayoutUtility.GetLastRect ().y, 240, 12),
			            "Name Already In Use", MyButtonStyle);
		}
	}
	public static void CreateMapGui()
	{
		if (GUILayout.Button ("Back", MyButtonStyle, GUILayout.Width (80))) 
		{
			IsCreatingNewMap = false;
			GetWorld().GetComponent<VoxelEngine.VoxelSaver>().Clear();
		}
		GUILayout.Label ("");
		if (GUILayout.Button ("Clear", MyButtonStyle, GUILayout.Width (80))) 
		{
			GetWorld().GetComponent<VoxelEngine.VoxelSaver>().Clear();
		}
		if (GUILayout.Button ("AddTerrain", MyButtonStyle, GUILayout.Width (80))) 
		{
			GetWorld().GetComponent<VoxelEngine.VoxelTerrain> ().CreateTerrain();
		}
		if (GUILayout.Button ("AddDungeon", MyButtonStyle, GUILayout.Width (80))) 
		{
			GetWorld().GetComponent<VoxelEngine.RoomGenerator> ().GenerateAll();
		}
		WorldNameGui();
		if (GUILayout.Button ("Save", MyButtonStyle, GUILayout.Width (80))) 
		{
			GetWorld().GetComponent<VoxelEngine.VoxelSaver> ().CreateNewWorld (NewWorldName);
			RefreshMapsList ();
		}
	}
	public static void MapSelectionGui2() 
	{
		if (MyMaps2.Length > 0) {
			if (Popup.List (MyListRect, 
			                ref showList,
			                ref SelectedMap, 
			                MyMaps2 [SelectedMap], 
			                MyMaps2,
			                MyButtonStyle,
			                MyDropDownStyle,
			                MyDropDownStyle)) 
			{

			}
		}
	}
}