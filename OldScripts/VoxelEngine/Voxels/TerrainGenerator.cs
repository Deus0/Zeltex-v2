/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;


// responsible for generating a chunk
// relies on SimplexNoise class

[System.Serializable]
public class TerrainGenerator : MonoBehaviour {
	public List<Vector3> ToSpawnBlocks = new List<Vector3>();
	public bool HasUpdated = false;
	// handles all the block manipulations
	public Blocks MyBlocks = new Blocks();
	public int CubeCount = 0;
	//public Vector3 MapSize = new Vector3(16,16,16);
	public Vector3 CubeSize = new Vector3(0.5f, 0.5f, 0.5f);
	public bool IsRefresh = false;
	public GameObject DestroyedBlock;
	//private CustomMesh CustomMeshScript;
	//private int[,,] BlockTypes = new int[16,16,16];
	// Terrain Generation
	public float NoiseScale = 0.1f;
	public float Amplitude = 10f;
	public float exp = 1f;
	public bool HasLoaded = false;
	public int DirtBlockType = 1;
	public int GrassBlockType = 2;
	//public MyMesh TerrainMesh = new MyMesh ();
	public bool IsSmoothTerrain = true;
	public MarchingCubes marchingCubes;
	//public List<int> TestBlockTypes = new List<int>();
	public int MaxTiles = 8;
	public int DefaultSubDivisionLevel = 1;
	public bool IsSubDivided = false;
	public BlockMesher MyBlockMesher = new BlockMesher ();
	private Player MyLocalPlayer;
	public bool TestUpdateBlock = false;
	public Vector3 TestUpdateBlockPosition;
	public int TestUpdateBlockType = 1;
	// Update is called once per frame

	~TerrainGenerator() {
		//TerrainMesh.ClearMesh ();
		//CustomMeshScript.UpdateMesh ( new MyMesh());
		MyBlocks.ClearData ();
	}
	// Use this for initialization
	void Start () {
		MyBlocks.ClearData();
		MyBlocks = new Blocks ();
		MyBlocks.InitilizeData ();
		//if (!HasLoaded)
		//HasLoaded = false;
		//LoadTerrain ();
	}

	public void CheckForCameraMouseHit() {
		if (MyLocalPlayer != null) {
			//if (Input.GetKeyDown (KeyCode.G)) {
			//	UpdateBlockWithRay (MyLocalPlayer.MouseHit,1);
			//}
		} else {
			BaseCharacter MyLocalCharacter = GetManager.GetCharacterManager ().GetLocalPlayer ();
			if (MyLocalCharacter)
				MyLocalPlayer = MyLocalCharacter.gameObject.GetComponent<Player> ();
		}
	}

	void Update () {
		if (TestUpdateBlock) {
			UpdateBlock(TestUpdateBlockPosition, TestUpdateBlockType);
			TestUpdateBlock = false;
		}
		CheckForCameraMouseHit ();
		if (IsRefresh) {
			IsRefresh = false;
			MyBlockMesher.ClearTerrain(MyBlocks);
			LoadTerrain();
		}
		UpdateBlockMesh ();
	}

	public void UpdateBlockMesh() {
		// ----- Updating Blocks-Meshes -----
		CheckForBlocksToSpawn ();	
		MyBlockMesher.Update ();
		// if has changed block data, and block mesher is not creating a mesh
		if (HasUpdated && !MyBlockMesher.IsUpdatingMesh) {
			//MyBlockMesher.UpdateMeshesOnThread (TerrainMesh, MyBlocks);
			HasUpdated = false;
		}
		if (MyBlockMesher.CanUpdateMesh) {	// has finished generating mesh data
			MyBlockMesher.CanUpdateMesh = false;
			UpdateMesh ();
			MyBlockMesher.IsUpdatingMesh = false;
		}
	}

	public void UpdateMesh() {
		if (gameObject.GetComponent<SkinnedMeshRenderer> ()) {
			//Debug.LogError ("Updating Terrain Mesh: " + Time.time);
			//gameObject.GetComponent<CustomBones>().MySkinnedMesh.GetSkinnedDataFromMesh();
			gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh = new Mesh ();
			//TerrainMesh.UpdateMeshWithData (gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh, gameObject.GetComponent<MeshCollider> ());
			//TerrainMesh.UpdateMeshWithData (gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh, gameObject.GetComponent<MeshCollider> ());
		} else {
			//TerrainMesh.UpdateMeshWithData (gameObject.GetComponent<MeshFilter> ().mesh, gameObject.GetComponent<MeshCollider> ());
		}
	}

