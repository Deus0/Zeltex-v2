using UnityEngine;
using System.Collections;

public static class Terrain {
	public static Vector3 GetBlockPosV(RaycastHit hit) {
		WorldPos NewPos = GetBlockPos (hit, false);
		return new Vector3 (NewPos.x, NewPos.y, NewPos.z);
	}
	// this should actually take into account 
	public static Vector3 GetBlockPosV(Vector3 pos) {
		Vector3 blockPos = new Vector3 (
			Mathf.RoundToInt(pos.x),
			Mathf.RoundToInt(pos.y),
			Mathf.RoundToInt(pos.z)
			);
		return blockPos;
	}
	
	public static Vector3 GetBlockPosV(World MyWorld, Vector3 pos) {
		pos = MyWorld.transform.InverseTransformPoint (pos);

		// i should multiply it by the scale here of blocks before rounding it
		Vector3 blockPos = new Vector3(
			Mathf.RoundToInt(pos.x),
			Mathf.RoundToInt(pos.y),
			Mathf.RoundToInt(pos.z)
			);
		
		return blockPos;
	}

    public static WorldPos GetBlockPos(Vector3 pos) {
        WorldPos blockPos = new WorldPos(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
            );

        return blockPos;
    }
	/*public static int GetHighestBlock(int i, int j, World InWorld) {
		for (int k = 64; k < -64; k--) {
			Block NewBlock = InWorld.GetBlock2 (i,k,j);
			if (NewBlock != null)
				return k;
		}
		return 0;
	}*/
	public static int MaxWorldHeight = 64;
	public static int GetTopBlock(Vector2 NewBlockLocation,  World InWorld) {
		//WorldPos pos = GetBlockPos(NewBlockLocation);
		for (int i = MaxWorldHeight; i > -MaxWorldHeight; i--) {
			BlockBase block = InWorld.GetBlock(Mathf.RoundToInt(NewBlockLocation.x), i, Mathf.RoundToInt(NewBlockLocation.y));
			if (block != null)
				if (block.TileIndex > 0)
					return i;
				//return block;
			// otherwise block is AirBlock
		}
		//Debug.LogError ("Inside: " + NewBlockLocation.ToString () + " could not find top block: " + Time.time);
		return -1;
	}
	public static WorldPos GetBlockPos(RaycastHit hit)
	{
		Vector3 pos = new Vector3(
			MoveWithinBlock(hit.point.x, hit.normal.x, false),
			MoveWithinBlock(hit.point.y, hit.normal.y, false),
			MoveWithinBlock(hit.point.z, hit.normal.z, false)
			);
		
		return GetBlockPos(pos);
	}
    public static WorldPos GetBlockPos(RaycastHit hit, bool adjacent)
    {
        Vector3 pos = new Vector3(
            MoveWithinBlock(hit.point.x, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z, hit.normal.z, adjacent)
            );

        return GetBlockPos(pos);
    }

	// this accounts for the normal of the mouse hit
    static float MoveWithinBlock(float pos, float norm, bool adjacent) {
        if (pos - (int)pos == 0.5f || pos - (int)pos == -0.5f)
        {
            if (adjacent)
            {
                pos += (norm / 2);
            }
            else
            {
                pos -= (norm / 2);
            }
        }

        return (float)pos;
    }
	
	public static bool SetBlock(World world, Vector3 BlockPosition, BlockBase block) {
		int x = Mathf.FloorToInt (BlockPosition.x);
		int y = Mathf.FloorToInt (BlockPosition.y);
		int z = Mathf.FloorToInt (BlockPosition.z);
		
		int ChunkPosX = 16*(Mathf.FloorToInt (BlockPosition.x/16f));
		int ChunkPosY = 16*(Mathf.FloorToInt (BlockPosition.y/16f));
		int ChunkPosZ = 16*(Mathf.FloorToInt (BlockPosition.z/16f));

		Chunk chunk = world.GetChunk(ChunkPosX, ChunkPosY, ChunkPosZ );

		if (chunk == null)
			return false;
		
		WorldPos pos = GetBlockPos(new Vector3(x, y, z));
		
		chunk.world.SetBlock(pos.x, pos.y, pos.z, block);
		
		//world.GetComponent<NetworkView>().RPC ("SetBlock", RPCMode.All,
		//                                           x,y,z, block   );

		// now save whenever i update blocks
		//Serialization.SaveChunk(chunk);
		return true;
	}
	public static bool SetBlock(World world, int x, int y, int z, BlockBase block, bool adjacent) {
		Chunk chunk = world.GetChunk (0, 0, 0);

		if (chunk == null)
			return false;
		
		WorldPos pos = GetBlockPos(new Vector3(x, y, z));
		
		chunk.world.SetBlock(pos.x, pos.y, pos.z, block);
		
		// now save whenever i update blocks
		//Serialization.SaveChunk(chunk);
		return true;
	}

	public static bool SetBlock(RaycastHit hit, BlockBase block) {
		if (hit.collider == null)
			return false;
		
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null)
			return false;
		
		WorldPos Pos = GetBlockPos(hit);
		chunk.world.SetBlock(Pos.x, Pos.y, Pos.z, block);
		
		// now save whenever i update blocks
		//Serialization.SaveChunk(chunk);
		return true;
	}
	
	/*public static bool SetBlockAndSave(RaycastHit hit, BlockBase block, bool adjacent) {
		if (hit.collider == null)
			return false;
		
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null)
			return false;
		
		WorldPos pos = GetBlockPos(hit, adjacent);
		
		chunk.world.SetBlock(pos.x, pos.y, pos.z, block);
		
		// now save whenever i update blocks
		Serialization.SaveChunk(chunk);
		return true;
	}*/

	public static Block GetBlockV(World HitWorld, Vector3 BlockPosition) {
		Block block = HitWorld.GetBlock2(Mathf.RoundToInt(BlockPosition.x),
		                                 Mathf.RoundToInt(BlockPosition.y), 
		                                 Mathf.RoundToInt(BlockPosition.z));
		
		return block;
	}
	public static Block GetBlock0(RaycastHit hit) {
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null)
			return null;
		
		WorldPos pos = GetBlockPos(hit);
		
		Block block = chunk.world.GetBlock2(pos.x, pos.y, pos.z);
		
		return block;
	}
	public static Block GetBlock2(RaycastHit hit, bool adjacent) {
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null)
			return null;
		
		WorldPos pos = GetBlockPos(hit, adjacent);
		
		Block block = chunk.world.GetBlock2(pos.x, pos.y, pos.z);
		
		return block;
	}
	public static BlockBase GetBlock(RaycastHit hit, bool adjacent)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return null;

        WorldPos pos = GetBlockPos(hit, adjacent);

		BlockBase block = chunk.world.GetBlock(pos.x, pos.y, pos.z);

        return block;
    }
}