using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class EditorVoxels 
{
	public static Transform MyHitObject = null;
	public static VoxelEngine.World MyWorld;
	public static Transform MyTargetWorld = null;
	// update block
	public static bool IsEditVoxels = false;
	private static float UpdateSize = 0;
	private static int BlockType = 2;
	private static GameObject MyCube = null;
	private static Camera SceneCamera;

	public static bool GetVoxelEditMode() 
	{
		return IsEditVoxels;
	}
	public static void VoxelGui() 
	{
		if (IsEditVoxels)
		{
			GUILayout.Label ("Press Escape to exit voxel edit mode");
			GUILayout.Label ("Size:");
			UpdateSize = float.Parse (GUILayout.TextField ("" + UpdateSize, GUILayout.Width (40)));
			GUILayout.Label ("BlockType:");
			BlockType = int.Parse (GUILayout.TextField ("" + BlockType, GUILayout.Width (40)));
			
			if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown)
			{
				SetVoxelMode(false);
			}
		}
	}

	public static void SetVoxelMode(bool NewMode) 
	{
		if (IsEditVoxels != NewMode) 
		{
			IsEditVoxels = NewMode;
			SetSelectionVisibility(NewMode);
		}
	}
	public static void SetSelectionVisibility(bool NewMode) 
	{
		if (MyCube) 
		{
			MyCube.GetComponent<MeshRenderer>().enabled = (NewMode);
		}
	}
	private static void UpdateWorld(Transform NewWorld) {
		if (MyTargetWorld != NewWorld) {
			MyTargetWorld = NewWorld;
			if (MyTargetWorld) {
				MyWorld = MyTargetWorld.GetComponent<VoxelEngine.World> ();
				if (MyWorld) 
				{
					//MyWorld.DebugChunkPositions ();
				} 
				else 
				{
					MyTargetWorld = null;
				}
			}
		}
	}
	public static void RayTraceVoxels(SceneView MySceneView)
	{ 
		if (IsEditVoxels) 
		{
			SceneCamera = MySceneView.camera;
			string DebugRayString = "";
			Vector3 MyMousePosition = Event.current.mousePosition;
			MyMousePosition.y = LevelSelectGui.GetMainCamera().pixelHeight - MyMousePosition.y;
			Ray MyRay = LevelSelectGui.GetMainCamera().ScreenPointToRay (MyMousePosition);
			
			RaycastHit MyHit;
			if (Physics.Raycast (MyRay, out MyHit)) {
				DebugRayString += " Ray YES hit.\n";
				SetSelectionVisibility (true);
			} else {
				DebugRayString += " Ray NO hit.\n";
				SetSelectionVisibility (false);
			}
			if (MyHitObject != null)
				DebugRayString += " Selected in debugger: " + MyHitObject.name + ".\n";
			else
				DebugRayString += " Debugger - Null Selected.\n";
			
			if (Selection.activeObject)
				DebugRayString += " Selected: " + Selection.activeObject.name + ".\n";
			else
				DebugRayString += " Heirarchy - Null Selected.\n";
			var style = new GUIStyle (GUI.skin.button);
			style.normal.textColor = Color.white;
			//GUILayout.Label (DebugRayString, style, GUILayout.Width (80), GUILayout.Width (500));   
			
			
			if (MyHit.collider && MyHit.collider.gameObject) { 	// if was hit
				MyHitObject = MyHit.collider.gameObject.transform;
				if (MyHitObject.GetComponent<VoxelEngine.Chunk> ()) {
					if (MyCube == null) 
					{
						MyCube = GameObject.Find ("SelectionMesh");
					}
					if (MyCube == null) 
					{
						MyCube = GameObject.CreatePrimitive (PrimitiveType.Cube);
						//DestroyImmediate (MyCube.GetComponent<BoxCollider> ());
						MyCube.name = "SelectionMesh";
					}
					if (MyCube != null) 
					{
						//MyHitObject.parent.GetComponent<VoxelEngine.World>(),MyHit.point))
						MyCube.transform.position = MyHitObject.parent.TransformPoint (VoxelEngine.World.RayHitToBlockPosition (MyHit, (BlockType == 0)) + MyHitObject.transform.lossyScale / 2f);
						MyCube.transform.rotation = MyHitObject.parent.transform.rotation;
					}
				}
				// Retrieve the control Id
				if (BlockType >= 0)
				if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0) {
					int controlId = GUIUtility.GetControlID (FocusType.Passive);
					Event.current.Use ();
					{
						//Debug.DrawLine(MyRay.origin, MyHit.point, Color.blue, 60);
						if (MyHitObject.GetComponent<VoxelEngine.Chunk> ()) {
							MyWorld = MyHitObject.parent.GetComponent<VoxelEngine.World> ();
							if (MyWorld) 
							{
								MyWorld.UpdateBlockType (MyHit, BlockType, UpdateSize);
								/*VoxelEngine.World.UpdateBlockCamera(MySceneView.camera, 
							                                    BlockType, 
							                                    UpdateSize/2f, 
							                                    10000);*/
							}
						}
						MyHitObject = MyHitObject.parent;
						
					}
					GUIUtility.hotControl = controlId;		
				}
				// Tell the UI your event is the main one to use, it override the selection in  the scene view
			}
		}
	}
}
