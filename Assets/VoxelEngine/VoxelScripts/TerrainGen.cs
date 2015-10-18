using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;

[System.Serializable]
public class Biome {
	public string BiomeName;
	public float BaseHeight = -24;
	public float LandHeightMinimum = -12;	// the mimimum level of height the land from bedrock can be
	public float GroundFrequency = 0.01f;
	public float LandAmplitude = 36;
	public bool IsMountain = false;

	
	public int StoneType;
	public int GrassType;
	public int DirtType;
	public int BedRockType;
	public int SandType;
	public int RoadType;
	public int WaterType;
	// trees
	public bool IsTrees = true;
	public float TreeFrequency = 0.05f;
	public float TreeSpawnThreshold = 0.9f;
	public float TreeHeightMinimum = 3;
	public float TreeHeightMaximum = 10;
	public int WoodType;
	public int LeafType;
	// ores
	public int BronzeType;
	public float BronzeFrequency = 0.01f;
	public float BronzeAmplitude = 36;

	Biome() {

	}
};

[System.Serializable]
public class TerrainGen {
	public Vector3 NoiseBuffer = new Vector3();
	public bool IsTowerDefence = false;
	public int MaxBiomes = 1;
	public bool IsGenerateTerrain = true;
	public List<Biome> MyBiomes = new List<Biome>();
	public float SeaLevel = 16;
	public bool IsSymmetrical = false;
	public bool HasAddedDungeon = false;

