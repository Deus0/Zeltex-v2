using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterSystem;
using UnityEngine.Events;

namespace WorldUtilities {
	[System.Serializable]
	public class MyEvent : UnityEvent<GameObject> {}

	public class ZoneSpawner : MonoBehaviour {
		public MyEvent OnSpawn;
		public GameObject MySpawnPrefab;
		public int MaximumSpawns = 5;
		public float SpawnRate = 5f;
		public bool IsStaticY = false;
		private List<GameObject> MySpawns = new List<GameObject>();
		private float LastSpawned = 0f;
		//private BoxCollider MyBox;
		public VoxelEngine.VoxelLoader MyWorld;

		void Start() 
		{
			if (MyWorld) {
				transform.localScale = MyWorld.LoadDistance * VoxelEngine.Chunk.ChunkSize * MyWorld.transform.lossyScale.x;
				Vector3 NewPosition;
				NewPosition = MyWorld.transform.position + transform.localScale/2f;
				NewPosition.y += transform.localScale.y;
				transform.position = NewPosition;
			}
		}
		public void ClearMinions() {
			for (int i = 0; i < MySpawns.Count; i++) {
				if (MySpawns[i])
					DestroyImmediate (MySpawns[i]);
			}
			MySpawns.Clear ();
		}
		void Update() 
		{
			if (PhotonNetwork.player.ID == PhotonNetwork.playerList[0].ID) {
				if (MySpawns.Count < MaximumSpawns && Time.time - LastSpawned >= SpawnRate) {
					Vector3 MySize = transform.lossyScale;
					Vector3 NewSpawnPosition = transform.position + new Vector3 (
						Random.Range (-MySize.x / 2f, MySize.x / 2f), 
						Random.Range (-MySize.y / 2f, MySize.y / 2f),
						Random.Range (-MySize.z / 2f, MySize.z / 2f));
					if (IsStaticY) {
						NewSpawnPosition.y = transform.position.y;
					}
					SpawnMinion(NewSpawnPosition);
				}

			} else {
				//Debug.LogError("Isnt master client.");
			}
		}
		string DefaultMinionName;
		public void SpawnMinion(Vector3 SpawnPosition) {
			if (MySpawns.Count > 0)
			for (int i = MySpawns.Count-1; i >= 0; i--) {
				if (MySpawns [i] == null)
					MySpawns.RemoveAt (i);
			}
			LastSpawned = Time.time;
			Quaternion NewAngle = Quaternion.identity;
			NewAngle.eulerAngles = new Vector3 (0, Random.Range (0, 360), 0);
			GameObject NewSpawn;
			if (PhotonNetwork.connected) {
				NewSpawn = PhotonNetwork.InstantiateSceneObject (MySpawnPrefab.name, SpawnPosition, NewAngle, 0, null);
				DefaultMinionName = NewSpawn.name;
				gameObject.GetComponent<PhotonView>().RPC(
					"UpdateMinion",
					PhotonTargets.All);
			} else {
				NewSpawn = (GameObject)Instantiate (MySpawnPrefab, SpawnPosition, NewAngle);
				UpdateMinionName(NewSpawn);
			}
			//MySpawns.Add (NewSpawn);
			//OnSpawn.Invoke (NewSpawn);
		}

		[PunRPC]
		public void UpdateMinion() {
			GameObject MyMinion = GameObject.Find ("MinionSpawner(Clone)");
			if (MyMinion) 
			{
				UpdateMinionName(MyMinion);
			}
		}
		// also called when the client connects
		public void UpdateMinionName(GameObject MyMinion) 
		{
			if (MyMinion && MyMinion.name == "MinionSpawner(Clone)") 
			{
				MySpawns.Add (MyMinion);
				string NewName = "Minion" + MySpawns.Count;
				GameObject MinionCharacter = MyMinion.gameObject.transform.GetChild (0).gameObject;
				MinionCharacter.name = NewName;
				MyMinion.name = NewName + "_Spawner";

				if (PhotonNetwork.connected) {
					CharacterSystem.CharacterStats MyStats = MinionCharacter.GetComponent<CharacterSystem.CharacterStats>();
					if (MyStats)
						MyStats.SynchAllStats();
				}

				GuiSystem.GuiManager MyGui = MinionCharacter.GetComponent<GuiSystem.GuiManager> ();
				if (MyGui)
					MyGui.UpdateLabel ();
				OnSpawn.Invoke (MyMinion);
			}
		}
	}
}