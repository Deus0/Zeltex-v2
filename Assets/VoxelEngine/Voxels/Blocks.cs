using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum BlockDirection { 
	Up, 
	Down, 
	Right, 
	Left, 
	Forward, 
	Back 
};

[System.Serializable]
public class Block0 {
	private int Type;
	private bool IsActivated;
	//private Color32 MyColor;
	private MyMesh BlockMesh;
	private bool Changed;
	//public int Index;
	public Block0() {
		Type = 0;
		SetDefaults ();
	}
	public Block0(int NewType) {
		Type = NewType;
		SetDefaults ();
	}
	public void SetDefaults() {
		Changed = false;
		BlockMesh = new MyMesh ();
		IsActivated = false;
		//MyColor = new Color32 (155, 155, 155, 255);	// start off dark
	}
	public MyMesh GetBlockMesh() {
		return BlockMesh;
	}
	public void ClearBlockMesh() {
		BlockMesh.ClearMesh ();
		Changed = true;
	}
	public void SetColor(Color32 NewColor) {
		//MyColor = NewColor;
	}
	public int GetType() {
		return Type;
	}
	public bool HasChanged() {
		return Changed;
	}
	public void ReCreate() {
		Changed = true;
	}
	public bool SetType(int NewType) {
		if (Type != NewType) {
			Type = NewType;
			Changed = true;
			return true;
		}
		return false;
	}
	public bool GetIsActivated() {
		return IsActivated ;
	}
	public void SetIsActivated(bool NewSwitch) {
		IsActivated = NewSwitch;
	}
	// Other Blocks can override this with their own solidity - e.g. Ramps will return true for up and its front
	public virtual bool IsSolid(BlockDirection direction) {
		switch(direction){
		case BlockDirection.Up:
			return true;
		case BlockDirection.Down:
			return true;
		case BlockDirection.Right:
			return true;
		case BlockDirection.Left:
			return true;
		case BlockDirection.Forward:
			return true;
		case BlockDirection.Back:
			return true;
		}
		
		return false;
	}
};

[System.Serializable]
public class Block1 {
	private List<Block0> Data;
	public Block1() {
		Data = new List<Block0> ();
	}
	public void AddData(Block0 NewData) {
		Data.Add (NewData);
	}
	public void Insert(int Position, Block0 NewData) {
		Data.Insert (Position, NewData);
	}
	public void RemoveAt(int Position) {
		Data.RemoveAt (Position);
	}
	public Block0 GetData(int PositionInList) {
		return Data [PositionInList];
	}
	public int GetSize() {
		return Data.Count;
	}
};
[System.Serializable]
public class Block2 {
	private List<Block1> Data;
	public Block2() {
		Data = new List<Block1> ();
	}
	public void AddData(Block1 NewData) {
		Data.Add (NewData);
	}
	public void Insert(int Position, Block1 NewData) {
		Data.Insert (Position, NewData);
	}
	public void RemoveAt(int Position) {
		Data.RemoveAt (Position);
	}
	public Block1 GetData(int PositionInList) {
		return Data [PositionInList];
	}
	public int GetSize() {
		return Data.Count;
	}
};


[System.Serializable]
public class Block0Serial {
	public int Type;
	public bool IsActivated;
	//private Color32 MyColor;
};

[System.Serializable]
public class Block1Serial {
	public List<Block0Serial> Data = new List<Block0Serial>();
};
[System.Serializable]
public class Block2Serial {
	public List<Block1Serial> Data = new List<Block1Serial>();
};

[System.Serializable]
public class BlocksSerial {
	public List<Block2Serial> Data = new List<Block2Serial>();
	public float SizeX, SizeY, SizeZ;

