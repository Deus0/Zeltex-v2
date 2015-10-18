using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

// MapLoader should be in another class
// this one should just handle the beginning and ending of games
// Record the score as well
// check if end game conditions are complete

// The game manage handles anything related to game rules
// the terrain loader will load maps
// the character manager will handle characters
// this manager just starts/stops games
// the checks are made after events, e.g. a player kills a player, it will check if the limit of the rules is breached

// only Adventure will have save files for characters, everything else will use temporary ons
[System.Serializable]
public enum GameMode {
	DeathMatch,
	TeamDeathMatch,		// teams versing each other
	Timed,				// unlimited lives, finishes in time
	Survival,			// lives limited to 1
	Adventure,			// unlimited lives
	CaptureTheFlag,		// 2 bases, you capture it
	KingOfTheHill,		// Multiple bases
	Moba				// troopes spawn, kill enemy base
};

// Used for different game mode settings
[System.Serializable]
public class GameModeSettings {
	public float TimeLimit = 0;				// if 0, the game doesn't end with time
	public float KillLimit = 0;				// if 0, the game doesn't end with kills
	public float ScoreLimit = 0;			// 0 for unlimited,	Score can be done for various things
	public float ScorePerKill = 0;
	public float ScorePerBasePerSecond;		// for every second a base is owned by a clan, it will give points to them
	public bool IsFreeForAll = false;		// if true, limit of 1 character per clan excluding summons
	public int MaxTeams = 2;				// how many teams can be created
	public bool AreBases = false;			// Certain blocks will change clan ownership if a number of characters of a clan are around them
	public bool FreeForAllMode = true;
	public bool FriendlFire = true;

	public GameModeSettings() {

	}
};


public class GameManager : MonoBehaviour {
	public GameObject LoadedWorld;
	// prefabs
	public GameObject WorldPrefab;	// i should be this in Terrain Manager?
	public List<GameModeSettings> MyGameModes = new List<GameModeSettings> ();
	public int CurrentGameMode = 0;	// instead of using an enum, we will have use one of the gameModeSettings

	//references
	private GuiManager MyGui;

	// game management stufff
	// need a game rules class for these settings
	public string GameName = "MyGameFile";		// all the saved data for the particular match will go in here
	public bool IsPaused = false;				// is the game paused? time.scale will be 0 instead of 1, slowing all physics
	public bool IsPlaying = true;
	public bool FreeForAllMode = true;
	//public bool FriendlFire = true;
	public GameMode MyGameMode;
	public int MaxKills = 4;	// Maximum kills for a player, deathmatch mode

	//0 for unlimited - end game when time is up
	// Time is updated in the Game Manager

	// time management 
	public float TimePlayed = 0f;
	//public float MaxTime = 0;
	//public float UpdateTimeScale = 1f;
	public float TimeScale = 1f;
	public float TimeBegan = 0;
	public bool IsWaves = false;

	// stuff that needs to go
	[SerializeField] private bool bCanToggleMenu = true;
	public List<string> PlayerScoreList;
	bool IsOfflineMode = false;

	public bool IsTowerDefence = false;
	int GameType = 0;

	// Use this for initialization
	void Start () {
		Time.timeScale = TimeScale;
		if (MyGui == null) {
			MyGui = GetManager.GetGuiManager();
		}
	}

	// Update is called once per frame
	void Update () {
		DoUpdateTimeScale ();
		CheckForEndOfGame ();
	}

	public void DoUpdateTimeScale() {
		//if (UpdateTimeScale != TimeScale) {
		//	TimeScale = UpdateTimeScale;
		//	Time.timeScale = TimeScale;
		//}
	}
	public void CheckForEndOfGame() {
		if (IsPlaying) {
			TimePlayed += Time.deltaTime;
			// check for time finished
			if (MyGameModes [CurrentGameMode].TimeLimit != 0) {	// only if time is limited
				if (TimePlayed > MyGameModes [CurrentGameMode].TimeLimit) {
					EndGame ();
				}
			}

		}
	}
	public bool IsFriendlyFire() {
		return MyGameModes [CurrentGameMode].FriendlFire;
	}
	public void EndGame() {
		MyGui.EnterNewState(ScreenType.EndGame);
		GetManager.GetCameraManager ().CreateMenuCamera ();
		PauseGame();
		IsPlaying = false;
		UpdateScoresData ();
		MyGui.UpdateScoreList (PlayerScoreList);
		ClearWorld ();
		GetLocalPlayer ().GetComponent<PlayerMovement> ().PauseGame ();
		GetManager.GetWaveManager ().EndAll ();
		GetManager.GetCameraManager ().EnableMouseGui ();
	}

	public void PauseGame() {
		if (IsPlaying) {
			UpdatePause (IsPaused);
		}
	}
	public void UpdatePause(bool IsPause) {
		IsPaused = IsPause;
		if (IsPaused) {
			IsPaused = false;
			//UpdateTimeScale = 1f;
		} else {
			IsPaused = true;
			//UpdateTimeScale = 0f;
		}
	}

