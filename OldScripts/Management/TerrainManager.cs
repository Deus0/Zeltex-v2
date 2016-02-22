using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
public class TerrainManager : MonoBehaviour {
	public Material HealthBarMaterial;	// used for block damage
	public GameObject BlockPrefab;
	public List<BlockDamage> MyBlockDamages;

	public bool IsBlockEmpty(RaycastHit hit) {
		return (Terrain.GetBlock2 (hit, false) == null);
	}

	// the idea here is to remove blocks in a spherical location
	// the destroyed blocks won't be lootable
	public List<GameObject> RemoveBlock(World HitWorld, Vector3 HitPosition, float Size) {
		Debug.Log ("Removing Block(s): " + Size);
		List<GameObject> NewBlockObjects = new List<GameObject> ();
		if (HitWorld != null) {
			Vector3 HitBlockLocation = Terrain.GetBlockPosV (HitPosition);
			for (float i = -Size; i <= Size; i++)
				for (float j = -Size; j <= Size; j++)
				for (float k = -Size; k <= Size; k++) {
						if (Vector3.Distance (new Vector3 (0, 0, 0), new Vector3 (i, j, k)) <= Size) {
							Block NewBlock = Terrain.GetBlockV (HitWorld, HitBlockLocation+new Vector3 (i, j, k));
							Terrain.SetBlock (HitWorld, HitBlockLocation+new Vector3 (i, j, k), new BlockAir ());
					
							if (BlockPrefab != null && NewBlock != null) {
								GameObject NewBlockObject = (GameObject)Instantiate (BlockPrefab, HitBlockLocation+new Vector3 (i, j, k), Quaternion.identity);
								if (NewBlockObject != null) {
									Destroy (NewBlockObject, 10f);
									NewBlockObject.GetComponent<MeshRenderer>().material.mainTexture = GetManager.GetTextureManager().BlockTextures[NewBlock.GetBlockIndex()];
								NewBlockObjects.Add (NewBlockObject);
								}
							}
						}
					}
		}
		return NewBlockObjects;
	}
	public int RemoveBlockStage1(RaycastHit hit) {
		Block NewBlock = Terrain.GetBlock2 (hit, false);
		Terrain.SetBlock (hit, new BlockAir ());
		return NewBlock.GetBlockIndex();
	}
	public void RemoveBlockStage2(RaycastHit hit, int BlockType) {
		//Block NewBlock = Terrain.GetBlock2 (hit, false);
		Vector3 HitBlockLocation = Terrain.GetBlockPosV (hit);
		if (BlockPrefab != null) {
			GameObject NewBlockObject = (GameObject)Instantiate (BlockPrefab, HitBlockLocation, Quaternion.identity);
			if (NewBlockObject != null)
				Destroy (NewBlockObject, 3f);
		}
	}
}
}