	void LoadTerrain() {
		CreateTerrainData ();
		//MyBlockMesher.UpdateMeshesOnThread (TerrainMesh, MyBlocks);
	}

	public void CheckForBlocksToSpawn() {
		if (!HasUpdated) {
			if (ToSpawnBlocks.Count != 0) {
				SpawnBlock(ToSpawnBlocks[0], 1);
				ToSpawnBlocks.RemoveAt (0);
			}
		}
	}

	public void SpawnBlock(Vector3 SpawnPosition, int BlockType) {	// later on i will spawn a model dependent on the destroyed block
		if (DestroyedBlock != null)
			Instantiate (DestroyedBlock, SpawnPosition, Quaternion.identity);
	}

	public bool UpdateBlockWithRay2(RaycastHit RayHit, int BlockType) {
		Vector3 BlockPosition = GetBlockPosition(RayHit.point-RayHit.normal*0.01f);
		Vector3 WorldPosition = GetWorldBlockPosition(BlockPosition);
		return UpdateBlock(BlockPosition,BlockType);
	}

	public bool UpdateBlockWithRay(RaycastHit RayHit, int BlockType) {
		Vector3 BlockPosition = GetBlockPosition(RayHit.point+RayHit.normal*0.01f);
		Vector3 WorldPosition = GetWorldBlockPosition(BlockPosition);
		return UpdateBlock(BlockPosition,BlockType);
	}

	public Vector3 GetBlockPosition(Vector3 WorldPosition) {
		WorldPosition = transform.InverseTransformPoint (WorldPosition);
		//WorldPosition
		// scaling here
		WorldPosition.x /= CubeSize.x;
		WorldPosition.y /= CubeSize.y;
		WorldPosition.z /= CubeSize.z;
		if (TerrainMesh.IsCentred) {
			WorldPosition -= TerrainMesh.DifferenceBounds;
		}
		WorldPosition.x = Mathf.FloorToInt (WorldPosition.x);
		WorldPosition.y = Mathf.FloorToInt (WorldPosition.y);
		WorldPosition.z = Mathf.FloorToInt (WorldPosition.z);
		return WorldPosition;
	}

	// Converts blockPosition to world position
	public Vector3 GetWorldBlockPosition(Vector3 BlockPosition) {
		Vector3 WorldPosition = BlockPosition;
		//WorldPosition += transform.position;
		WorldPosition = transform.TransformPoint (WorldPosition);
		WorldPosition.x *= CubeSize.x;
		WorldPosition.y *= CubeSize.y;
		WorldPosition.z *= CubeSize.z;
		//WorldPosition.x  *= transform.localScale.x;
		//WorldPosition.y *= transform.localScale.y;
		//WorldPosition.z *= transform.localScale.z;
		if (TerrainMesh.IsCentred) {
			WorldPosition += TerrainMesh.DifferenceBounds;
		}
		WorldPosition.x = Mathf.FloorToInt (WorldPosition.x);
		WorldPosition.y = Mathf.FloorToInt (WorldPosition.y);
		WorldPosition.z = Mathf.FloorToInt (WorldPosition.z);
		// account for the transform of the gameobject
		return WorldPosition;
	}

	public bool UpdateBlock(Vector3 BlockPosition, int BlockType) {
		Debug.Log ("Updating Block: " + BlockType.ToString() + " At time: " + Time.time);

			HasUpdated = MyBlocks.UpdateBlock (BlockPosition, BlockType);
				//if (BlockType == 0) {
					// Convert blockPosition to world position
					//Vector3 SpawnPosition = GetWorldBlockPosition(BlockPosition);
					//ToSpawnBlocks.Add (SpawnPosition);
				//}
			return HasUpdated;
	}
	
