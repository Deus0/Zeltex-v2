using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;

// Class: BlockMesher
//		Description: 	Simply converts block data into mesh data, on a thread
//					 	Then it informs the class that holds the block data when it can upload the generate mesh data to thread

namespace OldCode {
// need a class to convert MyBlocks into a Mesh
[System.Serializable]
public class BlockMesher {
	public bool CanUpdateMesh = false;
	public bool IsUpdatingMesh = false;
	public bool IsSmoothTerrain = false;
	public bool IsSubDivided = false;
	//public Vector3 CubeSize = new Vector3(1,1,1);
	public int DefaultSubDivisionLevel = 2;
	public int TileMapLength = 8;
	
	public BlockMesher() {
		
	}
	public void Update() {
		
	}
	public void UpdateMeshesOnThread(MyMesh MeshData, Blocks MyBlocks) {
		if (!IsUpdatingMesh) {
			IsUpdatingMesh = true;
			UnityThreading.ActionThread NewThread = UnityThreadHelper.CreateThread(() =>
			                                                                       {
				// thread processing
				GenerateAllMeshes(MeshData, MyBlocks);
			});
		}
	}
	
	public Mesh CreateMesh(MyMesh MeshData) {
		//UpdateMeshesOnThread ();
		Mesh NewMesh = new Mesh ();
		
		MeshData.UpdateMeshWithData(NewMesh, null, true);
		
		return NewMesh;
	}

	/*public void ClearTerrain(Blocks MyBlocks) {
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				MyBlocks.ClearBlockMesh(new Vector3(i,j,k));
			}
	}*/

	public void GenerateAllMeshes(MyMesh MeshData, Blocks MyBlocks) {
		//CreateSunlight ();
		MeshData.ClearMesh ();
		if (!IsSmoothTerrain) {
			// Just want to update any blocks that were changed and its surrounding ones
			// MyBlocks class keeps the mesh data stored in memory rather then remaking the meshes every time
			if (IsSubDivided) {
				//CreateMeshFromBlocksSubdivision (DefaultSubDivisionLevel);
			} else {
				CreateMeshFromBlocks (MyBlocks, MeshData);
			}
			for (int i = 0; i < MyBlocks.Size.x; i++)
				for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++) {
					if (MyBlocks.GetBlockType(new Vector3(i,j,k)) != 0) {
						MeshData.Add (MyBlocks.GetBlock(new Vector3(i,j,k)).GetBlockMesh());
					}
				}
			CanUpdateMesh = true;
		} else {
			//CreateSmoothTerrain ();
			//UpdateMesh ();
		}
	}
	