	public bool CanToggleMenu() {
		return bCanToggleMenu;
	}
	public void SetCanToggleMenu(bool Can) {
		bCanToggleMenu = Can;
	}

	// Settings for each type of game
	public void DeathMatchSettings() {
		RenderSettings.fog = false;
		GameType = 2;			
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<CharacterSaver>().IsLoadData = false;
		GetManager.GetCameraManager().CameraPrefab.GetComponent<LoadChunks> ().IsLoadOnce = true;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = false;
		WorldPrefab.GetComponent<World> ().IsLoadChunks = false;	// temporary world
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = false;	// this needs to be done before the world loads... :O
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsGenerateTerrain = false;	// this needs to be done before the world loads... :O
	}
	public void DungeonSettings() {
		RenderSettings.fog = true;
		GameType = 2;			
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = false;
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<CharacterSaver>().IsLoadData = false;
		GetManager.GetCameraManager().CameraPrefab.GetComponent<LoadChunks> ().IsLoadOnce = true;
		WorldPrefab.GetComponent<World> ().IsLoadChunks = false;	// temporary world
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = false;	// this needs to be done before the world loads... :O
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsGenerateTerrain = false;	// this needs to be done before the world loads... :O
		//GetManager.GetCharacterManager ().SpawnPosition = GetManager.GetDataManager ().MazeList [1].SpawnPosition;
	}
	public void TowerDefenceSettings() {
		RenderSettings.fog = false;
		GameType = 1;
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<CharacterSaver>().IsLoadData = false;		
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<BaseCharacter>().Gold = 20;	
		GetManager.GetCameraManager().CameraPrefab.GetComponent<LoadChunks> ().IsLoadOnce = true;
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<BaseCharacter>().HideBody(true);		
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsGenerateTerrain = true;
		WorldPrefab.GetComponent<World> ().IsLoadChunks = false;	// temporary world
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = true;	// this needs to be done before the world loads... :O
		WorldPrefab.GetComponent<World> ().MyTerrainGen.MyBiomes [0].GroundFrequency = 0.01f;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.MyBiomes [0].LandAmplitude = 4f;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.SeaLevel = -8f;	// no sea
		GetManager.GetCameraManager ().EnterNewCameraMode (CameraMode.TopDown);
		IsWaves = true;
	}
	public void AdventureSettings() {
		RenderSettings.fog = false;
		GameType = 0;
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<CharacterSaver>().IsLoadData = true;		
		GetManager.GetCharacterManager().PlayerPrefab.GetComponent<BaseCharacter>().HideBody(false);	
		GetManager.GetCameraManager().CameraPrefab.GetComponent<LoadChunks> ().IsLoadOnce = false;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsGenerateTerrain = true;
		WorldPrefab.GetComponent<World> ().IsLoadChunks = true;	// temporary world
		WorldPrefab.GetComponent<World> ().MyTerrainGen.IsTowerDefence = false;	// this needs to be done before the world loads... :O
		WorldPrefab.GetComponent<World> ().MyTerrainGen.MyBiomes [0].GroundFrequency = 0.015f;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.MyBiomes [0].LandAmplitude = 16f;
		WorldPrefab.GetComponent<World> ().MyTerrainGen.SeaLevel = 7f;	// lots of sea
	}
	
	public void PlayGameDeathMatch() {
		DeathMatchSettings ();
		StartGame ();	
	}
	public void PlayGameDungeon() {
		DungeonSettings ();
		StartGame ();	
	}
	public void PlayGameTowerDefence() {
		TowerDefenceSettings ();
		StartGame ();	
	}
	public void PlayAdventureGame() {
		AdventureSettings ();
		StartGame ();	
	}
	//GetManager.GetNetworkManager ().StartServer ();		


	// Now Instantiate objects
	public void StartGame() {
		StartGame (GameName, new Vector3(0,50f,0));	
	}
	public void StartGame(Vector3 NewSpawnPosition) {
		Debug.Log ("Playing game at: " + Time.time);
		StartGame (GameName, NewSpawnPosition);
	}
	// loads up the game modes - called from network manager when its finished connecting
	public bool StartGame(string NewSaveFile, Vector3 NewSpawnPosition) {
		GameObject.Find ("MySpellsManager").GetComponent<SpellManager> ().LoadAllSpells ();
		TimeBegan = Time.time;
		if (IsWaves) 
			GetManager.GetWaveManager ().BeginAll ();
		IsPlaying = true;
		GameName = NewSaveFile;
		GetManager.GetGuiManager ().EnterNewState (ScreenType.InGame);
		DestroyMainMenu();
		ClearWorld ();
		if (GameType == 1) {
			GetManager.GetCharacterManager ().SpawnPlayer (NewSpawnPosition);
		} else if (GameType == 2) {
			GetManager.GetCharacterManager ().SpawnPlayer (GetManager.GetDataManager ().MazeList [1].SpawnPosition);
		} else {
			GetManager.GetCharacterManager ().SpawnPlayer();
		}
		WorldPrefab.GetComponent<World> ().SaveFileName = GameName;
		LoadedWorld = (GameObject)Instantiate (WorldPrefab, new Vector3 (), Quaternion.identity);	// also spawn a world
		GetLocalPlayer ().gameObject.transform.FindChild("Camera(Clone)").GetComponent<LoadChunks> ().world = LoadedWorld.GetComponent<World> ();	

		return true;	// later check if any worlds still active, they will need to be cleared first
	}

