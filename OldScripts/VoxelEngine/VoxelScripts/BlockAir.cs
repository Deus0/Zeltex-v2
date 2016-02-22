using UnityEngine;
using System.Collections;
using System;

namespace OldCode {
[Serializable]
public class BlockAir : BlockBase
{
    public BlockAir()
        : base()
    {
		TileIndex = 0;
		LightValue = 0;
    }

    public override MeshData Blockdata
        (Chunk chunk, int x, int y, int z, MeshData meshData, int CycleNumber)
    {
        return meshData;
    }

	public override bool IsSolid(Chunk chunk, Direction direction, int CycleNumber)
    {
        return false;
    }
}
}