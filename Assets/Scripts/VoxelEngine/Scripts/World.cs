using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// add in world that stores many chunks
// subdivision (next)
// uv's
// various models inside the voxels
// fix water
// Polygonize voxels

namespace VoxelEngine 
{
	[ExecuteInEditMode]
	public class World : MonoBehaviour {
		[HideInInspector] public ChunkUpdater MyUpdater;
		private bool IsUpdating = false;
		[Header("Debug")]
		public bool IsDebugMode = false;
		public MyChunkEvent OnUpdateChunk;
		//[HideInInspector]
		public DictionaryChunks MyChunkData = new DictionaryChunks();
		public List<Material> MyMaterials = new List<Material>();
		//public Material MyTranslucentMaterial;
		[Header("Options")]
		public bool IsSmoothed = false;
		public bool IsConvex = false;
		public bool IsLighting = false;
		public bool IsCollisions = true;
		// instructions
		[Header("Actions")]
		[SerializeField] private string SaveFilePath;
		[Tooltip("Clears all the voxels")]
		[SerializeField] private bool IsClearVoxels;

		void Start() {
			MyUpdater = gameObject.GetComponent<ChunkUpdater> ();
			Rigidbody MyRigid = GetComponent<Rigidbody> ();
			if (MyRigid) {
				if (IsConvex) {
					MyRigid.isKinematic = false;
				} else {
					MyRigid.isKinematic = true;
				}
			}
			//MyChunkData.LoadFromLists ();
		}
		public void DebugChunkPositions() {
			for (int i = 0; i < MyChunkData.Count; i++) {
				MyChunkData.GetValueAt (i).RenderChunkPosition();
			}
		}
		void OnGUI() 
		{
			if (IsDebugMode) 
			{
				GUILayout.Label ("MyChunkData Count: " + MyChunkData.Count);
			}
		} 

		void Update() {	
			if (IsClearVoxels) {
				IsClearVoxels = false;
				ClearVoxels ();
			}
		}

		public void UpdateAllChunks() 
		{
			for (int i = 0; i < MyChunkData.Count; i++) 
			{
				GetComponent<ChunkUpdater>().Add(MyChunkData.GetValueAt (i));
				//MyChunkData.GetValueAt (i).UpdateChunk();
			}
		}
		public void ClearVoxels() {
			MyChunkData.Clear ();
			for (int i = transform.childCount-1; i >= 0; i--) {
				if (!DestroyChunk(transform.GetChild(i).gameObject.GetComponent<Chunk>().Position))
					DestroyImmediate(transform.GetChild(i).gameObject);
			}
		}
		// ===== CHUNK STUFF =====
		public void CreateChunk(float x, float y, float z) 
		{
			ChunkPosition MyChunkPosition = new ChunkPosition(Mathf.FloorToInt(x),
			                                                  Mathf.FloorToInt(y),
			                                                  Mathf.FloorToInt(z));

			GameObject NewObject = new GameObject ();
			NewObject.transform.SetParent(gameObject.transform,false);
			NewObject.transform.position = transform.TransformPoint
											(x*Chunk.ChunkSize,
			                                                        y*Chunk.ChunkSize,
			                                                        z*Chunk.ChunkSize);
			NewObject.transform.rotation = transform.rotation;
			NewObject.transform.localScale = new Vector3(1,1,1);
			NewObject.name = "Chunk:" + x + ":" + y + ":" + z;

			//Debug.LogError ("IsNull: " + (NewObject.transform == null));
			Chunk NewChunk = NewObject.AddComponent<Chunk> ();
			NewChunk.SetSmoothTerrain(IsSmoothed);
			NewChunk.SetConvex(IsConvex);
			NewChunk.Position = MyChunkPosition;
			NewChunk.SetWorld (this);
			//Debug.LogError ("Adding new chunk: " + MyChunkPosition.GetVector().ToString ());
			
			/*try 
			{
				//Debug.Log ("Adding new chunk to dictionary " + NewObject.name);*/
				MyChunkData.Add (MyChunkPosition, NewChunk);
			/*}
			catch (System.IndexOutOfRangeException e) 
			{
				Debug.LogError("Failure Adding chunk to dictionary");
			}*/
		}
		public bool DestroyChunk(ChunkPosition NewPosition) {
			return DestroyChunk (NewPosition.x, NewPosition.y, NewPosition.z);
		}
		public bool DestroyChunk(int x, int y, int z) {
			Chunk MyChunk = null;
			if (MyChunkData.TryGetValue(new ChunkPosition(x, y, z), out MyChunk))
			{	
				// atm it is saved when it is removed
				// SaveChunk 
				DestroyImmediate(MyChunk.gameObject);
				MyChunkData.Remove(new ChunkPosition(x, y, z));
				return true;
			}
			return false;
		}

