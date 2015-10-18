using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/* Rooms to have floors
		Doors	
		Chairs
		Tables
		Lanterns on walls
		Bookshelves
		Beds
		Chains on walls in some places
		Prison Cells
		Windows
		A level of difficulty . they will spawn enemies
		A number of floors, as some will be battle towers with big boss at top
*/
/*[System.Serializable]
public class DungeonData {
}*/

// first check if room is connected to the maze
// if so, then use astar to the other rooms if they are connected
// if no, find the closest maze block and create a direct path to it from the rooms doors
[System.Serializable]
public class DungeonRoom {
	public Vector3 RoomLocation;	// location in the maze
	public Vector3 RoomSize;		// max dimensions of the structure
	public int RoomFloorType;		// type of blocks on the floor of the room
	public int RoomWallType;
	public int RoomStructureType;	// might be a circular room - 0 is a square room
	public bool IsSpawnMonsters;	// should monsters spawn in the room
	public int MonsterType;			// 
	public bool IsPillars;			// pillars may be randomly placed in room
	public bool IsDoors;			// if false there will be no door blocks, just air that stops people coming in
	public bool IsDoorsLocked;	
	public bool IsPowerUps;			// powers up spawners in the room?
	public bool IsTreasureChest;	// yeah... gold! or items! pl0x

	public DungeonRoom() {

	}
}

// need a gui for this
// right click on maze item - edit maze properties
// use AStar to check the paths between rooms

[System.Serializable]
public class Maze : BlockStructure {
	public bool IsRandomizeStartPosition = false;
	public bool IsMiddleStartLocation = false;
	public Vector3 StartLocation;			// start of maze building
	public Vector3 SpawnPosition;			// spawn point for player
	private int PathDirection = 0;
	private Vector3 Position;
	// Position that our dungeon builder is in
	private List<Vector3> DebugPositions = new List<Vector3>();
	bool IsHallways = true;
	public bool IsPaths = true;
	public int MinimumRooms = 2;
	public int MaximumRooms = 4;
	public int RoomFloorType = 3;
	public int RoomWallType = 3;
	public int RoomRoofType = 3;
	public int RoomDoorType = 0;
	public int RoomDoorSize = 2;
	//public DungeonData Dungeon;
	public bool HasGeneratedMaze = false;
	public Vector3 RoomSize = new Vector3(5,1,5);
	public bool IsFillEmpty = false;

	// Dungeon Data
	public int PathBlockType;
	public int MazeWallType;
	public int MazeRoofType;
	public List<DungeonRoom> Rooms = new List<DungeonRoom>();
	
	public float Sparceness;
	public float Linearity;
	
	private float MaxSparce;
	private float CurrentSparce = 0;
	private float SparcePercentage;
	public float GetSparcePercent() {
		return SparcePercentage;
	}
	int Iterations;
	public bool IsSparceEnough = false;

	public bool bIsGenerateBiome;
	public bool IsVoidAxisX;
	public bool IsVoidAxisY;
	public bool IsVoidAxisZ;
	public bool IsMazeRoof;
	public int MaxSpawnPoints;
	public int SpawnBlockType;
	public int FloorsMax = 1;
	public float MazeWallHeight;
	public bool bIsEdge;
	public bool bIsRemoveSinglePillars;
	public Vector3 PathSizeMinimum;
	public Vector3 PathSizeMaximum;
	private Vector3 PathSize;
	public bool bIsMoveToSize;
	public bool bIsPathways = true;
	public bool bIsWrap;
	public bool bIsMirrorX;
	public bool bIsMirrorY = true;
	public bool bIsMirrorZ;

	// character generation
	List<Vector3> SpawnLocations = new List<Vector3>();	// where to spawn bots when level is loaded
	// room variables
	public bool IsRoomsSeperate = true;
	static int MaxRoomLocationChecks = 750;
	int RoomApartness = 1;
	
	static int MaxIterations = 10000;
	public List<int> DebugDirections = new List<int> ();

	//public GameObject MazeTextureObject;
	//public Texture2D MazeTexture;
	public int MaxMipMapLevel;
	//public RawImage MyRawImage;
	public List<Color32> BlockColors = new List<Color32> ();
	
	public Maze() {
		MyBlocks.Size = new Vector3 (64, 16, 64);
		MyBlocks.InitilizeData ();
	}
	public void GenerateDungeon() {
		MyBlocks.InitilizeData ();
		GenerateDungeon (MyBlocks, MyBlocks.Size);
		//SetMazeToTexture ();
	}
	public void CheckStartLocationInBounds() {
		if (FloorsMax < 1)
			FloorsMax = 1;
		StartLocation = new Vector3 (Mathf.Clamp (StartLocation.x, 0, MyBlocks.Size.x - 1), 
		                           Mathf.Clamp (StartLocation.y, 0, MyBlocks.Size.y - 1), 
		                           Mathf.Clamp (StartLocation.z, 0, MyBlocks.Size.z - 1));
	}

