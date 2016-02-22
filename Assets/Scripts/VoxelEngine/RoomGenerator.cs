using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine {
	
	
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

		public bool IsConnected = false;	// connected to main grid
		public float Closeness = 0;

		public DungeonRoom() {
			
		}
		public Vector3 GetMidPoint() {
			return RoomLocation + RoomSize / 2f;
		}
		public void CalculateCloseness(List<DungeonRoom> Rooms) {
			Closeness = 0;
			List<float> Distances = new List<float> ();
			for (int i = 0; i < Rooms.Count; i++) 
			{
				if (Rooms[i] != this)
				Distances.Add (Vector3.Distance(RoomLocation, Rooms[i].RoomLocation));
			}
			for (int i = 0; i < Distances.Count; i++) {
				Closeness += Distances[i];
			}
			Closeness /= Distances.Count;
		}
	}
public class SortRooms : IComparer<DungeonRoom>
	{
		int IComparer<DungeonRoom>.Compare(DungeonRoom Room1, DungeonRoom Room2) {
		//int t1 = _objA.GetComponent<CharacterStats>().initiative;
		//int t2 = _objB.GetComponent<CharacterStats>().initiative;
			return Room1.Closeness.CompareTo(Room2.Closeness);
	}
}

[ExecuteInEditMode]
public class RoomGenerator : WorldEditor 
{
		[Header("Actions")]
		[SerializeField] private bool IsGenerateAll = false;
		public void GenerateAll() { IsGenerateAll = true; }
		[SerializeField] private bool IsGenerateMetaData = false;
		public void GenerateMetaData() { IsGenerateMetaData = true; }
		[SerializeField] private bool IsGenerateRooms = false;
		public void GenerateRooms() { IsGenerateRooms = true; }
		[SerializeField] private bool IsGenerateMinions = false;
		public void GenerateMinions() { IsGenerateMinions = true; }
		[Header("Room Options")]
		public int MinimumRooms = 2;
		public int MaximumRooms = 4;
		public int RoomDoorSize = 2;
		public bool HasGeneratedMaze = false;
		public Vector3 RoomSizeMinimum = new Vector3(5,5,5);
		public Vector3 RoomSizeMaximum = new Vector3(12,10,12);
		public Vector3 RoomSize = new Vector3(5,1,5);
		public bool IsFillEmpty = false;	
		[Tooltip("if true will seperate, if false will join them together")]
		public bool IsRoomsSeperate = true;
		static int MaxRoomLocationChecks = 750;
		public int RoomApartness = 1;
		[Header("Hallway Options")]
		public bool IsHallways = true;
		public bool IsLinearHallways = true;
		public float HallwaySize = 1.5f;
		// Generated Room Data
		public List<DungeonRoom> Rooms = new List<DungeonRoom>();
		public List<Vector3> DoorSpawnLocations = new List<Vector3>();
		bool IsOverrideNonEmpty;
		//Vector3 StartLocation;
		[Header("BlockTypes")]
		public int PathBlockType = 1;
		public int RoomFloorType = 3;
		public int RoomWallType = 3;
		public int RoomRoofType = 3;
		public int RoomDoorType = 0;
		public int HallwayWallType = 9;

		public WorldUtilities.ZoneSpawner MySpawner;
		
		// Update is called once per frame
		void Update () {
			if (IsGenerateAll) {
				IsGenerateAll = false;
				IsGenerateMetaData = true;
				IsGenerateRooms = true;
				IsGenerateMinions = true;
			}
			if (IsGenerateMetaData) {
				IsGenerateMetaData = false;
				GenerateRoomsMeta();
			}
			if (IsGenerateRooms) {
				IsGenerateRooms = false;
				gameObject.GetComponent<VoxelLoader>().RefreshWorld();
				if (MinimumRooms > 0) 
				{
					GenerateRoomsAction();
				}
				if (MinimumRooms > 0)
				{
					if (IsHallways)
						GenerateHallways();
				}
				
				GeneratePathWalls(PathBlockType, 
				                  RoomSize.y,
				                  HallwayWallType, 
				                  HallwayWallType);
				//SpawnPosition = Rooms [0].RoomLocation + Rooms [0].RoomSize / 2f;	//StartLocation + new Vector3(0,1.5f,0);
			}
			if (IsGenerateMinions) {
				IsGenerateMinions = false;
				MySpawner.ClearMinions();
				for (int i = 0; i < Rooms.Count; i++) {
					Vector3 NewPosition = transform.TransformPoint (Rooms[i].GetMidPoint());
					MySpawner.SpawnMinion(NewPosition);
				}
			}
		}