		// returns null if the chunk isnt in the dictionary
		public Chunk GetChunk(int x, int y, int z) 
		{
			Chunk MyChunk = null;
			try {
			MyChunkData.TryGetValue(new ChunkPosition(x,y,z), out MyChunk);
			} catch (System.IndexOutOfRangeException e) {

			}
			return MyChunk;
		}

		// =====UPDATE VOXELS=====
		public static void UpdateBlockCamera(int NewType, float BrushSize, float BrushRange) {
			UpdateBlockCamera (Camera.main, NewType, BrushSize, BrushRange);
		}
		public static void UpdateBlockCamera(Camera MyCamera, int NewType, float BrushSize, float BrushRange) {
			//gameObject.GetComponent<PhotonView>().RPC(
				UpdateBlockCamera2(  NewType, BrushSize, BrushRange, 
			                                          MyCamera.transform.position, 
			                                          MyCamera.transform.forward);
		}
		public static void UpdateBlockCamera2(int NewType, float BrushSize, float BrushRange, Vector3 RayOrigin, Vector3 RayDirection) {
			//Debug.LogError ("Updaating a block!");
			RaycastHit MyHit;
			if (Physics.Raycast (RayOrigin, RayDirection, out MyHit, BrushRange)) 
			{
				Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
				//Debug.LogError("Hit object: " + MyHit.collider.transform.parent.name);
				//if (MyHit.collider.transform.parent != null && MyHit.collider.transform.parent.gameObject == gameObject)
				if (MyChunk)
				{
					World MyWorld = MyChunk.GetWorld();
					MyWorld.UpdateBlockType(MyHit, NewType, BrushSize);
					return;
				}
			}
			return;
		}
		public static Vector3 GetChunkPosition(World MyWorld, Vector3 WorldPosition) {
			Vector3 BlockPosition = WorldPosition;
			BlockPosition = MyWorld.transform.InverseTransformPoint (BlockPosition);
			int ChunkX = Mathf.FloorToInt(BlockPosition.x/Chunk.ChunkSize);
			int ChunkY = Mathf.FloorToInt(BlockPosition.y/Chunk.ChunkSize);
			int ChunkZ = Mathf.FloorToInt(BlockPosition.z/Chunk.ChunkSize);
			return new Vector3 (ChunkX, ChunkY, ChunkZ);
		}
		public static Vector3 GetBlockPosition(World MyWorld, Vector3 WorldPosition) {
			//Debug.LogError ("OriginalPosition: " + WorldPosition.ToString ());
			Vector3 BlockPosition = WorldPosition;
			BlockPosition = MyWorld.transform.InverseTransformPoint (BlockPosition);
			int ChunkX = Mathf.FloorToInt(BlockPosition.x/Chunk.ChunkSize);
			int ChunkY = Mathf.FloorToInt(BlockPosition.y/Chunk.ChunkSize);
			int ChunkZ = Mathf.FloorToInt(BlockPosition.z/Chunk.ChunkSize);
			int PosX = Mathf.FloorToInt(BlockPosition.x)%Chunk.ChunkSize;
			int PosY = Mathf.FloorToInt(BlockPosition.y)%Chunk.ChunkSize;
			int PosZ = Mathf.FloorToInt(BlockPosition.z)%Chunk.ChunkSize;
			//Chunk MyChunk = MyWorld.GetChunk (ChunkX, ChunkY, ChunkZ);
			BlockPosition = new Vector3(ChunkX*Chunk.ChunkSize+PosX,
			                            ChunkY*Chunk.ChunkSize+PosY, 
			                            ChunkZ*Chunk.ChunkSize+PosZ);
			//Debug.LogError ("Clicking thingie: " + BlockPosition.ToString ());
			return BlockPosition;
		}
		public static Vector3 RayHitToBlockPosition(RaycastHit MyHit, bool Direction) 
		{
			return RayHitToBlockPosition (MyHit.collider.GetComponent<Chunk> ().GetWorld (), 
			                              MyHit.normal, MyHit.point, Direction, 
			                              MyHit.collider.transform.lossyScale.sqrMagnitude / 3f);// MyHit.collider.transform.localScale.x);
		}
		public static Vector3 RayHitToBlockPosition(World MyWorld, Vector3 Normal, Vector3 MyHitPosition, bool Direction, float BlockScale) 
		{
			if (Direction) 
			{
				MyHitPosition += -Normal*BlockScale/2f;
			}
			else 
			{
				MyHitPosition += Normal*BlockScale/2f;
			}
			Vector3 BlockPosition = GetBlockPosition (MyWorld, MyHitPosition);
			return BlockPosition;
		}
		// size
		public void UpdateBlockType(RaycastHit MyHit, int NewType, float BrushSize) {
			Vector3 BlockPosition = RayHitToBlockPosition(MyHit, (NewType == 0));
			UpdateBlockTypeSize (BlockPosition, 
			                     NewType, 
			                     BrushSize);

			return;
		}
		public bool UpdateBlockTypeSize(Vector3 BlockPosition, int NewType, float SetSize) {
			//if (PhotonNetwork);
			if (PhotonNetwork.connected) {
				gameObject.GetComponent<PhotonView> ().RPC ("UpdateBlockTypeSize2",
			                                            PhotonTargets.All,
			                                            BlockPosition.x, BlockPosition.y, BlockPosition.z, 
			                                          NewType,
			                                          SetSize);
			} else {
				UpdateBlockTypeSize2(BlockPosition.x, BlockPosition.y, BlockPosition.z, NewType, SetSize);
			}
			return true;
			//return UpdateBlockTypeSize (BlockPosition.x, BlockPosition.y, BlockPosition.z, NewType,SetSize);
		}
		// uses block grid positions
		[PunRPC]
		private bool UpdateBlockTypeSize2(float x, float y, float z, int Type, float SetSize) {
			//Debug.LogError (gameObject.name + " Updating block at: " + x + ":" + y + ":" + z + ": with size: " + SetSize);
			for (float i = -SetSize; i <= SetSize; i += 1) {
				for (float j = -SetSize; j <= SetSize; j += 1) {
					for (float k = -SetSize; k <= SetSize; k += 1) {
						UpdateBlockType(Mathf.FloorToInt(x+i),
						                Mathf.FloorToInt(y+j),
						                Mathf.FloorToInt(z+k),
						                Type);
					}
				}
			}
			for (float i = SetSize; i <= SetSize; i += 1) {
				for (float j = SetSize; j <= SetSize; j += 1) {
					for (float k = SetSize; k <= SetSize; k += 1) {
						UpdateBlockType(Mathf.CeilToInt(x+i),
						                Mathf.CeilToInt(y+j),
						                Mathf.CeilToInt(z+k),
						                Type);
					}
				}
			}
			return true;
		}

