using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : MonoBehaviour {
	public static int BaseLightValue = 255;

	public DataManager DataManager;
	public BlockBase[, ,] blocks = new BlockBase[chunkSize, chunkSize, chunkSize];	// block base is air atm - extended to block for basic blocks

    public static int chunkSize = 16;
	public bool IsUpdateOnThread = true;
	public bool update = false;
	public bool rendered;
	public bool HasInitialLoad;

    MeshFilter filter;
    MeshCollider coll;

    public World world;
    public WorldPos pos;

	public bool HasUpdated = false;
	MeshData MyMeshData;
	MeshData MyWaterMeshData;

	public List<Vector3> MyMonsterSpawns = new List<Vector3>();

	public bool IsCollisions = true;
	public bool HasWater = false;
	public int PolygonCount = 0;

    void Start()
	{
		if (DataManager == null) 
			DataManager = GetManager.GetDataManager ();	// need a better way to do this, keep references in a static object..!
		HasInitialLoad = false;
        filter = gameObject.GetComponent<MeshFilter>();
        coll = gameObject.GetComponent<MeshCollider>();
    }
	bool CanUpdateRenderer = false;
	bool IsUpdating = false;
	public bool HasUpdatedLights = false;
    //Update is called once per frame
    void Update()
    {
		if (update) {
			//HasUpdatedLights = false;
			if (!IsUpdating) {
				if (!CanUpdateRenderer) {
					IsUpdating = true;
					MyMeshData = new MeshData();
					MyWaterMeshData = new MeshData();
					if (IsUpdateOnThread) {
					UnityThreading.ActionThread NewThread = UnityThreadHelper.CreateThread(() =>
				                                                                       {
						// thread processing
						UpdateChunk();
						CanUpdateRenderer = true;
					});
					} else {
							UpdateChunk();
							CanUpdateRenderer = true;
					}
				}
			} else {
				if (CanUpdateRenderer) {
					UpdateRender();
					update = false;
					IsUpdating = false;
					CanUpdateRenderer = false;
				}
			}
        }
		//if (HasUpdatedLights) {
			//UpdateChunkLightsOnly();
		//	HasUpdatedLights = false;
		//}
    }
	public void AddToUpdateList() {
		world.ChunkLoader.AddToUpdateList (new Vector3 (pos.x, pos.y, pos.z));
		// then later update = true will be set
	}
	public Block GetBlock2(int x, int y, int z)
	{
		if (InRange (x) && InRange (y) && InRange (z)) {
			Block NewBlock = null;
			try {
				NewBlock = (Block)blocks [x, y, z];
			}catch// (InvalidCastException e)  
			{
				// if invalid, it is air
			}
			return NewBlock;
		}
		// if not in this chunk, return a block from another chunk
		return world.GetBlock2(pos.x + x, pos.y + y, pos.z + z);
	}
	public BlockBase GetBlock(int x, int y, int z)
    {
        if (InRange(x) && InRange(y) && InRange(z))
            return blocks[x, y, z];
        return world.GetBlock(pos.x + x, pos.y + y, pos.z + z);
    }

    public static bool InRange(int index)
    {
        if (index < 0 || index >= chunkSize)
            return false;

        return true;
    }

	public void SetBlock(int x, int y, int z, BlockBase block)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            if (blocks[x,y,z] != block) {
				blocks[x, y, z] = block;
				blocks[x,y,z].changed = true;
				HasUpdated = true;
			}
        }
        else
        {
            world.SetBlock(pos.x + x, pos.y + y, pos.z + z, block);
        }
    }

    public void SetBlocksUnmodified()
    {
		foreach (BlockBase block in blocks)
        {
			if (block != null)
            block.changed = false;	// which are true by default
        }
    }

	public void RefreshLights() {
		if (world.IsLighting) {
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						blocks [x, y, z].LightValue = world.LightValueMin;
						if (blocks [x, y, z].TileIndex == 11)
							blocks [x, y, z].LightValue = 255;
					}
				}
			}
			for (int x = 0; x < chunkSize; x++) {
				for (int z = 0; z < chunkSize; z++) {
					int TopY = Terrain.GetTopBlock (new Vector2 (pos.x + x, pos.z + z), world);
					if ((TopY >= pos.y && TopY < pos.y + chunkSize) || TopY >= pos.y + chunkSize) {
						for (int y = chunkSize-1; y >= 0; y--) {
							if (blocks [x, y, z].TileIndex == 0) {
								blocks [x, y, z].LightValue = 255;
							} else if (blocks [x, y, z].TileIndex == 4) {	// water gets darker as it gets deeper
								BlockBase AboveBlock = GetBlock (x, y + 1, z);
								if (AboveBlock != null)
									blocks [x, y, z].LightValue = AboveBlock.LightValue - 25;
								else
									blocks [x, y, z].LightValue = 255;
							} else {
								y = -1;
							}
						}
					}
				}
			}
			// propogate the edges of the chunk into this chunk
			for (int x = -1; x <= chunkSize; x++) {
				for (int y = -1; y <= chunkSize; y++) {
					for (int z = -1; z <= chunkSize; z++) {
						BlockBase MyBlock = GetBlock (x, y, z);
						if (MyBlock.LightValue == 255 || MyBlock.TileIndex == 11) {
							PropogateLight (MyBlock.LightValue, x + 1, y, z);
							PropogateLight (MyBlock.LightValue, x - 1, y, z);
							PropogateLight (MyBlock.LightValue, x, y + 1, z);
							PropogateLight (MyBlock.LightValue, x, y - 1, z);
							PropogateLight (MyBlock.LightValue, x, y, z + 1);
							PropogateLight (MyBlock.LightValue, x, y, z - 1);
						}
					}
				}
			}
		} else {
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						blocks[x,y,z].LightValue = 255;
					}
				}
			}
		}
	}

	// until light value is less then 50, keep propogating
	public void PropogateLight(int LightValue, int x, int y, int z) {
		// if light value is greater then light, keep propogating
		if (x < -1 || x > chunkSize || y < -1 || y > chunkSize || z < -1 || z > chunkSize)	// don't propogate too far! only 1 block around each chunk to make them propogate accross
			return;
		BlockBase MyBlock = GetBlock (x, y, z);
		if (MyBlock != null) {
			if (LightValue > world.LightValueMin && LightValue > MyBlock.LightValue && MyBlock.TileIndex == 0) {
				if (!(InRange (x) && InRange (y) && InRange (z))) {	// if not in range, then the updated light is not in this chunk
					HasUpdatedLights = true;
				}
				MyBlock.LightValue = LightValue;
				
				//LightValue = Mathf.RoundToInt(((float)LightValue) / 2f);
				//LightValue = Mathf.RoundToInt(((float)LightValue) / 1.5f);
				LightValue = 5*(LightValue / 9);
				PropogateLight (LightValue, x + 1, y, z);
				PropogateLight (LightValue, x - 1, y, z);
				PropogateLight (LightValue, x, y + 1, z);
				PropogateLight (LightValue, x, y - 1, z);
				PropogateLight (LightValue, x, y, z + 1);
				PropogateLight (LightValue, x, y, z - 1);
			}
		}
	}
	// set to true if a light source affects surrounding chunks lights
	// then only the lights are updated!
    // Updates the chunk based on the blocks mesh data
	// i should add in a render pass here
	void UpdateChunk() {
		MyMonsterSpawns.Clear ();
		RefreshLights ();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
				{
					if (blocks[x,y,z].TileIndex == 16) {
						MyMonsterSpawns.Add (new Vector3(x,y,z));
						blocks[x,y,z] = new BlockAir();
					} 

					if (blocks[x, y, z] == null)
						Debug.LogError("Huge issues in chunk! doesn't have block iniitialized :O");
					MyMeshData = blocks[x, y, z].Blockdata(this, x, y, z, MyMeshData,0);
					MyWaterMeshData = blocks[x, y, z].Blockdata(this, x, y, z, MyWaterMeshData,1);
                }
            }
        }
		CanUpdateRenderer = true;
    }
	void UpdateChunkLightsOnly() {
		Debug.LogError ("Updating chunks lights only");
		for (int x = 0; x < chunkSize; x++)
		{
			for (int y = 0; y < chunkSize; y++)
			{
				for (int z = 0; z < chunkSize; z++)
				{
					MyMeshData = blocks[x, y, z].BlockdataLights(this, x, y, z, MyMeshData,0);
					MyWaterMeshData = blocks[x, y, z].BlockdataLights(this, x, y, z, MyWaterMeshData,1);
				}
			}
		}
		CanUpdateRenderer = true;
	}
	void UpdateRender() {
		// now update the Render Mesh with the created mesh data
		RenderMesh(MyMeshData, MyWaterMeshData);
		rendered = true;
		MyMeshData = null;
		MyWaterMeshData = null;
	}
    // Sends the calculated mesh information
    // to the mesh and collision components
    void RenderMesh(MeshData meshData, MeshData waterMeshData)
    {
		filter.mesh.Clear();
		world.DebugPolygonCount -= PolygonCount;
		PolygonCount = 0;
		
		CombineInstance[] MeshList = new CombineInstance[2];
		if (waterMeshData.vertices.Count > 0) {		// if chunk has water
			MeshList = new CombineInstance[2];
			MeshList [1].mesh = new Mesh ();
			MeshList [1].mesh.vertices = waterMeshData.vertices.ToArray();
			MeshList [1].mesh.triangles = waterMeshData.triangles.ToArray();
			MeshList [1].mesh.uv = waterMeshData.uv.ToArray();
			MeshList [1].mesh.colors32 = waterMeshData.colors.ToArray();
			MeshList [1].transform = filter.transform.transform.localToWorldMatrix;
			HasWater = true;
			Material[] MyMaterials = new Material[2];
			MyMaterials[0] = GetManager.GetTextureManager().MyTerrainMaterial[0];
			MyMaterials[1] = GetManager.GetTextureManager().MyTerrainMaterial2;
			gameObject.GetComponent<MeshRenderer>().materials = MyMaterials;
			PolygonCount += MeshList [1].mesh.vertices.Length/3;
		} else {
			MeshList = new CombineInstance[1];
			HasWater = false;
			Material[] MyMaterials = new Material[1];
			MyMaterials[0] = GetManager.GetTextureManager().MyTerrainMaterial[0];
			gameObject.GetComponent<MeshRenderer>().materials = MyMaterials;
		}
		MeshList [0].mesh = new Mesh ();
		MeshList [0].mesh.vertices = meshData.vertices.ToArray();
		MeshList [0].mesh.triangles = meshData.triangles.ToArray();
		MeshList [0].mesh.uv = meshData.uv.ToArray();
		MeshList [0].mesh.colors32 = meshData.colors.ToArray();
		MeshList [0].transform = filter.transform.transform.localToWorldMatrix;

		PolygonCount += MeshList [0].mesh.vertices.Length/3;
		world.DebugPolygonCount += PolygonCount;

		filter.mesh.CombineMeshes(MeshList, false, false);
		filter.mesh.RecalculateNormals();


		if (IsCollisions) {
			coll.sharedMesh = null;
			Mesh mesh = new Mesh ();
			mesh.vertices = meshData.colVertices.ToArray ();
			mesh.triangles = meshData.colTriangles.ToArray ();	
			mesh.RecalculateNormals ();

			coll.sharedMesh = mesh;
		}
		HasInitialLoad = true;	// used for knowing if the chunks been loaded from save file - for disabling character movement
		for (int i = 0; i < MyMonsterSpawns.Count; i++) {
			Vector3 NewSpawnPosition = transform.position + MyMonsterSpawns[i]+new Vector3(0,2,0);
			//Debug.LogError("Spawning at: " + NewSpawnPosition.ToString());
			GetManager.GetMonsterSpawner().SpawnMonster(1, NewSpawnPosition);
		}
		MyMonsterSpawns.Clear ();
    }
}
}