	/*public void CreateMeshFromBlocksSubdivision(int SubDivisionLevel) {	// example im coding for is 2
		ClearMeshes ();
		TerrainMesh.FaceCount = 0;
		//customMesh.ClearMesh();
		Debug.Log ("Adding Cubes as Models.");
		CubeCount = 0;
		float MaxSize = MyBlocks.Size.x;
		if (MaxSize < MyBlocks.Size.y)
			MaxSize = MyBlocks.Size.y;
		if (MaxSize < MyBlocks.Size.z)
			MaxSize = MyBlocks.Size.z;
		if (MaxSize % 2 == 1) 
			MaxSize += 1;
		float MaxX = MaxSize;	//MyBlocks.Size.x-(Mathf.RoundToInt(MyBlocks.Size.x) % SubDivisionLevel);
		float MaxY = MaxSize;	//MyBlocks.Size.y-(Mathf.RoundToInt(MyBlocks.Size.y) % SubDivisionLevel);
		float MaxZ = MaxSize;	//MyBlocks.Size.z-(Mathf.RoundToInt(MyBlocks.Size.z) % SubDivisionLevel);
		Debug.LogError ("MaxX: " + MaxX + " : MaxY: " + MaxY + " : MaxZ: " + MaxZ);
		for (int i = 0; i < MaxX; i += SubDivisionLevel)
			for (int j = 0; j < MaxY;  j += SubDivisionLevel)
			for (int k = 0; k < MaxZ;  k += SubDivisionLevel) {
				MyBlocks.GetBlockMesh(new Vector3(i,j,k)).ClearMesh ();
				
				int NewBlockType = GetBlockTypeInGroup(i, j, k, SubDivisionLevel);
				//Debug.LogError ("At block: " + i + ": j: " + j + ":k:" + k + " BlockType: " + NewBlockType);
				if (NewBlockType > 0) {	// if no mesh create a mesh 
					//if (MyBlocks.GetBlock(new Vector3(i,j,k)).HasChanged) {	// if no mesh create a mesh 
					MyBlocks.GetBlock(new Vector3(i,j,k)).HasChanged = false;
					CubeCount++;
					//Debug.Log ("Adding Cube number: " + CubeCount + " Pos: " + i + " : " + j + " : " + k);
					bool IsFrontFace = false;
					bool IsBackFace = false;
					bool IsLeftFace = false;
					bool IsRightFace = false;
					bool IsTopFace = false;
					bool IsBottomFace = false;
					
					// the bug is definitely here in the culling
					
					//if (i+SubDivisionLevel <= MaxX)
					if (!IsBlockInGroup(i+SubDivisionLevel+1,j,k,SubDivisionLevel)) // should be something that searches the chunk, then searches the world which is referenced in chunk
						IsRightFace = true;
					//if (i-SubDivisionLevel >= 0)
					if (!IsBlockInGroup(i-SubDivisionLevel-1,j,k,SubDivisionLevel)) 
						IsLeftFace = true;
					
					//if (k+SubDivisionLevel <= MaxZ)
					if (!IsBlockInGroup(i,j,k+SubDivisionLevel+1,SubDivisionLevel)) 
						IsFrontFace = true;
					//if (k-SubDivisionLevel >= 0)
					if (!IsBlockInGroup(i,j,k-SubDivisionLevel-1,SubDivisionLevel))
						IsBackFace = true;
					
					//if (j+SubDivisionLevel <=MaxY)
					if (!IsBlockInGroup(i,j+SubDivisionLevel+1,k,SubDivisionLevel))
						IsTopFace = true;
					//if (j-SubDivisionLevel >= 0)
					if (!IsBlockInGroup(i,j-SubDivisionLevel-1,k,SubDivisionLevel))
						IsBottomFace = true;
					
					IsLeftFace = true;
						IsRightFace = true;
						IsBottomFace = true;
						IsTopFace = true;
						IsBackFace = true;	// this one faces cam
						IsFrontFace = true;
					
					
					Vector3 TemporaryCubeSize = new Vector3(CubeSize.x*SubDivisionLevel, CubeSize.y*SubDivisionLevel,CubeSize.z*SubDivisionLevel);
					
					Vector3 NewCubePosition = new Vector3 (((float)(i)) * TemporaryCubeSize.x/((float)SubDivisionLevel),
					                                       ((float)(j)) * TemporaryCubeSize.y/((float)SubDivisionLevel), 
					                                       ((float)(k)) * TemporaryCubeSize.z/((float)SubDivisionLevel));
					//MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount = TerrainMesh.FaceCount;	// has to correspond to its positioning in the mesh
					
					int TileIndex = NewBlockType;	//MyBlocks.GetBlockType (new Vector3(i,j,k));
					//TileIndex = 2;
					Vector2 TilesPosition = new Vector2();
					// Every Column
					TilesPosition.x = (TileIndex % MaxTiles);	// 4 = 3,1  - 5 = 2,1 - 6 = 1,1 - 7 = 0,1
					// Every Row
					TilesPosition.y = (TileIndex / MaxTiles);		// 4 - 1 = 5 - 1
					
					//Debug.LogError ("Now updating with tile index: " + TileIndex + " : At TilePosition: " + TilesPosition.ToString());
					
					MyBlocks.GetBlockMesh(new Vector3(i,j,k)).CreateCube (NewCubePosition,
					                                                      TemporaryCubeSize, 
					                                                      IsFrontFace, IsBackFace, IsLeftFace, IsRightFace, IsTopFace, IsBottomFace, 
					                                                      TilesPosition, 
					                                                      new Color32(255,255,255,255));
					
					// MyBlocks.Data[i].Data[j].Data[k].MyColor);
					
					//MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount -= TerrainMesh.FaceCount;
					//TerrainMesh.FaceCount += MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount;
					
					//TerrainMesh.Add (MyBlocks.Data [i].Data [j].Data [k].BlockMesh);
				} else {
					//Debug.LogError ("At block: " + i + ": j: " + j + ":k:" + k + " BlockType: " + NewBlockType);
				}
			}
	}*/
	// creates the mesh - should keep track of the mesh
	public void CreateMeshFromBlocks(Blocks MyBlocks, MyMesh TerrainMesh) {
		TerrainMesh.FaceCount = 0;
		
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				//MyBlocks.Data [i].Data [j].Data [k].BlockMesh.ClearMesh ();
				//if (MyBlocks.Data [i].Data [j].Data [k].Type != 0) 
				if (MyBlocks.GetBlock(new Vector3(i,j,k)).HasChanged()) { //has no mesh create a mesh 
					MyBlocks.ClearBlockMesh(new Vector3(i,j,k));
					//Debug.Log ("Adding Cube number: " + CubeCount + " Pos: " + i + " : " + j + " : " + k);
					bool IsFrontFace = false;
					bool IsBackFace = false;
					bool IsLeftFace = false;
					bool IsRightFace = false;
					bool IsTopFace = false;
					bool IsBottomFace = false;
					
					if (i != MyBlocks.Size.x - 1)
						if (MyBlocks.GetBlockType (new Vector3 (i + 1, j, k)) == 0)	// should be something that searches the chunk, then searches the world which is referenced in chunk
							IsRightFace = true;
					if (i != 0) 
						if (MyBlocks.GetBlockType (new Vector3 (i - 1, j, k)) == 0)
							IsLeftFace = true;
					if (k != MyBlocks.Size.z - 1) 	// if not on edge
						if (MyBlocks.GetBlockType (new Vector3 (i, j, k + 1)) == 0)
							IsFrontFace = true;
					if (k != 0)
						if (MyBlocks.GetBlockType (new Vector3 (i, j, k - 1)) == 0)
							IsBackFace = true;
					if (j != MyBlocks.Size.y - 1)
						if (MyBlocks.GetBlockType (new Vector3 (i, j + 1, k)) == 0)
							IsTopFace = true;
					if (j != 0)
						if (MyBlocks.GetBlockType (new Vector3 (i, j - 1, k)) == 0)
							IsBottomFace = true;
					if (i == 0)
						IsLeftFace = true;
					if (i == MyBlocks.Size.x - 1)
						IsRightFace = true;
					if (j == 0)
						IsBottomFace = true;
					if (j == MyBlocks.Size.y - 1)
						IsTopFace = true;
					if (k == 0)
						IsBackFace = true;
					if (k == MyBlocks.Size.z - 1)
						IsFrontFace = true;
					
					Vector3 NewCubePosition = new Vector3 (((float)(i)) * MyBlocks.Scale.x, 
					                                       ((float)(j)) * MyBlocks.Scale.y, 
					                                       ((float)(k)) * MyBlocks.Scale.z);
					//MyBlocks.Data [i].Data [j].Data [k].BlockMesh.FaceCount = TerrainMesh.FaceCount;
					
					int TileIndex = MyBlocks.GetBlockType (new Vector3(i,j,k));
					//TileIndex = 2;
					Vector2 TilesPosition = new Vector2();
					// Every Column
					TilesPosition.x = (TileIndex % TileMapLength);	// 4 = 3,1  - 5 = 2,1 - 6 = 1,1 - 7 = 0,1
					// Every Row
					TilesPosition.y = (TileIndex / TileMapLength);		// 4 - 1 = 5 - 1
					
					//Debug.LogError ("Now updating with tile index: " + TileIndex + " : At TilePosition: " + TilesPosition.ToString());
					
					MyBlocks.GetBlockMesh(new Vector3(i,j,k)).CreateCube (NewCubePosition, 
					                                                      MyBlocks.Scale, 
					                                                          IsFrontFace, IsBackFace, IsLeftFace, IsRightFace, IsTopFace, IsBottomFace, 
					                                                          TilesPosition, 
					                                                          new Color32(255,255,255,255));
					// MyBlocks.Data[i].Data[j].Data[k].MyColor);
				}
			}
	}
	void CreateSmoothTerrain(MyMesh TerrainMesh, MarchingCubes marchingCubes, Blocks MyBlocks) {
		Debug.Log ("Creating Smooth Terrain");
		Vector3 BlockScale = MyBlocks.Scale;
		// Marching cubes is one too small!
		// for some reason the size has to be like this
		Vector3 WorldSize_ = MyBlocks.Size;
		int expand = 0;// WorldCore->MarchingExpand;
		WorldSize_.x += expand;
		WorldSize_.y += expand;
		WorldSize_.z += expand;
		// = 18 now out of 16 0 to 15
		// I need to perform this calculation PER CUBE and not per grid
		// So perform calculation depending on surrounding cubes, but not whole thing
		//MarchingCubesData.Clear();
		//for (int i = 0; i < (WorldSize_.x)*(WorldSize_.y)*(WorldSize_.z); i++)
		//	MarchingCubesData.Add(0.0f);
		//marchingCubes.ClearData(MarchingCubesData);
		marchingCubes = new MarchingCubes(new Vector3(WorldSize_.x, WorldSize_.x, WorldSize_.x), Mathf.RoundToInt(WorldSize_.x));
		marchingCubes.PolygonizeData(MyBlocks, WorldSize_);
		
		TerrainMesh.ClearMesh ();
		for (int i = 0; i < marchingCubes.trilist.Count; i++) {	// for every triangle in list
			marchingCubes.trilist[i].calcnormal(true);
			
			TerrainMesh.Verticies.Add (marchingCubes.trilist[i].position[0]);
			TerrainMesh.Verticies.Add (marchingCubes.trilist[i].position[1]);
			TerrainMesh.Verticies.Add (marchingCubes.trilist[i].position[2]);
			
			TerrainMesh.Colors.Add (marchingCubes.trilist[i].colors[0]);
			TerrainMesh.Colors.Add (marchingCubes.trilist[i].colors[1]);
			TerrainMesh.Colors.Add (marchingCubes.trilist[i].colors[2]);
			
			TerrainMesh.Indicies.Add (TerrainMesh.Verticies.Count-3);
			TerrainMesh.Indicies.Add (TerrainMesh.Verticies.Count-2);
			TerrainMesh.Indicies.Add (TerrainMesh.Verticies.Count-1);
		}
		Debug.Log ("Created Smooth Terrain");
	}
}
}