		// converts WorldBlockPosition to ChunkBlockPosition
		// then updates block inside chunk
		public bool UpdateBlockType(int x, int y, int z, int NewType) {
			int PosX = (x)%Chunk.ChunkSize;
			int PosY = (y)%Chunk.ChunkSize;
			int PosZ = (z)%Chunk.ChunkSize;
			if (!(Chunk.IsInRange (PosX) && Chunk.IsInRange (PosY) && Chunk.IsInRange (PosZ))) {	// prevents stack overflow lol
				//Debug.LogError("Not in range for some reason: " + PosX +":"+PosY+":"+PosZ);
				// if not in range
				// normally if the chunk isn't loaded
				return false; 
			}
			int ChunkX = (x/Chunk.ChunkSize);
			int ChunkY = (y/Chunk.ChunkSize);
			int ChunkZ = (z/Chunk.ChunkSize);
			//Debug.LogError ("Looking for chunk: " + ChunkX + ":" + ChunkY + ":" + ChunkZ);
			//Debug.LogError ("Updating block: " + x + ":" + y + ":" + z);
			Chunk MyChunk = GetChunk(ChunkX, ChunkY, ChunkZ);
			if (MyChunk != null) {
				//Debug.LogError ("Found chunk: " + MyChunk.name);
				return MyChunk.UpdateBlockType (PosX, PosY, PosZ, NewType);
			} else {
				//Debug.LogError("No chunk found.");
				return false;
			}
		}
		// floats to int
		public bool UpdateBlockType(Vector3 BlockPosition, int NewType) {
			return UpdateBlockType (BlockPosition.x, BlockPosition.y, BlockPosition.z, NewType);
		}
		public bool UpdateBlockType(float x, float y, float z, int NewType) {
			return UpdateBlockType (Mathf.FloorToInt (x),
			                        Mathf.FloorToInt (y),
			                        Mathf.FloorToInt (z),
			                        NewType);
		}