		Vector3 GenerateRoomSize() {
			return new Vector3 (Mathf.RoundToInt (Random.Range (RoomSizeMinimum.x, RoomSizeMaximum.x)),
			                   	Mathf.RoundToInt (Random.Range (RoomSizeMinimum.y, RoomSizeMaximum.y)),
			                 	Mathf.RoundToInt (Random.Range (RoomSizeMinimum.z, RoomSizeMaximum.z)));
		}

		private void GenerateRoomsMeta() {
			//Debug.LogError(" Generating Rooms!");
			Rooms.Clear ();
			int RoomsToGenerate = Random.Range (MinimumRooms, MaximumRooms);
			//RoomsToGenerate = MinimumRooms;
			for (int i = 0; i < RoomsToGenerate; i++) {
				DungeonRoom NewRoom = new DungeonRoom();
				NewRoom.RoomSize =  GenerateRoomSize();;
				NewRoom.RoomLocation = FindNewRoomLocation(RoomSize);	
				
				if (NewRoom.RoomLocation != new Vector3(-1,-1,-1)) {
					if (Rooms.Count == 0) {
						//SpawnPosition = NewRoom.RoomLocation+NewRoom.RoomSize/2f;
					}
					Rooms.Add (NewRoom);
				}
			}
		}
		private void GenerateRoomsAction() 
		{
			for (int z = 0; z < Rooms.Count; z++)
			{
				UpdateWithRoom(Rooms[z]);
			}
		}
		public void UpdateWithRoom(DungeonRoom MyRoom) {
			Vector3 RoomLocation = MyRoom.RoomLocation;
			Vector3 MyRoomSize = MyRoom.RoomSize;
			
			for (float i = RoomLocation.x; i <= RoomLocation.x + MyRoomSize.x; i++)
				for (float j =  RoomLocation.y; j <= RoomLocation.y + MyRoomSize.y; j++)
					for (float k =  RoomLocation.z; k <= RoomLocation.z + MyRoomSize.z; k++) 
				{
					
					if (i == RoomLocation.x || i == RoomLocation.x + MyRoomSize.x ||
					    j == RoomLocation.y || j == RoomLocation.y + MyRoomSize.y ||
					    k == RoomLocation.z || k == RoomLocation.z + MyRoomSize.z)
						UpdateBlock(new Vector3(i, j, k), RoomFloorType);
					else
						UpdateBlock(new Vector3(i, j, k), 0);
				}
			// spawn monster!
			//UpdateBlock(RoomLocation+MyRoomSize/2f, 16);
		}
		public bool IsOnEdgeOfRoom(Vector3 Position, DungeonRoom MyRoom) {
			return IsOnEdgeOfRoom (Position, MyRoom, true);
		}
		public bool IsOnEdgeOfRoom(Vector3 Position, DungeonRoom MyRoom, bool IsCorners) {
			Vector3 RoomLocation = MyRoom.RoomLocation;
			Vector3 MyRoomSize = MyRoom.RoomSize;
			int i = Mathf.FloorToInt(Position.x);
			int j = Mathf.FloorToInt(Position.y);
			int k = Mathf.FloorToInt(Position.z);
			if (i == RoomLocation.x || i == RoomLocation.x + MyRoomSize.x ||
			    //j == RoomLocation.y || j == RoomLocation.y + MyRoomSize.y ||
			    k == RoomLocation.z || k == RoomLocation.z + MyRoomSize.z) {
				// if not corners of room
				if (!IsCorners)
				if ((i == RoomLocation.x && i == RoomLocation.x + MyRoomSize.x)
				    ||(k == RoomLocation.z && i == RoomLocation.x + MyRoomSize.x)
				    ||(k == RoomLocation.z && k == RoomLocation.z + MyRoomSize.z)
				    ||(i == RoomLocation.x && k == RoomLocation.z + MyRoomSize.z))
					return false;
				return true;
			}
			return false;
		}