	BaseCharacter GetLocalPlayer() {
		return GetManager.GetCharacterManager ().GetLocalPlayer ();
	}

	// save files
	public void DeleteLocalCharacter() {
		//DeleteCharacter (LocalPlayer.name);
		DeleteCharacter ("Player" + GetManager.GetCharacterManager().GetLocalPlayer().PlayerIndex);
	}
	public void DeleteCharacter(string PlayerName) {
		Debug.LogError ("Deleting character: " + PlayerName);
		string FileName = FileLocator.SaveLocation (GameName, PlayerName);
		//FileUtil.DeleteFileOrDirectory(FileName);
	}

	// utilities
	public void RestartLevel() {
		Application.LoadLevel ("MainLevel");
	}

	// Terrain Manager Stuff
	// destroys all the loaded worlds of the universe
	public void DestroyUniverse() {

	}
	// loads the Universe loader - similar to chunk loader for worlds - loads in the worlds
	public void LoadUniverse() {

	}
	public void DestroyWorlds() {
		World MyWorld = GetManager.GetWorld ();
		if (MyWorld != null)
			Destroy(MyWorld.gameObject,0.5f);
	}
	
	public void ClearWorld() {
		DestroyWorlds ();
		GetManager.GetCharacterManager().DestroyLocalPlayer ();
		GetManager.GetCharacterManager().DestroyLoadedCharacters ();
	}
	// Camera Manager
	public void DestroyMainMenu() {
		GetManager.GetCameraManager ().DestroyMainMenuCamera ();
	}

	public void SaveWorld() 
	{
		/*World MyWorld = (World) GetManager.GetWorld ();
		for (int i = 0; i < MyWorld.transform.childCount; i++) {
			Chunk NewChunk = (Chunk) MyWorld.transform.GetChild(i).gameObject.GetComponent<Chunk>();
			Debug.Log ("Saving chunk! " + NewChunk.name);
			Serialization.SaveChunk(NewChunk);
		}*/
	}

	public void LoadWorld()
	{
		
	}

	// Gui Manager?
	// scores
	public void ToggleScores() 
	{
		if (MyGui.CurrentScreen == ScreenType.EndGame) {
			MyGui.EnterNewState (ScreenType.InGame);
			UpdateScores ();
		} else if (MyGui.CurrentScreen == ScreenType.InGame) {
			MyGui.EnterNewState (ScreenType.EndGame);
		}
	}

	public void UpdateScoresData() {
		PlayerScoreList = new List<string> ();
		PlayerScoreList.Clear ();
		BaseCharacter[] Enemies = FindObjectsOfType(typeof(BaseCharacter)) as BaseCharacter[];
		//Debug.Log ("Enemies.Length: " + Enemies.Length);
		for (int i = 0; i < Enemies.Length; i++) {
			BaseCharacter NewCharacter = (BaseCharacter) Enemies[i].GetComponent ("BaseCharacter");
			// if (!NewCharacter.IsSummon)
			PlayerScoreList.Add ("[" + NewCharacter.PlayerIndex + "]: " + NewCharacter.name + ", Kills: " + NewCharacter.TotalKills);
		}
	}
	
	public void UpdateScores() {
		UpdateScoresData ();
		if (MyGui != null)
			MyGui.UpdateScoreList (PlayerScoreList);
	}
	// end scores

	public void ExitGame() {
		Application.Quit ();
	}
	// world save file manager
	public EventsManager MyWorldsList;

	// Loads all the database's of a specific worlds save location
	public void LoadWorldData() {

	}
	public void UpdateGameName() {
		GameName = MyWorldsList.GetSelectedText ();
	}

	// When creating world, copy all the default spell files into the new folder
	// as well as the block files, and items, skills or anything else
	public void CreateWorld() {

	}
	// go to create world screen
	public void NewWorld() {

	}
	// delete
	public void DeleteWorld() {
		
	}
	public void CloneWorld() {
		
	}
	public void LoadWorlds() {
		List<string> GameWorldNames = new List<string> ();
		string FolderName = FileLocator.SaveLocation ("", "", "", "");
		Debug.Log ("Loading world names from: " + FolderName + " At Time: " + Time.time);
		DirectoryInfo info = new DirectoryInfo(FolderName);
		FileSystemInfo[] MyFiles = info.GetFileSystemInfos();
		for (int i = 0; i < MyFiles.Length; i++) {
			// if folder kinda thing
			GameWorldNames.Add (MyFiles[i].Name);
		}
		MyWorldsList.ClearAll();
		for (int i = 0; i < GameWorldNames.Count; i++) {
			if (GameWorldNames[i] != "DefaultFiles")
				MyWorldsList.NewEvent (GameWorldNames[i]);
		}
	}
}