	public BlocksSerial(List<Block2> NewData, Vector3 NewSize) {
		SizeX = NewSize.x; SizeY = NewSize.y; SizeZ = NewSize.z;
		
		for (int i = 0; i < SizeX; i++) 
		{
			Block2Serial NewBlock2 = new Block2Serial(); 
			for (int j = 0; j < SizeY; j++) 
			{
				Block1Serial NewBlock1 = new Block1Serial();
				for (int k = 0; k < SizeZ; k++) 
				{
					Block0Serial NewBlock0 = new Block0Serial();
					NewBlock0.Type = NewData[i].GetData(j).GetData(k).GetType();
					NewBlock0.IsActivated = NewData[i].GetData(j).GetData(k).GetIsActivated();
					NewBlock1.Data.Add(NewBlock0);
				}
				NewBlock2.Data.Add(NewBlock1);
			}
			Data.Add(NewBlock2);
		}
	}
	public List<Block2> GetData() {
		List<Block2> NewData = new List<Block2>();
		
		for (int i = 0; i < SizeX; i++) 
		{
			Block2 NewBlock2 = new Block2(); 
			for (int j = 0; j < SizeY; j++) 
			{
				Block1 NewBlock1 = new Block1();
				for (int k = 0; k < SizeZ; k++) 
				{
					Block0 NewBlock0 = new Block0();
					NewBlock0.SetType(Data[i].Data[j].Data[k].Type);
					NewBlock0.SetIsActivated(Data[i].Data[j].Data[k].IsActivated);
					NewBlock1.AddData(NewBlock0);
				}
				NewBlock2.AddData(NewBlock1);
			}
			NewData.Add(NewBlock2);
		}
		return NewData;
	}
	public Vector3 GetSize() {
		return new Vector3 (SizeX, SizeY, SizeZ);
	}

}
//Stores the block data
//Used to get data about it
[System.Serializable]
public class Blocks {
	private List<Block2> Data;
	public Vector3 Size = new Vector3(16,16,16);
	public Vector3 Scale = new Vector3(1,1,1);
	public bool IsOverride = false;
	public bool DoesChangeSize = true;
	public List<int> BlockAmounts = new List<int> ();
	public bool HasUpdated = false;
	public bool IsCentred = true;

	public List<Block2> GetData() { return Data; }
	public void SetData(List<Block2> NewData) { Data = NewData; }
	public Blocks() {
		Data = new List<Block2> ();
		InitilizeData ();
	}
	public void ReCreateBlocks() {
		for (int i = 0; i < Size.x; i++) 
		{
			for (int j = 0; j < Size.y; j++) 
			{
				for (int k = 0; k < Size.z; k++) 
				{
					GetBlock(new Vector3(i,j,k)).ReCreate();
				}
			}
		}
	}
	public void ClearData() {
		Data.Clear ();
	}
	public void InitilizeData() {
		ClearData ();
		for (int i = 0; i < Size.x; i++) 
		{
			Block2 NewBlock2 = new Block2(); 
			for (int j = 0; j < Size.y; j++) 
			{
				Block1 NewBlock1 = new Block1();
				for (int k = 0; k < Size.z; k++) 
				{
					Block0 NewBlock0 = new Block0();
					NewBlock1.AddData(NewBlock0);
				}
				NewBlock2.AddData(NewBlock1);
			}
			Data.Add(NewBlock2);
		}
		for (int i = 0; i < 32; i++) {
			BlockAmounts.Add (0);
		}
	}
	public void SetBlockData0(Block1 NewBlock1) {
		for (int k = 0; k < Size.z; k++) 
		{
			Block0 NewBlock0 = new Block0();
			NewBlock1.AddData(NewBlock0);
		}
	}
	public bool ClearBlockMesh(Vector3 Position) {
		if (IsPositionInBlockBoundaries (Position)) {
			GetBlock(Position).ClearBlockMesh();
			return true;
		}
		return false;
	}
	
