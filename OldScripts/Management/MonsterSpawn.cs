using UnityEngine;
using System.Collections;

// used for individual spawning
// input is spawn details
// and a prefab for the monster
// output is the creation of gameobjects in the scene at varied times

namespace OldCode {
public class MonsterSpawn : MonoBehaviour {
	public float SpawnInitialDelay = 5.0f;
	public float SpawnCoolDown = 60.0f;
	public int MonsterType = 0;
	public bool IsSpawnUnlimited = true;
	public int SpawnTimes;
	private int SpawnCount = 0;
	private float TimeCreated;
	private bool IsInitialSpawn = true;

	// Use this for initialization
	void Start () {
		TimeCreated = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if ((IsInitialSpawn && Time.time-TimeCreated >= SpawnInitialDelay) || (!IsInitialSpawn && Time.time-TimeCreated >= SpawnCoolDown)  ) {
			IsInitialSpawn = false;
			GetManager.GetMonsterSpawner().SpawnMonster (MonsterType,transform.position);
			SpawnCount++;
			if (!IsSpawnUnlimited && SpawnCount >= SpawnTimes) {
				Destroy (this,0.5f);
				gameObject.SetActive(false);
			} else {
				TimeCreated = Time.time;
			}
		}
	}
}
}