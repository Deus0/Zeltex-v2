using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GetManager : MonoBehaviour {	
	public static void LoadAllChunks() {
		Chunk[] MyChunks = FindObjectsOfType(typeof(Chunk)) as Chunk[];
		for (int i = 0; i < MyChunks.Length; i++) {
			Serialization.Load(MyChunks[i]);
		}
	}
	public static void SaveAllChunks() {
		Chunk[] MyChunks = FindObjectsOfType(typeof(Chunk)) as Chunk[];
		for (int i = 0; i < MyChunks.Length; i++) {
			Serialization.SaveChunk(MyChunks[i]);
		}
	}
	
	public static WaveManager GetWaveManager() {
		WaveManager[] WaveManagers = FindObjectsOfType(typeof(WaveManager)) as WaveManager[];
		if (WaveManagers.Length > 0)
			return WaveManagers[0];
		else 
			return new WaveManager();
	}
	public static ZoneManager GetZoneManager() {
		ZoneManager[] ZoneManagers = FindObjectsOfType(typeof(ZoneManager)) as ZoneManager[];
		if (ZoneManagers.Length > 0)
			return ZoneManagers[0];
		else return new ZoneManager();
	}
	public static CameraManager GetCameraManager() {
		CameraManager[] NetworkManagers = FindObjectsOfType(typeof(CameraManager)) as CameraManager[];
		if (NetworkManagers.Length > 0)
			return NetworkManagers[0];
		else return new CameraManager();
	}
	public static CharacterManager GetCharacterManager() {
		CharacterManager[] NetworkManagers = FindObjectsOfType(typeof(CharacterManager)) as CharacterManager[];
		if (NetworkManagers.Length > 0)
			return NetworkManagers[0];
		else return new CharacterManager();
	}
	public static NetworkManager GetNetworkManager() {
		NetworkManager[] NetworkManagers = FindObjectsOfType(typeof(NetworkManager)) as NetworkManager[];
		if (NetworkManagers.Length > 0)
			return NetworkManagers[0];
		else return new NetworkManager();
	}
	public  static  TerrainManager GetTerrainManager() {
		TerrainManager[] TerrainManagers = FindObjectsOfType(typeof(TerrainManager)) as TerrainManager[];
		if (TerrainManagers.Length > 0)
			return TerrainManagers[0];
		else return new TerrainManager();
	}
	public  static  World GetWorld() {
		World[] Worlds = FindObjectsOfType(typeof(World)) as World[];
		//Debug.LogError ("TotalWorlds: " + Worlds.Length);
		if (Worlds.Length > 0)
			return  Worlds[0];
		else
			return null;
	}
	public static Player GetMyPlayer() {
		return GetCharacterManager ().GetLocalPlayer ().gameObject.GetComponent<Player>();
	}
	public static ChatBox GetChatBox() {
		ChatBox[] ChatBoxs = FindObjectsOfType(typeof(ChatBox)) as ChatBox[];
		if (ChatBoxs.Length > 0)
			return ChatBoxs[0];
		else return new ChatBox();
	}
	public static MapCreator GetMapCreator() {
		MapCreator[] MapCreators = FindObjectsOfType(typeof(MapCreator)) as MapCreator[];
		if (MapCreators.Length > 0)
			return MapCreators[0];
		else return new MapCreator();
	}
	public static GameManager GetGameManager() {
		GameManager[] GameManagers = FindObjectsOfType(typeof(GameManager)) as GameManager[];
		if (GameManagers.Length > 0)
			return GameManagers[0];
		else return new GameManager();
	}
	public static TextureManager GetTextureManager() {
		return GetDataManager().BlocksTextureManager;
	}
	public static MonsterSpawner GetMonsterSpawner() {
		MonsterSpawner[] MonsterSpawners = FindObjectsOfType(typeof(MonsterSpawner)) as MonsterSpawner[];
		if (MonsterSpawners.Length > 0)
			return MonsterSpawners[0];
		else return new MonsterSpawner();
	}
	public static DataManager GetDataManager() {
		DataManager[] DataManagers = FindObjectsOfType(typeof(DataManager)) as DataManager[];
		if (DataManagers.Length > 0)
			return DataManagers[0];
		else return new DataManager();
	}
	public static MusicManager GetMusicManager() {
		MusicManager[] DataManagers = FindObjectsOfType(typeof(MusicManager)) as MusicManager[];
		if (DataManagers.Length > 0)
			return DataManagers[0];
		else return new MusicManager();
	}
	public  static  GuiManager GetGuiManager() {
		GuiManager[] GuiManagers = FindObjectsOfType(typeof(GuiManager)) as GuiManager[];
		if (GuiManagers.Length > 0)
			return  GuiManagers[0];
		else
			return null;
	}
	public  static  InventoryGui GetInventoryGui() {
		InventoryGui[] GuiManagers = FindObjectsOfType(typeof(InventoryGui)) as InventoryGui[];
		if (GuiManagers.Length > 0)
			return  GuiManagers[0];
		else
			return null;
	}
	public  static  GuiCreator GetGuiCreator() {
		GuiCreator[] GuiManagers = FindObjectsOfType(typeof(GuiCreator)) as GuiCreator[];
		if (GuiManagers.Length > 0)
			return  GuiManagers[0];
		else
			return null;
	}
	public  static Camera GetMainCamera() {
		return Camera.main;
	}
	public  static  Canvas GetCanvas() {
		Canvas[] Canvases = FindObjectsOfType(typeof(Canvas)) as Canvas[];
		if (Canvases.Length > 0)
			return  Canvases[0];
		else
			return null;
	}
	public  static  Button GetDeselectButton() {
		Button[] Buttons = FindObjectsOfType(typeof(Button)) as Button[];
		if (Buttons.Length > 0)
			for (int i = 0; i < Buttons.Length; i++)
				if (Buttons [i].gameObject.name == "DeselectButton")
					return Buttons [i];	//nagers[0];
		return null;
	}
}