		// I should first connect the ones with the highest amount of closeness to other rooms

		public DungeonRoom FindClosestRoom(DungeonRoom MyRoom, bool IsConnected) {
			float ClosestDistance = 1000;
			int ClosestIndex = -1;
			for (int i = 0; i < Rooms.Count; i++) {
				if (MyRoom != Rooms[i] && 
				    ((Rooms[i].IsConnected && IsConnected) || (!IsConnected))) 
				{
					float NewDistance = Vector3.Distance(MyRoom.RoomLocation, Rooms[i].RoomLocation);
					if (NewDistance < ClosestDistance) {
						ClosestDistance = NewDistance;
						ClosestIndex = i;
					}
				}
			}
			if (ClosestIndex == -1)
				return new DungeonRoom ();
			return Rooms [ClosestIndex];
		}
		void GenerateHallways() 
		{
			DoorSpawnLocations.Clear ();
			for (int i = 0; i < Rooms.Count; i++) 
			{
				Rooms[i].IsConnected = false;
			}
			for (int i = 0; i < Rooms.Count; i++) {
				Rooms[i].CalculateCloseness(Rooms);
			}
			/*List<DungeonRoom> Rooms2 = new List<DungeonRoom> ();
			for (int z = Rooms.Count-1; z>=0; z--) {
				float LowestCloseness = 0;
				int LowestClosenessIndex;
				for (int i = 0; i < Rooms.Count; i++) {

				}
			}*/
			Rooms.Sort((IComparer<DungeonRoom>)new SortRooms());

			if (IsHallways)
			for (int i = 0; i < Rooms.Count; i++) {
				DungeonRoom Room2 =  Rooms[i];

				if (!Room2.IsConnected) {
					Vector3 Position2 = Room2.RoomLocation+Room2.RoomSize/2f; 
					Position2.y = Room2.RoomLocation.y;

					DungeonRoom Room1 = FindClosestRoom(Room2, (i != 0));
					Vector3 Position1 = Room1.RoomLocation+Room1.RoomSize/2f; // mid point of the room?
					Position1.y = Room1.RoomLocation.y;

					Room1.IsConnected = true;
					Room2.IsConnected = true;

					// calculate door positions
					Vector3 Difference = (Position1-Position2);
					Vector3 RoomDirection = Difference.normalized;
					// find out which sides the ray is going from
					float CornerBuffer = 2;
					Vector3 ThingieDirection;
					if (Mathf.Abs(Difference.x) > Mathf.Abs(Difference.z)) // use x sides for doors
					{
						if (RoomDirection.x < 0) {
							ThingieDirection = new Vector3(1,0,0);
							Position1.x += Room1.RoomSize.x/2f;
							Position2.x -= Room2.RoomSize.x/2f;
						} else {
							ThingieDirection = new Vector3(-1,0,0);
							Position1.x -= Room1.RoomSize.x/2f;
							Position2.x += Room2.RoomSize.x/2f;
						}
						Position1.z = Room1.RoomLocation.z + Random.Range (CornerBuffer, Room1.RoomSize.z-CornerBuffer);
						Position2.z = Room2.RoomLocation.z + Random.Range (CornerBuffer, Room2.RoomSize.z-CornerBuffer);
					} 
					else 
					{
						if (RoomDirection.z < 0) {
							ThingieDirection = new Vector3(0,0,1);
							Position1.z += Room1.RoomSize.z/2f;
							Position2.z -= Room2.RoomSize.z/2f;
						} else {
							ThingieDirection = new Vector3(0,0,-1);
							Position1.z -= Room1.RoomSize.z/2f;
							Position2.z += Room2.RoomSize.z/2f;
						}
						Position1.x = Room1.RoomLocation.x + Random.Range (CornerBuffer, Room1.RoomSize.x-CornerBuffer);
						Position2.x = Room2.RoomLocation.x + Random.Range (CornerBuffer, Room2.RoomSize.x-CornerBuffer);
					}
					// recalculate direction
					Difference = (Position1-Position2);
					RoomDirection = Difference.normalized;
					float Distance = Vector3.Distance(Position1, Position2);//+1;

					Vector3 BeginPosition = Position2;
					Vector3 EndPosition = Position1;
					Debug.DrawRay(transform.TransformPoint(BeginPosition),transform.TransformPoint(EndPosition), Color.red, 20);
					IsOverrideNonEmpty = false;
					//bool HasAddedDoor = false;
					Vector2 PaintSize = 
						new Vector2(Mathf.Abs (RoomDirection.z), 
						            Mathf.Abs (RoomDirection.x))*HallwaySize;

					//if (i != 0)
					CreateDoor(BeginPosition,//-RoomDirection, 
					           ThingieDirection, 
					           Room2);
					if (IsLinearHallways) {

					} else {
						float Increment = transform.lossyScale.x;
						Vector3 HallwayPosition = BeginPosition;
						for (float g = Increment; g < Distance; g += Increment) 
						{
							// fliped direction (perpendicular) to lines direction, so width of line will be drawn on
							UpdateBlock(HallwayPosition, 
							            PathBlockType,
							            PaintSize);
							HallwayPosition += RoomDirection*Increment;
						}
					}
					CreateDoor(EndPosition,//+RoomDirection, 
					           -ThingieDirection, 
					           Room1);
					IsOverrideNonEmpty = true;
				}
			}
		}
		public void CreateDoor(Vector3 DoorPosition, Vector3 DoorDirection, DungeonRoom Room2) {
			float DoorHeight = 3;
			float DoorWidth = HallwaySize/2f;
			float BalconySize = 2;
			Vector2 DoorPerpendicularDirection = new Vector2 (DoorDirection.z, DoorDirection.x);
			DoorPerpendicularDirection = new Vector2 (1+Mathf.Abs (DoorDirection.z)*DoorWidth, 
			                                          1+Mathf.Abs (DoorDirection.x)*DoorWidth);
			// now doors
			if (IsOnEdgeOfRoom(DoorPosition, Room2, true))
			{
				//IsOverrideNonEmpty = true;
				//DoorPosition += DoorDirection;
				//Debug.LogError(g + " : " + ThingieDirection.ToString());
				for (int i = 1; i <= DoorHeight; i++)
					UpdateBlock(DoorPosition+new Vector3(0,i,0), RoomDoorType, DoorPerpendicularDirection);
				// create things next to door
				for (int i = 1; i <= BalconySize; i++)
					UpdateBlock(DoorPosition-DoorDirection*i, 
				            PathBlockType, 
				            DoorPerpendicularDirection);
				
				//DoorSpawnLocations.Add (DoorPosition);
				//IsOverrideNonEmpty = false;	
				//HasAddedDoor = true;
			}
		}
		
