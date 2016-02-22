using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimplexNoise;

namespace OldCode {
[Serializable]
public class BlockDamaged : Block {
	public BlockDamageGui MyBlockDamage;
}

[Serializable]
public class Block : BlockBase {
	public static int MaxBlockFaces = 8;
	
	public Block()
		: base()
	{
		
	}
	public Block(int NewIndex)
		: base()
	{
		TileIndex = NewIndex;
	}
	public int GetBlockIndex() {
		return TileIndex;
	}
	public void SetBlockIndex(int NewIndex) {
		TileIndex = NewIndex;
	}
	public override Tile TexturePosition(Direction direction)
	{
		Tile tile = new Tile();
		// Every Column
		tile.x = (TileIndex % MaxBlockFaces);	// 4 = 3,1  - 5 = 2,1 - 6 = 1,1 - 7 = 0,1
		// Every Row
		tile.y = (TileIndex / MaxBlockFaces);		// 4 - 1 = 5 - 1
		
		return tile;
	}
	
	// each side will take in 2 variables, the block models
	// they will then output a solidity value
	public override bool IsSolid(Chunk chunk, Direction direction, int CycleNumber)
	{
		if (TileIndex == 4 && CycleNumber == 0)
			return false;
		//if (TileIndex != 4 && CycleNumber == 1)
			//return false;
		if (TileIndex == 11)	// lighting
			return false;

		BlockModel MyModel = chunk.DataManager.GetBlockModel(TileIndex);
		return MyModel.GetSolidity (direction);
	}

	// converts the block model into mesh data
	// block model stores 6 different meshes that correspond to sides
	protected override MeshData GetFace(Direction direction, Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber) {
		if ((CycleNumber == 0 && TileIndex != 4) || (CycleNumber == 1 && TileIndex == 4)) {
			if (chunk.DataManager == null)  {
				Debug.LogError("Chunk.DataManager ?,? is null inside mesh creation");
				return meshData;
			}
			BlockModel MyModel = chunk.DataManager.GetBlockModel (TileIndex);
			if (MyModel == null) {
				Debug.LogError("MyModel is null inside mesh creation");
				return meshData;
			}
			MyMesh FaceModel = MyModel.GetModel (direction);
			if (FaceModel == null) {
				Debug.LogError("My FaceModel! is null inside mesh creation");
				return meshData;
			}
			BlockData MyBlockData = chunk.DataManager.GetBlockData (TileIndex);
			if (MyBlockData == null) {
				Debug.LogError("MyBlockData is null inside mesh creation");
				return meshData;
			}

			// reposition indicies by the the previously added vertice count
			int TrianglesBuffer = meshData.vertices.Count;
			for (int i = 0; i < FaceModel.Indicies.Count; i++) {
				meshData.triangles.Add (TrianglesBuffer + FaceModel.Indicies [i]);
				if (chunk.IsCollisions)
					meshData.colTriangles.Add (TrianglesBuffer + FaceModel.Indicies [i]);
			}
			// reposition meshes by the grid
			for (int i = 0; i < FaceModel.Verticies.Count; i++) {
				Vector3 NewVert = new Vector3 (x, y, z) + FaceModel.Verticies [i];
				if (MyBlockData.IsDeformed) {
					float VertexNoiseValue = Noise.Generate (NewVert.x + chunk.pos.x, NewVert.y + chunk.pos.y, NewVert.z + chunk.pos.z) / 4f;
					NewVert += new Vector3 (VertexNoiseValue, VertexNoiseValue, VertexNoiseValue);
				}
				meshData.AddVertex (NewVert);
			}
			List<Vector2> BlockTextureCoordinates = MyModel.GetTextureCoordinates (TileIndex, direction);
			for (int i = 0; i < BlockTextureCoordinates.Count; i++) {
				meshData.uv.Add (BlockTextureCoordinates [i]);
			}
			
			for (int i = 0; i < FaceModel.Verticies.Count; i++) {
				if (direction == Direction.up) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x,y+1,z);
					if (AdjacentBlock != null) {
						LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				} 
				else if (direction == Direction.down) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x,y-1,z);
					if (AdjacentBlock != null) {
						LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				} else if (direction == Direction.west) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x-1,y,z);
					if (AdjacentBlock != null) {
							LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				} 
				else if (direction == Direction.east) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x+1,y,z);
					if (AdjacentBlock != null) {
						LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				}
				else if (direction == Direction.north) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x,y,z+1);
					if (AdjacentBlock != null) {
						LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				} 
				else if (direction == Direction.south) {
					int LightValue2 = 255;
					BlockBase AdjacentBlock = chunk.GetBlock(x,y,z-1);
					if (AdjacentBlock != null) {
						LightValue2 = AdjacentBlock.LightValue;
					}
					meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
				}
			}
		}
		return meshData;
	}
	
	protected override MeshData Lights(Direction direction, Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber) {
		/*BlockModel MyModel = chunk.DataManager.GetBlockModel (TileIndex);
		MyMesh FaceModel = MyModel.GetModel (direction);
		for (int i = 0; i < FaceModel.Verticies.Count; i++) {
			if (direction == Direction.up) {
				int LightValue2 = chunk.GetBlock(x,y+1,z).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			} 
			else if (direction == Direction.down) {
				int LightValue2 = chunk.GetBlock(x,y-1,z).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			} else if (direction == Direction.west) {
				int LightValue2 = chunk.GetBlock(x-1,y,z).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			} 
			else if (direction == Direction.east) {
				int LightValue2 = chunk.GetBlock(x+1,y,z).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			}
			else if (direction == Direction.north) {
				int LightValue2 = chunk.GetBlock(x,y,z+1).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			} 
			else if (direction == Direction.south) {
				int LightValue2 = chunk.GetBlock(x,y,z-1).LightValue;
				meshData.colors.Add (new Color32((byte)LightValue2,(byte)LightValue2,(byte)LightValue2,255));
			}
		}*/
		return meshData;
	}

	protected override MeshData FaceDataUp
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		return GetFace (Direction.up, chunk, x, y, z, meshData, CycleNumber);
	}
	
	protected override MeshData FaceDataDown
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		return GetFace (Direction.down, chunk, x, y, z, meshData, CycleNumber);
	}
	
	protected override MeshData FaceDataNorth
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		return GetFace (Direction.north, chunk, x, y, z, meshData, CycleNumber);
	}
	
	protected override MeshData FaceDataEast
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		return GetFace (Direction.east, chunk, x, y, z, meshData, CycleNumber);
	}
	
	protected override MeshData FaceDataSouth
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		
		return GetFace (Direction.south, chunk, x, y, z, meshData, CycleNumber);
	}
	
	protected override MeshData FaceDataWest
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		return GetFace (Direction.west, chunk, x, y, z, meshData, CycleNumber);
	}
}
}