	void GenerateDungeon(Blocks MyBlocks, Vector3 Size) {
		if (IsMiddleStartLocation) {
			StartLocation.x = MyBlocks.Size.x/2f;
			StartLocation.z = MyBlocks.Size.z/2f;
		}
		CurrentSparce = 0;
		if (bIsGenerateBiome) {
			GenerateBiomeMap(MyBlocks, Size);
		}
		else {
			Debug.Log ("Generating  ");
			//GenerateWallsOnEdge(MyBlocks, Size, MazeWallType);
			if (MinimumRooms > 0)
			{
				GenerateRooms();
			}
			if (IsPaths && Sparceness > 0) 
			{
				for (int i = 0; i < FloorsMax; i++) {
					GeneratePaths(Size, 
				              new Vector3(StartLocation.x, StartLocation.y, StartLocation.z + (MazeWallHeight + 1)*i), 
				             			PathBlockType, 
				              			Sparceness, 
				              			Linearity);
				}
			}
			if (MinimumRooms > 0)
			{
				if (IsHallways)
					GenerateHallways();
			}
			GeneratePathWalls(Size, 
			                  PathBlockType, 
			                  MazeWallType);
			//RemoveSinglePillars(MyBlocks, Size, PathBlockType);
			//GenerateSpawnLocations(MyBlocks, Size);
			/*if (IsFillEmpty) {
				FillEmptyPath(Size, PathBlockType, MazeWallType);
			}*/
		}
		if (IsRandomizeStartPosition)
			FindStartPosition (Size);
		else
			SpawnPosition = Rooms [0].RoomLocation + Rooms [0].RoomSize / 2f;	//StartLocation + new Vector3(0,1.5f,0);
	}
	public void FindStartPosition(Vector3 Size) {
		for (int i = 0; i < 1000; i++) {
			Vector3 CheckPosition = new Vector3(Random.Range (0, Size.x), Random.Range (0, Size.x), Random.Range (0, Size.x));
			if (IsVoidAxisX)
				CheckPosition.x = StartLocation.x;
			if (IsVoidAxisY)
				CheckPosition.y = StartLocation.y;
			if (IsVoidAxisZ)
				CheckPosition.z = StartLocation.z;
			if (MyBlocks.GetBlockType(CheckPosition) == PathBlockType 
			   && MyBlocks.GetBlockType(CheckPosition+new Vector3(0,1,0)) == 0) {
				SpawnPosition = CheckPosition+new Vector3(0,1.5f,0);
				Debug.Log("Found new position for spawning: " + SpawnPosition.ToString());
				break;
			}
		}
	}
	void GenerateSpawnLocations(Blocks MyBlocks, Vector3 Size) {
		//MyBlocks.UpdateBlock(StartLocation, SpawnBlockType);
		int MaxBotSpawnBlocks = MaxSpawnPoints;
		int BotSpawnBlocksCount = 0;
		bool bIsBotSpawns = true;
		if (bIsBotSpawns) {
			while (BotSpawnBlocksCount <= MaxBotSpawnBlocks) {
				int i = Mathf.RoundToInt(Random.Range(0f, Size.x-1f));
				int j = Mathf.RoundToInt(Random.Range(0f, Size.y-1f));
				int k = Mathf.RoundToInt(Random.Range(0f, Size.z-1f));
				if (IsVoidAxisZ)
					k = (int)StartLocation.z;
				if (IsVoidAxisY)
					j = (int)StartLocation.y;				
				if (IsVoidAxisX)
					i = (int)StartLocation.x;
				if (MyBlocks.GetBlockType(new Vector3(i, j, k)) == PathBlockType) {
					SpawnLocations.Add (new Vector3(i,j,k));
					//MyBlocks.UpdateBlock(new Vector3(i, j, k), SpawnBlockType);
					BotSpawnBlocksCount++;
				}
			}
		}
	}
	// can inverse the overlap check to make the rooms stick together
	public Vector3 FindNewRoomLocation(Vector3 MyRoomSize) {
		if (!IsRoomsSeperate)
			return new Vector3( Random.Range(0,MyBlocks.Size.x-1-MyRoomSize.x), 
		                                           StartLocation.y,
		                                           Random.Range(0,MyBlocks.Size.z-1-MyRoomSize.z));
		int CheckCount = 0;
		Vector3 NewRoomLocation = new Vector3 (-1, -1, -1);
		while (CheckCount < MaxRoomLocationChecks) 
		{
			CheckCount++;
			// check a new room location within bounds of map
			Vector3 CheckNewRoomLocation = new Vector3( Mathf.RoundToInt(Random.Range(0,MyBlocks.Size.x-1-MyRoomSize.x)), 
		                                      StartLocation.y,
			                              Mathf.RoundToInt(Random.Range(0,MyBlocks.Size.z-1-MyRoomSize.z)));
			if (Rooms.Count == 0)
				return CheckNewRoomLocation;
			bool DoesOverlapRoom = false;
			for (int i = 0; i < Rooms.Count; i++) {
				Vector3 RoomLocation2 = Rooms[i].RoomLocation;
				Vector3 RoomSize2 = Rooms[i].RoomSize;
				//RoomLocation2 -= new Vector3(RoomApartness,0,RoomApartness);
				//RoomSize2 += new Vector3(RoomApartness,0,RoomApartness);
				// if AARB do not overlap
				if (!(CheckNewRoomLocation.x+RoomApartness + MyRoomSize.x < RoomLocation2.x-RoomApartness 
				      || CheckNewRoomLocation.x-RoomApartness > RoomLocation2.x+RoomSize2.x+RoomApartness
				    // || CheckNewRoomLocation.y + MyRoomSize.y < RoomLocation2.y || CheckNewRoomLocation.y > RoomLocation2.y+RoomSize2.y
				      || CheckNewRoomLocation.z+RoomApartness + MyRoomSize.z < RoomLocation2.z -RoomApartness
				      || CheckNewRoomLocation.z-RoomApartness > RoomLocation2.z+RoomSize2.z+RoomApartness))
				   {
					DoesOverlapRoom = true;
					i = Rooms.Count;
				}
			}
			if (!DoesOverlapRoom)
				return CheckNewRoomLocation;
		}
		return NewRoomLocation;
	}
	void GenerateRooms() {
		//Debug.LogError(" Generating Rooms!");
		Rooms.Clear ();
		int RoomsToGenerate = Random.Range (MinimumRooms, MaximumRooms);
		//RoomsToGenerate = MinimumRooms;
		for (int i = 0; i < RoomsToGenerate; i++) {
			DungeonRoom NewRoom = new DungeonRoom();
			NewRoom.RoomSize = new Vector3(RoomSize.x,RoomSize.y,RoomSize.z);
			NewRoom.RoomLocation = FindNewRoomLocation(RoomSize);	
			
			if (NewRoom.RoomLocation != new Vector3(-1,-1,-1)) {
				if (Rooms.Count == 0) {
					SpawnPosition = NewRoom.RoomLocation+NewRoom.RoomSize/2f;
				}
				Rooms.Add (NewRoom);
			}
		}

		for (int z = 0; z < Rooms.Count; z++) {
			Vector3 RoomLocation = Rooms[z].RoomLocation;
			Vector3 MyRoomSize = Rooms[z].RoomSize;

			for (float i = RoomLocation.x; i <= RoomLocation.x + MyRoomSize.x; i++)
				for (float j =  RoomLocation.y; j <= RoomLocation.y + MyRoomSize.y; j++)
					for (float k =  RoomLocation.z; k <= RoomLocation.z + MyRoomSize.z; k++) 
					{

					if (i == RoomLocation.x || i == RoomLocation.x + MyRoomSize.x ||
					    j == RoomLocation.y || j == RoomLocation.y + MyRoomSize.y ||
					    k == RoomLocation.z || k == RoomLocation.z + MyRoomSize.z)
							MyBlocks.UpdateBlock(new Vector3(i, j, k), RoomFloorType);
				}
			// spawn monster!
			MyBlocks.UpdateBlock(RoomLocation+MyRoomSize/2f, 16);
		}
	}
	public bool IsLinearHallways = true;	// first room to next one!
	public float HallwaySize = 1.5f;
	public List<Vector3> DoorSpawnLocations = new List<Vector3>();