		// can inverse the overlap check to make the rooms stick together
		public Vector3 FindNewRoomLocation(Vector3 MyRoomSize) 
		{
			if (!IsRoomsSeperate)
				return new Vector3( Random.Range(0,GetSize().x-1-MyRoomSize.x), 
				                   0,	//Random.Range(0,GetSize().y/3f-1-MyRoomSize.y),// StartLocation.y,
				                   Random.Range(0,GetSize().z-1-MyRoomSize.z));
			int CheckCount = 0;
			Vector3 NewRoomLocation = new Vector3 (-1, -1, -1);
			while (CheckCount < MaxRoomLocationChecks) 
			{
				CheckCount++;
				// check a new room location within bounds of map
				Vector3 CheckNewRoomLocation = new Vector3( Mathf.RoundToInt(Random.Range(0,GetSize().x-1-MyRoomSize.x)), 
				                                           0,	//Random.Range(0,GetSize().y/3f-1-MyRoomSize.y),// StartLocation.y,
				                                           Mathf.RoundToInt(Random.Range(0,GetSize().z-1-MyRoomSize.z)));
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
					       || CheckNewRoomLocation.y+RoomApartness + MyRoomSize.y < RoomLocation2.y-RoomApartness 
					      || CheckNewRoomLocation.y-RoomApartness > RoomLocation2.y+RoomSize2.y+RoomApartness
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
	}
}