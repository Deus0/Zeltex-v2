using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MonsterSpawnType {
	public GameObject MonsterToSpawn;
	public float ChanceToSpawn;
};

[System.Serializable]
public class SpawnTimer {
	public float TimeSpawnedLast;
	public float TimeElapsed;
	public float Cooldown = 5.0f;
};

public class MonsterSpawner : MonoBehaviour {
	public Vector2 RandomSpawnRange = new Vector2 (10, 10);
	public GameObject MyPlayer;
	public float TimeSpawnedLast;
	public float Cooldown = 5.0f;
	public int MaximumSpawns;
	public int TotalSpawns;
	public int SpawnClanIndex = 3;
	public List<MonsterSpawnType> MonsterList;
	public List<GameObject> SpawnedList;
	public bool IsInitSpawn = true;
	GameObject NewSpawn;
	public List<GameObject> RespawnList;
	public Vector3 SpawnPosition;
	public bool IsSpawnHostile = true;

	// Use this for initialization
	void Start () {
		SpawnPosition = transform.position;
	}
	void Awake () {
		if (IsInitSpawn) {
			SpawnMonsterOnTimer ();
		}
	}
	// Update is called once per frame
	void Update () {
		SpawnMonsterOnTimer ();
		if (Network.isClient)
			gameObject.SetActive (false);
	}

	public void RespawnPlayer() {
		if (MyPlayer != null) {
			RespawnList.Add (MyPlayer);
		}
	}
	public GameObject GetLastSpawn() {
		return SpawnedList [SpawnedList.Count - 1];
	}
	
	public bool SpawnMonster(int MonsterIndex) {
		if (MyPlayer == null) {
			if (GetManager.GetCharacterManager().GetLocalPlayer() != null)
				MyPlayer = GetManager.GetCharacterManager().GetLocalPlayer().gameObject;
		}
		if (MyPlayer != null) {
			SpawnPosition = MyPlayer.transform.position + new Vector3 (Random.Range (-RandomSpawnRange.x, RandomSpawnRange.x), 10, Random.Range (-RandomSpawnRange.y, RandomSpawnRange.y));
			return SpawnMonster (MonsterIndex, SpawnPosition);
		}
		return false;
	}
	public bool SpawnMonster(int MonsterIndex, Vector3 NewSpawnPosition) {
		if (MonsterList [MonsterIndex].MonsterToSpawn && MonsterIndex >= 0 && MonsterIndex < MonsterList.Count) {
			
			if (GetManager.GetNetworkManager ().IsConnected ()) {
				NewSpawn = (GameObject)Network.Instantiate (MonsterList [MonsterIndex].MonsterToSpawn, NewSpawnPosition, Quaternion.identity, SpawnedList.Count);
			} else { 
				NewSpawn = (GameObject)Instantiate (MonsterList [MonsterIndex].MonsterToSpawn, NewSpawnPosition, Quaternion.identity);
			}

		}
		else
			NewSpawn = null;
		if (NewSpawn != null)
			SpawnedList.Add (NewSpawn);

		if (NewSpawn != null)
			return true;
		else
			return false;
	}
	public bool CanSpawn() {
		// first remove any invalids from list - this probaly should be only handled on the monsters destruct functino
		for (int i = SpawnedList.Count-1; i > 0; i--) {
			if (SpawnedList[i] == null) {
				SpawnedList.RemoveAt (i);
			}
		}
		TotalSpawns = SpawnedList.Count;
		if (TotalSpawns < MaximumSpawns)
			return true;
		else
			return false;
	}
	public int GetMonsterSpawnIndex() {
		int NewSpawnType = (int) (Random.Range (1,100));
		
		float SpawnChanceCount = 0f;
		for (int i = 0; i < MonsterList.Count; i++) {
			if (i == 0) {
				if (NewSpawnType < MonsterList[i].ChanceToSpawn)
					return i;
			}
			else {
				SpawnChanceCount += MonsterList[i-1].ChanceToSpawn;
				if (NewSpawnType < MonsterList[i].ChanceToSpawn + SpawnChanceCount && NewSpawnType >= SpawnChanceCount)
					return i;
			}
		}
		return 0;
	}
	// reimplement this into a coroutine
	void SpawnMonsterOnTimer() {
		if (Time.time - TimeSpawnedLast > Cooldown) {
			TimeSpawnedLast = Time.time;
			if (IsSpawnHostile) {
				SpawnClanIndex = 0;
			}
			if (CanSpawn ()) {
				NewSpawn = null;
					int MonsterSpawnIndex = GetMonsterSpawnIndex();
					SpawnMonster (MonsterSpawnIndex);

				if (NewSpawn) {
					TotalSpawns ++;
					BaseCharacter NewChar = (BaseCharacter) NewSpawn.GetComponent ("BaseCharacter");
					//NewChar.ClanIndex = TotalSpawns;
					NewChar.ClanIndex = SpawnClanIndex;
				}
			} else { // else check how many are spawned
				BaseCharacter[] Enemies = FindObjectsOfType(typeof(BaseCharacter)) as BaseCharacter[];
				TotalSpawns = Enemies.Length;
			}
		}
	}
}