	public bool IsOnEdgeOfRoom(Vector3 Position, DungeonRoom MyRoom) {
		Vector3 RoomLocation = MyRoom.RoomLocation;
		Vector3 MyRoomSize = MyRoom.RoomSize;
		int i = Mathf.FloorToInt(Position.x);
		int j = Mathf.FloorToInt(Position.y);
		int k = Mathf.FloorToInt(Position.z);
		if (i == RoomLocation.x || i == RoomLocation.x + MyRoomSize.x ||
			//j == RoomLocation.y || j == RoomLocation.y + MyRoomSize.y ||
			k == RoomLocation.z || k == RoomLocation.z + MyRoomSize.z) {
			return true;
		}
		return false;
	}

	void GenerateHallways() {
		for (int i = 0; i < Rooms.Count; i++) {
			if (i != Rooms.Count-1 && IsHallways) {
				Vector3 Position1 = Rooms[i+1].RoomLocation+Rooms[i+1].RoomSize/2f; 
				Position1.y = Rooms[i+1].RoomLocation.y;
				Vector3 Position2 = Rooms[i].RoomLocation+Rooms[i].RoomSize/2f; 
				Position2.y = Rooms[i].RoomLocation.y;
				Vector3 RoomDirection = (Position1-Position2).normalized;
				float Distance = Vector3.Distance(Position1, Position2);
				Vector3 HallwayPosition = Position2;
				MyBlocks.IsOverrideNonEmpty = false;
				bool HasAddedDoor = false;
				for (int g = 0; g < Distance; g++) 
				{
					MyBlocks.UpdateBlock(HallwayPosition, PathBlockType, new Vector2(HallwaySize,HallwaySize));
					MyBlocks.UpdateBlock(HallwayPosition+new Vector3(0,1,0), 0, new Vector2(HallwaySize,HallwaySize));
					MyBlocks.UpdateBlock(HallwayPosition+new Vector3(0,2,0), 0, new Vector2(HallwaySize,HallwaySize));

					if (!HasAddedDoor && 
					    MyBlocks.GetBlockType(HallwayPosition) == RoomFloorType && 
					    (IsOnEdgeOfRoom(HallwayPosition, Rooms[i]) || IsOnEdgeOfRoom(HallwayPosition, Rooms[i+1]))) 
					{
						MyBlocks.IsOverrideNonEmpty = true;
						MyBlocks.UpdateBlock(HallwayPosition+new Vector3(0,1,0), RoomDoorType);
						MyBlocks.UpdateBlock(HallwayPosition+new Vector3(0,2,0), RoomDoorType);
						DoorSpawnLocations.Add (HallwayPosition);
						MyBlocks.IsOverrideNonEmpty = false;
						//HasAddedDoor = true;
					}
					HallwayPosition += RoomDirection;
				}
				MyBlocks.IsOverrideNonEmpty = true;
			}
		}
	}

