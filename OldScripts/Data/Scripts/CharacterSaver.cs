using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace OldCode {
// used to save the data
// I need to break up the inventory data
// and the items/spells
// save them in seperate files
// so I only save whats needed every 3 seconds
// similar to how I update guis
[Serializable]
public class CharacterData {
	public string Name;
	public float PositionX;
	public float PositionY;
	public float PositionZ;
	public List<Icon> MyIcons = new List<Icon>();
	//public Stats MyStats;
	public List<SpellData> MySpells = new List<SpellData>();
	public List<ItemData> MyItems = new List<ItemData>();
	public Stats MyStats;
	// inventory data - serializable
	
	public CharacterData() {
		
	}
};

public class CharacterSaver : MonoBehaviour {
	public string SaveGameName;		// where all the maps data is stored
	public string SaveFileName;		// where this character's file is stored
	public Chunk InChunk;
	public Vector3 InChunkPosition;
	public Vector3 PreviousChunkPosition;
	public bool IsInitLoad = true;	// ?kek
	public bool IsLoadData = false;
	public bool IsPausedOnInitialStart = true;
	
	public bool IsAutoSave = false;
	float LastSavedTime = 0;
	public Vector3 SavedPosition;
	public float SaveCoolDown = 10f;

	void Start() {
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
		MyCharacter.PlayerIndex = GetManager.GetCharacterManager().GetPlayerIndex (gameObject);
		//Debug.LogError("v1 Loading: " + MyCharacter.PlayerIndex.ToString());
		bool HasLoaded = Load (GetManager.GetGameManager ().GameName);	// inventory data defaults
		gameObject.GetComponent<ItemGenerator> ().enabled = !HasLoaded;	// deactivate item generation of loading the character
		MyCharacter.MyInventory.ResetIconsTextures ();
		PauseOnInitialStart ();
		LastSavedTime = Time.time;
		SavedPosition = transform.position;
		if (!HasLoaded) {
			GetManager.GetChatBox().EnterText("/changename");
		}
	}
	void Update() {
		PauseOnInitialStart ();
		if (IsLoadData && IsAutoSave) {
			if (Time.time-LastSavedTime > SaveCoolDown && SavedPosition != transform.position) {	// if time has increased and has moved position
				SavedPosition = transform.position;
				Save ();
			}
		}
	}

	// --- Saving/Loading ---
	public void Save(string NewSaveGameName) {
		SaveGameName = NewSaveGameName;
		Save ();
	}
	
	public void Save() {
		NetworkView MyNetworkView = gameObject.GetComponent<NetworkView> ();
		if ((MyNetworkView && MyNetworkView.isMine) || !MyNetworkView) {
			if (IsLoadData) {
				BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
				//BinaryFormatter BinaryFile = new BinaryFormatter ();
				IFormatter BinaryFile = new BinaryFormatter ();
				//UpdateInChunk ();
				SaveFileName = FileLocator.SaveLocation (SaveGameName, "Player" + MyCharacter.GetPlayerIndex ().ToString ());
				Debug.Log ("Saving Character: " + SaveFileName);
				//Debug.LogError(name + " :Saving player in: " + SaveFileLocation);
				//FileStream MyFile = File.Create (SaveFileLocation);
				Stream MyFile = new FileStream (SaveFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				CharacterData MyCharacterData = new CharacterData ();
				MyCharacterData.PositionX = transform.position.x;
				MyCharacterData.PositionY = transform.position.y;
				MyCharacterData.PositionZ = transform.position.z;
				MyCharacterData.Name = gameObject.name;
				MyCharacterData.MyStats = MyCharacter.MyStats;
				Debug.Log ("Saving: " + MyCharacterData.MyIcons.Count + " Icons.");
				MyCharacterData.MyIcons = MyCharacter.MyInventory.IconsList;
				Debug.Log ("Saving: " + MyCharacter.MyInventory.SpellsList.Count + " Spells.");
				for (int i = 0; i < MyCharacter.MyInventory.SpellsList.Count; i++) {
					MyCharacterData.MySpells.Add (new SpellData (MyCharacter.MyInventory.SpellsList [i]));
				}
				Debug.Log ("Saving: " + MyCharacter.MyInventory.ItemsList.Count + " Items.");
				for (int i = 0; i < MyCharacter.MyInventory.ItemsList.Count; i++) {
					MyCharacterData.MyItems.Add (new ItemData (MyCharacter.MyInventory.ItemsList [i]));
				}
				BinaryFile.Serialize (MyFile, MyCharacterData);
				MyFile.Close ();
			}
		}
	}
	
	public bool Load(string NewSaveGameName) {
		SaveGameName = NewSaveGameName;
		return Load ();
	}
	
	// returns true if successfull
	public bool Load() 
	{
		if (IsLoadData) {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			//Debug.LogError (name + " : Player Index: " + PlayerIndex.ToString());
			//UpdateInChunk ();
			//if (InChunk != null) {
			//if (PlayerIndex == 1) PlayerIndex = 0;
			gameObject.name = "Player" + MyCharacter.GetPlayerIndex().ToString();
			SaveFileName = FileLocator.SaveLocation (SaveGameName, "Player" + MyCharacter.GetPlayerIndex().ToString());
			//Debug.Log (".... LoadingCharacter from: " + SaveFileName + " : And player index: " + MyCharacter.GetPlayerIndex().ToString());
			Debug.Log("Loading: " + SaveFileName);
			//Debug.Break ();
			if (File.Exists (SaveFileName)) {
				IFormatter BinaryFile = new BinaryFormatter ();
				FileStream MyFile = new FileStream (SaveFileName, FileMode.Open);
				
				CharacterData MyCharacterData = (CharacterData)BinaryFile.Deserialize (MyFile);
				MyFile.Close ();
				transform.position = new Vector3 (MyCharacterData.PositionX, MyCharacterData.PositionY, MyCharacterData.PositionZ);
				MyCharacter.UpdateName (MyCharacterData.Name);
				//MyInventory.SpellsList = MyCharacterData.MySpells;
				MyCharacter.MyStats = MyCharacterData.MyStats;
				MyCharacter.MyStats.ResetCoolDowns();
				// now clear the inventory first
				MyCharacter.MyInventory.Clear();
				Debug.Log ("Saving: " + MyCharacterData.MyIcons.Count + " Icons.");
				MyCharacter.MyInventory.IconsList = MyCharacterData.MyIcons; 
				Debug.Log ("Loading: " + MyCharacterData.MySpells.Count + " Spells.");
				for (int i = 0; i < MyCharacterData.MySpells.Count; i++) {
					MyCharacter.MyInventory.AddSpellWithoutIcon(new Spell(MyCharacterData.MySpells[i]));
					/*if (! ) {
						Debug.LogError ("Failure to add saved Spell.");
						return false;
					}*/
					//MyInventory.SpellsList.Add (  ) );
				}
				Debug.Log ("Loading: " + MyCharacterData.MyItems.Count + " Items.");
				for (int i = 0; i < MyCharacterData.MyItems.Count; i++) {
					MyCharacter.MyInventory.AddItemWithoutIcon(new Item( MyCharacterData.MyItems[i]));
					//MyInventory.SpellsList.Add (  ) );
				}
				MyCharacter.MyInventory.HasSwitchedItem = true;
				return true;
			} else {
				Debug.Log ("Loading from: " + SaveFileName + " Failed at: " + Time.time);
				return false;
			}
			//Debug.Break ();
			//}
			//IsLoadData = false;
		}
		return false;
	}

	World BaseWorld;
	public void UpdateInChunk() {
		//Debug.LogError ("Updating in chunk");
		PreviousChunkPosition = InChunkPosition;
		InChunkPosition = transform.position;
		int InChunkPositionX = Mathf.RoundToInt (InChunkPosition.x);// / Mathf.RoundToInt(16));
		int InChunkPositionY = Mathf.RoundToInt(InChunkPosition.y);// / Mathf.RoundToInt(16));
		int InChunkPositionZ = Mathf.RoundToInt(InChunkPosition.z);// / Mathf.RoundToInt(16));
		InChunkPosition = new Vector3(InChunkPositionX, InChunkPositionY, InChunkPositionZ);
		if (InChunkPosition != PreviousChunkPosition || InChunk == null) {
			BaseWorld = GetManager.GetWorld ();
			if (BaseWorld != null)
				InChunk = BaseWorld.GetChunk (Mathf.RoundToInt (InChunkPosition.x), Mathf.RoundToInt (InChunkPosition.y), Mathf.RoundToInt (InChunkPosition.z));
			else
				Debug.LogError ("No base world for in chunk update at : " + name);
		}
	}

	public void PauseOnInitialStart() {
		if (IsPausedOnInitialStart) {
			FreezeMovement(true);
			UpdateInChunk ();
			if (BaseWorld != null && InChunk != null) 
			{
				if (InChunk.HasInitialLoad == true) {
					Block MyBlock = Terrain.GetBlockV(InChunk.world, transform.position);
					if (MyBlock != null) {
						Debug.Log("Problem placing: " + gameObject.name);
						transform.position = new Vector3(transform.position.x, transform.position.y+20f, transform.position.z);// += 50;
					} else {
						Debug.Log("UnFreezing: " + gameObject.name + " at " + transform.position.ToString ());
					}
					FreezeMovement(false);
					IsPausedOnInitialStart = false;
				} 
			}
			else if (BaseWorld == null) 
			{
				FreezeMovement(false);
				IsPausedOnInitialStart = false;
			}
		}
	}

	public void FreezeMovement(bool IsMovementFrozen) {
		if (gameObject.GetComponent<CharacterController> () != null)
			gameObject.GetComponent<CharacterController> ().enabled = !IsMovementFrozen;
	}

	// Loads data from the MyDataManager into the players inventory
}
}
