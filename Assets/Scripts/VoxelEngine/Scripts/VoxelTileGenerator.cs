using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine 
{
	[System.Serializable]
	public class VoxelMeta 
	{
		public string Name = "Block";
		public int ModelID;			// various models, ie cube, slopes, etc
		public int TextureMapID;	// various texture maps, ie grass, etc
		public int MaterialID;		// things like animated water, animated power cores, or normal dirt etc

	}
	[System.Serializable]
	public class VoxelModel 
	{
		public List<MeshData> MyModels = new List<MeshData> ();
		public VoxelModel() 
		{
			GenerateCubeMesh ();
		}

		// i should switch the 6 sides to just a plane being rotated
		public void GenerateCubeMesh() 
		{
			MeshData MeshFaceUp = new MeshData ();
			float x, y, z;
			x = 0; y = 0; z = 0;
			x += 0.5f; y += 0.5f; z += 0.5f;

			// face up
			MeshFaceUp.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			MeshFaceUp.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			MeshFaceUp.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			MeshFaceUp.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			MeshFaceUp.AddQuadTriangles();
			MyModels.Add (MeshFaceUp);
			
			MeshData MeshFaceDown = new MeshData ();
			//public MeshData FaceDataDown(float x, float y, float z, MeshData meshData, int MaterialType, byte Brightness)
			{
				MeshFaceDown.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceDown.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceDown.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceDown.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceDown.AddQuadTriangles();
			}
			MyModels.Add (MeshFaceDown);
			
			MeshData MeshFaceNorth = new MeshData ();
			//public MeshData FaceDataNorth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
			{
				MeshFaceNorth.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceNorth.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
				MeshFaceNorth.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
				MeshFaceNorth.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceNorth.AddQuadTriangles();
			}
			MyModels.Add (MeshFaceNorth);
			MeshData MeshFaceSouth = new MeshData ();
			//public MeshData FaceDataSouth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
			{
				MeshFaceSouth.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceSouth.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
				MeshFaceSouth.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
				MeshFaceSouth.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceSouth.AddQuadTriangles();
			}
			MyModels.Add (MeshFaceSouth);
			
			MeshData MeshFaceEast = new MeshData ();
			//public MeshData FaceDataEast(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
			{
				MeshFaceEast.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceEast.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
				MeshFaceEast.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
				MeshFaceEast.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceEast.AddQuadTriangles();
			}
			MyModels.Add (MeshFaceEast);
			
			MeshData MeshFaceWest = new MeshData ();
			//public MeshData FaceDataWest(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
			{
				MeshFaceWest.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
				MeshFaceWest.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
				MeshFaceWest.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
				MeshFaceWest.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
				MeshFaceWest.AddQuadTriangles();
			}
			MyModels.Add (MeshFaceWest);
		}
	}
	[ExecuteInEditMode]
	public class VoxelTileGenerator : MonoBehaviour 
	{
		public List<VoxelMeta> Data = new List<VoxelMeta> ();
		public List<VoxelModel> MyModels = new List<VoxelModel>();
		public Texture2D MyTileMap;
		public Material MyMaterial;
		public List<Texture2D> MyTextures = new List<Texture2D>();

		// used to convert meshes into voxel data
		// make sure bounds are in between 0 and 1 - else scale+position by maximum difference
		public List<Mesh> MyMeshes = new List<Mesh> ();
		[Header("Actions")]
		public bool IsGenerateModels = false;
		public int TileResolution = 4;
		public bool IsGenerateTileMap = false;

		void Start() {
			//GenerateVoxelModels ();
		}
		void Update() {
			if (IsGenerateModels) {
				IsGenerateModels = false;
				GenerateVoxelModels();
			}
			if (IsGenerateTileMap) {
				IsGenerateTileMap = false;
				MyTileMap = TileMapGenerator.CreateTileMap(VoxelSaver.GetWorldFolderPath(GetComponent<VoxelSaver>().SaveFileName) + "TileMap", MyTextures, MyTileMap, TileResolution);
				// Create block meta data
				Data.Clear();
				for (int i = 0; i < MyTextures.Count; i++) 
				{
					VoxelMeta MyMeta = new VoxelMeta();
					MyMeta.Name = MyTextures[i].name;
					MyMeta.TextureMapID = i;
					Data.Add (MyMeta);
				}
				MyMaterial.mainTexture = MyTileMap;
			}

			if (MyTileMap == null)
			{
				string FileName = VoxelSaver.GetWorldLocalPath (GetComponent<VoxelSaver> ().SaveFileName) + "TileMap";
				//FileName = FileUtil.GetFolderPath () + "TileMap";
				//Debug.LogError ("Loading from: " + FileName);
				MyTileMap = Resources.Load (FileName, typeof(Texture2D)) as Texture2D;
			}

			if (MyMaterial.mainTexture != MyTileMap)
			{
				MyMaterial.mainTexture = MyTileMap;
			}
		}
		public void GenerateVoxelModels() {
			Data.Clear ();
			MyModels.Clear ();
			VoxelMeta NewMeta = new VoxelMeta ();
			Data.Add (NewMeta);
			VoxelModel NewModel = new VoxelModel ();
			MyModels.Add (NewModel);
		}

		// maybe take in light sources too
		public MeshData GetModel(bool[] IsSide, int VoxelIndex, int MaterialType, int[] MyLights, bool[] IsSolid)
		{ 
			MeshData MyVoxelMesh = new MeshData ();

			if (VoxelIndex >= Data.Count || VoxelIndex < 0 || Data.Count == 0)
				return MyVoxelMesh;
			VoxelMeta MyMeta = Data [VoxelIndex];
			//if (VoxelIndex == MaterialType+ 1)
			if (MaterialType == MyMeta.MaterialID)
			{
				//Debug.LogError ("Doing the thing");
				//VoxelIndex = Mathf.Clamp (VoxelIndex, 0, Data.Count-1);
				int MyModelID = Mathf.Clamp (MyMeta.ModelID, 0, MyModels.Count-1);
				for (int i = 0; i < IsSide.Length; i++) 
				{
					if (IsSide[i]) 
					{
						byte ThisLight = (byte)MyLights[i];
						MyVoxelMesh.Add (MyModels [MyModelID].MyModels[i]);
						MyVoxelMesh.AddQuadUVs(MyMeta.TextureMapID, TileResolution);
						MyVoxelMesh.AddQuadColours(ThisLight);
					}
				}
				/*
				List<Vector3> LightPositions;
				for (int z = 0; z < MyVoxelMesh.Verticies.Count; z++) 
				{

					MyVoxelMesh.AddQuadColours(ThisLight);
				}*/
			}
			return MyVoxelMesh;
		}
	}
}