	public void GenerateWallsOnEdge(int MazeWallType) {
		for (int i = 0; i < MyBlocks.Size.x; i++)
			//for (int j = 0; j < Size.y; j++)
		for (int k = 0; k < MyBlocks.Size.z; k++) {
			if (IsOnEdge(new Vector3(i, StartLocation.y, k), MyBlocks.Size) && MyBlocks.GetBlockType (new Vector3 (i, StartLocation.y, k)) == 0) {
				for (int z = 0; z < MazeWallHeight + 1; z++) {
							MyBlocks.UpdateBlock (new Vector3 (i, 0 + z, k), MazeWallType);
						}
					}
				}
	}

	public void FillEmptyPath(Vector3 Size, int PathBlockType, int FillBlockType) {
		int j = Mathf.RoundToInt (StartLocation.y);
		for (int i = 0; i < Size.x; i++)
			//for (int j = 0; j < Size.y; j++)
			for (int k = 0; k < Size.z; k++) {
			if (MyBlocks.GetBlockType(new Vector3(i, j, k)) == 0) {
				
				for (int z = 0; z < MazeWallHeight + 1; z++) {
					MyBlocks.UpdateBlock(new Vector3(i, j + z, k), FillBlockType);
				}
			}
			}
	}
	// needs to use is void axis to find out how to build walls
	// for every path block, build a wall in the empty spot next to it
	public void GeneratePathWalls(Vector3 Size, int PathBlockType, int MazeWallType) {
		for (int i = 0; i < Size.x; i++)
			for (int j = 0; j < Size.y; j++)
				for (int k = 0; k < Size.z; k++)
			{
				if (IsMazeRoof)
				if (MyBlocks.GetBlockType(new Vector3(i, j, k)) == PathBlockType) {
					MyBlocks.UpdateBlock(new Vector3(i, j + MazeWallHeight, k), MazeRoofType);
				}
				if (MyBlocks.GetBlockType(new Vector3(i, j, k)) == 0
				    && (MyBlocks.GetBlockType(new Vector3(i + 1, j, k)) == PathBlockType || MyBlocks.GetBlockType(new Vector3(i - 1, j, k)) == PathBlockType
				    || MyBlocks.GetBlockType(new Vector3(i, j, k + 1)) == PathBlockType || MyBlocks.GetBlockType(new Vector3(i, j, k - 1)) == PathBlockType)) {
					for (int z = 0; z < MazeWallHeight + 1; z++) {
						MyBlocks.UpdateBlock(new Vector3(i, j + z, k), MazeWallType);
					}
				}
			}
		if (bIsEdge) {
			for (int i = 0; i < Size.x; i++)
				for (int j = 0; j < Size.y; j++)
					for (int k = 0; k < Size.z; k++)
				{
					if (IsOnEdge(new Vector3(i, j, k), Size) && MyBlocks.GetBlockType(new Vector3(i, j, k)) == PathBlockType) {
						for (int z = 0; z < MazeWallHeight + 1; z++) {
							MyBlocks.UpdateBlock(new Vector3(i, j + z, k), MazeWallType);
						}
					}
				}
		}
	}
	
