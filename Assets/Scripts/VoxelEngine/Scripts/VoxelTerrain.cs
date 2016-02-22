using UnityEngine;
using System.Collections;

namespace VoxelEngine 
{
	[System.Serializable]
	public class TerrainMetaData
	{
		[SerializeField] public float BaseHeight = 16;
		[SerializeField] public float MinimumHeight = 1;
		[SerializeField] public float Amplitude = 0.5f;
		[SerializeField] public float Frequency = 0.05f;
		[SerializeField] public Vector3 WorldOffset = new Vector3();
		[SerializeField] public float Frequency2 = 0.05f;
		public TerrainMetaData()
		{

		}
	}

	[ExecuteInEditMode]
	[RequireComponent(typeof (World))]
	public class VoxelTerrain : MonoBehaviour {
		private World MyWorld;
		public bool IsDebugMode = false;
		public KeyCode LoadTerrainKey;

		// Terrain Generation Settings
		[Header("Terrain Generation")]
		[SerializeField] private bool IsCreateTerrain;
		[SerializeField] private TerrainMetaData MyTerrainMetaData;
		// Sphere
		[Header("Sphere Generation")]
		[SerializeField] private bool IsCreateNoiseSphere;
		[SerializeField] private float SphereRadius = 24f;
		[SerializeField] private Vector3 SphereOffset = new Vector3();
		[SerializeField] private float SphereNoiseCutoff = 0.5f; 
		[SerializeField] private float SphereMultiplier = 2f;
		// Plane
		[Header("Plane Generation")]
		[SerializeField] private bool IsCreatePlane;
		[SerializeField] private int CreatePlaneWidth = 5;
		[SerializeField] private int CreatePlaneDepth = 5;
		[SerializeField] private int PlanePosition = 0;
		// Cube
		[Header("Cube Generation")]
		[SerializeField] private bool IsCreateCube;
		[SerializeField] private int CreateCubeHeight = 5;

		// Use this for initialization
		void Start () {
			MyWorld = GetComponent<World> ();
		}
		
		void Update() {
			if (IsDebugMode)
			{
				if (Input.GetKeyDown (LoadTerrainKey))
					IsCreateTerrain = true;
			}
			HandleWorldActions ();
		}

		public void HandleWorldActions() {	
			if (IsCreateTerrain) {
				IsCreateTerrain = false;
				CreateTerrain();
			}
			else if (IsCreatePlane) {
				IsCreatePlane = false;
				CreateVoxelPlane (PlanePosition, 1);
				//MyWorld.UpdateAllChunks ();
			} else if (IsCreateCube) {
				IsCreateCube = false;
				for (int i = 0; i < CreateCubeHeight; i++)
					CreateVoxelPlane (i, 1);
				//MyWorld.UpdateAllChunks ();
			} 
			else if (IsCreateNoiseSphere) {
				IsCreateNoiseSphere = false;
				CreateNoiseSphere();
				//MyWorld.UpdateAllChunks ();
			}
		}
		public void CreateTerrain() {
			CreateTerrain (MyWorld, MyTerrainMetaData);
		} 
		public static void CreateTerrain(World MyWorld) {
			CreateTerrain (MyWorld, new TerrainMetaData());
		}
		public static void CreateTerrain(World MyWorld, TerrainMetaData MyMeta) 
		{
			foreach (var element in MyWorld.MyChunkData) {
				InitializeTerrain (element.Value, MyMeta);
			}
		}
		