	public float TerrainNoise(int x, int y, int z, float scale, float max)
	{
		return ((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
	}
	public bool IsRoundedTerrain = false;
	// Generates the initial terrain
	void CreateTerrainData() {
		if (!HasLoaded) {
			MyBlocks = new Blocks();
			Vector3 MapSize = MyBlocks.Size;
			MyBlocks.InitilizeData();
			for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++) {
				for (int k = 0; k < MyBlocks.Size.z; k++) {
					//float RandomHeight = Random.Range (0, MapSize.y);
					float RandomHeight = TerrainNoise(i, j, k, NoiseScale, Amplitude);
				//for (int j = 0; j < MapSize.y; j++) {
					//MyBlocks.Data[i].Data[j].Data[k].Type  = 0;
						//if (j == 1 + RandomHeight) {
						//MyBlocks.Data[i].Data[j].Data[k].Type  = GrassBlockType;
						//MyBlocks.Data[i].Data[j].Data[k].MyColor = new Color32(255,255,255,255);
						//}
					if (IsRoundedTerrain) {
						Vector3 DifferenceFromMid = new Vector3(Mathf.Abs(MapSize.x/2-i),Mathf.Abs(MapSize.y/2-j),Mathf.Abs(MapSize.z/2-k));
						if (DifferenceFromMid.magnitude <= RandomHeight) {
							MyBlocks.UpdateBlock (new Vector3(i,j,k),DirtBlockType);	//Data[i].Data[j].Data[k].Type  = DirtBlockType;
							//MyBlocks.Data[i].Data[j].Data[k].MyColor = new Color32(0,255,0,255);
						}
					}
					else {
						Vector3 DifferenceFromMid = new Vector3(Mathf.Abs(MapSize.x/2-i),Mathf.Abs(-j),Mathf.Abs(MapSize.z/2-k));
						if (DifferenceFromMid.magnitude <= RandomHeight || j == 0) {
							MyBlocks.UpdateBlock (new Vector3(i,j,k),DirtBlockType);
							//MyBlocks.Data[i].Data[j].Data[k].MyColor = new Color32(0,255,0,255);
						}
					}
					}
				}
			HasLoaded = true;
		}
		//CreateSunlight ();
	}
	public void ClearMeshes() {
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++) {
					MyBlocks.ClearBlockMesh(new Vector3(i,j,k));
				}
	}
	public void ClearAirMeshes() {
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				if (MyBlocks.GetBlockType(new Vector3(i,j,k)) == 0)  {
					MyBlocks.ClearBlockMesh(new Vector3(i,j,k));
				}
			}
	}
	// with subdivision its like normal
	// only it checks a fraction of the blocks
	// example: 2x2x2 - 8 blocks, if any of them have 
	public bool IsBlockInGroup(int i, int j, int k, int SubDivisionLevel) {
		if (GetBlockTypeInGroup (i, j, k, SubDivisionLevel) == 0)
			return false;
		else
			return true;
	}
	
	public int GetBlockTypeInGroup(int i, int j, int k, int SubDivisionLevel) {
		for (int i2 = i; i2 < i+SubDivisionLevel; i2++)
			for (int j2 = j; j2 <  j+SubDivisionLevel; j2++)
				for (int k2 = k; k2 <  k+SubDivisionLevel; k2++) {
					int BlockType = MyBlocks.GetBlockType(new Vector3(i2,j2,k2));
					if (BlockType > 0) 	// if not empty
						return BlockType;
			}
		return 0;
	}
	public void IncreaseSubDivision() {
		IsSubDivided = !IsSubDivided;
		IsRefresh = true;
	}

	
	
	//FaceCount += MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount;
	//FaceCount = TerrainMesh.FaceCount;
	//MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount -= FaceCount;
	// why does MyMesh need terrains facecount 
	//TerrainMesh.FaceCount += MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount;

	public void CreateSunlight() {
		Color32 SunColor = new Color32 (255, 255, 255, 255);
		for (int i = 0; i < MyBlocks.Size.x; i++)
		for (int k = 0; k < MyBlocks.Size.z; k++) {
			// for each height
			float SunlightHeightStart =  (MyBlocks.Size.y) - 1;
			for (float j = SunlightHeightStart; j > 0; j--) {
				MyBlocks.UpdateLights(new Vector3(i,j,k), SunColor);
				if (MyBlocks.GetBlockType (new Vector3(i,j-1,k)) != 0)
					MyBlocks.UpdateLights(new Vector3(i,j-1,k), SunColor);
					break; //j = 0;	// skip to next column
				}
			}

}*/