	bool IsOnEdge(Vector3 Position, Vector3 Size) {
		if (Position.x == Size.x - 1 || Position.x == 0 
		    // ||Position.y == Size.y - 1 || Position.y == 0
		    || Position.z == Size.z - 1 || Position.z == 0
		    )
			return true;
		return false;
	}
	// Runs the maze like algorithm, from starting points of biomes, until a biome type covers the whole surface
	// render it to texture to see
	// generate paths from multiple positionsh
	void GenerateBiomeMap(Blocks MyBlocks, Vector3 Size) {
		/*Vector3 MinimumDistanceToOthers;
		Vector3 MaximumDistanceToOthers;
		List<Vector3> StartingLocations;
		int MinimumBiomes = 2;
		int MaximumBiomes = 5;
		int NumberOfBiomes = 3;
		for (int i = 0; i < NumberOfBiomes; i++) {
			if (IsLockedAxis)
				StartingLocations.Add(new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), StartLocation.z));
			else
				StartingLocations.Add(new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z)));
		}
		CurrentSparce = 0;
		for (int i = 0; i < StartingLocations.Count; i++) {
			GeneratePaths(MyBlocks, Size, Dungeon, StartingLocations[i], PathBlockType + i, (Sparceness)*(i + 1), Linearity);
		}
		*/
	}
	int CheckedBlocks = 0;
	bool HasFoundNewPosition = false;
	void CheckForNewPosition() {

	}
	// Need a way that gives a maximum amount of up/down blocks at once
	// maybe it should remember the last few directions
	// no it shou	ld create a direction map lol.
	void GeneratePaths(Vector3 Size, Vector3 StartLocation,  int PathBlockType, float Sparceness, float Linearity) {
		// set defaults
		
		//if (bIsEdge && IsPositionOutsideBounds(StartLocation, Size, bIsEdge)) {
		//	if (StartLocation.y == 0) StartLocation.y = 1;
		//}
		    Debug.Log ("Generating paths!");
		DebugDirections.Clear();
		Iterations = 0;
		IsSparceEnough = false; 
		CurrentSparce = 0;
		MaxSparce = 1;
		if (!IsVoidAxisX)
			MaxSparce *= Size.x;
		if (!IsVoidAxisY)
			MaxSparce *= Size.y;
		if (!IsVoidAxisZ)
			MaxSparce *= Size.z;
		//MaxSparce = Size.x*Size.z;
		//PathSizeMinimum = PathSizeMinimum;
		if (PathSizeMinimum.x < 1) PathSizeMinimum.x = 1;
		if (PathSizeMinimum.y < 1) PathSizeMinimum.y = 1;
		if (PathSizeMinimum.z < 1) PathSizeMinimum.z = 1;
		if (PathSizeMaximum.x < 1) PathSizeMaximum.x = 1;
		if (PathSizeMaximum.y < 1) PathSizeMaximum.y = 1;
		if (PathSizeMaximum.z < 1) PathSizeMaximum.z = 1;
		PathSize = new Vector3 (Random.Range (PathSizeMinimum.x, PathSizeMaximum.x), 1, Random.Range (PathSizeMinimum.z, PathSizeMaximum.z));
		DebugPositions.Clear ();
		Position = StartLocation;
		PathDirection = 0;
		// starting block
		if (MyBlocks.UpdateBlock(Position, PathBlockType))
			CurrentSparce++;
		while (!IsSparceEnough) {
			IteratePath(Size, StartLocation, PathBlockType, Sparceness, Linearity);
		}
	}
	public bool IsTypeDungeonBlock(int Type) {
		return (Type != 0 && Type != MazeWallType);
	}
	void  IteratePath(Vector3 Size, Vector3 StartLocation, int PathBlockType, float Sparceness, float Linearity) {

		List<Vector3> Positions = new List<Vector3>();
		// Check for Sparce Percentage
		int TurnChance = Mathf.RoundToInt(Random.Range(1f, 100f));
		if (PathDirection <= 0 || TurnChance >= Linearity) {
			// Randomly Make a new direction	
			if (PathDirection == 0) {
				HasFoundNewPosition = false;
				CheckedBlocks = 0;
				int FindPositionBlock = 200;
				int MaximumChecksForNewPosition = 0;
				while (!HasFoundNewPosition || CheckedBlocks == MaxSparce || MaximumChecksForNewPosition >= 250) {
					MaximumChecksForNewPosition++;
					Position.x = Mathf.RoundToInt(Random.Range(1, Size.x-2));
					Position.z = Mathf.RoundToInt(Random.Range(1, Size.z-2));
					if (IsTypeDungeonBlock(MyBlocks.GetBlockType(Position))) {
						HasFoundNewPosition = true;
					} else {
						/*if (MyBlocks.GetBlockType(Position) != FindPositionBlock) {
							CheckedBlocks++;
							MyBlocks.UpdateBlock(Position, FindPositionBlock);
						}*/
					}
				}
				if (MaximumChecksForNewPosition >= 250) {
					Debug.LogError("Wow exceded maximum checks..");
					Position = StartLocation;
				}
				/*for (int i = 0; i < Size.x; i++)
					for (int j = 0; j < Size.z; j++)
				{
					Vector3 NewPosition = new Vector3(i, Position.y, j);
					if (MyBlocks.GetBlockType(NewPosition) == FindPositionBlock)
						MyBlocks.UpdateBlock(Position, 0);
				}*/
			}
			// Search for a new place in the maze, where the path already is, and start digging again
			List<int> PossiblePathDirection = new List<int>();

			if (!IsVoidAxisX) {
				PossiblePathDirection.Add (1);
				PossiblePathDirection.Add (2);
			}
			if (!IsVoidAxisY) {
				PossiblePathDirection.Add (3);
				PossiblePathDirection.Add (4);
			}
			if (!IsVoidAxisZ) {
				PossiblePathDirection.Add (5);
				PossiblePathDirection.Add (6);
			}
			if (PossiblePathDirection.Count > 0) {
				int PathDirectionIndex = Mathf.RoundToInt(Random.Range(0f, PossiblePathDirection.Count-1f));
				PathDirection = PossiblePathDirection[PathDirectionIndex];
			}
			//PathDirection = (int)(Random.Range(1,4));
			//Debug.Log ("PathDirection: " + PathDirection + " : Iteration: " + Iterations);
			DebugDirections.Add (PathDirection);
		}
		if (PathDirection == 1) {	// going right
			Position.x++;
			if (bIsMoveToSize)
				Position.x += PathSize.x;
		}
		else if (PathDirection == 2) {	// going left
			Position.x--;
			if (bIsMoveToSize)
				Position.x -= PathSize.x;
		}
		else if (PathDirection == 3) {	// going up
			Position.y++;
			if (bIsMoveToSize)
				Position.y += PathSize.y;
		}
		else if (PathDirection == 4) {	// going down
			Position.y--;
			if (bIsMoveToSize)
				Position.y -= PathSize.y;
		}
		else if (PathDirection == 5) {	// going forward
			Position.z++;
			if (bIsMoveToSize)
				Position.z += PathSize.z;
		}
		else if (PathDirection == 6) {	// going back
			Position.z--;
			if (bIsMoveToSize)
				Position.z -= PathSize.z;
		}
		Positions.Clear();
		for (float i = -PathSize.x; i < PathSize.x + 1; i++)
			for (float j = -PathSize.y; j < PathSize.y + 1; j++)
			for (float k = -PathSize.z; k < PathSize.z + 1; k++) {
				Positions.Add(new Vector3(Position.x + i, Position.y + j, Position.z + k));
				//DebugPositions.Add (new Vector3(Position.x + i, Position.y + j, Position.z + k));
			}
		//DebugDirections.Add (PathDirection);

		// Checks to Stop Growing of Dungeon!
		// now here if Moving position, do checks on blocks etc for pathways
		// if blocks on left and right are not taken, then build path, else don't build path
		if (bIsPathways) 
		{
			// this is now up down
			Vector3 BackPosition = new Vector3(Position.x, Position.y - 1 - PathSize.y, Position.z);
			Vector3 ForwardPosition = new Vector3(Position.x, Position.y + 1 + PathSize.y, Position.z);
			if (PathDirection == 3 || PathDirection == 4) {
				//if (IsStopPath(MyBlocks.GetBlockType(BackPosition)) || MyBlocks.GetBlockType(ForwardPosition) == PathBlockType) {
					//PathDirection = 0;
				//}
			}

			Vector3 LeftPosition = new Vector3(Position.x - 1 - PathSize.x, Position.y, Position.z);
			Vector3 RightPosition = new Vector3(Position.x + 1 + PathSize.x, Position.y, Position.z);
			
			if (PathDirection == 5 || PathDirection == 6) {
				if (IsStopMaze(MyBlocks.GetBlockType(LeftPosition)) || IsStopMaze(MyBlocks.GetBlockType(RightPosition))) {
					PathDirection = 0;
				}
			}
			// forward, back
			Vector3 BottomPosition = new Vector3(Position.x, Position.y, Position.z - 1 - PathSize.z);
			Vector3 TopPosition = new Vector3(Position.x, Position.y, Position.z + 1 + PathSize.z);
			if (PathDirection == 1 || PathDirection == 2) {
					if (IsStopMaze(MyBlocks.GetBlockType(BottomPosition))  || IsStopMaze(MyBlocks.GetBlockType(BottomPosition))) {
					PathDirection = 0;
				}
			}
		}

		// Now do checks for positions intersecting with path
		if (IsPositionOutsideBounds(Position, Size, bIsEdge)) {
			if (!bIsWrap) {
				//PathDirection = -1;	// Maybe I should just force the direction to turn rather then just end it like this
				PathDirection = 0;	// Maybe I should just force the direction to turn rather then just end it like this
				if (Position.x > Size.x - 1) Position.x = Size.x - 1;
				if (Position.x < 0) Position.x = 0;
				if (Position.y > Size.y - 1) Position.y = Size.y - 1;
				if (Position.y < 0) Position.y = 0;
				if (Position.z > Size.z - 1) Position.z = Size.z - 1;
				if (Position.z < 0) Position.z = 0;
			}
			else {
				PathDirection = 0;	// Maybe I should just force the direction to turn rather then just end it like this
				if (Position.x > Size.x - 1) Position.x = 0;
				if (Position.x < 0) Position.x = Size.x - 1;
				if (Position.y > Size.y - 1) Position.y = 0;
				if (Position.y < 0) Position.y = Size.y - 1;
				if (Position.z > Size.z - 1) Position.z = 0;
				if (Position.z < 0) Position.z = Size.z - 1;
			}
		}

		DebugDirections.Add (PathDirection);
		if (PathDirection > 0) {
			for (int z = 0; z < Positions.Count; z++) {
				if (MyBlocks.GetBlockType(Positions[z]) == 0) {
					//MyBlocks.UpdateBlock(Positions[z], PathBlockType);
					UpdateBlock(Positions[z], Size, PathBlockType);

				}
			}
		}
		//Debug.Log (Iterations + " : PositionsCount: " + Positions.Count);
		
		Iterations++;
		if (Iterations >= MaxIterations) {
			IsSparceEnough = true;
		}
		SparcePercentage = (CurrentSparce / MaxSparce);
		if (100f * SparcePercentage > Sparceness) {
			IsSparceEnough = true;
			Debug.Log ("Is Sparce Enough: " + 100f * SparcePercentage + " Percent.");
		}
	}
	bool IsPositionOutsideBounds(Vector3 BoundsPosition, Vector3 Size, bool IsEdge) {
		bool IsOutOfBounds = false;
		if (!IsVoidAxisX)
			if (BoundsPosition.x >= Size.x - PathSize.x -1 || BoundsPosition.x <= PathSize.x+1)
				IsOutOfBounds = true;
		if (!IsVoidAxisY)
			if (BoundsPosition.y >= Size.y - PathSize.y - 1 || BoundsPosition.y <= PathSize.y + 1)
				IsOutOfBounds = true;
		if (!IsVoidAxisZ)
			if (BoundsPosition.z >= Size.z - PathSize.z - 1 || BoundsPosition.z <= PathSize.z + 1)
			IsOutOfBounds = true;
		/*if (!IsEdge)
			return (BoundsPosition.x > Size.x - 1 || BoundsPosition.x < 0 ||
		        BoundsPosition.y > Size.y - 1 || BoundsPosition.y < 0 ||
		        BoundsPosition.z > Size.z - 1 || BoundsPosition.z < 0);
		else
			return (BoundsPosition.x > Size.x - 2 || BoundsPosition.x < 1 ||
			        BoundsPosition.y > Size.y - 2 || BoundsPosition.y < 1 ||
			        BoundsPosition.z > Size.z - 2 || BoundsPosition.z < 1);*/
		return IsOutOfBounds;
	}

