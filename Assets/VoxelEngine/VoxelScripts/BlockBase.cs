using UnityEngine;
using System.Collections;
using System;

public enum Direction { north, east, south, west, up, down };
[Serializable]
public class BlockBase {
	public int LightValue = Chunk.BaseLightValue;
	public int TileIndex = 0;
	//public int Type = 0;
	//public Color32 MyColor = new Color(255,255,255,255);
	//public bool IsActivated = false;


	public struct Tile { public int x; public int y;}
	const float tileSize = 0.125f;
	public bool changed = true;

    //Base block constructor
	public BlockBase()
    {

    }
	
	
	public virtual MeshData BlockdataLights
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
	{
		//meshData = Lights(chunk, x, y, z, meshData, CycleNumber);
		return meshData;
	}
	
	protected virtual MeshData Lights(Direction direction, Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber) {
		
		return meshData;
	}
    public virtual MeshData Blockdata
     (Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
		if (CycleNumber == 0)
			meshData.useRenderDataForCol = true;
		else if (CycleNumber == 1)
			meshData.useRenderDataForCol = false;

		if (!chunk.GetBlock(x, y + 1, z).IsSolid(chunk, Direction.down, CycleNumber))
        {
			meshData = FaceDataUp(chunk, x, y, z, meshData, CycleNumber);
        }

		if (!chunk.GetBlock(x, y - 1, z).IsSolid(chunk, Direction.up, CycleNumber))
        {
			meshData = FaceDataDown(chunk, x, y, z, meshData, CycleNumber);
        }

		if (!chunk.GetBlock(x, y, z + 1).IsSolid(chunk, Direction.south, CycleNumber))
        {
			meshData = FaceDataNorth(chunk, x, y, z, meshData, CycleNumber);
        }

		if (!chunk.GetBlock(x, y, z - 1).IsSolid(chunk, Direction.north, CycleNumber))
        {
			meshData = FaceDataSouth(chunk, x, y, z, meshData, CycleNumber);
        }

		if (!chunk.GetBlock(x + 1, y, z).IsSolid(chunk, Direction.west, CycleNumber))
        {
			meshData = FaceDataEast(chunk, x, y, z, meshData, CycleNumber);
        }

		if (!chunk.GetBlock(x - 1, y, z).IsSolid(chunk, Direction.east, CycleNumber))
        {
			meshData = FaceDataWest(chunk, x, y, z, meshData, CycleNumber);
        }

        return meshData;

	}

	protected virtual MeshData GetFace(Direction direction, Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber) {

		return meshData;
	}
    protected virtual MeshData FaceDataUp
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.up));
        return meshData;
    }

    protected virtual MeshData FaceDataDown
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.down));
        return meshData;
    }

    protected virtual MeshData FaceDataNorth
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.north));
        return meshData;
    }

    protected virtual MeshData FaceDataEast
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.east));
        return meshData;
    }

    protected virtual MeshData FaceDataSouth
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.south));
        return meshData;
    }

    protected virtual MeshData FaceDataWest
		(Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.west));
        return meshData;
    }

    public virtual Tile TexturePosition(Direction direction) {
        Tile tile = new Tile();
        tile.x = 0;
        tile.y = 0;

        return tile;
    }

    public virtual Vector2[] FaceUVs(Direction direction)
    {
        Vector2[] UVs = new Vector2[4];
        Tile tilePos = TexturePosition(direction);

        UVs[0] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y);
        UVs[1] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y + tileSize);
        UVs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize);
        UVs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);

        return UVs;
    }

	public virtual bool IsSolid(Chunk chunk, Direction direction, int CycleNumber)
    {
        switch (direction)
        {
            case Direction.north:
                return true;
            case Direction.east:
                return true;
            case Direction.south:
                return true;
            case Direction.west:
                return true;
            case Direction.up:
                return true;
            case Direction.down:
                return true;
        }

        return false;
    }

}