using UnityEngine;
using System.Collections;

// notes:
//		Game will start in a black room (black voxel walls)
//		with 3d guis on walls
//		you then chose the map mode using the rooms guis
//		it will then load the map while you are in the room
//		when the loading is done, a teleporter will activate, allowing the player to move to the map
//			(or a portal)

// 	chunks to fade in and out
// 	chunks to use subdivision on longer distances

/*
	Loads from a position, size, centred?
	Uses threads to load the chunks
	
*/
namespace VoxelEngine 
{
	[ExecuteInEditMode]
	[RequireComponent(typeof (World))]
	public class VoxelLoader : MonoBehaviour 
	{
		// Events
		[Header("Events")]
		public MyChunkEvent OnLoadEmptyTerrain;	// Add function to load terrain here
		// actions
		[Header("Actions")]
		public bool IsRefresh = false;
		[Header("Options")]
		public GameObject MyLoadPosition;
		public Vector3 LoadDistance = new Vector3(3,1,3);
		public bool IsAutoUpdate = false;	// updates chunks when walk into new chunk position
		public bool IsThreading = false;		//
		// states
		private World MyWorld;
		private bool HasLoaded = false;		// initial loading
		private bool IsLoading = false;		// is currently loading new chunks

		void Start() 
		{
			MyWorld = gameObject.GetComponent<World> ();
			if (MyLoadPosition == null)
				MyLoadPosition = gameObject;
		}
		void Update() {
			if (IsAutoUpdate) 
			{

			}
			if (IsRefresh) 
			{
				IsRefresh = false;
				RefreshWorld ();
			}
		}
		public void RefreshWorld() 
		{
			MyWorld.GetComponent<ChunkUpdater> ().Reset ();
			MyWorld.ClearVoxels ();
			LoadWorld ();
		}
		public void LoadWorld() {
			Debug.LogError ("Loading new world at: " + transform.position.ToString () + " with distance " + LoadDistance);
			LoadWorld (MyWorld, 
			           MyLoadPosition.transform.position,
			          LoadDistance,
			          false);
		}
		
		public static void LoadWorld(World MyWorld, Vector3 Position, Vector3 Size) 
		{
			LoadWorld (MyWorld, Position, Size, false);
		}
		public static void LoadWorld(World MyWorld,Vector3 Size) 
		{
			LoadWorld (MyWorld, new Vector3(0,0,0), Size, false);
		}
		public static void LoadWorld(World MyWorld, Vector3 Position, Vector3 Size, bool IsCentred) 
		{
			if (!IsCentred) {
				for (int i = 0; i < Size.x; i++)
					for (int j = 0; j < Size.y; j++) 
					for (int k = 0; k < Size.z; k++) {
							MyWorld.CreateChunk (i, j, k);
						}
			} else {
				for (float i = -Size.x/2f; i <= Size.x/2f; i++)
					for (float j = -Size.y/2f; j <= Size.y/2f; j++)
					for (float k = -Size.z/2f; k <= Size.z/2f; k++){
						MyWorld.CreateChunk (i, j, k);
					}
			}
		}
	}
}