	public void Start() {
		//NoiseBuffer.x = Random.Range (0, 9999999);
		//NoiseBuffer.y = Random.Range (0, 9999999);
		//NoiseBuffer.z = Random.Range (0, 9999999);
	}
	public int GetHeightY(int PositionX, int PositionZ) {	
		Biome MyBiome = MyBiomes [0];

		int LandHeight = Mathf.FloorToInt (MyBiome.BaseHeight);
		LandHeight = GetNoise (Mathf.RoundToInt (NoiseBuffer.x) + PositionX, 
		                       Mathf.RoundToInt (NoiseBuffer.y) + 0, 
		                       Mathf.RoundToInt (NoiseBuffer.z) + PositionZ, 
		                       MyBiome.GroundFrequency, 
		                       Mathf.FloorToInt (MyBiome.LandAmplitude));
		
		if (LandHeight < MyBiome.LandHeightMinimum)
			LandHeight = Mathf.FloorToInt (MyBiome.LandHeightMinimum);
		return LandHeight;
	}
    public Chunk ChunkGen(Chunk chunk)
    {
        for (int x = chunk.pos.x; x < chunk.pos.x + Chunk.chunkSize; x++)
        {
            for (int z = chunk.pos.z; z < chunk.pos.z + Chunk.chunkSize; z++)
			{
				if (IsGenerateTerrain) {
                	chunk = ChunkColumnGen(chunk, x, z);
				} else {
					for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.chunkSize; y++) {
						chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new BlockAir ());
					}
				}
            }
        }
		
		if (IsTowerDefence) {
			BlockStructure MyTownHall = GetManager.GetDataManager ().BlockStructuresList [0];
			Vector3 TownHallSize = MyTownHall.MyBlocks.Size;
			Vector3 TownHallPosition = new Vector3(0,0,0);
			Vector3 Buffer = new Vector3(-TownHallSize.x/2f, 0, -TownHallSize.z/2f);
			if (IsInsideBlockStructure(chunk, MyTownHall, TownHallPosition+Buffer, TownHallSize))
			{	// if centre chunk
				AddBlockStructureToTerrain(chunk, MyTownHall, TownHallSize, TownHallPosition, Buffer);
			}
		}
		// if dungeon generation
		if (!IsGenerateTerrain) {
			if (!HasAddedDungeon) {
				Maze MyDungeon = GetManager.GetDataManager ().MazeList [1];
				Vector3 TownHallSize = MyDungeon.MyBlocks.Size;
				Vector3 TownHallPosition = new Vector3(0,0,0);
				//TownHallPosition.x -= TownHallSize.x/2f;
				//TownHallPosition.z -= TownHallSize.z/2f;
				if (IsInsideBlockStructure(chunk, (BlockStructure)MyDungeon, TownHallPosition, TownHallSize))
				{	// if centre chunk
					Debug.Log ("Adding Dungeon To Terrain");	
					AddBlockStructureToTerrain(chunk, (BlockStructure)MyDungeon, TownHallSize, TownHallPosition);
					//HasAddedDungeon = true;
				}
			}
		}
        return chunk;
    }

	public void AddBlockStructureToTerrain(Chunk chunk, BlockStructure MyTownHall, Vector3 TownHallSize, Vector3 TownHallPosition) {
		AddBlockStructureToTerrain(chunk, MyTownHall, TownHallSize, TownHallPosition, new Vector3());
	}

	public void AddBlockStructureToTerrain(Chunk chunk, BlockStructure MyTownHall, Vector3 TownHallSize, Vector3 TownHallPosition, Vector3 Buffer) {
		for (int i = 0; i < TownHallSize.x; i++)
			for (int j = 0; j < TownHallSize.y; j++)
			for (int k = 0; k < TownHallSize.z; k++) {
				Vector3 MyBlockStructurePosition = new Vector3 (i, j, k);
				MyBlockStructurePosition += TownHallPosition;
				MyBlockStructurePosition.x -= chunk.pos.x;
				MyBlockStructurePosition.y -= chunk.pos.y;
				MyBlockStructurePosition.z -= chunk.pos.z;
				{
					int BlockType = MyTownHall.MyBlocks.GetBlockType (MyBlockStructurePosition + new Vector3(chunk.pos.x, chunk.pos.y, chunk.pos.z));
						// spawn town hall here
					MyBlockStructurePosition += Buffer;
					int x = Mathf.FloorToInt (MyBlockStructurePosition.x);
					int y = Mathf.FloorToInt (MyBlockStructurePosition.y);
					int z = Mathf.FloorToInt (MyBlockStructurePosition.z);
					if (Chunk.InRange(x) && Chunk.InRange(y) && Chunk.InRange(z)) {
						if (BlockType != 0)
							chunk.SetBlock (x,y,z,
							                new Block (BlockType));	// cobble stone
						else
						    chunk.SetBlock (x,y,z,
							                new BlockAir ());
						}
				}
				}
	}
	public bool IsInsideBlockStructure(Chunk chunk, BlockStructure MyBlockStructure, Vector3 TownHallPosition, Vector3 TownHallSize) {
		//Vector3 LowerBounds = new Vector3(TownHallPosition.x-TownHallSize.x/2f,TownHallPosition.y-TownHallSize.y/2f,TownHallPosition.z-TownHallSize.z/2f);
		//Vector3 UpperBounds = new Vector3(TownHallPosition.x+TownHallSize.x/2f,TownHallPosition.y+TownHallSize.y/2f,TownHallPosition.z+TownHallSize.z/2f);
		Vector3 LowerBounds = new Vector3(0,0,0);
		Vector3 UpperBounds = new Vector3(TownHallPosition.x+TownHallSize.x,TownHallPosition.y+TownHallSize.y,TownHallPosition.z+TownHallSize.z);
		//Debug.LogError ("Before Rounding - Lower: " + LowerBounds.ToString() + " : Upper: " + UpperBounds.ToString());
		LowerBounds.x = 16f*(Mathf.FloorToInt(LowerBounds.x/16f));
		LowerBounds.y = 16f*(Mathf.FloorToInt(LowerBounds.y/16f));
		LowerBounds.z = 16f*(Mathf.FloorToInt(LowerBounds.z/16f));
		UpperBounds.x = 16f*(Mathf.CeilToInt(UpperBounds.x/16f));
		UpperBounds.y = 16f*(Mathf.CeilToInt(UpperBounds.y/16f));
		UpperBounds.z = 16f*(Mathf.CeilToInt(UpperBounds.z/16f));
		//Debug.LogError ("LowerBounds: " + LowerBounds.ToString() + " : Upper: " + UpperBounds.ToString());
		if (chunk.pos.x >= LowerBounds.x && chunk.pos.x <= UpperBounds.x
			&& chunk.pos.y >= LowerBounds.y && chunk.pos.y <= UpperBounds.y
			&& chunk.pos.z >= LowerBounds.z && chunk.pos.z <= UpperBounds.z) // if town hall position+size is inside chunk!
			return true;
		else
			return false;
	}

	// if closer to 'region points' increase the noise amplitude
    public Chunk ChunkColumnGen(Chunk chunk, int x, int z) {
		//float BiomeTypeFloat = GetNoiseF (NoiseBuffer.x + x, NoiseBuffer.y+0, NoiseBuffer.z+z, 0.01f, MyBiomes.Count);
			//int BiomeType = Mathf.Clamp (Mathf.FloorToInt (BiomeTypeFloat), 0, MyBiomes.Count - 1);
			//BiomeType = Mathf.Clamp (BiomeType, 0, MaxBiomes);
			int BiomeType = 0;
			Biome MyBiome = MyBiomes [BiomeType];

			// first generate the height value
			int LandHeight = Mathf.FloorToInt (MyBiome.BaseHeight);
			if (IsTowerDefence) {
				if (IsSymmetrical) {
					int NewX = Mathf.Abs(x)+Mathf.Abs (z);
					LandHeight += GetNoise (Mathf.RoundToInt(NoiseBuffer.x) + NewX, 
				                        Mathf.RoundToInt(NoiseBuffer.y) +  0,
				                        Mathf.RoundToInt(NoiseBuffer.z) +  NewX, 
				                        MyBiome.GroundFrequency, 
				                        Mathf.FloorToInt (MyBiome.LandAmplitude));
			} else {	
				LandHeight += GetNoise (Mathf.RoundToInt(NoiseBuffer.x) + x, 
				                        Mathf.RoundToInt(NoiseBuffer.y) +  0,
				                        Mathf.RoundToInt(NoiseBuffer.z) +  z, 
				                        MyBiome.GroundFrequency, 
				                        Mathf.FloorToInt (MyBiome.LandAmplitude));
				}
				float ClosenessToCentre = Vector3.Distance (new Vector3(0,0,0), new Vector3(x,0,z));
				int LandAddition = Mathf.RoundToInt(ClosenessToCentre / 8f)+1;
				if (LandAddition < 1)	LandAddition = 1;
				LandHeight -= LandAddition;
			} else {
				LandHeight += GetNoise (Mathf.RoundToInt (NoiseBuffer.x) + x, Mathf.RoundToInt (NoiseBuffer.y) + 0, Mathf.RoundToInt (NoiseBuffer.z) + z, MyBiome.GroundFrequency, Mathf.FloorToInt (MyBiome.LandAmplitude));
			}
			if (LandHeight < MyBiome.LandHeightMinimum)
				LandHeight = Mathf.FloorToInt (MyBiome.LandHeightMinimum);
			int DirtHeight = 2;
			//stoneHeight += GetNoise (x, 0, z, stoneBaseNoise, Mathf.FloorToInt (stoneBaseNoiseHeight));

			//int dirtHeight = stoneHeight + Mathf.FloorToInt (dirtBaseHeight);
			//dirtHeight += GetNoise (x, 100, z, dirtNoise, Mathf.FloorToInt (dirtNoiseHeight));
			
			//if () 
			{	// boundaries
				//chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new BlockAir ());	// air
				for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.chunkSize; y++) 
			{

					if (y == MyBiome.LandHeightMinimum && MyBiome.BedRockType > 0) {
						chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.BedRockType));
					} else if (y > MyBiome.LandHeightMinimum && y < LandHeight - DirtHeight) { // the stone in between
						// makes adjustments for chunk position as the x and z are world positions
						float ChanceBronze = GetNoiseFloat (x, y, z, MyBiome.BronzeFrequency, MyBiome.BronzeAmplitude);
						if (ChanceBronze > 0.95f) {
							if (MyBiome.BronzeType > 0)
								chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.BronzeType));	// cobble stone
						} else {
							if (MyBiome.StoneType > 0)
								chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.StoneType));	// cobble stone
						}
					} else if (y >= MyBiome.LandHeightMinimum - DirtHeight && y < LandHeight) {
						chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.DirtType));	// cobble stone
					} else if (y == LandHeight) { // top blocks
						if (y <= SeaLevel) {
							if (MyBiome.SandType > 0)
								chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.SandType));
						} else {
							if (MyBiome.GrassType > 0)
								chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.GrassType));
						}
					} else if (y >= MyBiome.LandHeightMinimum && y <= SeaLevel && MyBiome.WaterType > 0) {
						chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.WaterType));
					} else {
						chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new BlockAir ());
					}
					
					// roads for the TD
					if (IsTowerDefence) {
						float PathSize = 4;
						if (y == LandHeight && ((x >= - PathSize && x <= PathSize) || (z >= - PathSize && z <= PathSize)))
							chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new Block (MyBiome.RoadType));	// cobble stone
						if((x > 80 || x < -80 || z > 80 || z < -80))
							chunk.SetBlock (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, new BlockAir ());
					}
				}

				if (MyBiome.IsTrees) {
					//if (Mathf.RoundToInt(x) % 2 == 0 && Mathf.RoundToInt(z) % 2 == 0) 
					if (LandHeight > SeaLevel) {
						float IsTree = (Noise.Generate (x * MyBiome.TreeFrequency, 0, z * MyBiome.TreeFrequency));
						if (IsTree > MyBiome.TreeSpawnThreshold) {
						
							int y = (LandHeight + 1) - chunk.pos.y;
							// height based on x,z coordinates and noise algorithm
							float TreeHeight = Mathf.RoundToInt (10 * (Noise.Generate (x * MyBiome.TreeFrequency, 0, z * MyBiome.TreeFrequency)));
							TreeHeight = Mathf.Clamp (TreeHeight, MyBiome.TreeHeightMinimum, MyBiome.TreeHeightMaximum);
							
							// branch
							for (int i = 0; i <= 6; i++)
								chunk.SetBlock (x - chunk.pos.x, i + y, z - chunk.pos.z, new Block (MyBiome.WoodType));
							// leaves
							for (int i = 6; i <= 10; i++)
								for (int j = -2; j <= 2; j++)
									for (int k = -2; k <= 2; k++)
										chunk.SetBlock (x - chunk.pos.x + j, y + i, k + z - chunk.pos.z, new Block (MyBiome.LeafType));
						}
					}
				}
			}
		return chunk;
	}
	
	public static float GetNoiseFloat(float x, float y, float z, float scale, float max)
	{
		return (Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f);
	}
	public static int GetNoiseF(float x, float y, float z, float scale, int max)
	{
		return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
	}
	public static int GetNoise(int x, int y, int z, float scale, int max)
    {
        return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
    }
}