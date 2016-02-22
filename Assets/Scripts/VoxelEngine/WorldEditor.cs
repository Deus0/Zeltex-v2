using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine {
	public class WorldEditor : MonoBehaviour {
		private World MyWorld; 
		[Header("Debug")]
		[SerializeField] protected bool IsDebugMode = false;
		[Header("Voxel Options")]
		[SerializeField] protected  bool bIsMirrorX = false;
		[SerializeField] protected  bool bIsMirrorY = true;
		[SerializeField] protected  bool bIsMirrorZ = false;
		protected float CurrentSparce = 0;
		
		void Start()
		{
			MyWorld = gameObject.GetComponent<World> ();
		}
		
		// world interfaces
		protected Vector3 GetSize() {
			if (MyWorld == null)
				MyWorld = gameObject.GetComponent<World> ();
			return gameObject.GetComponent<VoxelLoader>().LoadDistance*Chunk.ChunkSize;
		}
		protected bool UpdateBlock(Vector3 BlockPosition, int Type) {
			if (Type != 0 && MyWorld.GetVoxelType (BlockPosition) != 0)
				return false;
			return MyWorld.UpdateBlockType (BlockPosition, Type);
		}
		protected int GetBlockType(Vector3 BlockPosition) {
			return MyWorld.GetVoxelType (BlockPosition);
		}
		protected void UpdateBlock(Vector3 BlockPosition, int BlockType, Vector2 BlockSize) 
		{
			for (int i = 0; i < BlockSize.x; i++)
				for (int j = 0; j < BlockSize.y; j++)
			{
				UpdateBlock(BlockPosition + new Vector3(i,0,j),
				            BlockType);
			}
		}

		protected void UpdateBlock(Vector3 Position, Vector3 Size, int BlockType) {
			if (UpdateBlock(Position, BlockType))
				CurrentSparce++;
			//DebugPositions.Add (Position);
			
			List<Vector3> BlockPlacementPositions = new List<Vector3>();
			BlockPlacementPositions.Add(Position);
			Vector3 MirrorAxis = new Vector3(Size.x / 2, Size.y / 2, Size.z / 2);
			if (bIsMirrorX) {
				float DifferenceX = Mathf.Abs(Position.x - MirrorAxis.x);	// difference from mid
				if (Position.x != MirrorAxis.x) {
					int MaxBlocks = BlockPlacementPositions.Count;
					for (int i = 0; i < MaxBlocks; i++) {
						Vector3 NewPosition = new Vector3(BlockPlacementPositions[i].x, BlockPlacementPositions[i].y, BlockPlacementPositions[i].z);
						if (NewPosition.x > MirrorAxis.x)
							NewPosition.x -= DifferenceX * 2;
						else if (NewPosition.x < MirrorAxis.x)
							NewPosition.x += DifferenceX * 2;
						BlockPlacementPositions.Add(NewPosition);
					}
				}
			}
			if (bIsMirrorY) {
				float DifferenceY = Mathf.Abs(Position.y - MirrorAxis.y);	// difference from mid
				if (Position.y != MirrorAxis.y) {
					int MaxBlocks = BlockPlacementPositions.Count;
					for (int i = 0; i < MaxBlocks; i++) {
						Vector3 NewPosition = new Vector3(BlockPlacementPositions[i].x, BlockPlacementPositions[i].y, BlockPlacementPositions[i].z);
						if (NewPosition.y > MirrorAxis.y)
							NewPosition.y -= DifferenceY * 2;
						else if (NewPosition.y < MirrorAxis.y)
							NewPosition.y += DifferenceY * 2;
						BlockPlacementPositions.Add(NewPosition);
					}
				}
			}
			if (bIsMirrorZ) {
				float DifferenceZ = Mathf.Abs(Position.z - MirrorAxis.z);	// difference from mid
				if (Position.z != MirrorAxis.z) {
					int MaxBlocks = BlockPlacementPositions.Count;
					for (int i = 0; i < MaxBlocks; i++) {
						Vector3 NewPosition = new Vector3(BlockPlacementPositions[i].x, BlockPlacementPositions[i].y, BlockPlacementPositions[i].z);
						if (NewPosition.z > MirrorAxis.z)
							NewPosition.z -= DifferenceZ * 2;
						else if (NewPosition.z < MirrorAxis.z)
							NewPosition.z += DifferenceZ * 2;
						BlockPlacementPositions.Add(NewPosition);
					}
				}
			}
			for (int i = 0; i < BlockPlacementPositions.Count; i++) {
				if (UpdateBlock(BlockPlacementPositions[i], BlockType))
					CurrentSparce++;
			}
		}

		
		// needs to use is void axis to find out how to build walls
		// for every path block, build a wall in the empty spot next to it
		public void GeneratePathWalls(int PathBlockType, float MazeWallHeight,  int MazeWallType, int MazeRoofType) {
			Debug.LogError ("Generating hallway walls.");
			Vector3 Size = GetSize ();
			for (int i = 0; i < Size.x; i++)
				for (int j = 0; j < Size.y; j++)
					for (int k = 0; k < Size.z; k++)
				{
					if (GetBlockType(new Vector3(i, j, k)) == PathBlockType)
					{
						UpdateBlock(new Vector3(i, j + MazeWallHeight, k), MazeRoofType);
					}
					if (GetBlockType(new Vector3(i, j, k)) == 0
					    && (GetBlockType(new Vector3(i + 1, j, k)) == PathBlockType 
					    || GetBlockType(new Vector3(i - 1, j, k)) == PathBlockType
					    || GetBlockType(new Vector3(i, j, k + 1)) == PathBlockType 
					    || GetBlockType(new Vector3(i, j, k - 1)) == PathBlockType)) 
					{
						for (int z = 0; z < MazeWallHeight + 1; z++) {
							UpdateBlock(new Vector3(i, j + z, k), MazeWallType);
						}
					}
				}

			/*if (bIsEdge) {
				for (int i = 0; i < Size.x; i++)
					for (int j = 0; j < Size.y; j++)
						for (int k = 0; k < Size.z; k++)
					{
						if (IsOnEdge(new Vector3(i, j, k), Size) && GetBlockType(new Vector3(i, j, k)) == PathBlockType) {
							for (int z = 0; z < MazeWallHeight + 1; z++) {
								UpdateBlock(new Vector3(i, j + z, k), MazeWallType);
							}
						}
					}
			}*/
		}
	}
}