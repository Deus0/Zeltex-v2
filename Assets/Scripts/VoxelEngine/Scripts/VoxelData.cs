using UnityEngine;
using System.Collections;

namespace VoxelEngine {
	[System.Serializable]
	public class VoxelDataK {
		[SerializeField] public Voxel Data = new Voxel();
	}
	[System.Serializable]
	public class VoxelDataJ {
		[SerializeField] public VoxelDataK[] Data = new VoxelDataK[Chunk.ChunkSize];	// k
		public VoxelDataJ() {
			for (int i = 0; i < Chunk.ChunkSize; i++) {
				Data[i] = new VoxelDataK();
			}
		}
	}

	[System.Serializable]
	public class VoxelDataI {
		[SerializeField] public VoxelDataJ[] Data = new VoxelDataJ[Chunk.ChunkSize];	// j
		public VoxelDataI() {
			for (int i = 0; i < Chunk.ChunkSize; i++) {
				Data[i] = new VoxelDataJ();
			}
		}
	}

	[System.Serializable]
	public class VoxelData {
		//[HideInInspector]
		[SerializeField]public VoxelDataI[] Data = new VoxelDataI[Chunk.ChunkSize];	// i
		public VoxelData() {
			for (int i = 0; i < Chunk.ChunkSize; i++) {
				Data[i] = new VoxelDataI();
			}
		}
		public bool IsInRange(int z) {
			return (z >= 0 && z < Chunk.ChunkSize);
		}
		public void Reset(int i, int j, int k) 
		{
			if (IsInRange(i) && IsInRange(j) && IsInRange(k))
				Data [i].Data[j].Data[k].Data = new Voxel();
		}
		public void SetVoxelType(Chunk MyChunk, int i, int j, int k, int Type) 
		{
			if (IsInRange(i) && IsInRange(j) && IsInRange(k))
				Data [i].Data[j].Data[k].Data.SetType(MyChunk, i, j, k, Type);
		}
		public Voxel GetVoxel(int i, int j, int k) 
		{
			if (IsInRange (i) && IsInRange (j) && IsInRange (k))
				return Data [i].Data [j].Data [k].Data;
			else
				return null;
		}
		public int GetVoxelType(int i, int j, int k) 
		{
			if (IsInRange (i) && IsInRange (j) && IsInRange (k))
				return Data [i].Data [j].Data [k].Data.GetBlockIndex();
			else
				return 0;
		}
	}
}