	public bool IsPathOverride = false;	// if override, then only stop path on path blocks, else stop on any other block type thats solid
	public bool IsStopMaze(int BlockType) {
		if (BlockType == 0) {
			return false;
		} else if (!IsPathOverride)
				return true;
		else if (IsPathOverride && !IsTypeDungeonBlock(BlockType)) // stops for anything but itself
			return true;
		else
			return false;
	}

	void UpdateBlock(Vector3 Position, Vector3 Size, int BlockType) {
		if (MyBlocks.UpdateBlock(Position, BlockType))
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
			if (MyBlocks.UpdateBlock(BlockPlacementPositions[i], BlockType))
				CurrentSparce++;
		}
	}
	
	// Searches the Blocks for a empty block, that is surrounded by 4 path blocks, then it removes it or filles it with a special pillar looking block
	void RemoveSinglePillars(Blocks Blocks, Vector3 Size, int PathBlockType) {
		if (bIsRemoveSinglePillars)
			for (int i = 0; i < Size.x; i++)
				for (int j = 0; j < Size.y; j++)
					for (int k = 0; k < Size.z; k++)
				{
					if (Blocks.GetBlockType(new Vector3(i, j, k)) == 0) {
						if (Blocks.GetBlockType(new Vector3(i + 1, j, k)) == PathBlockType && MyBlocks.GetBlockType(new Vector3(i - 1, j, k)) == PathBlockType &&
						    MyBlocks.GetBlockType(new Vector3(i, j + 1, k)) == PathBlockType && MyBlocks.GetBlockType(new Vector3(i, j - 1, k)) == PathBlockType)
							Blocks.UpdateBlock(new Vector3(i, j, k + MazeWallHeight), PathBlockType);
					}
				}
	}
	
	public void SetDefaultColors() {
		BlockColors.Clear ();
		BlockColors.Add (new Color32(125,165,165,255));
		BlockColors.Add (new Color32(200,200,200,255));	// cobblestone
		BlockColors.Add (new Color32(55,255,255,255));	// grass
		BlockColors.Add (new Color32(66,66,66,255));	// brick - 3 
		BlockColors.Add (new Color32(255,0,0,255));		// red brick - 4
		BlockColors.Add (new Color32(55,155,55,255));		// Hexgon brick - 4
		BlockColors.Add (new Color32(255,255,55,255));		// 1,0 brick - 4
		BlockColors.Add (new Color32(155,155,55,255));		// 1,0 brick - 4
		for (int i = BlockColors.Count; i < 64; i++) {
			BlockColors.Add (new Color32((byte)(i*25),(byte)(i*15),(byte)(i*5),255));
		}
	}
	public Texture2D GetMazeTexture() {
		SetDefaultColors ();
		Debug.Log ("Generating Texture.");
		//MeshRenderer MyRenderer = MazeTextureObject.GetComponent<MeshRenderer> ();
		
		// duplicate the original texture and assign to the material
		Texture2D MyTexture2D = new Texture2D (Mathf.FloorToInt(MyBlocks.Size.x), 
		                                       Mathf.FloorToInt(MyBlocks.Size.z));	//MyRenderer.material.mainTexture);
		//MyRenderer.materials[0].SetTexture(0, MyTexture2D);
		//MyRawImage.texture = (MyTexture2D);
		MaxMipMapLevel = MyTexture2D.mipmapCount;
		MyTexture2D.filterMode = FilterMode.Point;
		int MipMapLevel = 0;
		//for (int z = MaxMipMapLevel; MaxMipMapLevel < MaxMipMapLevel; MaxMipMapLevel++) {
		MyTexture2D.Resize (Mathf.RoundToInt (MyBlocks.Size.x), Mathf.RoundToInt (MyBlocks.Size.z));
		//Texture2D MyTexture2D = MyTexture;
		Color32[] NewColors = MyTexture2D.GetPixels32 (MipMapLevel);
		for (int i = 0; i < MyBlocks.Size.x; i++)
		for (int k = 0; k < MyBlocks.Size.z; k++) {
			int PixelIndex = Mathf.RoundToInt (i * MyBlocks.Size.x + k);
			NewColors[PixelIndex] = BlockColors[MyBlocks.GetBlockType (new Vector3 (i, StartLocation.y, k))];
		}
		MyTexture2D.SetPixels32 (NewColors, MipMapLevel);
		MyTexture2D.Apply( true );
		Debug.Log ("Applied Texture.");
		return MyTexture2D;
	}
}

