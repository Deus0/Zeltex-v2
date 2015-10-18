using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityStandardAssets.Characters.FirstPerson;

// Character Manager
// handles:	Loading
//			Saving
//			Removing from scene
//			Ressurection
//			Stores references to them

[System.Serializable]
public class CharacterRessurection {
	public Vector3 RespawnPoint;
	private float TimeDied;
	private float CoolDown;
	private BaseCharacter DeadCharacter;
	
	public CharacterRessurection(BaseCharacter NewCharacter, float NewCoolDown) {
		TimeDied = Time.time;
		DeadCharacter = NewCharacter;
		CoolDown = NewCoolDown;
	}
	public bool CanRessurect() {
		if (GetTimeLeft() > CoolDown) {
			return true;
		} else {
			return false;
		}
	}
	public float GetTimePassed() {
		return Time.time - TimeDied;
	}
	public float GetTimeLeft() {
		return CoolDown - GetTimePassed ();
	}
	public BaseCharacter GetDeadCharacter() {
		return DeadCharacter;
	}
};

public class CharacterManager : MonoBehaviour {
	public Color32 InCombatColor = new Color32 (255, 200, 200, 255);
	//public Texture EmptyEffectsTexture;
	public Material MyBotPathMaterial;
	public Material MyLaserBeamMaterial;
	public Material MyChainLightningMaterial;
	// Prefabs
	public GameObject PlayerPrefab;
	// should be in the terrain manager	- used for block drops
	public GameObject BlockItemDrop;

	public Vector3 SpawnPosition = new Vector3(0,9,0);
	public float BaseRessurectionCoolDown = 15;
	public bool IsLoadPlayerData = true;	// if true, it will load the player data from the previous saved version of the map
	// references
	private BaseCharacter LocalPlayer;
	public List<GameObject> LoadedCharacters = new List<GameObject>();
	public List<CharacterRessurection> RespawnList;

	// character manager stuff

	int PlayersSpawned = 0;		// the count for how many players spawned

	// Use this for initialization
	void Start () {
		CheckLoadedCharactersInMap ();
	}
	
	// Update is called once per frame
	void Update () {
		RespawnCharacters ();
	}

	public GameObject SpawnPlayer() {
		return SpawnPlayer (GetDefaultSpawnPositionBeforeGenerated ());
	}
	public GameObject SpawnPlayerOnline() {
		SpawnPlayer (GetDefaultSpawnPositionBeforeGenerated ());
		/*NetworkedPlayer MyNetworkedPlayer = LocalPlayer.gameObject.AddComponent<NetworkedPlayer> ();
		NetworkView MyNetworkView = LocalPlayer.gameObject.AddComponent<NetworkView> ();
		MyNetworkView.observed = MyNetworkedPlayer;*/
		PlayersSpawned++;	// used in networking
		return LocalPlayer.gameObject;
	}
	
	public BaseCharacter GetLocalPlayer() {
		return LocalPlayer;
	}
	public void DestroyLocalPlayer() {
		if (LocalPlayer != null)
			Destroy(LocalPlayer.gameObject);
	}
	// use terrain generator to find height at an x,z position
	public Vector3 GetDefaultSpawnPositionIfMapLoaded() {
		LayerMask OnlyChunksLayer = (1 << 10);	// ignore chunk layer
		//OnlyChunksLayer = ~OnlyChunksLayer;	// only do chunk layer
		RaycastHit MyHit;
		if (Physics.Raycast (new Vector3 (0, 100f, 0), new Vector3 (1, -1, 1), out MyHit, 200f, OnlyChunksLayer)) 
		{
			return new Vector3(Mathf.RoundToInt(MyHit.point.x), Mathf.RoundToInt(MyHit.point.y)+2, Mathf.RoundToInt(MyHit.point.z));
		}
		return new Vector3 (0, 0, 0);
	}
	public Vector3 GetDefaultSpawnPositionBeforeGenerated() {
		return new Vector3 (1, GetManager.GetGameManager().WorldPrefab.GetComponent<World> ().MyTerrainGen.GetHeightY (1, 1)+2f, 1);
	}
	public GameObject SpawnPresetMapPosition() {
		return SpawnPlayer (SpawnPosition);
	}
	// number of players connected should be transmitted by server every time a player is spawned and updated in game manager/network manager
	public GameObject SpawnPlayer(Vector3 PlayerSpawnPosition) {
		if (Network.isClient || Network.isServer) {
			LoadedCharacters.Add ((GameObject) Network.Instantiate (PlayerPrefab, PlayerSpawnPosition, Quaternion.identity, PlayersSpawned));
		} else {
			LoadedCharacters.Add ((GameObject) Instantiate (PlayerPrefab, PlayerSpawnPosition, Quaternion.identity));
		}
		LocalPlayer = LoadedCharacters[LoadedCharacters.Count-1].GetComponent<BaseCharacter> ();
		LocalPlayer.name = "LocalPlayer";
		//LocalPlayer.GetComponent<BaseCharacter>().InitialLoad();	// this is only called inside client..
		LocalPlayer.gameObject.GetComponent<BaseCharacter>().IsLocalPlayer = true;
		GameObject MyCamera = GetManager.GetCameraManager ().SpawnCamera(LocalPlayer);
		CharacterController MyCC = LocalPlayer.gameObject.AddComponent<CharacterController> ();
		MyCC.center = new Vector3 (0,-.3f,0);
		MyCC.radius = 0.3f;
		MyCC.height = 1.4f;
		//LocalPlayer.gameObject.AddComponent<CustomController> ();
		LocalPlayer.gameObject.AddComponent<PlayerMovement>();
		//LocalPlayer.gameObject.GetComponent<CustomController> ().UpdateCamera (MyCamera.GetComponent<Camera>());
		Destroy(LocalPlayer.gameObject.GetComponent<BoxCollider>());
		UpdateManagersWithLocalPlayer ();
		return LocalPlayer.gameObject;
	}


