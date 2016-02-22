using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
public class BlockModelGenerator {

	public static BlockModel GetCubeModel() {
		BlockModel NewBlockModel = new BlockModel ();
		// top
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(- 0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(- 0.5f, 0.5f, - 0.5f));
		NewBlockModel.AddQuadIndicies (0);
		// bottom
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(- 0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(- 0.5f, -0.5f, 0.5f));
		NewBlockModel.AddQuadIndicies (1);
		// north
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(-0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(-0.5f, -0.5f, 0.5f));
		NewBlockModel.AddQuadIndicies (2);
		// east
		NewBlockModel.BlockMeshes [3].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [3].Verticies.Add (new Vector3(0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [3].Verticies.Add (new Vector3(0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [3].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.AddQuadIndicies (3);
		
		// south
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(-0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(-0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.AddQuadIndicies (4);
		
		// west
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, -0.5f, -0.5f));
		NewBlockModel.AddQuadIndicies (5);
		
		NewBlockModel.AllSolid ();
		// add in uvs
		for (int i = 0; i < 6; i++) {
			//NewBlockModel.ResetBlockFace (i, 1);
			/*NewBlockModel.BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y));
			NewBlockModel.BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y + tileSize));
			NewBlockModel.BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize));
			NewBlockModel.BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * tilePos.x, tileSize * tilePos.y));*/
		}
		NewBlockModel.GenerateTextureMapsSingleFaceCube (16);
		return NewBlockModel;
	}
	public static BlockModel GetSlopeModel(bool IsFlipX) {
		BlockModel NewBlockModel = new BlockModel ();
		// top
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(- 0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [0].Verticies.Add (new Vector3(- 0.5f, 0.5f, - 0.5f));
		NewBlockModel.AddQuadIndicies (0);
		NewBlockModel.IsSolid[0] = false;
		// bottom
		// no change for bottom
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(- 0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [1].Verticies.Add (new Vector3(- 0.5f, -0.5f, 0.5f));
		NewBlockModel.AddQuadIndicies (1);
		NewBlockModel.IsSolid[1] = false;
		// north
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(-0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [2].Verticies.Add (new Vector3(-0.5f, -0.5f, 0.5f));
		NewBlockModel.AddQuadIndicies (2);
		NewBlockModel.IsSolid[2] = false;
		// east
		// nothing for east
		NewBlockModel.IsSolid[3] = false;
		
		// south
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(-0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(-0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.BlockMeshes [4].Verticies.Add (new Vector3(0.5f, -0.5f, -0.5f));
		NewBlockModel.AddQuadIndicies (4);
		NewBlockModel.IsSolid[4] = false;
		
		// west
		// west is also the same
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, -0.5f, 0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, 0.5f, 0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, 0.5f, -0.5f));
		NewBlockModel.BlockMeshes [5].Verticies.Add (new Vector3(-0.5f, -0.5f, -0.5f));
		NewBlockModel.AddQuadIndicies (5);
		NewBlockModel.IsSolid[5] = true;
		
		NewBlockModel.GenerateTextureMaps (16, true, true, true, false, true, true);
		if (IsFlipX) {
			NewBlockModel.FlipModel ();
			NewBlockModel.IsSolid[3] = true;
			NewBlockModel.IsSolid[5] = false;
		}
		return NewBlockModel;
	}
}

// maybe add any voxel mesh culling rules or stuff here
// like which sides are culled if something is next to something
[System.Serializable]
public class BlockTextureMap {
	public List<Vector2> TextureCoordinates = new List<Vector2> ();
	
	public BlockTextureMap () {}
};
// maybe this should be seperate?
// each one of these will be for a different tile
[System.Serializable]
public class BlockModelTextureCoordinates {
	public static int MaxBlockFaces = 8;
	public BlockTextureMap[] BlockTextureMaps = new BlockTextureMap[6];
	
	public BlockModelTextureCoordinates() {
		for (int i = 0; i < 6; i++) {
			BlockTextureMap NewBlockTexture = new BlockTextureMap ();
			BlockTextureMaps [i] = (NewBlockTexture);
		}
	}
	
	
	public void GenerateQuadTextureCoordinates(int TileIndex, bool IsUp, bool IsDown, bool IsNorth, bool IsEast, bool IsSouth, bool IsWest) {
		GenerateQuadTextureCoordinates (TileIndex, 0.125f, IsUp, IsDown, IsNorth, IsEast, IsSouth, IsWest);
	}
	public void GenerateQuadTextureCoordinates(int TileIndex, float TileSize, bool IsUp, bool IsDown, bool IsNorth, bool IsEast, bool IsSouth, bool IsWest) {
		Vector2 TilePosition;
		// Every Column
		TilePosition.x = (TileIndex % MaxBlockFaces);
		// Every Row
		TilePosition.y = (TileIndex / MaxBlockFaces);

		for (int i = 0; i < 6; i++) {
			if ((i == 0 && IsUp) || (i == 1 && IsDown) || (i == 2 && IsNorth) || (i == 3 && IsEast) || (i == 4 && IsSouth) || (i == 5 && IsWest)) {
				BlockTextureMap NewBlockTexture = new BlockTextureMap ();
				NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x + TileSize, TileSize * TilePosition.y));
				NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x + TileSize, TileSize * TilePosition.y + TileSize));
				NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x, TileSize * TilePosition.y + TileSize));
				NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x, TileSize * TilePosition.y));
				BlockTextureMaps [i] = (NewBlockTexture);
			}
		}
	}
	public void GenerateQuadTextureCoordinatesSingleFaceCube(int TileIndex) {
		GenerateQuadTextureCoordinatesSingleFaceCube (TileIndex, 0.125f);
	}
	public void GenerateQuadTextureCoordinatesSingleFaceCube(int TileIndex, float TileSize) {
		Vector2 TilePosition;
		// Every Column
		TilePosition.x = (TileIndex % MaxBlockFaces);
		// Every Row
		TilePosition.y = (TileIndex / MaxBlockFaces);
		for (int i = 0; i < 6; i++) {
			BlockTextureMap NewBlockTexture = new BlockTextureMap ();
			NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x + TileSize, TileSize * TilePosition.y));
			NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x + TileSize, TileSize * TilePosition.y + TileSize));
			NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x, TileSize * TilePosition.y + TileSize));
			NewBlockTexture.TextureCoordinates.Add (new Vector2 (TileSize * TilePosition.x, TileSize * TilePosition.y));
			BlockTextureMaps[i] = (NewBlockTexture);
		}
	}
}

