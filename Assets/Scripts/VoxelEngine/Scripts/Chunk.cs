using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VoxelEngine {
	// chunks handle the updates of the mesh bythemselves
	// once the update flag is ticked inside the updateblock function it will refresh straight away

	[ExecuteInEditMode]
	[System.Serializable]
	public class Chunk : MonoBehaviour {
		public List<GameObject> MyCharacterSpawns = new List<GameObject>();
		public static int SunBrightness = 255;
		public static int DefaultBrightness = 0;
		private static bool IsPropogation = true;
		public static int ChunkSize = 16;
		[HideInInspector] [SerializeField] private VoxelData MyVoxels = new VoxelData();
		[HideInInspector] [SerializeField] public ChunkPosition Position;
		[HideInInspector] [SerializeField] private World MyWorld;
		[HideInInspector] [SerializeField] private bool IsSmoothTerrain = true;
		private MeshFilter MyMeshFilter;
		private MeshRenderer MyMeshRenderer;
		private MeshCollider MyMeshCollider;
		private Rigidbody MyRigidBody;
		public bool HasUpdated = false;
		public bool HasUpdatedLights = false;
		private bool IsConvex = false;
		
		private bool IsPrimaryChange = false;

		/*void OnGUI ()
		{
			if (MyWorld.IsDebugMode) {
				RenderChunkPosition (false);
			}
		}*/
		
		// need this on a thread for editor mode
		void Update() 
		{
			if (HasUpdated) 
			{
				HasUpdated = false;
				HasUpdatedLights = false;

				MyWorld.MyUpdater.Add(this);
			}
			if (HasUpdatedLights)
			{
				HasUpdatedLights = false;
				MyWorld.MyUpdater.AddLights(this);
			}
		}
		
		public Vector3 GetMidPoint() {
			return transform.TransformPoint(new Vector3(ChunkSize, ChunkSize, ChunkSize)/2f);
		}
		public void RenderChunkPosition() {
			RenderChunkPosition (true);
		}
		private void RenderChunkPosition(bool IsRenderAll) 
		{
			#if UNITY_EDITOR
			//var _target = (Type)target;
			if (IsRenderAll || Vector3.Distance(transform.position, Camera.main.transform.position) < ChunkSize) 
			{
				UnityEditor.Handles.color = Color.red;
				UnityEditor.Handles.Label(GetMidPoint(), name);
			}
			#endif
		}
		/*public void SetVoxelType(int i, int j, int k, int Type) 
		{
			MyVoxels.SetVoxelType (this, i, j, k, Type);
		}*/
		public World GetWorld() 
		{
			return MyWorld;
		}

		public void SetWorld(World NewWorld)
		{
			MyWorld = NewWorld;
		}

		public void SetSmoothTerrain(bool IsSmooth) 
		{
			IsSmoothTerrain = IsSmooth;
		}

		public void SetConvex(bool IsSmooth) 
		{
			IsConvex = IsSmooth;
		}

		void Start() 
		{
			InitializeComponents ();
		}

		public void RunUpdates() 
		{
			//MyWorld.OnUpdateChunk.Invoke(Position);
			float BeforeBuildMeshTime = ChunkUpdater.GetCurrentTime();
			IsPrimaryChange = true;
			UpdateLighting ();
			UpdateChunk();
			IsPrimaryChange = false;
			float TimeTaken = (ChunkUpdater.GetCurrentTime () - BeforeBuildMeshTime) * 1000f;
			if (MyWorld.IsDebugMode)
				Debug.LogError ("[" + ChunkUpdater.GetCurrentTime() + "s]" + " - Mesh [" + name + "] - [" + ((int)TimeTaken) + "ms]");
		}

		public void RunUpdateLights() 
		{
			float BeforeBuildMeshTime = ChunkUpdater.GetCurrentTime();
			HasUpdatedLights = false;
			UpdateLighting ();
			//PropogateLights();
			UpdateChunk();
			float TimeTaken = (ChunkUpdater.GetCurrentTime () - BeforeBuildMeshTime) * 1000f;
			//Debug.LogError ("Updating Lights [" + name + "] - ["  + ((int)TimeTaken) + "ms] at time [" + ChunkUpdater.GetCurrentTime() + "s]");
			if (MyWorld.IsDebugMode)
				Debug.LogError ("[" + ChunkUpdater.GetCurrentTime() + "s]" + " - Lights [" + name + "] - [" + ((int)TimeTaken) + "ms]");
		}
		public void Updated() 
		{
			if (!HasUpdated) {
				HasUpdated = true;
			}
			if (HasUpdated) 
			{
				HasUpdated = false;
				HasUpdatedLights = false;
				
				MyWorld.MyUpdater.Add(this);
			}
		}
		public void UpdatedLights() 
		{
			if (!HasUpdatedLights) {
				HasUpdatedLights = true;
			}
			if (HasUpdatedLights)
			{
				HasUpdatedLights = false;
				MyWorld.MyUpdater.AddLights(this);
			}
		}

		public void InitializeComponents() 
		{
			if (MyMeshCollider == null) 
			{
				MyMeshCollider = gameObject.GetComponent<MeshCollider> ();
				if (MyMeshCollider == null) 
				{
					MyMeshCollider = gameObject.AddComponent<MeshCollider> ();
					MyMeshCollider.convex = IsConvex;
				}
			}
			if (MyMeshFilter == null) 
			{
				MyMeshFilter = gameObject.GetComponent<MeshFilter>();
				if (MyMeshFilter == null)
				{
					MyMeshFilter = gameObject.AddComponent<MeshFilter>();
					MyMeshFilter.sharedMesh = new Mesh();
				}
			}
			if (MyMeshRenderer == null)
			{
				MyMeshRenderer = gameObject.GetComponent<MeshRenderer>();
				if (MyMeshRenderer == null)
				{
					MyMeshRenderer = gameObject.AddComponent<MeshRenderer>();
				}
			}
		}
		public Chunk() 
		{
			for (int i = 0; i < ChunkSize; i++) {
				for (int j = 0; j < ChunkSize; j++) {
					for (int k = 0; k < ChunkSize; k++) {
						MyVoxels.Reset(i,j,k);
					}
				}
			}
		}
		public float GetDensity(float x, float y, float z) {
			int PosX = Mathf.RoundToInt (x);
			int PosY = Mathf.RoundToInt (y);
			int PosZ = Mathf.RoundToInt (z);
			// (PosX == ChunkSize - 1 || PosZ == ChunkSize - 1 || PosX == 0 || PosZ == 0)
			//	return 0;
			int MyType = GetVoxel (PosX, PosY, PosZ).GetBlockIndex();
			if (MyType == 0)// || MyType == 2)
				return 0;
			else
				return 1;
		}

		public List<Vector3> GetCollisionPositions() {
			List<Vector3> CollisionPositions = new List<Vector3>();
			for (int i = 0; i < ChunkSize; i++) {
				for (int j = 0; j < ChunkSize; j++) {
					for (int k = 0; k < ChunkSize; k++) {
						if (GetVoxel(i,j,k).GetBlockIndex() != 0) {
							//if (GetVoxel(i+1,j,k).Type == 0 || GetVoxel(i-1,j,k).Type == 0 || 
							//    GetVoxel(i,j+1,k).Type == 0 || GetVoxel(i,j-1,k).Type == 0 || 
							//    GetVoxel(i,j,k+1).Type == 0 || GetVoxel(i,j,k-1).Type == 0) 
							{
								CollisionPositions.Add(new Vector3(i,j,k));
							}
						}
					}
				}
			}
			return CollisionPositions;
		}

		public static bool IsInRange(int a) {
			return (a >= 0 && a < Chunk.ChunkSize);
		}
		public Voxel GetVoxel(int x, int y, int z) {
			if (IsInRange (x) && IsInRange (y) && IsInRange (z))
				return MyVoxels.GetVoxel (x, y, z);
			else 
			{
				//Debug.LogError("Is MyWorld: " + (MyWorld == null));
				// Assuming that these position values are relative to chunk Position
				return MyWorld.GetVoxel (x+Position.x * Chunk.ChunkSize, 
				                         y+Position.y * Chunk.ChunkSize, 
				                         z+Position.z * Chunk.ChunkSize);
			}
		}
		/*public void HasUpdatedAll() 
		{
			Updated();
			Chunk ChunkLeft = MyWorld.GetChunk(Position.x-1, Position.y, Position.z);
			if (ChunkLeft != null) {
				ChunkLeft.Updated();
			}
			Chunk ChunkRight = MyWorld.GetChunk(Position.x+1, Position.y, Position.z);
			if (ChunkRight != null) {
				ChunkRight.Updated();
			}
			Chunk ChunkFront = MyWorld.GetChunk(Position.x, Position.y, Position.z+1);
			if (ChunkFront != null) {
				ChunkFront.Updated();
			}
			Chunk ChunkBehind = MyWorld.GetChunk(Position.x, Position.y, Position.z-1);
			if (ChunkBehind != null) {
				ChunkBehind.Updated();
			}
			Chunk ChunkBelow = MyWorld.GetChunk(Position.x, Position.y-1, Position.z);
			if (ChunkBelow != null) {
				ChunkBelow.Updated();
			}
			Chunk ChunkAbove = MyWorld.GetChunk(Position.x, Position.y+1, Position.z);
			if (ChunkAbove != null) {
				ChunkAbove.Updated();
			}
		}*/
		// using the world position
		public bool UpdateBlockTypeWorldPosition(Vector3 WorldPosition, int Type) 
		{
			return UpdateBlockTypeBlockPosition (WorldPosition, Type);
		}

		public bool UpdateBlockTypeBlockPosition(Vector3 BlockPosition, int Type) 
		{
			return UpdateBlockType (Mathf.FloorToInt(BlockPosition.x),
			                              Mathf.FloorToInt(BlockPosition.y),
			                              Mathf.FloorToInt(BlockPosition.z),
			                             Type);
		}

		public void HasChangedAt(int x, int y, int z) 
		{
			HasChangedAt(x,y,z,false);
		}
		public void HasChangedAt(int x, int y, int z, bool IsLightsOnly) {
			if (x == 0) {
				Chunk ChunkLeft = MyWorld.GetChunk(Position.x-1, Position.y, Position.z);
				if (ChunkLeft != null) {
					if (IsLightsOnly)
						ChunkLeft.UpdatedLights();
					else
						ChunkLeft.Updated();
				}
			} else if (x == ChunkSize-1) {
				Chunk ChunkRight = MyWorld.GetChunk(Position.x+1, Position.y, Position.z);
				if (ChunkRight != null) {
					if (IsLightsOnly)
						ChunkRight.UpdatedLights();
					else
						ChunkRight.Updated();
				}
			}
			if (z == 0) {
				Chunk ChunkLeft = MyWorld.GetChunk(Position.x, Position.y, Position.z-1);
				if (ChunkLeft != null) {
					if (IsLightsOnly)
						ChunkLeft.UpdatedLights();
					else
						ChunkLeft.Updated();
				}
			} else if (z == ChunkSize-1) {
				Chunk ChunkRight = MyWorld.GetChunk(Position.x, Position.y, Position.z+1);
				if (ChunkRight != null) {
					if (IsLightsOnly)
						ChunkRight.UpdatedLights();
					else
						ChunkRight.Updated();
				}
			}
			if (y == 0) {
				Chunk ChunkLeft = MyWorld.GetChunk(Position.x, Position.y-1, Position.z);
				if (ChunkLeft != null) {
					if (IsLightsOnly)
						ChunkLeft.UpdatedLights();
					else
						ChunkLeft.Updated();
				}
			} else if (y == ChunkSize-1) {
				Chunk ChunkRight = MyWorld.GetChunk(Position.x, Position.y+1, Position.z);
				if (ChunkRight != null) {
					if (IsLightsOnly)
						ChunkRight.UpdatedLights();
					else
						ChunkRight.Updated();
				}
			}
		}
		// uses local chunk coordinates
		// if not local, it is worldBlockPosition, so it asks world to update the block
		public bool UpdateBlockType(int x, int y, int z, int Type)
		{
			if (IsInRange (x) && IsInRange (y) && IsInRange (z)) 
			{
				//Debug.LogError("IsInRange!");
				bool HasChanged = GetVoxel (x, y, z).SetType (this, x, y, z, Type);
				if (HasChanged) 
				{
					Updated();
					HasChangedAt(x,y,z);
				}
				return HasChanged;
			} 
			else 
			{
				// assuming it is relative to the chunks position rather then WorldBlockPosition
				return MyWorld.UpdateBlockType(x+Position.x * Chunk.ChunkSize, 
				                               y+Position.y * Chunk.ChunkSize,
				                               z+Position.z * Chunk.ChunkSize,
				                               Type);
			}
		}


		
		public void UpdateChunk() 
		{
			HasUpdated = false;
			//Debug.LogError ("Updating Chunk " + name);
			List<MeshData> MyMeshes = new List<MeshData> ();

			for (int i = 0; i < MyWorld.MyMaterials.Count; i++) 
			{
				MeshData NewMeshData = new MeshData ();
				NewMeshData = BuildMesh(NewMeshData, i);
				MyMeshes.Add (NewMeshData);
			}
			SetAllUpdated ();
			UpdateMesh(MyMeshes);
			// activate any characters that were saved into the mesh
			for (int i = MyCharacterSpawns.Count-1; i >= 0; i--)
			{
				MyCharacterSpawns[i].SetActive(true);
				MyCharacterSpawns.RemoveAt (i);
			}
		}

		void UpdateMesh(List<MeshData> MyMeshData)
		{
			//Debug.LogError ("Updating mesh with: " + MyMeshData.Count);
			if (MyMeshFilter == null)
				InitializeComponents ();
			MyMeshFilter.sharedMesh = null;
			MyMeshFilter.sharedMesh = new Mesh ();
			MyMeshFilter.sharedMesh.Clear();
			List<CombineInstance> MeshList = new List<CombineInstance>();
			List<Material> MyMaterials = new List<Material> ();
			for (int i = 0; i < MyMeshData.Count; i++)
			{	//
				//Debug.LogError(i + " Verts count: " + MyMeshData[i].vertices.Count);
				if (MyMeshData[i].HasData()) {		// if chunk has water
					CombineInstance NewCombineInstance = new CombineInstance();
					NewCombineInstance.mesh = new Mesh ();
					NewCombineInstance.mesh.vertices = MyMeshData[i].GetVerticies();
					NewCombineInstance.mesh.triangles = MyMeshData[i].GetTriangles();
					NewCombineInstance.mesh.uv = MyMeshData[i].GetTextureCoordinates();
					NewCombineInstance.mesh.SetColors(MyMeshData[i].GetColors());
					NewCombineInstance.transform = MyMeshFilter.transform.transform.localToWorldMatrix;
					MeshList.Add (NewCombineInstance);	
					if (i >= MyWorld.MyMaterials.Count)
						MyMaterials.Add (MyWorld.MyMaterials[0]);
					else
						MyMaterials.Add (MyWorld.MyMaterials[i]);
				}
			}
			MyMeshFilter.sharedMesh.CombineMeshes (MeshList.ToArray (), false, false);//
			MyMeshFilter.sharedMesh.name = gameObject.name + "_Render";
			MyMeshFilter.sharedMesh.subMeshCount = MeshList.Count;
			//Debug.LogError (name + " sub mesh count: " + MyMeshFilter.sharedMesh.subMeshCount + ":" + MeshList.Count);
			//Debug.LogError (name + " has a combined mesh with " + MyMeshFilter.sharedMesh.colors32.Length + " colors!");
			MyMeshFilter.sharedMesh.RecalculateNormals();
			MyMeshRenderer.materials = MyMaterials.ToArray();
			
			// colliders
			if (MyWorld.IsCollisions) {
				MyMeshCollider.sharedMesh = null;
				Mesh CombinedMeshes = new Mesh ();
				CombinedMeshes.CombineMeshes (MeshList.ToArray (), true, false);
				CombinedMeshes.name = gameObject.name + "_Collision";
				MyMeshCollider.sharedMesh = CombinedMeshes;	// should be a combination of the solid parts!
			}
			MyMeshData.Clear ();
			//Debug.LogError("Inside Chunk: " + name + " combined with " + MeshList.Count + " Meshes.");
		}

		void SetAllUpdated() 
		{
			for (int i = 0; i < ChunkSize; i++)
			{
				for (int j = 0; j < ChunkSize; j++) 
				{
					for (int k = 0; k < ChunkSize; k++) 
					{
						GetVoxel (i,j,k).SetHasUpdated(false);
					}

				}
			}
		}

		public MeshData BuildMesh(MeshData MyVoxelsMesh, int MaterialType) 
		{
			if (!IsSmoothTerrain) 
			{
				VoxelTileGenerator MyData = GetWorld().gameObject.GetComponent<VoxelTileGenerator>();
				if (MyData) 
				{
					for (int i = 0; i < ChunkSize; i++)
					{
						for (int j = 0; j < ChunkSize; j++) 
						{
							for (int k = 0; k < ChunkSize; k++) 
							{
								Voxel ThisVoxel = GetVoxel (i,j,k);
								if (ThisVoxel != null) 
								{
									int VoxelIndex = ThisVoxel.GetBlockIndex();
									if (VoxelIndex != 0) 
									if (0 == MaterialType) 
									{
										if (ThisVoxel.GetHasUpdated()) 
										{
											bool[] MySides = ThisVoxel.GetSides(this, i, j, k, MaterialType);
											int[] MyLights = ThisVoxel.GetLights(this, i, j, k, MaterialType);
											bool[] MySolids = new bool[27];
											int d = 0;
											for (int a = i-1; a <= i+1; a++)
												for (int b = i-1; b <= i+1; b++)
													for (int c = i-1; c <= i+1; c++)
												{
													MySolids[d] = (!ThisVoxel.IsAir ());
													d++;
												}
											ThisVoxel.MyMeshData = MyData.GetModel(MySides,
											                                       ThisVoxel.GetBlockIndex(), 
											                                       MaterialType, 
											                                       MyLights, 
											                                       MySolids);
											ThisVoxel.MyMeshData.AddToVertex(new Vector3(i,j,k));
											MyVoxelsMesh.Add(ThisVoxel.MyMeshData);
										} 
										else 
										{
											MyVoxelsMesh.Add(ThisVoxel.MyMeshData);
										}
									}
								}
							}
						}
					}
					//Debug.LogError("Generated: " + MyVoxelsMesh.Verticies.Count);
				} else {
					for (int i = 0; i < ChunkSize; i++) 
					{
						for (int j = 0; j < ChunkSize; j++) 
						{
							for (int k = 0; k < ChunkSize; k++) 
							{
								if (GetVoxel (i,j,k).GetBlockIndex() != 0)
									MyVoxelsMesh = MyVoxels.GetVoxel(i, j, k).BuildMesh (this, i, j, k, MyVoxelsMesh, MaterialType);
							}
						}
					}
				}
			} else {
				for (int i = 0; i < ChunkSize; i++) 
				{
					for (int j = 0; j < ChunkSize; j++) 
					{
						for (int k = 0; k < ChunkSize; k++) 
						{
							MyVoxelsMesh = MarchingCubes.BuildMesh (this, new Vector3(i, j, k), MyVoxelsMesh);
						}
					}
				}
				
			}
			return MyVoxelsMesh;
		}
		public void UpdateLighting() 
		{
			if (MyWorld.IsLighting) {
				Sunlight ();
				PropogateLights ();
			} else {
				ResetLighting();
			}
		}
		public void ResetLighting() {
			for (int i = 0; i < ChunkSize; i++) 
			{
				for (int j = 0; j < ChunkSize; j++) 
				{
					for (int k = 0; k < ChunkSize; k++) 
					{
						SetBrightness(i,j,k, SunBrightness);
					}
				}
			}
		}
		// maybe later do directional sunlight?
		public void Sunlight() 
		{
			//Debug.LogError("Updating Sunlight!");
			for (int i = 0; i < ChunkSize; i++)
				for (int k = 0; k < ChunkSize; k++) 
				{
					bool IsDarkness = false;
				for (int j = ChunkSize-1; j >= 0; j--) 
				{
					Voxel ThisVoxel = GetVoxel (i, j, k);
						//bool HasChanged = false;
						if (IsDarkness) 
						{
							if (ThisVoxel.GetBlockIndex() == 6) 
							{
								SetBrightness(i,j,k,SunBrightness*0.8f);
							} else {
								SetBrightness(i,j,k,DefaultBrightness);
							}
						//HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
						} else {
							if (ThisVoxel.IsAir ()) {
								SetBrightness(i,j,k,SunBrightness);
								//HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, SunBrightness);
							} else {
								IsDarkness = true;
								SetBrightness(i,j,k,DefaultBrightness);
							//HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
							}
						}
				}
			}
		}

		public void PropogateLights() 
		{
			if (IsPropogation)
			{
				int Buffer = 1;
				//if (IsPrimaryChange)
				//	Buffer = 0;
				for (int i = -Buffer; i < ChunkSize+Buffer; i++) 
				{
					for (int j = -Buffer; j < ChunkSize+Buffer; j++) 
					{
						for (int k = -Buffer; k < ChunkSize+Buffer; k++) 
						{
							Voxel ThisVoxel = GetVoxel (i, j, k);
							if (ThisVoxel != null)
							//if (Buffer != 1 || (Buffer == 1 && (i == -Buffer || i == ChunkSize || i == -Buffer || i == ChunkSize || i == -Buffer || i == ChunkSize)))
							{
								int Brightness = (int)ThisVoxel.GetLight ();
								int BlockIndex = ThisVoxel.GetBlockIndex();
								if (BlockIndex == 0 || BlockIndex == 6) 
								{
									//PropogateLight (i, j, k, MyBrightnss, 0);
									
									PropogateLight (i + 1, j, k, Brightness, 1);
									PropogateLight (i - 1, j, k, Brightness, 1);
									PropogateLight (i, j + 1, k, Brightness, 1);
									PropogateLight (i, j - 1, k, Brightness, 1);
									PropogateLight (i, j, k + 1, Brightness, 1);
									PropogateLight (i, j, k - 1, Brightness, 1);
								}
							}
						}
					}
				}
			}
		}
		
		public void SetBrightness(int i, int j, int k, float Brightness) 
		{
			SetBrightness (i, j, k,
			              Mathf.RoundToInt (Brightness));
		}
		public void SetBrightness(int i, int j, int k, int Brightness) 
		{
			if (GetVoxel (i, j, k).SetBrightness (this, i, j, k, Brightness)) 
			{
				if (IsPrimaryChange)
					HasChangedAt (i, j, k, true);
			}
		}
		public void PropogateLight(int i, int j, int k, int Brightness, int PropogationCount) 
		{
			Voxel ThisVoxel = GetVoxel (i, j, k);
			Brightness = Mathf.RoundToInt (Brightness*0.8f);
			if (ThisVoxel != null)
			if (PropogationCount <= 8
			     && Brightness > DefaultBrightness
			    // && IsInRange (i) && IsInRange (j) && IsInRange (k)		// is within this chunk
			     && Brightness > GetVoxel (i, j, k).GetLight ()
			    && ThisVoxel.GetBlockIndex () == 0)
			{
				SetBrightness(i,j,k, Brightness);
				PropogationCount++;
				PropogateLight (i + 1, j, k, Brightness,PropogationCount);
				PropogateLight (i - 1, j, k, Brightness,PropogationCount);
				PropogateLight (i, j + 1, k, Brightness,PropogationCount);
				PropogateLight (i, j - 1, k, Brightness,PropogationCount);	
				PropogateLight (i, j, k + 1, Brightness,PropogationCount);
				PropogateLight (i, j, k - 1, Brightness,PropogationCount);
			} else {
				//Debug.LogError("Blarg!: " + Brightness);
			}
		}
	}
}