	public Block0 GetBlock(Vector3 Position) {
		if (IsPositionInBlockBoundaries (Position)) {
			return Data [Mathf.FloorToInt(Position.x)].GetData (Mathf.FloorToInt(Position.y)).GetData (Mathf.FloorToInt(Position.z));
		} else {
			// should get it from world class instead
			return new Block0 ();
		}
	}
	public MyMesh GetBlockMesh(Vector3 Position) {
		if (IsPositionInBlockBoundaries (Position)) {
			return GetBlock(Position).GetBlockMesh();
		} else {
			// should get it from world class instead
			return new MyMesh ();
		}
	}
	public bool UpdateLights(Vector3 Position, Color32 NewColor) {
		if (IsPositionInBlockBoundaries (Position)) {
			GetBlock(Position).SetColor(NewColor);
		}
		return false;
	}
	public bool CheckSliceX(Vector3 Position) {
		for (int i = 0; i < Size.y; i++) 
		for (int j = 0; j < Size.z; j++) {
			Vector3 CheckPosition = new Vector3(Position.x, i, j);
			if (GetBlockType(CheckPosition) != 0 && CheckPosition != Position) {
				return false;
			}
		}
		return true;
	}
	public bool CheckSliceY(Vector3 Position) {
		for (int i = 0; i < Size.x; i++) 
		for (int j = 0; j < Size.z; j++) {
			Vector3 CheckPosition = new Vector3(i, Position.y, j);
			if (GetBlockType(CheckPosition) != 0 && CheckPosition != Position) {
				return false;
			}
		}
		return true;
	}
	public bool CheckSliceZ(Vector3 Position) {
		for (int i = 0; i < Size.x; i++) 
		for (int j = 0; j < Size.y; j++) {
			Vector3 CheckPosition = new Vector3(i, j, Position.z);
			if (GetBlockType(CheckPosition) != 0 && CheckPosition != Position) {
				return false;
			}
		}
		return true;
	}
	public bool AdjustSizeForShrink(Vector3 Position) {
		if (DoesChangeSize) {// && IsPositionOnBlockBoundariesShrink (Position)) {
			bool HasShrunk = false;
			if (Position.x == 0)
			{
				if (CheckSliceX(Position)) {
					ShrinkStructureX(true);
					Size.x--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			} else if (Position.x == Size.x-1) {
				if (CheckSliceX(Position)) {
					ShrinkStructureX(false);
					Size.x--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			}
			if (Position.y == 0)
			{
				if (CheckSliceZ(Position)) {
					ShrinkStructureY(true, Position);
					Size.y--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			} else if (Position.y == Size.y-1) {
				if (CheckSliceY(Position)) {
					ShrinkStructureY(false, Position);
					Size.y--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			}
			if (Position.z == 0)
			{
				if (CheckSliceZ(Position)) {
					ShrinkStructureZ(true, Position);
					Size.z--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			} else if (Position.z == Size.z-1) {
				if (CheckSliceZ(Position)) {
					ShrinkStructureZ(false, Position);
					Size.z--;
					if (!HasShrunk)
						HasShrunk = true;
				}
			}
			return HasShrunk;
		}
		return false;
	}
	public void ShrinkStructureX(bool IsBefore) {
		int i = Mathf.RoundToInt(Size.x)-1;
		if (IsBefore) {
			i = 0;
		}
		//Debug.LogError ("Shrinking X: " + i);
		Data.RemoveAt (i);
	}
	public void ShrinkStructureY(bool IsBefore, Vector3 Position) {
		int y = Mathf.RoundToInt(Size.y)-1;
		if (IsBefore) {
			y = 0;
		}
		//Debug.LogError ("Shrinking Y: " + y);
		for (int i = 0; i < Data.Count; i++) {
			Data[i].RemoveAt(y);
		}
	}
	public void ShrinkStructureZ(bool IsBefore, Vector3 Position) {
		int z = Mathf.RoundToInt(Size.z)-1;
		if (IsBefore) {
			z = 0;
		}
		Debug.LogError ("Shrinking Z: " + z);
		//Data.RemoveAt (i);
		for (int i = 0; i < Data.Count; i++) {
			for (int j = 0; j < Data[i].GetSize(); j++) {
				Data[i].GetData(j).RemoveAt(z);
			}
		}
	}
	// returns the new size
	public Vector3 AdjustSize(Vector3 Position) {
		if (DoesChangeSize && IsPositionOnBlockBoundaries (Position)){
			if (Position.x == -1)
			{
				ExtendStructureX(true);
				Size.x++;
				Position.x++;
			} 
			else if (Position.x == Size.x)
			{
				ExtendStructureX(false);
				Size.x++;
			}
			if (Position.z == Size.z)
			{
				ExtendStructureZ(false);
				Size.z++;
			} 
			else if (Position.z == -1)
			{
				ExtendStructureZ(true);
				Size.z++;
				Position.z++;
			}
			
			if (Position.y == Size.y)
			{
				ExtendStructureY(false);
				Size.y++;
			}
			else if (Position.y == -1)
			{
				ExtendStructureY(true);
				Size.y++;
				Position.y++;
			}
		}
		return Position;
	}
	public void ExtendStructureX(bool IsBefore) {
		float i = Size.x;
		{
			Block2 NewBlock2 = new Block2(); 
			for (int j = 0; j < Size.y; j++) 
			{
				Block1 NewBlock1 = new Block1();
				for (int k = 0; k < Size.z; k++) 
				{
					Block0 NewBlock0 = new Block0();
					NewBlock1.AddData(NewBlock0);
				}
				NewBlock2.AddData(NewBlock1);
			}
			if (!IsBefore)
				Data.Add(NewBlock2);
			else 
				Data.Insert (0,NewBlock2);
		}
	}
	public void ExtendStructureY(bool IsBefore) {
		for (int i = 0; i < Data.Count; i++) {
			Block1 NewBlock1 = new Block1();
			for (int j = 0; j < Data[i].GetSize(); j++) {
				Block0 NewBlock0 = new Block0();
				NewBlock1.AddData(NewBlock0);
			}
			if (!IsBefore)
				Data[i].AddData(NewBlock1);
			else 
				Data[i].Insert(0,NewBlock1);
		}
	}
	public void ExtendStructureZ(bool IsBefore) {
		for (int i = 0; i < Data.Count; i++) {
			for (int j = 0; j < Data[i].GetSize(); j++) {
				Block0 NewBlock0 = new Block0();
				if (!IsBefore)
					Data[i].GetData(j).AddData(NewBlock0);
				else 
					Data[i].GetData(j).Insert(0,NewBlock0);
			}
		}
	}

	// returns true if a block is on the z side
	public bool IsBlockOnSideZ(bool IsBefore) {
		for (int i = 0; i < Data.Count; i++) {
			for (int j = 0; j < Data[i].GetSize(); j++) {
				if (IsBefore) {
					if (GetBlockType (new Vector3(i,j,0)) != 0)
						return true;
				} else {
					if (GetBlockType (new Vector3(i,j,Size.z-1)) != 0)
						return true;
				}
			}
		}
		return false;
	}
	public void ReduceStructureZ(bool IsBefore) {
		for (int i = 0; i < Data.Count; i++) {
			for (int j = 0; j < Data[i].GetSize(); j++) {
				if (!IsBefore)
					Data[i].GetData(j).RemoveAt(Data[i].GetData(j).GetSize()-1);
				if (!IsBefore)
					Data[i].GetData(j).RemoveAt(0);
			}
		}
	}
	void AddBlockIntoStructure(int i, int j, int k) {
		if (i >= Data.Count) {
			Data.Add (new Block2());
		}
	}
	
	public void UpdateBlock(Vector3 Position, int NewType, Vector2 UpdateSize) {
		for (float i = -UpdateSize.x; i < UpdateSize.x; i++)
		for (float j = -UpdateSize.y; j < UpdateSize.y; j++) {
			UpdateBlock(new Vector3(i, 0, j)+Position, NewType); 
		}
	}

	
	public bool IsOverrideNonEmpty = true;
	public bool UpdateBlock(Vector3 Position, int NewType) {
		if (DoesChangeSize) {
			if (NewType == 0) {
				Block0 ThisBlock = GetBlock (Position);
				int PreviousBlockType = ThisBlock.GetType ();
				bool DidShrink = AdjustSizeForShrink (Position);
				if (DidShrink) {
					HasUpdated = true;
					BlockAmounts [PreviousBlockType] --;
					return true;
				}
			} else {
				Position = AdjustSize (Position);
			}
		}

		if (IsPositionInBlockBoundaries (Position)) {
			if (IsOverrideNonEmpty || (!IsOverrideNonEmpty && GetBlockType(Position) == 0))
			if (GetBlockType (Position) != NewType) {
				Block0 ThisBlock = GetBlock (Position);
				int PreviousBlockType = ThisBlock.GetType();
				bool WasUpdated = ThisBlock.SetType (NewType);
				int CurrentBlockType = ThisBlock.GetType();
				if (WasUpdated) {
					//Debug.LogError ("Updating blocks mesh: " + Time.time);
					if (PreviousBlockType != 0)
						BlockAmounts[PreviousBlockType] --;
					if (CurrentBlockType != 0)
						BlockAmounts[CurrentBlockType] ++;
					HasUpdated = true;
				}
			}
			return true;
		}
		return false;
	}

	public int GetBlockType(Vector3 Position) {
		if (IsPositionInBlockBoundaries(Position)) {
			return GetBlock(Position).GetType ();
		}
		return 0;	// default value
	}

	public bool IsPositionOnBlockBoundariesShrink(Vector3 Position) {
		if (!IsPositionInBlockBoundaries(Position)) {
			return false;
		}
		Position.x = Mathf.Clamp (Position.x, 1, Size.x - 2);
		Position.y = Mathf.Clamp (Position.y, 1, Size.y - 2);
		Position.z = Mathf.Clamp (Position.z, 1, Size.z - 2);
		if (IsPositionInBlockBoundaries(Position)) {
			return false;
		}
		return true;
	}

	public bool IsPositionOnBlockBoundaries(Vector3 Position) {
		if (!IsPositionInBlockBoundaries (Position))
			return true;
		if (!(Position.x >= -1 && Position.x < Data.Count
			&& Position.y >= -1 && Position.y < Data [(int)(Position.x)].GetSize ()
			&& Position.z >= -1 && Position.z < Data [(int)(Position.x)].GetData ((int)(Position.y)).GetSize ()))
			return false;
		if (Position.x == -1 || Position.x == Data.Count)
			return true;

		if (Position.y == -1 || Position.y == Data [(int)(Position.x)].GetSize ())
			return true;
		if (Position.z == -1 || Position.z == Data [(int)(Position.x)].GetData ((int)(Position.y)).GetSize ())
			return true;
		return false;
	}
	public void Simplify() {

	}
	public bool IsPositionInBlockBoundaries(Vector3 Position) {
		if (Position.x >= 0 && Position.x < Data.Count)
			if (Position.y >= 0 && Position.y < Data [(int)(Position.x)].GetSize())
			if (Position.z >= 0 && Position.z < Data [(int)(Position.x)].GetData((int)(Position.y)).GetSize()) {
				return true;
			}
		return false;
	}

	// for every 8 blocks, check if any are not air, if so fill in all the blocks
	// if they are all air, leave them
	// or keep this information, fill in a new blocks data, and increase scale by times 2
	// then set updated to true
	public void IncreaseSubDivision() {

	}

	// Ray Tracing stuff
	
	// Converts blockPosition to world position
	public Vector3 GetWorldBlockPosition(Vector3 BlockPosition, Transform ModelTransform) {
		Vector3 WorldPosition = BlockPosition;
		//WorldPosition += transform.position;
		WorldPosition = ModelTransform.TransformPoint (WorldPosition);
		// Block Scaling
		WorldPosition.x *= Scale.x;
		WorldPosition.y *= Scale.y;
		WorldPosition.z *= Scale.z;
		// Mesh Centring hotspot
		if (IsCentred) 
		{
			if (ModelTransform.GetComponent<MyMeshHolder>()) {
				MyMesh BlockStructureMesh = ModelTransform.GetComponent<MyMeshHolder>().MeshData;
				if (BlockStructureMesh  != null)
					WorldPosition += BlockStructureMesh.DifferenceBounds;
			}
		}
		WorldPosition.x = Mathf.FloorToInt (WorldPosition.x);
		WorldPosition.y = Mathf.FloorToInt (WorldPosition.y);
		WorldPosition.z = Mathf.FloorToInt (WorldPosition.z);
		// account for the transform of the gameobject
		return WorldPosition;
	}

	public Vector3 GetBlockPosition(Vector3 WorldPosition, Transform ModelTransform) {
		WorldPosition = ModelTransform.InverseTransformPoint (WorldPosition);
		//WorldPosition
		// Block Scaling
		WorldPosition.x /= Scale.x;
		WorldPosition.y /= Scale.y;
		WorldPosition.z /= Scale.z;

		if (IsCentred) // Mesh Centring hotspot
		{
			if (ModelTransform.GetComponent<MyMeshHolder>()) {
				MyMesh BlockStructureMesh = ModelTransform.GetComponent<MyMeshHolder>().MeshData;
				if (BlockStructureMesh != null)
					WorldPosition -= BlockStructureMesh.DifferenceBounds;
			}
		}
		WorldPosition.x = Mathf.FloorToInt (WorldPosition.x);//+0.5f);
		WorldPosition.y = Mathf.FloorToInt (WorldPosition.y);//+0.5f);
		WorldPosition.z = Mathf.FloorToInt (WorldPosition.z);//+0.5f);
		return WorldPosition;
	}

	
	public bool UpdateBlockWithRay2(RaycastHit RayHit, int BlockType) {
		if (RayHit.collider == null || RayHit.collider.gameObject == null) {
			Debug.LogError ("No colliding!!");
			return false;
		}
		Vector3 BlockPosition = GetBlockPosition(RayHit.point-RayHit.normal*(RayHit.collider.gameObject.transform.localScale.x)*(Scale.x/2f), RayHit.collider.gameObject.transform);
		//Vector3 WorldPosition = GetWorldBlockPosition(BlockPosition, RayHit.collider.gameObject.transform);
		return UpdateBlock(BlockPosition,BlockType);
	}
	
	public bool UpdateBlockWithRay(RaycastHit RayHit, int BlockType) {
		if (RayHit.collider == null || RayHit.collider.gameObject == null) {
			Debug.LogError ("No colliding!!");
			return false;
		}
		//Debug.LogError ("Updating block with gameobjects transform: " + RayHit.collider.gameObject.transform.gameObject.name);
		//Debug.LogError ("Normal: " + RayHit.normal);
		Vector3 BlockNormal = RayHit.normal*RayHit.collider.gameObject.transform.localScale.x/2f;
		//Debug.LogError ("Inversed Normal: " + BlockNormal.ToString());
		Vector3 BlockPosition = GetBlockPosition(RayHit.point+BlockNormal, RayHit.collider.gameObject.transform);
		return UpdateBlock(BlockPosition,BlockType);
	}
};