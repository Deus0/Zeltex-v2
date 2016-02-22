using UnityEngine;
using UnityEditor;
using VoxelEngine;

public class VoxelUpdaterGUI : EditorWindow 
{
	static World MyWorld = null;
	static bool IsEnabled = false;
	static int DebugCount = 0;
	static float DebugTimeBegin = 0;

	//[MenuItem ("Marz/VoxelUpdating")]
	public static void Enable () {
		if (!IsEnabled) {
			IsEnabled = true;
			Debug.LogError ("starting Voxel Updating!");
			DebugTimeBegin = (float)EditorApplication.timeSinceStartup;
			if (MyWorld == null) {
				MyWorld = GameObject.FindObjectOfType (typeof(World)) as World;
			}
			MyWorld.GetComponent<ChunkUpdater> ().Reset ();
			UnityEditor.EditorApplication.update += Update;
		}
	}
	
	/*void OnGUI () {
		//EditorGUILayout.LabelField ("Count: " + DebugCount + " - Time: " + ( EditorApplication.timeSinceStartup-DebugTimeBegin));
		if (MyWorld == null)
		{
			return;
		}
		EditorGUILayout.LabelField ("World Selected: " + MyWorld.name);
		EditorGUILayout.LabelField ("Loading: " + MyWorld.GetComponent<ChunkUpdater>().UpdateList.Count);
		EditorGUILayout.LabelField ("Lights: " + MyWorld.GetComponent<ChunkUpdater>().LightsUpdateList.Count);
	}*/
	
	private static void Update ()
	{
		//Debug.LogError ("Doing the thing.");
		if (MyWorld == null) 
		{
			MyWorld = GameObject.FindObjectOfType(typeof(World)) as World;
		}
		DebugCount++;
		MyWorld.GetComponent<ChunkUpdater> ().ManuallyRunUpdate ();
	}
}