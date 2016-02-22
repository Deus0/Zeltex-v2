using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WorldEditor : EditorWindow
{
	private static bool IsEnabled = false;
	public static bool IsOn = true;
	public static GUISkin MySkin = null;
	//private static SceneView MySceneView;
	
	static void Init ()
	{
		Enable ();
	}
	[MenuItem("Marz/WorldEditor")]
	public static void Enable()
	{
		Debug.Log ("WorldEditor: Enabled");
		if (EditorVoxels.MyWorld == null) {
			EditorVoxels.MyWorld = GameObject.FindObjectOfType<VoxelEngine.World> ();
		}
		SceneView.onSceneGUIDelegate += OnScene;
		VoxelUpdaterGUI.Enable();
		
		WorldEditor MyWindow = 
			(WorldEditor)EditorWindow.GetWindow(typeof(WorldEditor));
	}
	
	/*[MenuItem("Marz/Disable Tools")]*/
	public static void Disable()
	{
		Debug.Log ("WorldEditor: Disabled");
		SceneView.onSceneGUIDelegate -= OnScene;
	}

	//[MenuItem("Marz/Modes/Enable Voxel Mode")]
	public static void EnableVoxels()
	{
		EditorVoxels.SetVoxelMode (true);
	}
	
	//[MenuItem("Marz/Modes/Disable Voxel Mode Voxels")]
	public static void DisableVoxels()
	{
		EditorVoxels.SetVoxelMode (false);
	}
	void OnGUI() {
		
		// scene view mouse position
		
		Handles.BeginGUI();
		UpdateSkin();
		//Rect MyRenderRect = new Rect(0,0,350,400);
		//GUI.Label (MyRenderRect, "");
		EditorVoxels.SetVoxelMode (GUILayout.Toggle (EditorVoxels.IsEditVoxels, "Voxel Edit Mode"));
		if (!EditorVoxels.GetVoxelEditMode ()) {
			LevelSelectGui.MapSelectionGui ();
		} else {
			EditorVoxels.VoxelGui ();
		}

		LevelSelectGui.MapSelectionGui2 ();
		Handles.EndGUI();
	}
	private static void OnScene(SceneView MySceneView)
	{
		EditorVoxels.RayTraceVoxels (MySceneView);
	}

	private static void UpdateSkin() 
	{
		if (MySkin == null) 
		{
			MySkin = Resources.Load("Photon Unity Network/Demos/Shared Assets/PhotonGUISkin",typeof(GUISkin)) as GUISkin;
		}
		if (MySkin != null) {
			GUI.skin = MySkin;
		}
	}
}