	// updates all the managers with the local player spawned
	public void UpdateLocalPlayer(GameObject NewPlayer) {
		LocalPlayer = NewPlayer.GetComponent<BaseCharacter>();
		UpdateManagersWithLocalPlayer ();
	}
	void UpdateManagersWithLocalPlayer() {
		GuiCreator MyGuiCreator = GetManager.GetGuiCreator ();
		if (MyGuiCreator != null) 
			MyGuiCreator.UpdatePlayer (LocalPlayer);
		
		GuiManager MyGuiManager = GetManager.GetGuiManager ();
		if (MyGuiManager != null) 
			MyGuiManager.UpdatePlayer (LocalPlayer);
		
		MonsterSpawner MyMonsterSpawner = GetManager.GetMonsterSpawner ();
		if (MyMonsterSpawner != null) 
			MyMonsterSpawner.MyPlayer = (LocalPlayer).gameObject;
		
		InventoryGui MyInventoryGui = GetManager.GetInventoryGui ();
		if (MyInventoryGui != null) {
			MyInventoryGui.UpdatePlayer(LocalPlayer);
		}
		GameObject MyStatsObject = GameObject.Find ("MyStats");
		if (MyStatsObject != null)
			MyStatsObject.GetComponent<PlayerGUI> ().MyPlayer = LocalPlayer;
	}

	// all players stuff
	public GameObject GetCharacter(int PlayerIndex) {
		if (PlayerIndex >= 0 && PlayerIndex < LoadedCharacters.Count) {
			return LoadedCharacters[PlayerIndex];
		}
		return null;
	}
	public void DestroyLoadedCharacters() {
		for (int i = 0; i < LoadedCharacters.Count; i++) {
			Destroy (LoadedCharacters[i].gameObject);
		}
	}

	public void CheckLoadedCharactersInMap() {
		BaseCharacter[] Enemies = FindObjectsOfType(typeof(BaseCharacter)) as BaseCharacter[];
		for (int i = 0; i < Enemies.Length; i++) {
			Enemies[i].PlayerIndex = GetPlayerIndex(Enemies[i].gameObject);
		}
	}
	
	public int HasCharacterLoaded(GameObject NewPlayer) {
		for (int i = 0; i < LoadedCharacters.Count; i++) {
			if (LoadedCharacters[i] == NewPlayer)
				return i;
		}
		return -1;
	}
	public int GetPlayerIndex(GameObject NewPlayer) {
		int CharacterLoadedIndex = HasCharacterLoaded (NewPlayer);
		if (CharacterLoadedIndex == -1) {	// if isn't in spawn list - add to it
			LoadedCharacters.Add (NewPlayer);
			int ReturnIndex = LoadedCharacters.Count-1;
			//Debug.LogError("Inside GetPlayerIndex: ReturnIndex: " + ReturnIndex);
			//Debug.Break ();
			return ReturnIndex;
		}
		return CharacterLoadedIndex;
	}
	public void RemovePlayer(GameObject NewPlayer) {
		for (int i = LoadedCharacters.Count-1; i >= 0; i--) {
			if (LoadedCharacters[i] != null) {
				if (LoadedCharacters[i].GetComponent<BaseCharacter>().PlayerIndex > NewPlayer.GetComponent<BaseCharacter>().PlayerIndex) {
					LoadedCharacters[i].GetComponent<BaseCharacter>().PlayerIndex--;
				}
			} else {	// might as well remove any that go randomly destroyed
				//Debug.LogError ("Loaded Character: " + i + " - references to null. Object in the scene was unexpectengly destroyed.");
				LoadedCharacters.RemoveAt (i);
			}
		}
		LoadedCharacters.Remove (NewPlayer);
	}

	// respawning
	
	public void AddToRespawn(BaseCharacter NewCharacter) {
		CharacterRessurection NewRessurection = new CharacterRessurection (NewCharacter, BaseRessurectionCoolDown);
		NewRessurection.RespawnPoint = NewCharacter.transform.position;
		
		RespawnList.Add (NewRessurection);
	}
	
	public void AddLocalPlayerToRespawn() {
		if (LocalPlayer != null) {
			AddToRespawn (LocalPlayer);
			GetManager.GetGuiManager ().EnterNewState (ScreenType.PreGame);
		}
	}
	public float GetRespawnTime(BaseCharacter MyCharacter) {
		for (int i = 0; i < RespawnList.Count; i++) {
			if (RespawnList[i].GetDeadCharacter() == MyCharacter) {
				return RespawnList[i].GetTimeLeft ();
			}
		}
		return 0;
	}
	public void RespawnCharacters() {
		for (int i = 0; i < RespawnList.Count; i++) {
			if (RespawnList[i].CanRessurect()) {
				RespawnList [i].GetDeadCharacter().Ressurect (RespawnList [i].RespawnPoint);
				RespawnList.RemoveAt (i);
			}
		}
	}
}