[System.Serializable]
public class BlockModel {
	public int BlockModelIndex = 0;
	public MyMesh[] BlockMeshes = new MyMesh[6];
	public bool[] IsSolid = new bool[6];
	// this is to be used per model
	bool IsMultipleTextureMaps = true;
	public List<BlockModelTextureCoordinates> TextureCoordinatesList = new List<BlockModelTextureCoordinates> ();
	public static int MaxBlockFaces = 8;
	public static float tileSize = 0.125f;

	public void Initialize() {
		for (int i = 0; i < 6; i++) {
			BlockMeshes[i] = new MyMesh();
		}
	}
	public BlockModel() {
		Initialize ();
	}
	public BlockModel(Mesh NewMesh) {
		Initialize ();
		IsMultipleTextureMaps = false;
		BlockMeshes [0] = new MyMesh (NewMesh);
	}
	public void AllSolid() {
		for (int i = 0; i < 6; i++) {
			IsSolid[i] = true;
		}
	}
	public bool GetSolidity(Direction direction) {
		if (direction == Direction.up) {
			return IsSolid [0];
		} else if (direction == Direction.down) {
			return IsSolid [1];
		} else if (direction == Direction.north) {
			return IsSolid [2];
		} else if (direction == Direction.east) {
			return IsSolid [3];
		} else if (direction == Direction.south) {
			return IsSolid [4];
		} else if (direction == Direction.west) {
			return IsSolid [5];
		} else
			return false;
	}
	public MyMesh GetModel(Direction direction) {
		if (direction == Direction.up) {
			return BlockMeshes [0];
		} else if (direction == Direction.down) {
			return BlockMeshes [1];
		} else if (direction == Direction.north) {
			return BlockMeshes [2];
		} else if (direction == Direction.east) {
			return BlockMeshes [3];
		} else if (direction == Direction.south) {
			return BlockMeshes [4];
		} else if (direction == Direction.west) {
			return BlockMeshes [5];
		} else
			return new MyMesh ();
	}
	public void AddQuadIndicies(int BlockMesh) {
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 4);
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 3);
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 2);
		
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 4);
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 2);
		BlockMeshes [BlockMesh].Indicies.Add(BlockMeshes [BlockMesh].Verticies.Count - 1);
	}
	// for single sided cubes
	public void ResetCubeTextureCoordinates(int TileIndex) {
		for (int i = 0; i < 6; i++) {
			ResetBlockFace(i, TileIndex);
		}
	}
	public void ResetBlockFace(int i, int TileIndex) {
		Vector2 TilePosition;
		// Every Column
		TilePosition.x = (TileIndex % MaxBlockFaces);
		// Every Row
		TilePosition.y = (TileIndex / MaxBlockFaces);
		BlockMeshes [i].TextureCoordinates.Clear();
		BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * TilePosition.x + tileSize, tileSize * TilePosition.y));
		BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * TilePosition.x + tileSize, tileSize * TilePosition.y + tileSize));
		BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * TilePosition.x, tileSize * TilePosition.y + tileSize));
		BlockMeshes [i].TextureCoordinates.Add (new Vector2(tileSize * TilePosition.x, tileSize * TilePosition.y));
	}
	
	public void GenerateTextureMaps(int MaxTextureMaps, bool IsUp, bool IsDown, bool IsNorth, bool IsEast, bool IsSouth, bool IsWest) {
		for (int i = 0; i < MaxTextureMaps; i++) {
			BlockModelTextureCoordinates NewUVs = new BlockModelTextureCoordinates();
			NewUVs.GenerateQuadTextureCoordinates(i, IsUp, IsDown, IsNorth, IsEast, IsSouth, IsWest);
			TextureCoordinatesList.Add(NewUVs);
		}
	}
	public void GenerateTextureMapsSingleFaceCube(int MaxTextureMaps) {
		for (int i = 0; i < MaxTextureMaps; i++) {
			BlockModelTextureCoordinates NewUVs = new BlockModelTextureCoordinates();
			NewUVs.GenerateQuadTextureCoordinatesSingleFaceCube(i);
			TextureCoordinatesList.Add(NewUVs);
		}
	}
	
	public List<Vector2> GetTextureCoordinates(int TextureMapIndex, Direction direction) {
		if (direction == Direction.up) {
			return GetTextureCoordinates (TextureMapIndex, 0);
		} else if (direction == Direction.down) {
			return GetTextureCoordinates (TextureMapIndex, 1);
		} else if (direction == Direction.north) {
			return GetTextureCoordinates (TextureMapIndex, 2);
		} else if (direction == Direction.east) {
			return GetTextureCoordinates (TextureMapIndex, 3);
		} else if (direction == Direction.south) {
			return GetTextureCoordinates (TextureMapIndex, 4);
		} else if (direction == Direction.west) {
			return GetTextureCoordinates (TextureMapIndex, 5);
		} else {
			return GetTextureCoordinates (TextureMapIndex, 0);
		}
	}

	// just fliip it over x for now
	// i have to switch the block meshes too
	public void FlipModel() {
		// flip x solidity
		bool TempSolidity = IsSolid [3];
		IsSolid [3] = IsSolid [5];
		IsSolid [5] = TempSolidity;
		for (int i = 0; i < BlockMeshes.Length; i++) {	// for all block meshes
			for (int j = 0; j < BlockMeshes[i].Verticies.Count; j++) {
				BlockMeshes[i].Verticies[j] = new Vector3(-BlockMeshes[i].Verticies[j].x,
				                                          BlockMeshes[i].Verticies[j].y,
				                                          BlockMeshes[i].Verticies[j].z);
			}
			// flip the order of verticies
			for (int j = 0; j < BlockMeshes[i].Verticies.Count; j+= 3) {
				if (j+2 < BlockMeshes[i].Verticies.Count) {
					Vector3 NewVertex1 = new Vector3(BlockMeshes[i].Verticies[j].x,BlockMeshes[i].Verticies[j].y, BlockMeshes[i].Verticies[j].z);
					Vector3 NewVertex2 = new Vector3(BlockMeshes[i].Verticies[j+1].x,BlockMeshes[i].Verticies[j+1].y,BlockMeshes[i].Verticies[j+1].z);
					Vector3 NewVertex3 = new Vector3(BlockMeshes[i].Verticies[j+2].x,BlockMeshes[i].Verticies[j+2].y,BlockMeshes[i].Verticies[j+2].z);
					BlockMeshes[i].Verticies[j] = NewVertex3;
					BlockMeshes[i].Verticies[j+2] = NewVertex1;
					BlockMeshes[i].Verticies[j+1] = NewVertex2;
				}
			}
		}
		MyMesh TemporaryBlockMesh = BlockMeshes [3];
		BlockMeshes [3] = BlockMeshes [5];
		BlockMeshes [5] = TemporaryBlockMesh;
		for (int i = 0; i < TextureCoordinatesList.Count; i++) {
			BlockTextureMap TemporaryBlockTextureMap = TextureCoordinatesList[i].BlockTextureMaps[3];
			TextureCoordinatesList[i].BlockTextureMaps[3] = TextureCoordinatesList[i].BlockTextureMaps[5];
			TextureCoordinatesList[i].BlockTextureMaps[5] = TemporaryBlockTextureMap;
		}
	}

	public List<Vector2> GetTextureCoordinates(int TextureMapIndex, int direction) {
		if (IsMultipleTextureMaps)
			return TextureCoordinatesList [TextureMapIndex].BlockTextureMaps [direction].TextureCoordinates;
		else
			return BlockMeshes [direction].TextureCoordinates;	// TextureCoordinatesList [TextureMapIndex].BlockTextureMaps [direction].TextureCoordinates;
	}
};
}
