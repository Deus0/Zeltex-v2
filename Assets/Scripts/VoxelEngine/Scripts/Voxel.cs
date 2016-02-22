using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine {

	[System.Serializable]
	public enum Direction {
		Up, 
		Down,
		Left,
		Right,
		Forward,
		Back
	};
	[System.Serializable]
	public class Voxel 
	{
		//public static int DefaultLight = 55;
		[SerializeField] private int Type;				// type of material / tile index
		[SerializeField] public byte Light = (byte)(Chunk.DefaultBrightness);				// 0 to 255 - general brightness
		[SerializeField] private bool HasUpdated;
		[SerializeField] public MeshData MyMeshData;

		public void SetHasUpdated(bool NewUpdated) {
			HasUpdated = NewUpdated;
		}

		public bool GetHasUpdated() {
			bool HasHas = HasUpdated;
			//HasUpdated = false;
			return HasHas;
		}
		public Voxel() 
		{
			Type = 0;
			HasUpdated = true;
			//Light = (byte)(Random.Range (0, 255));
			MyMeshData = new MeshData();
		}

		public Voxel(int NewType) 
		{
			Type = NewType;
			HasUpdated = true;
			MyMeshData = new MeshData();
		}
		public bool SetBrightness(Chunk MyChunk, int x, int y, int z, int Brightness) {
			byte NewLight = (byte)(Brightness);
			if (Light != NewLight) 
			{
				Light = NewLight;
				HasUpdated = true;
				UpdateSurroundings(MyChunk, x, y, z);
				return true;
			}
			return false;
		}
		public bool IsAir()
		{
			return (Type == 0);
		}
		public int GetBlockIndex() {
			return Type;
		}
		public byte GetLight() {
			return Light;
		}
		private void UpdateSurroundings(Chunk MyChunk, int x, int y, int z) 
		{
			SetHasUpdated(MyChunk, x+1, y, z);
			SetHasUpdated(MyChunk, x-1, y, z);
			SetHasUpdated(MyChunk, x, y+1, z);
			SetHasUpdated(MyChunk, x, y-1, z);
			SetHasUpdated(MyChunk, x, y, z+1);
			SetHasUpdated(MyChunk, x, y, z-1);
		}
		private void SetHasUpdated(Chunk MyChunk, int x, int y, int z) {
			Voxel ThisVoxel = MyChunk.GetVoxel (x, y, z);
			if (ThisVoxel != null) {
				ThisVoxel.HasUpdated = true;
			}
		}
		public bool SetType(Chunk MyChunk, int x, int y, int z, int NewType) {
			if (MyChunk && Type != NewType) {
				Type = NewType;
				HasUpdated = true;
				UpdateSurroundings(MyChunk, x, y, z);
				/*MyChunk.GetVoxel(x+1,y,z).HasUpdated = true;
				MyChunk.GetVoxel(x-1,y,z).HasUpdated = true;
				MyChunk.GetVoxel(x,y+1,z).HasUpdated = true;
				MyChunk.GetVoxel(x,y-1,z).HasUpdated = true;
				MyChunk.GetVoxel(x,y,z+1).HasUpdated = true;
				MyChunk.GetVoxel(x,y,z-1).HasUpdated = true;*/
				return true;
			}
			return false;
		}
		public bool IsTypeSolid(int Type) {
			//return (Type != 0 && Type != 1);
			return (Type != 0);
		}
		public bool IsSolid(Direction MyDirection, int MaterialType) {
			switch (MyDirection) {
				case(Direction.Up): 
				if (IsTypeSolid(Type))// && Type != 1)
						return true;
					else 
						return false;
				//break;
			case(Direction.Down): 
				if (IsTypeSolid(Type))
						return true;
					else 
						return false;
					//break;
			case(Direction.Left): 
				if (IsTypeSolid(Type))
						return true;
					else 
						return false;
					//break;
			case(Direction.Right): 
				if (IsTypeSolid(Type))
					return true;
					else 
						return false;
					//break;
			case(Direction.Forward): 
				if (IsTypeSolid(Type))
					return true;
					else 
						return false;
					//break;
			case(Direction.Back): 
				if (IsTypeSolid(Type))
					return true;
					else 
						return false;
					//break;
			}
			return false;
		}
		public int[] GetLights(Chunk MyChunk, int x, int y, int z, int MaterialType) {
			int LightAbove = Chunk.SunBrightness;
			if (MyChunk.GetVoxel (x, y + 1, z) != null)
				LightAbove = MyChunk.GetVoxel (x, y + 1, z).GetLight ();

			int LightBelow = Chunk.DefaultBrightness;
			if (MyChunk.GetVoxel (x, y - 1, z) != null)
				LightBelow = MyChunk.GetVoxel (x, y - 1, z).GetLight ();

			int LightFront = Chunk.SunBrightness;
			if (MyChunk.GetVoxel (x, y, z + 1) != null)
				LightFront = MyChunk.GetVoxel (x, y, z + 1).GetLight ();

			int LightBehind = Chunk.SunBrightness;
			if (MyChunk.GetVoxel (x, y, z - 1) != null)
				LightBehind = MyChunk.GetVoxel (x, y, z - 1).GetLight ();

			int LightRight = Chunk.SunBrightness;
			if (MyChunk.GetVoxel (x + 1, y, z) != null)
				LightRight = MyChunk.GetVoxel (x + 1, y, z).GetLight();

			int LightLeft = Chunk.SunBrightness;
			if (MyChunk.GetVoxel (x - 1, y, z) != null)
				LightLeft = MyChunk.GetVoxel (x - 1, y, z).GetLight();

			int[] MyLights = new int[]
			{
				LightAbove, 
				LightBelow, 
				LightFront, 
				LightBehind, 
				LightRight, 
				LightLeft
			};
			return MyLights;
		}
		public bool[] GetSides(Chunk MyChunk, int x, int y, int z, int MaterialType) {
			bool[] MySides = new bool[]{false, false, false, false, false, false};

			Voxel VoxelAbove = MyChunk.GetVoxel (x, y + 1, z);
			if (VoxelAbove != null) {
				if (!VoxelAbove.IsSolid (Direction.Up, MaterialType)) {
					MySides [0] = true;
				}
			} else {
				MySides [0] = true;
			}

			Voxel VoxelBelow = MyChunk.GetVoxel (x, y - 1, z);
			if (VoxelBelow != null) {
				if (!VoxelBelow.IsSolid (Direction.Down, MaterialType)) {
					MySides[1] = true;
				}
			} else {
				MySides [1] = true;
			}
			
			Voxel VoxelFront = MyChunk.GetVoxel (x, y, z + 1);
			if (VoxelFront != null) {
				if (!VoxelFront.IsSolid (Direction.Forward, MaterialType)) {
					MySides[2] = true;
				}
			} else {
				MySides [2] = true;
			}
			Voxel VoxelBehind = MyChunk.GetVoxel (x, y, z - 1);
			if (VoxelBehind != null) {
				if (!VoxelBehind.IsSolid (Direction.Back, MaterialType)) {
					MySides[3] = true;
				}
			} else {
				MySides [3] = true;
			}

			Voxel VoxelRight = MyChunk.GetVoxel (x + 1, y, z);
			if (VoxelRight != null) {
				if (!VoxelRight.IsSolid (Direction.Right, MaterialType)) {
					MySides[4] = true;
				}
			} else {
				MySides [4] = true;
			}

			Voxel VoxelLeft = MyChunk.GetVoxel (x - 1, y, z);
			if (VoxelLeft != null) {
				if (!VoxelLeft.IsSolid (Direction.Left, MaterialType)) {
					MySides[5] = true;
				}
			} else {
				MySides [5] = true;
			}
			
			return MySides;
		}
		// maybe make this seperate, has updated mesh, has updated lights
		public MeshData BuildMesh(Chunk MyChunk, int x, int y, int z, MeshData MyVoxelsMesh, int MaterialType)
		{
			if ((Type - 1 == MaterialType || (Type - 1 >= MyChunk.GetWorld().MyMaterials.Count && MaterialType == 0))) {
				if (HasUpdated) 
				{
					HasUpdated = false;
					MyMeshData.Clear ();

					Voxel VoxelAbove = MyChunk.GetVoxel (x, y + 1, z);
					if (!VoxelAbove.IsSolid (Direction.Up, MaterialType)) 
					{
						MyMeshData = FaceDataUp (x, y, z, MyMeshData, MaterialType, VoxelAbove.GetLight());
					}
					
					Voxel VoxelBelow = MyChunk.GetVoxel (x, y - 1, z);
					if (!VoxelBelow.IsSolid (Direction.Down, MaterialType)) {
						MyMeshData = FaceDataDown (x, y, z, MyMeshData, MaterialType, VoxelBelow.GetLight());
					}
					
					Voxel VoxelFront = MyChunk.GetVoxel (x, y, z + 1);
					if (!VoxelFront.IsSolid (Direction.Forward, MaterialType)) {
						MyMeshData = FaceDataNorth (x, y, z, MyMeshData, MaterialType, VoxelFront.GetLight());
					}
					
					Voxel VoxelBehind = MyChunk.GetVoxel (x, y, z - 1);
					if (!VoxelBehind.IsSolid (Direction.Back, MaterialType)) {
						MyMeshData = FaceDataSouth (x, y, z, MyMeshData, MaterialType, VoxelBehind.GetLight());
					}
					
					Voxel VoxelRight = MyChunk.GetVoxel (x + 1, y, z);
					if (!VoxelRight.IsSolid (Direction.Right, MaterialType)) {
						MyMeshData = FaceDataEast (x, y, z, MyMeshData, MaterialType, VoxelRight.GetLight());
					}
					Voxel VoxelLeft = MyChunk.GetVoxel (x - 1, y, z);
					if (!VoxelLeft.IsSolid (Direction.Left, MaterialType)) {
						MyMeshData = FaceDataWest (x, y, z, MyMeshData, MaterialType, VoxelLeft.GetLight());
					}
					MyVoxelsMesh.Add (MyMeshData);
				} 
				else 
				{
					MyVoxelsMesh.Add(MyMeshData);
				}
			}
			return MyVoxelsMesh;
			
		}
		public MeshData FaceDataUp(float x, float y, float z, MeshData meshData, int MaterialType, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
		
		public MeshData FaceDataDown(float x, float y, float z, MeshData meshData, int MaterialType, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
		
		public MeshData FaceDataNorth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
		
		public MeshData FaceDataEast(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
		
		public MeshData FaceDataSouth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
		
		public MeshData FaceDataWest(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
		{
			x += 0.5f; y += 0.5f; z += 0.5f;
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			
			meshData.AddQuadTriangles();
			meshData.AddQuadUVs ();
			meshData.AddQuadColours (Brightness);
			return meshData;
		}
	}
}