		// =====GET VOXELS=====
		public int GetVoxelType(Vector3 BlockPosition) {
			return GetVoxel (BlockPosition).GetBlockIndex ();
		}
		public Voxel GetVoxel(Vector3 BlockPosition) {
			return GetVoxel (Mathf.FloorToInt (BlockPosition.x),
			                 Mathf.FloorToInt (BlockPosition.y),
			                 Mathf.FloorToInt (BlockPosition.z));
		}
		public Voxel GetVoxel(int x, int y, int z) {
			Chunk MyChunk = GetChunk(Mathf.FloorToInt(x/((float)Chunk.ChunkSize)), 
			                         Mathf.FloorToInt(y/((float)Chunk.ChunkSize)), 
			                         Mathf.FloorToInt(z/((float)Chunk.ChunkSize)));
			if (MyChunk != null) {
				return MyChunk.GetVoxel (x - MyChunk.Position.x*Chunk.ChunkSize, 
				                         y - MyChunk.Position.y*Chunk.ChunkSize, 
				                         z - MyChunk.Position.z*Chunk.ChunkSize);
			}
			return null;
			//return new Voxel();	// return default voxel
		}

		// ===== COLLIDER FUNCTIONS =====
		//public MeshData MyMeshData = new MeshData();
		public void ClearColliders() {
			BoxCollider[] MyColliders = gameObject.GetComponents<BoxCollider> ();
			for (int i = MyColliders.Length-1; i >= 0; i--) {
				DestroyImmediate (MyColliders[i]);
			}
		}
		public void ClearCollider(Vector3 Position) {
			BoxCollider[] MyColliders = gameObject.GetComponents<BoxCollider> ();
			for (int i = MyColliders.Length-1; i >= 0; i--) {
				if (MyColliders[i].center == Position) {
					DestroyImmediate (MyColliders[i]);
					break;
				}
			}
		}
		public void ResetColliders() {
			ClearColliders ();
			CreateColliders ();
		}
		public void CreateColliders() {
			/*List<Vector3> MyCreatedVoxelPositions = MyChunk.GetCollisionPositions ();
			for (int i = 0; i < MyCreatedVoxelPositions.Count; i++) {
				CreateCollider( MyCreatedVoxelPositions[i]);
			}
			MyRigidBody.mass = MyCreatedVoxelPositions.Count;*/
		}
		public void CreateCollider(Vector3 Position) {
			//BoxCollider NewCollider = gameObject.AddComponent<BoxCollider>();
			//NewCollider.center = Position;
			//NewCollider.size = new Vector3(1,1,1);
		}
	}
}