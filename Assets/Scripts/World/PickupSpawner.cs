using UnityEngine;
using System.Collections;

public class PickupSpawner : MonoBehaviour {
	public float CoolDown = 10.0f;	// cooldown for how logn to wait to spawn new pickkup
	public GameObject PickupPrefab;
	public Vector3 SpawnOffset;
	private GameObject SpawnedPickup;
	private float LastSpawnedTime;
	private bool HasSpawned = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (SpawnedPickup == null) {
			if (HasSpawned) {
				HasSpawned = false;
				LastSpawnedTime = Time.time;
			} else {	//  now wait to spawn
				if (Time.time - LastSpawnedTime > CoolDown) {
					SpawnedPickup = (GameObject) Instantiate (PickupPrefab, gameObject.transform.position+SpawnOffset, Quaternion.identity);
					SpawnedPickup.transform.SetParent(gameObject.transform);
				}
			}
		} else {
				// already spawned
		}
	}
}
