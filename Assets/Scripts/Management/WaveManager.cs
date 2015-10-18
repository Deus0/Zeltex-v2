using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour {
	public int WaveCount = 0;
	public float WaveCoolDown = 60f;
	public float InitialWaveCoolDown = 30f;
	public float LastSpawnedWave = 0;
	public List<GameObject> MySpawnZones = new List<GameObject>();
	public int SpawnAmount = 5;
	public int SpawnType = 0;
	public Vector3 TownHallCorePosition = new Vector3(7,15,7);
	public int SpawnZonesAmount = 4;
	public Text MyWaveNumberText;
	public Text MyWaveTimeText;
	public int OverrideSpawnType = -1;
	private int SpawnMinionCount = 0;
	public int MinSpawns = 3;
	public int MaxSpawns = 3;
	public int Difficulty = 0;
	public bool IsMixedWave = false;
	public List<GameObject> SpawnedMinions = new List<GameObject>();
	public bool IsSpawningWave = false;
	float LastSpawnedMinion = 0f;
	bool IsActive = false;

	public void CreateSpawners() {
		SpawnAmount = Mathf.Clamp(SpawnAmount, MinSpawns, MaxSpawns);
		if (WaveCoolDown != 0) {
			LastSpawnedWave = Time.time;
			Vector3 ZoneSize = new Vector3 (8, 8, 8);
			Vector3 BufferSize = -ZoneSize / 2f;	// centres them
			BufferSize.y = 0;
			if (MySpawnZones.Count < SpawnZonesAmount) 
			if (GetManager.GetZoneManager ().PlaceBuildingZone (new Vector3 (0, -3, -55) + BufferSize, ZoneSize)) {
				MySpawnZones.Add (GetManager.GetZoneManager ().BuildingZones[GetManager.GetZoneManager ().BuildingZones.Count-1]);
			}
			if (MySpawnZones.Count < SpawnZonesAmount) 
			if (GetManager.GetZoneManager ().PlaceBuildingZone (new Vector3 (0, -3, 55) + BufferSize, ZoneSize)) {
				MySpawnZones.Add (GetManager.GetZoneManager ().BuildingZones[GetManager.GetZoneManager ().BuildingZones.Count-1]);
			}
			if (MySpawnZones.Count < SpawnZonesAmount) 
			if (GetManager.GetZoneManager ().PlaceBuildingZone (new Vector3 (-55, -3, 0) + BufferSize, ZoneSize)) {
				MySpawnZones.Add (GetManager.GetZoneManager ().BuildingZones[GetManager.GetZoneManager ().BuildingZones.Count-1]);
			}
			if (MySpawnZones.Count < SpawnZonesAmount) 
			if (GetManager.GetZoneManager ().PlaceBuildingZone (new Vector3 (55, -3, 0) + BufferSize, ZoneSize)) {
				MySpawnZones.Add (GetManager.GetZoneManager ().BuildingZones[GetManager.GetZoneManager ().BuildingZones.Count-1]);
			}
		}
	}
	public void BeginAll() {
		WaveCount = 0;
		LastSpawnedWave = Time.time;
		IsActive = true;
		CreateSpawners ();
	}
	public void EndAll() {
		IsActive = false;
		// destroy spawners
	}
	// Update is called once per frame
	void Update () {
		if (IsActive) {
			if (IsSpawningWave) {
				LastSpawnedWave = Time.time;
				if (Time.time - LastSpawnedMinion > 1f) {
					SpawnMinion ();
					LastSpawnedMinion = Time.time;
				}
			} else {
				if (WaveCount != 0)
					CheckIfSpawnsAreDead ();
				if (WaveCoolDown != 0) 
				{
					if (WaveCount == 0) {
						if (Time.time - LastSpawnedWave >= InitialWaveCoolDown) {
							BeginSpawning ();
						}
					} else {
						if (Time.time - LastSpawnedWave >= WaveCoolDown) {
							BeginSpawning ();
						}
					}
				}
			}
			MyWaveNumberText.text = "Wave: " + WaveCount.ToString ();
			if (WaveCount != 0)
				MyWaveTimeText.text = "Time: " + (Mathf.CeilToInt (WaveCoolDown - (Time.time - LastSpawnedWave))).ToString ();
			else
				MyWaveTimeText.text = "Time: " + (Mathf.CeilToInt (InitialWaveCoolDown - (Time.time - LastSpawnedWave))).ToString ();
			if (IsSpawningWave) 
			{
				MyWaveTimeText.text = "Spawning";
			}
		}
	}
	public void BeginSpawning() {
		IsSpawningWave = true;
		SpawnMinionCount = 0;
		SpawnedMinions.Clear ();
		SpawnType = Random.Range(0, GetManager.GetMonsterSpawner ().MonsterList.Count);
	}

	public void CheckIfSpawnsAreDead() 
	{
		bool IsAllSpawnsDead = true;
		for (int i = 0; i < SpawnedMinions.Count; i++) {
			if (SpawnedMinions[i] != null)
			if (!SpawnedMinions[i].GetComponent<BaseCharacter>().MyStats.IsDead){
				IsAllSpawnsDead = false;
				break;
			}
		}
		if (IsAllSpawnsDead) {
			BeginSpawning();
		}
	}

	public void BeginWaves() 
	{
		WaveCount = 0;
	}

	public void SpawnMinion() {
		if (IsMixedWave) {
			SpawnType = GetManager.GetMonsterSpawner ().GetMonsterSpawnIndex();
				//Random.Range(0, GetManager.GetMonsterSpawner ().MonsterList.Count);
		}
		if (OverrideSpawnType >= 0)
			SpawnType = OverrideSpawnType;
		int i = Random.Range (0,MySpawnZones.Count);
		int j = SpawnMinionCount;
		
		Difficulty = (WaveCount) / (MaxSpawns-MinSpawns+1);
		
		GetManager.GetMonsterSpawner ().MonsterList[SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.SetDefaults ();
		GetManager.GetMonsterSpawner ().MonsterList[SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.SetAttribute ("Strength", (Mathf.Pow(1.1f,Difficulty))*1f);
		GetManager.GetMonsterSpawner ().MonsterList[SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.SetAttribute ("Vitality", (Mathf.Pow(1.05f,Difficulty))*0.2f);
		//GetManager.GetMonsterSpawner ().MonsterList[SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.SetAttribute ("Vitality", 0);
		GetManager.GetMonsterSpawner ().MonsterList [SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.SetStateMax ("Health");
		GetManager.GetMonsterSpawner ().MonsterList [SpawnType].MonsterToSpawn.GetComponent<BaseCharacter> ().MyStats.MyLevel.Level = WaveCount / 5;	//Difficulty;
		GetManager.GetMonsterSpawner ().SpawnMonster (SpawnType, MySpawnZones[i].transform.position + new Vector3(Random.Range (-MySpawnZones[i].GetComponent<Zone>().Size.x/2f, MySpawnZones[i].GetComponent<Zone>().Size.x/2f)
		                                                                                                          ,0,
		                                                                                                          Random.Range (-MySpawnZones[i].GetComponent<Zone>().Size.z, MySpawnZones[i].GetComponent<Zone>().Size.z)));
		GameObject Spawn = GetManager.GetMonsterSpawner ().GetLastSpawn();
		if (Spawn.GetComponent<BotMovement>()) 
			Spawn.GetComponent<BotMovement>().CommandMineBlock(TownHallCorePosition);
		if (Spawn.GetComponent<BotGroundMove> ()) 
			Spawn.GetComponent<BotGroundMove> ().MoveToPosition (TownHallCorePosition);
		SpawnedMinions.Add(Spawn);

		SpawnMinionCount++;
		if (SpawnMinionCount >= SpawnAmount) {
			WaveCount++;
			IsSpawningWave = false;
			LastSpawnedWave = Time.time;
			//SpawnAmount++;
			SpawnAmount = (WaveCount % (MaxSpawns-MinSpawns+1))+MinSpawns;
			SpawnAmount = Mathf.Clamp(SpawnAmount, MinSpawns, MaxSpawns);
		}
	}
}