		public static void InitializeTerrain(Chunk MyChunk, TerrainMetaData MyMeta)
		                                    // Vector3 WorldOffset, float Frequency, float Amplitude,
		                                    // float MinimumHeight, float BaseHeight, float Frequency2) 
		{
			if (MyChunk == null)
				return;
			Vector3 HillPosition = new Vector3 (16, 16, 16);
			for (int i = 0; i < Chunk.ChunkSize; i++) {
				for (int j = 0; j < Chunk.ChunkSize; j++) {
					for (int k = 0; k < Chunk.ChunkSize; k++) {
						Vector3 MyPosition = MyMeta.WorldOffset + MyChunk.Position.GetVector()*Chunk.ChunkSize + new Vector3(i,j,k);
						//float MyBasicNoise = MyWorld.Amplitude*SimplexNoise.Noise.Generate(MyPosition.x*MyWorld.Frequency, MyPosition.z*MyWorld.Frequency);
						float MyBasicNoise = SimplexNoise.Noise.Generate(MyPosition.x* MyMeta.Frequency,
						                                                 MyPosition.y* MyMeta.Frequency,  
						                                                 MyPosition.z* MyMeta.Frequency);
						float StretchedNoise = MyBasicNoise* MyMeta.Amplitude;
						//MyBasicNoise *= 4*Vector3.Distance(Position.GetVector()*Chunk.ChunkSize + new Vector3(i,j,k),
						//                                 new Vector3(HillPosition.x, Position.y*Chunk.ChunkSize+j, HillPosition.z))/HillPosition.y;	// percentage of total height of hill?
						StretchedNoise += MyMeta.BaseHeight;

						if (StretchedNoise <= MyMeta.MinimumHeight)
							StretchedNoise = MyMeta.MinimumHeight;

						if (MyChunk.Position.y*Chunk.ChunkSize+j <= StretchedNoise)	// height of land
							MyChunk.UpdateBlockType(i,j,k, 1);
						
						float MyBasicNoise2 = SimplexNoise.Noise.Generate(MyPosition.x*MyMeta.Frequency2,
						                                                  MyPosition.y*MyMeta.Frequency2,  
						                                                  MyPosition.z*MyMeta.Frequency2);
						if (MyBasicNoise2 >= 0.6f) 
							MyChunk.UpdateBlockType(i,j,k, 1);
						
						/*float MyNoise = MyWorld.Amplitude*SimplexNoise.Noise.Generate(Position.x+i*MyWorld.Frequency,
						                                                              Position.y+j*MyWorld.Frequency,
						                                                              Position.z+k*MyWorld.Frequency);
						MyNoise = (MyNoise+(1f-(j+1)/Chunk.ChunkSize)+
						           ((Vector3.Distance(new Vector3(i,Chunk.ChunkSize-1,k),new Vector3(i,j,k)))/Chunk.ChunkSize))/3f;	// 0 - 1 * 0 to 8

						if (MyNoise > 0.5f)
							UpdateBlockType(i,j,k, 1);*/
						
					}
				}
			}
		}

		public void CreateVoxelPlane(int y, int Type) 
		{
			for (int x = 0; x < CreatePlaneWidth; x++) 
			{
				for (int z = 0; z < CreatePlaneDepth; z++) 
				{
					MyWorld.UpdateBlockType(x,y,z, Type);
				}
			}
		}
		public void CreateNoiseSphere() 
		{
			Chunk MyChunk = MyWorld.MyChunkData [new ChunkPosition (0, 0, 0)];
			for (float i = -SphereRadius; i <= SphereRadius; i++) {
				for (float j = -SphereRadius; j <= SphereRadius; j++) {
					for (float k = -SphereRadius; k <= SphereRadius; k++) {
						float MyNoise = SimplexNoise.Noise.Generate (i, j, k);
						MyNoise = (MyNoise / SphereMultiplier + SphereMultiplier * ((SphereRadius - Vector3.Distance (new Vector3 (0, 0, 0), new Vector3 (i, j, k))) / SphereRadius)) / 2f;	// 0 - 1 * 0 to 8
						if (MyNoise >= 1 - SphereNoiseCutoff)
							MyWorld.UpdateBlockType (SphereOffset.x + i, 
							                         SphereOffset.y + j, 
							                         SphereOffset.z + k, 1);
					}
				}
			}
		}
	}
}
