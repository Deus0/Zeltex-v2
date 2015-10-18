using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

// This class handles: (Move this into seperate classes)
//			- Inventory
//			- Casts spells
//			- Stats
//			- Animating Colour of material - for things like getting hit
//			- Sound handling - E.g. Spawning, Dying, Stepping on different materials, speaking
//			- Gui handler
//	Converted already to other classes:
//			- Loading/Saving of character
// 			- Inventory Item Generator

// Co existing with:
//			- BotMovement
//			- BotPerception
//			- BotBrain	(Decisions)
//			- CharacterAnimator - bone points

// need to break up this class into multiple classes
// animation class
// mouse over class?
// ray tracer class - has ray trace game object, ray hit data
// BaseCharacter -> with stats and states
// 
// things to do here:
// link up triggers with gui controls options
// put animation into a seperate class - ie animationcontroller script
// ..

//RayCastForCharacters ();	// check mouse hit for players


// Idea for Trigger System:

// for all Triggers - triggers attached to inventory slot
// get the spell from the item in the inventory
// if trigger is pressed activate
// if spell is on cool down wait
// if spell is charging keep charging
// if spell is charged fire
//for (int i = 0; i < TriggerList.Count; i++) {
//	if (Input.GetMouseButtonDown(TriggerList[i].KeyPressed)) {
// Activate any generic function -> can set other keys to keys
//	}
//}
//if (MyData) {
//if (IsShoot) {	// trigger
//Item SelectedItem =	MyInventory.GetItem (SelectedItem);
// if inventory item is spell, check mana, spawn projectile
// if it is a weapon, do the same, decrease durability
// if weapon gets a kill, give it experience points


// Add Base Character Script
// it will add skeletal mesh to the player with a basic model - melee, flying, or ranged
// Add on a Movement script for the bot to move
public class BaseCharacter : CharacterBasic {
	public Inventory MyInventory;		// holds all the spells, items, gear, macros etc - all are represented by an icon
	public Stats MyStats = new Stats();	// any kind of stats that affects game mechanics
	//	CharacterStatistics - as well - move to class - record all actions etc

	// :: references to other classes ::
	private GameManager MyGameManager;	// reference for game manager
	//private World InWorld;
	private DataManager MyData;
	private CharacterAnimator MyAnimator;  

	// .:Combat:.
	public bool CanBeTargeted = true;	// can AI target the player
	public bool IsInCombat = false; // true if in combat
	private GameObject LastHitPlayer;	  // the player who last hit this one
	public float BulletOffset = 0;
	public Vector3 bounds = new Vector3 (1, 1, 1);
	// :: Items ::
	public GameObject ItemDropPrefab;	// drops on player death - this should come out of inventorye character has remaining
	public int Gold = 10;
	// these will be chucked into inventory classs


	// Statistics
	public List<HitStats> HitList = new List<HitStats>();
	public int TotalKills = 0;			// total amount of kills the player has got this round/lifetime
	public int TotalDeaths = 0;			// how many times the character has died
	
	// :: Sound stuff ::
	public float SoundCoolDown = 5f;
	public float LastSound = 0f;
	public AudioSource SoundSource;	// source of where the sound is emmited from - getting hit, casting spell, dying etc
	public AudioSource SoundSource2;	// casting spells
	public float SoundVolume = 1f;
	public AudioClip WalkingSoundEffect;

	// Visual Effects
	public GameObject SelectedBlockModel;
	public GameObject SelectedBlockModelPrefab;

	// :: RayTracing ::
	public bool IsMouseHit;				// the state if the mouse is currently hit
	public RaycastHit MouseHit;			// Player, PlayerGui uses this
	public float RayCastRange = 75;		// Maximum length of the ray cast
	public GameObject SelectedPlayer;	// The target of the ray - stored for loner term
	public GameObject MouseOverPlayer;	// The Player the ray is hitting
	public GameObject MouseOverZone;	// The Zone the player is hitting
	public Vector3 ShootPosition;		// Origin of the ray
	public Vector3 ShootForwardVector;	// Direction the ray is facing
	// atm these are used for the turrets
	public Vector3 ShootPositionOffset;	// offset for the object its shooting from
	public bool IsShootUpwards;

	// :: Block Placement/destruction ::
	private float ToolUsedTime;		// last time any item was used
	public bool IsHitBlocks = false;
	public Transform HitBlocksTransform;
	public Vector3 HitBlock;
	public Vector3 PlaceBlock;
	public bool IsBlockStructureCentred = true;
	public Block SelectedBlock;	// used for debug purposes

	// :: Networking ::
	public Quaternion SpawnRotationQuaternion;
	public GameObject LatestSpawnedBullet;		// <-- not sure?

	// :: Spawning ::
	public bool IsCustomRespawnPoint;
	public Vector3 RespawnPoint;


	public bool IsLocalPlayer = false;	// used when spawning a bullet
	LineRenderer MyLaser;
	public GameObject MasterCharacter;
	// animating colour
	public Color32 ColorBegin = new Color32(255,88,88,125);
	public Color32 ColorEnd = new Color32(255,255,255,255);
	public float ColorTimeBegin = 0;
	private float ColorAnimationTime1 = 0.6f;
	private float ColorAnimationTime2 = 0.4f;
	public bool IsAnimatingForwards = true;
	public bool IsAnimateColor = false;
	private bool IsFadingOut = false;
	private float DeathAnimationTime = 10f;
	// Sounds
	public AudioClip MyDeathSound;
	public AudioClip MyLifeSound;

	public Transform ShootTransform;
	public Vector3 ShootTransformRotationOffset;

	// Getters and setters for indexing
	public int GetPlayerIndex() {return PlayerIndex;}
	public int GetClanIndex() {return ClanIndex;}
	
	// Use this for initialization
	// This shit is messy, need to clean it up
	void Awake () 
	{
		InitialLoad ();
		//Debug.LogError ("Inside Awake: " + name + " : Player Index: " + PlayerIndex);
		SoundSource.PlayOneShot (MyLifeSound);
		//NetworkView MyNetworkView2 = gameObject.AddComponent<NetworkView> ();
		//MyNetworkView2.observed = gameObject.transform;
	}
	public void InitialLoad() {
		SetDefaults ();
		PlayerIndex = GetManager.GetCharacterManager().GetPlayerIndex (gameObject);
	}
	public void SetDefaults() 
	{
		// references to other classes
		MyGameManager = GetManager.GetGameManager ();	
		MyData = GetManager.GetDataManager ();
		MyAnimator = gameObject.GetComponent<CharacterAnimator> ();
		// Sound stuff
		SoundSource = GetComponent<AudioSource>();
		if (transform.childCount >= 2)
			SoundSource2 = transform.GetChild(1).GetComponent<AudioSource>();
		// sound cooldown
		LastSound = Time.time;
		MyStats.SetDefaults ();	// need a load option instead of defaults
		MyInventory.Clear ();
	}

	void OnDestroy() {
		GetManager.GetCharacterManager().RemovePlayer (gameObject);
	}
	
	public void UpdateName(string NewName) {
		if (NewName != gameObject.name) {
			gameObject.name = NewName;
			MyStats.HasUpdated = true;
		}
	}
	
	// Update is called once per frame
	void Update() {
		ShootProjectiles();
		UpdateStats();
		AnimateColor();
	}

	public void BeginAnimateColor() {
		if (!IsFadingOut) {
			ColorTimeBegin = Time.time;
			IsAnimatingForwards = true;
		}
	}
	public void BeginAnimateColor(bool AnimationDirection) {
		if (!IsFadingOut) {
			ColorTimeBegin = Time.time;
			IsAnimatingForwards = AnimationDirection;
		}
	}
	public void BeginFadeOutAnimation() {
		MeshRenderer MyMeshRenderer = GetComponent<MeshRenderer> (); 
		if (MyMeshRenderer) {
			ColorBegin = MyMeshRenderer.material.color;	// fade away
			ColorEnd = new Color32 (255, 255, 255, 0);	// fade away
			BeginAnimateColor (false);
			ColorAnimationTime2 = DeathAnimationTime;
		}
		IsFadingOut = true;
	}
	public void ReverseAnimationColor() {
		IsAnimatingForwards = false;
		ColorTimeBegin = Time.time;
	}
	public void AnimateColor() {
		if (IsAnimateColor) {
			MeshRenderer MyMeshRenderer = GetComponent<MeshRenderer> (); 
			if (MyMeshRenderer) {
				if (ColorTimeBegin != 0) {
						if (IsAnimatingForwards) {
							if (MyMeshRenderer.material.color != ColorBegin) {	// if colour not normal state
								MyMeshRenderer.material.color = Color32.Lerp (ColorEnd, ColorBegin, (Time.time - ColorTimeBegin) / ColorAnimationTime1);
						} else {
							ReverseAnimationColor();
						}
					} else {
						if (MyMeshRenderer.material.color != ColorEnd) {	// if colour not normal state
							MyMeshRenderer.material.color = Color32.Lerp (ColorBegin, ColorEnd, (Time.time - ColorTimeBegin) / ColorAnimationTime2);
						}
					}
				} else {	// for the beginning
					MyMeshRenderer.material.color = ColorEnd;
				}
			}
		}
	}
	// later move this to character animator script
	public void HideBody(bool IsHide) {
		if (gameObject.GetComponent<MeshRenderer> ())
		gameObject.GetComponent<MeshRenderer> ().enabled = !IsHide;
		if (gameObject.GetComponent<BoxCollider> ())
		gameObject.GetComponent<BoxCollider> ().enabled = !IsHide;
		//gameObject.GetComponent<Rigidbody> ().isKinematic = IsHide;
		CanBeTargeted = !IsHide;
	}
	



	// From transform.position, find the position of chunk
	// Should use something like Terrain.GetChunkPosition(World InWorld, Vector3 Position) like dis <><<===
	public void SetClanIndex(int NewClanIndex) {
		//HasChangedClan = true;
		ClanIndex = NewClanIndex;
	}

	// used for any gui element, or anything with a collider
	public void OnMouseDown() {

	}
	public void ShootProjectiles() {
		
		if (!MyInventory.IsShoot) {
			StopSpellCasting();
		}
		if (MyInventory.IsShoot || MyInventory.IsShoot2) {
			if (MyInventory.GetSelectedIcon().MyIconType == IconType.Spell) 
			{
				if (MyInventory.IsShoot) 
				{
					CastSpell ();
				}
			} 
			else if (MyInventory.IsShoot && MyInventory.GetSelectedIcon().MyIconType == IconType.Item
			         && Time.time-ToolUsedTime > 0.1f) 
			{	// ie blocks
				ToolUsedTime = Time.time;
				//if (IsHitBlocks)
				//{
				//Chunk MyChunk = MouseHit.collider.gameObject.GetComponent<Chunk> ();
				//if (MyChunk) 
				{
					//if (IsShoot) 
					//{
						Item ItemInUse = MyInventory.GetSelectedItem ();	//MyInventory.ItemsList [MyData.IconsList [SelectedItem].Index];	// objects stored in memory, changes to the variables work
						if (ItemInUse.Quantity > 0) 
					{
							if (ItemInUse.MyItemType == ItemType.BlockStructure) 
							{
								if (IsHitBlocks)
									PlaceBlockStructureWithItem(ItemInUse);
							}
							else if (ItemInUse.MyItemType == ItemType.Dungeon) 
							{
								if (IsHitBlocks)
									PlaceDungeonWithItem(ItemInUse);
							}
							else if (ItemInUse.MyItemType == ItemType.Block) 
							{
								PlaceBlockWithItem(ItemInUse);
							}
							else if (ItemInUse.MyItemType == ItemType.Tool) 
							{
								HitBlockWithTool(ItemInUse);
									// add physics block to point
							} else if (ItemInUse.MyItemType == ItemType.Ingredient) 
							{
								// if commands etc	// using ingredient here now for placeholder
								if (SelectedPlayer) {
									BotMovement MyBot = (BotMovement)SelectedPlayer.GetComponent ("BotMovement");
									MyBot.Follow (gameObject);
								}
							}
						}
					}
				}
			//}
		}

		// Right click and terrain is hit
		if (MyInventory.IsShoot2 && IsHitBlocks) 
		{
			CommandBot(0);
		}
		// rather then activating items, it activates world objects
		// for blocks, it will try and convert the block to a 'function block'
		// a function block may perform a function based on its FunctionType
		// it can cast a spell, ie fire a projectile or teleport a player - or perhaps activate a portal
		if (MyInventory.IsShoot && MyInventory.SelectedItem == -1)
		{
			SelectPlayer();
			if (IsMouseHit) {	// if door is being hit by mouse
				if (MouseHit.transform != null) {
					DoorAnimation MyDoor = MouseHit.transform.gameObject.GetComponent<DoorAnimation> ();
					if (MyDoor != null) {
							MyDoor.ToggleDoor ();
					}
					/*Torchelight MyTorch = MouseHit.transform.gameObject.GetComponent<Torchelight> ();
					if (MyTorch != null) {
						MyTorch.Toggle();
					}*/
				}
			}
		}
		MouseOverPlayer = null;
	}
	
	
	public void SelectPlayer() {
		if (MouseOverPlayer) {
			if (MouseOverPlayer != SelectedPlayer) {
				EnableTurretGui(false);
				SelectedPlayer = MouseOverPlayer;
				EnableTurretGui(true);
			}
			else {
				if (SelectedPlayer != null) {
					SelectedPlayer = null;
					EnableTurretGui(false);
				}
			}
		} else {
			if (IsHitBlocks)
				SelectedPlayer = null;
			EnableTurretGui(false);
		}
		MyInventory.IsShoot = false;
	}
	public void EnableTurretGui(bool IsEnable) {
		if (IsLocalPlayer && SelectedPlayer) {
			if (SelectedPlayer.transform.parent && SelectedPlayer.transform.parent.FindChild ("UpgradeGui")) {
				GameObject UpgradeGui = SelectedPlayer.transform.parent.FindChild ("UpgradeGui").gameObject;
				if (UpgradeGui) {
					UpgradeGui.SetActive (IsEnable);
				}
			}
			else if (SelectedPlayer && SelectedPlayer.transform.FindChild ("UpgradeGui")) {
				GameObject UpgradeGui = SelectedPlayer.transform.FindChild ("UpgradeGui").gameObject;
				if (UpgradeGui) {
					UpgradeGui.SetActive (IsEnable);
				}
			}
		}
	}
	public void PlaceDungeonWithItem(Item ItemInUse) {
		MouseHit.point += MouseHit.normal * 0.5f;
		Maze MyDungeon = GetManager.GetDataManager ().MazeList [ItemInUse.BlockIndex];
		if (IsBlockStructureCentred)
			MouseHit.point -= new Vector3 (MyDungeon.MyBlocks.Size.x / 2f, 0, MyDungeon.MyBlocks.Size.z / 2f);

		//MyDungeon.MyBlocks.ClearData ();
		//MyDungeon.Reset();
		//MyDungeon.GenerateDungeon();
		// blockStructure.createRoom etc
		// now check the game manager for previously placed building zones
		//bool CanCreateBuilding = GetManager.GetZoneManager ().PlaceBuildingZone (Terrain.GetBlockPosV (MouseHit) - new Vector3 (0.5f, 0.5f, 0.5f), MyDungeon.MyBlocks.Size);
		//if (CanCreateBuilding) {
			if (MyDungeon.UpdateTerrainAtPosition (MouseHit)) {
				MyInventory.DecreaseSelectedItemQuantity ();
			}
		//}
	}
	public bool CanPlaceBlock() {
		int BlockPlaceIndex = Terrain.GetBlock (MouseHit, false).TileIndex;
		return (BlockPlaceIndex == 0 || BlockPlaceIndex == 4);	// if air or water
	}
	public void PlaceBlockStructureWithItem(Item ItemInUse) {
		MouseHit.point += MouseHit.normal * 0.5f;
		if (CanPlaceBlock()) {
			BlockStructure blockStructure = MyInventory.GetSelectedBlockStructure ();
			if (IsBlockStructureCentred)
				MouseHit.point -= new Vector3 (blockStructure.MyBlocks.Size.x / 2f, 0, blockStructure.MyBlocks.Size.z / 2f);
		
			// blockStructure.createRoom etc
			// now check the game manager for previously placed building zones

			//if (CanCreateBuilding) {
			//blockStructure.Reset();
			//blockStructure.UpdateBlockStructureWithType();
			if (blockStructure.UpdateTerrainAtPosition (MouseHit, false)) {
				MyInventory.DecreaseSelectedItemQuantity ();
			}
			//}
		}
	}

	// Removes the zone with its blocks
	public void RemoveZoneBlocks(Vector3 HitPosition, World MyHitWorld) {
		//Debug.LogError ("Building Core being Removed");
		// get zone
		Zone MyZone = GetManager.GetZoneManager().GetZoneFromBlock(HitPosition);
		Vector3 ZoneSize = MyZone.Size;
		for (float i = -Mathf.FloorToInt(ZoneSize.x); i <= Mathf.FloorToInt(ZoneSize.x); i++)
			for (float j = -Mathf.FloorToInt(ZoneSize.y)+1; j <= Mathf.FloorToInt(ZoneSize.y); j++)
			for (float k = -Mathf.FloorToInt(ZoneSize.z); k <= Mathf.FloorToInt(ZoneSize.z); k++) {
				if (GetManager.GetNetworkManager().IsConnected()) {
					MyHitWorld.GetComponent<NetworkView>().RPC ("SetBlockAir", 
					                                            RPCMode.All,
					                                            Mathf.FloorToInt(i+MyZone.InBlockPosition.x),
					                                            Mathf.FloorToInt(j+MyZone.InBlockPosition.y),
					                                            Mathf.FloorToInt(k+MyZone.InBlockPosition.z)
					                                            );
				}
			}
		GetManager.GetZoneManager().DestroyZone (MyZone);
	}

	public void PlaceBlockWithItem(Item ItemInUse) {
		if (IsHitBlocks) {
			MouseHit.point += MouseHit.normal * 0.5f;
			if (CanPlaceBlock()) {
				World MyHitWorld = MouseHit.collider.gameObject.GetComponent<Chunk> ().world;
				int HitPositionX = Mathf.RoundToInt (MouseHit.point.x);
				int HitPositionY = Mathf.RoundToInt (MouseHit.point.y);
				int HitPositionZ = Mathf.RoundToInt (MouseHit.point.z);
				if (GetManager.GetNetworkManager ().IsConnected ())
					MyHitWorld.GetComponent<NetworkView> ().RPC ("SetBlock", 
				                                            RPCMode.All,
				                                            HitPositionX,
				                                            HitPositionY,
				                                            HitPositionZ,
				                                            ItemInUse.BlockIndex 
					);
				else
					MyHitWorld.SetBlock(HitPositionX, HitPositionY, HitPositionZ, ItemInUse.BlockIndex);
				// should also pass in the player index - and take the item off the player index inside the set block function - so itll take items away server side			
				BlockData MyBlockData = GetManager.GetDataManager ().GetBlockData (ItemInUse.BlockIndex);
				if (MyBlockData.PlaceSoundEffect)
					SoundSource.PlayOneShot (MyBlockData.PlaceSoundEffect);
				//bool IsSuccess = Terrain.SetBlock(MouseHit, new Block(ItemInUse.BlockIndex));
				//if (IsSuccess) 
				{
					MyInventory.DecreaseSelectedItemQuantity ();

				}
			}
		}
	}

	// Called when user left clicks on a block while holding a tool
	public void HitBlockWithTool(Item ItemInUse) {
		Chunk MyHitChunk = MouseHit.collider.gameObject.GetComponent<Chunk> ();
		if (MyHitChunk) {
			Vector3 MouseHitPoint = MouseHit.point - MouseHit.normal * 0.5f;
			GameObject ItemDrop = GetManager.GetCharacterManager ().BlockItemDrop;
			Vector3 ItemDropPosition = new Vector3 (Mathf.RoundToInt (MouseHitPoint.x) / 1, Mathf.RoundToInt (MouseHitPoint.y) / 1, Mathf.RoundToInt (MouseHitPoint.z) / 1);
			//ItemDropPosition += new Vector3(0.5f, 0.5f, 0.5f);
		
		
			//Debug.Log ("Using Pickaxe at " + Time.time);
			BlockBase MyBlockBase = Terrain.GetBlock (MouseHit, false);
		
			World MyHitWorld = MyHitChunk.world;
			int HitPositionX = Mathf.RoundToInt (MouseHitPoint.x);
			int HitPositionY = Mathf.RoundToInt (MouseHitPoint.y);
			int HitPositionZ = Mathf.RoundToInt (MouseHitPoint.z);
			Vector3 HitPosition = new Vector3 (HitPositionX, HitPositionY, HitPositionZ);
			// atm this sends the signal to everything
			// first it should check if it can be removed - on client side - then send the signal
			// blocks should be damaged as well - and an animation should be created

			// Get the block index
			//This doesn't work sometimes?
			Block MyBlock = null;
			int BlockIndex = 0;
			BlockDamaged MyBlockDamaged2;
			try {
				MyBlock = (Block)MyBlockBase;
				BlockIndex = MyBlock.GetBlockIndex ();
			} catch {
				//Debug.LogError (gameObject.name + "'s Problem with: " + HitPosition.ToString());
				try {
					MyBlockDamaged2 = (BlockDamaged)MyBlockBase;
					BlockIndex = MyBlockDamaged2.GetBlockIndex ();
				} catch { 

				}
			}

		
			BlockData MyBlockData = GetManager.GetDataManager ().GetBlockData (BlockIndex);
			if (MyBlockData.CanBeRemoved) {
				BlockDamaged MyBlockDamaged;
				try {
					MyBlockDamaged = (BlockDamaged)MyBlockBase;
				} catch {
					MyBlockDamaged = CreateDamagedBlock (MyBlock, MyHitChunk, HitPosition, MyBlockData);
				}
				if (MyBlockDamaged.MyBlockDamage) {
					float DamageDone = MyBlockDamaged.MyBlockDamage.ApplyDamage (ItemInUse.Damage);

					if (DamageDone != 0 && MyBlockDamaged.MyBlockDamage) {
						//CreateDamagePopup (gameObject, MyBlockDamaged.MyBlockDamage.gameObject, DamageDone, 40);
					}

					if (MyBlockDamaged.MyBlockDamage.IsDead ()) {
						Destroy (MyBlockDamaged.MyBlockDamage.gameObject);	// remove the block damage from the scene!
						RemoveBlock (MyBlockData, HitPosition, MyHitWorld);
						///bool IsSuccess = true;
						//if (IsSuccess) 
						{
							//Debug.LogError ("Destroying " + BlockIndex +" Block at time: " + Time.time);
							ItemInUse.DecreaseDurability (BlockIndex);

							//BlockData MyBlockData =  GetManager.GetMyDataManager().GetBlockData(BlockIndex);
							if (MyBlockData != null) {
								int ItemDropType = MyBlockData.DropIndex;
								ItemDrop = GetManager.GetTerrainManager ().BlockPrefab;
								if (ItemDrop != null) {// && ItemDropType != 0) {
									Item DropItem = null;
									for (int i = 0; i < GetManager.GetDataManager().ItemsList.Count; i++) {
										Item NewItem = GetManager.GetDataManager().ItemsList[i];
										if (NewItem.Name == MyBlockData.Name) {
											DropItem = new Item(NewItem);
											break;
										}
									}
									if (DropItem != null) {
										GameObject ItemDropObject = (GameObject)Instantiate (ItemDrop, ItemDropPosition, Quaternion.identity);
										ItemDropObject.GetComponent<ItemPickup> ().DropItem = DropItem;
										ItemDropObject.GetComponent<ItemPickup> ().IsAddEffect = false;
										ItemDropObject.GetComponent<ItemPickup> ().IsStatIncrease = false;
										ItemDropObject.GetComponent<ItemPickup> ().DropType = MyBlockData.DropType;
										ItemDropObject.GetComponent<MeshRenderer> ().material.mainTexture = GetManager.GetDataManager ().BlocksTextureManager.BlockTextures [BlockIndex];
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public BlockDamaged CreateDamagedBlock(Block MyBlock, Chunk MyHitChunk, Vector3 HitPosition, BlockData MyBlockData) {
		BlockDamaged MyBlockDamaged = new BlockDamaged();
		if (MyBlock == null)
			return MyBlockDamaged;
		MyBlockDamaged.SetBlockIndex(MyBlock.GetBlockIndex());
		
		// spawn the block damage
		GameObject NewBlockDamage = new GameObject();
		NewBlockDamage.transform.position = HitPosition;
		NewBlockDamage.name = "BlockDamage: " + HitPosition.ToString();
		
		NewBlockDamage.AddComponent<BlockDamageGui>();
		MyBlockDamaged.MyBlockDamage = NewBlockDamage.GetComponent<BlockDamageGui>();	// blocks reference to its damage
		MyBlockDamaged.MyBlockDamage.Health = MyBlockData.Health;
		MyBlockDamaged.MyBlockDamage.Defence = MyBlockData.Defence;
		MyBlockDamaged.MyBlockDamage.MyChunk = MyHitChunk;
		MyBlockDamaged.MyBlockDamage.MyBlock = MyBlock;
		MyBlockDamaged.MyBlockDamage.x = Mathf.RoundToInt(HitPosition.x);
		MyBlockDamaged.MyBlockDamage.y = Mathf.RoundToInt(HitPosition.y);
		MyBlockDamaged.MyBlockDamage.z = Mathf.RoundToInt(HitPosition.z);
		MyBlockDamaged.MyBlockDamage.LookAtObject = GetManager.GetMainCamera ().gameObject;//LookAtPlayer;
		//MyBlockDamaged.MyBlockDamage.LookAtObject = gameObject;
		
		Canvas MyBlockDamageCanvas = NewBlockDamage.AddComponent<Canvas>();
		MyBlockDamageCanvas.renderMode = RenderMode.WorldSpace;
		
		RawImage MyHealthBarBackground = NewBlockDamage.AddComponent<RawImage>();
		MyHealthBarBackground.color = new Color32 (100, 0, 0, 155);
		GameObject NewBlockDamageSon = new GameObject();
		RawImage MyHealthBar = NewBlockDamageSon.AddComponent<RawImage>();
		MyHealthBar.color = new Color32(255,0,0,255);
		MyHealthBar.material = GetManager.GetTerrainManager().HealthBarMaterial;
		MyHealthBarBackground.material = MyHealthBar.material;
		NewBlockDamageSon.transform.SetParent (NewBlockDamage.transform);
		NewBlockDamageSon.transform.localPosition = new Vector3 (0, 0, 0);
		// now reset the block
		Terrain.SetBlock (MouseHit, MyBlockDamaged);
		return MyBlockDamaged;
	}

	public void RemoveBlock(BlockData MyBlockData, Vector3 HitPosition, World MyHitWorld) {
		switch (MyBlockData.OnDestroyFunctionId)
		{
		case(1):	// for zone cores
			RemoveZoneBlocks (HitPosition, MyHitWorld);
			break;
		case(2):	// town hall end of game condition?
			GetManager.GetGameManager().EndGame();
			RemoveZoneBlocks (HitPosition, MyHitWorld);
			break;
		case(3):// TNT

			break;
		}
		if (MyBlockData.RemovedSoundEffect)
			SoundSource.PlayOneShot (MyBlockData.RemovedSoundEffect);
		if (GetManager.GetNetworkManager ().IsConnected ())
			MyHitWorld.GetComponent<NetworkView> ().RPC ("SetBlockAir", 
			                                            RPCMode.All,
			                                            Mathf.RoundToInt (HitPosition.x),
			                                            Mathf.RoundToInt (HitPosition.y),
			                                            Mathf.RoundToInt (HitPosition.z)
			);
		else
			MyHitWorld.SetBlockAir (Mathf.RoundToInt (HitPosition.x),
			                       Mathf.RoundToInt (HitPosition.y),
			                       Mathf.RoundToInt (HitPosition.z));
	}

	public void CastSpell() {
		if (ShootTransform == null) {
			ShootTransform = transform;
		}
		else {
			ShootPosition = ShootTransform.transform.position + ShootPositionOffset;
			if (IsShootUpwards) {
				ShootForwardVector = ShootTransform.transform.up;
			} else {
				ShootForwardVector = ShootTransform.transform.forward;
			}

			//SpawnRotationQuaternion = new Quaternion();
			//SpawnRotationQuaternion.eulerAngles = new Vector3(ShootTransform.rotation.eulerAngles.x, ShootTransform.rotation.eulerAngles.y, ShootTransform.rotation.eulerAngles.z);
			SpawnRotationQuaternion = ShootTransform.rotation;
			SpawnRotationQuaternion.eulerAngles += ShootTransformRotationOffset;
			//SpawnRotationQuaternion.eulerAngles = SpawnRotationQuaternion.eulerAngles-ShootTransformRotationOffset;
			Spell SpellInUse = MyInventory.GetSelectedSpell ();	// objects stored in memory, changes to the variables work
			//BulletOffset = bounds.magnitude;
			BulletOffset = 1f;
			Vector3 SpellSpawnPosition = ShootPosition + ShootForwardVector * BulletOffset;// * bounds.magnitude;
			// if instant cast, explode on point of hit
			if (IsLocalPlayer && GetManager.GetCameraManager ().MyCameraMode == CameraMode.TopDown) { // and local player lel... need to check dis
				SpellSpawnPosition = MouseHit.point + MouseHit.normal * 1f;	// spell should be positioned based on its size, outside of the ray hit point
			}
			// or maybe instant spell so appear right before the enemy
			if (SpellInUse != null)
				CastSpell (SpellInUse, SpellSpawnPosition, ShootForwardVector);	// they are also passed by reference
			else
				Debug.Log ("How the hell don't i have a spell(=null) to cast?!?!");
			if (!IsShootUpwards)
				ShootTransform.eulerAngles -=  ShootTransformRotationOffset;
		}
	}

	// ideally it should have a trigger for every spell slot and handle spell deactivate functions
	public void StopSpellCasting() {
		if (MyLaser != null) {
			Destroy (MyLaser);
			MyLaser = null;
		}
	}
	
	public bool WasTerrainHit() {
		if (!IsMouseHit)
			return false;
		Chunk MyHitChunk = MouseHit.collider.gameObject.GetComponent<Chunk> ();
		return (MyHitChunk != null); 
	}
	public bool NoTurretInSpawnLocation(Vector3 NewSpawnLocation) {
		Collider[] hitColliders = Physics.OverlapSphere (NewSpawnLocation, 0.5f);
		for (int j = 0; j < hitColliders.Length; j++) {
			BaseCharacter MyEnemy = hitColliders [j].gameObject.GetComponent<BaseCharacter>();
			if (MyEnemy) {
				return false;
			}
		}
		return true;
	}
	int GoldCost = 5;
	// need to edit for different spell casting types
	public void CastSpell(Spell SpellInUse, Vector3 SpawnPosition, Vector3 SpawnRotation) {
		Vector3 TurretSpawnPosition = MouseHit.point;
		TurretSpawnPosition = new Vector3 (Mathf.RoundToInt (TurretSpawnPosition.x), (TurretSpawnPosition.y), Mathf.RoundToInt (TurretSpawnPosition.z));

		if (SpellInUse.IsInstant) {
			SpawnPosition = MouseHit.point;
		}
		Debug.Log ("Casting spell " + SpellInUse.Name +  " using: " + SpellInUse.StatType);
		switch (SpellInUse.FunctionIndex) {
		case (0):	// Projectile
			if (MyStats.GetStat (SpellInUse.StatType) > SpellInUse.ManaCost) {
				// for instant cast ones
				//if ((SpellInUse.SpawnObject.tag == "Turret" && Gold >= GoldCost && WasTerrainHit()	// if summoning turret
				//     && NoTurretInSpawnLocation(TurretSpawnPosition) && MouseHit.normal == new Vector3(0,1,0)) 
				//    || (SpellInUse.SpawnObject.tag != "Turret"))
				if (SpellInUse.CanCast ()) 
				{
					//if (SpellInUse.SpawnObject.tag == "Turret")	// special case for turrets mode
					{
						Gold -= GoldCost;
						MyInventory.UnSelectItem();
					}
					// first charge the character of its stat
					MyStats.IncreaseState (SpellInUse.StatType, -SpellInUse.ManaCost);

					// Sound effect
					//if (SoundSource)
					//	SoundSource.PlayOneShot (SpellInUse.SpawnSoundEffect, SoundVolume);

					// if spawning objects
					//if (SpellInUse.SpawnObject != null) 
					{
						GameObject SpawnedObject = new GameObject();
						Quaternion TempSpawnQuaternion = SpawnRotationQuaternion;
						//Quaternion TempSpawnQuaternion = Quaternion.Euler(ShootForwardVector);
						/*if (SpellInUse.SpawnObject.GetComponent<BaseProjectile>()) 
						{
							float Accuracy = 0.03f;
							TempSpawnQuaternion.eulerAngles += new Vector3(UnityEngine.Random.Range (Accuracy, -Accuracy)*360f,
						                                            UnityEngine.Random.Range (Accuracy, -Accuracy)*360f,
						                                            UnityEngine.Random.Range (Accuracy, -Accuracy)*360f);
						}*/
						//if (GetManager.GetNetworkManager ().IsConnected ()) {
						//	SpawnedObject = (GameObject)Network.Instantiate (SpellInUse.SpawnObject, SpawnPosition, TempSpawnQuaternion, PlayerIndex);
						//} else { 
						//	SpawnedObject = (GameObject)Instantiate (SpellInUse.SpawnObject, SpawnPosition, TempSpawnQuaternion);
						//}
						//
						//gameObject.GetComponent<NetworkView>().viewID.
						BaseProjectile bullet = SpawnedObject.GetComponent<BaseProjectile> ();
						if (bullet != null) {
							//LatestSpawnedBullet = SpawnedObject;
							// this doesn't get called as BaseCharacter is part of the other client
							if (GetManager.GetNetworkManager ().IsConnected () && bullet.GetComponent<NetworkView> ())
								bullet.GetComponent<NetworkView> ().RPC ("UpdateDataWithCharacter", RPCMode.All,
						                                        PlayerIndex,
						                                        ClanIndex,
						                                        SpellInUse.Damage,
						                                        SpellInUse.TravelSpeed,
						                                        SpellInUse.LifeTime);
							else
								bullet.UpdateDataWithCharacter (PlayerIndex,
							                                        ClanIndex,
							                                        SpellInUse.Damage,
							                                        SpellInUse.TravelSpeed,
							                                        SpellInUse.LifeTime);
						}
						// for summoned monsters
						if (SpawnedObject.tag == "Turret") {
							SpawnedObject.transform.position = TurretSpawnPosition;
							SpawnedObject.transform.eulerAngles = MouseHit.normal;
						}
						BaseCharacter Minion = SpawnedObject.GetComponent<BaseCharacter>();
						if (Minion != null) {
							Minion.ClanIndex = ClanIndex;
							Minion.MasterCharacter = gameObject;
							BotMovement MinionMovement = (SpawnedObject.GetComponent<BotMovement>());
							if (MinionMovement != null) {
								MinionMovement.Follow (gameObject);
							}
						}

						// Causes bullet to be destroyed in time
					}
					// this should be cast when pressed and released after animation has passed
					//if (MyAnimator != null) {
					//	MyAnimator.PlayAnimation (SpellInUse.AnimationType);
					//}
				}
			}
			break;
		case (1):	// summoning

			break;
		case (2):	// laser
			float LaserDefinition = 16;
			float WaveSpeed = 5f;
			if (MyLaser == null) {
				// initial spawn laser
				MyLaser = gameObject.AddComponent<LineRenderer>();
				MyStats.IncreaseState (SpellInUse.StatType, -SpellInUse.ManaCost);
				MyLaser.SetVertexCount(Mathf.RoundToInt(LaserDefinition));
				MyLaser.SetWidth(0.25f, 0.15f);
				MyLaser.material = GetManager.GetCharacterManager().MyLaserBeamMaterial;
			} else {
				if (IsMouseHit) {
					if (SpellInUse.CanCast ()) {	// use mana per second
						MyStats.IncreaseState (SpellInUse.StatType, -SpellInUse.ManaCost);
						// Sound effect
						//if (SoundSource)
						//	SoundSource.PlayOneShot (SpellInUse.SpawnSoundEffect, SoundVolume);
						if (IsMouseHit) {
							MouseHit.collider.gameObject.GetComponent<BaseCharacter>().TakeDamage(gameObject, PlayerIndex, ClanIndex, SpellInUse.Damage);
						}
					}
					float Range = Vector3.Distance(ShootPosition, MouseHit.point);
					MyLaser.SetPosition(0, ShootPosition);
					MyLaser.SetPosition(Mathf.RoundToInt(LaserDefinition)-1, MouseHit.point);
					for (int i = 1; i < LaserDefinition-1; i++) {
						Vector3 SinBuffer = new Vector3(Mathf.Cos((i/LaserDefinition)*Range + Time.time*WaveSpeed)/6f, 
						                                Mathf.Sin((i/LaserDefinition)*Range + Time.time*WaveSpeed)/6f, 
						                                Mathf.Sin((i/LaserDefinition)*Range + Time.time*WaveSpeed)/6f);
						MyLaser.SetPosition(i, ShootPosition+ShootForwardVector*(Range/LaserDefinition)*i + SinBuffer);
					}
				}
			}

			break;
		case (3):	// teleport
			if (SpellInUse.CanCast ()) {
				// first charge the character of its stat
				MyStats.IncreaseState (SpellInUse.StatType, -SpellInUse.ManaCost);
				//if (SoundSource)
				//	SoundSource.PlayOneShot (SpellInUse.SpawnSoundEffect, SoundVolume);
				TeleportPlayer (SpellInUse.Range);
			}
			break;
		case (4):	// Chain Lightning
			if (SpellInUse.CanCast ()) {	// use mana per attack
				MyStats.IncreaseState (SpellInUse.StatType, -SpellInUse.ManaCost);
				//if (SoundSource)
				//	SoundSource.PlayOneShot (SpellInUse.SpawnSoundEffect, SoundVolume);
				if (MyLaser != null) {
					Destroy (MyLaser);
					MyLaser = null;
				}
				if (MyLaser == null) {
					// initial spawn laser
					MyLaser = gameObject.AddComponent<LineRenderer>();
					if (MyLaser) {
						MyLaser.material = GetManager.GetCharacterManager().MyChainLightningMaterial;
						List<BaseCharacter> MyChainTargets = GetChainedTargets(5);
						MyLaser.SetVertexCount(MyChainTargets.Count+1);
						MyLaser.SetWidth(0.3f, 0.2f);
						MyLaser.SetPosition(0, ShootPosition);
						for (int i = 0; i < MyChainTargets.Count; i++) {
							MyChainTargets[i].TakeDamage(gameObject, PlayerIndex, ClanIndex, SpellInUse.Damage);
							MyLaser.SetPosition(i+1, MyChainTargets[i].transform.position);
						}
					}
				}
			}
			break;
		}
	}
	public List<BaseCharacter> GetChainedTargets(int NumberOfChainTargets) {
		List<BaseCharacter> MyChainTargets = new List<BaseCharacter> ();
		for (int i = 0; i < NumberOfChainTargets; i++) {
			Debug.Log ("Applying explosion damage");
			if (MyChainTargets.Count == 0) {
				BaseCharacter GetCharacter = GetClosestCharacter(this,MyChainTargets);
				if (GetCharacter != null)
					MyChainTargets.Add (GetCharacter);
				else
					break;
			} else {
				BaseCharacter GetCharacter = GetClosestCharacter(MyChainTargets[MyChainTargets.Count-1],MyChainTargets);
				if (GetCharacter != null)
					MyChainTargets.Add (GetCharacter);
				else
					break;
			}
		}
		return MyChainTargets;
	}

	BaseCharacter GetClosestCharacter(BaseCharacter MyCharacter, List<BaseCharacter> MyChainTargets) {
		BaseCharacter ReturnCharacter = null;
		float DistanceToMe = 10f;
		Collider[] hitColliders = Physics.OverlapSphere (MyCharacter.transform.position, 5);
		for (int j = 0; j < hitColliders.Length; j++) 
		{
			BaseCharacter MyEnemy = (BaseCharacter) hitColliders [j].gameObject.GetComponent ("BaseCharacter");
			if (MyEnemy) 
			if (!MyEnemy.MyStats.IsDead) {
				bool IsInList = false;
				for (int k = 0; k < MyChainTargets.Count; k++) {
					if (MyEnemy == MyChainTargets[k]) {
						IsInList = true;
						break;
					}
				}
				if (!IsInList && MyEnemy.ClanIndex != ClanIndex && MyEnemy.PlayerIndex != PlayerIndex) {
					float PossibleDistanceToMe = Vector3.Distance(MyCharacter.transform.position, MyEnemy.gameObject.transform.position);
					if (PossibleDistanceToMe < DistanceToMe) {
						ReturnCharacter = MyEnemy;
						DistanceToMe = PossibleDistanceToMe;
					}
				}
			}
		}
		return ReturnCharacter;
	}

	// take into account that all the other clients, if there are network spawned objects, they have no data at all, i need to synch them
	//[RPC]
	//public void UpdateBullet(float LifeTime) {
	//	LatestSpawnedBullet.GetComponent<BaseProjectile>().UpdateDataWithCharacter(this, LifeTime);
	//}
	// I should put this in another function - use a static function and have character as input
	// if i did this with every spell itd be too big
	public void TeleportPlayer(float TeleportDistance) {
		Debug.Log ("Teleporting Player");
		RaycastHit TeleportRayHit;
		//BaseCharacter MyPlayer = this;//gameObject.GetComponent<BaseCharacter> ();
		//Vector3 MyBounds = new Vector3 (1, 2, 1);
		if (Physics.Raycast (transform.position, ShootForwardVector, out TeleportRayHit, TeleportDistance)) {
			transform.position = TeleportRayHit.point - ShootForwardVector*1;
			Debug.Log ("Hit point: " +  TeleportRayHit.point);
		} else {
			transform.position += ShootForwardVector*TeleportDistance;
			Debug.Log ("No Hit, Moved: " + (ShootForwardVector*TeleportDistance).ToString());
		}
	}

	public void UpdateStats() {
		if (!MyStats.IsDead) {	// and has x time passed
			CheckForDeath();

			if (!MyStats.IsDead) {
				MyStats.Regenerate ();	

				// make sounds
				if (WalkingSoundEffect != null) 
				if (Time.time - LastSound > SoundCoolDown) {
					LastSound = Time.time;
					SoundSource.PlayOneShot (WalkingSoundEffect, SoundVolume);
				}
			}
		}
	}

	public void IncreaseKills() {
		TotalKills++;
		if (MyGameManager.MaxKills != 0 && TotalKills >= MyGameManager.MaxKills) {	// win condition
			MyGameManager.EndGame();
		}
	}

	// how the character ressurects
	public void Ressurect(Vector3 NewPosition) {
		MyStats.Ressurect (0.5f);
		transform.position = NewPosition;
		
		//if (MyAnimator != null)
		//	MyAnimator.PlayDefaultAnimation ();
		//else
		//	Debug.Log (name + " has no animations!");
	}
	// if health is 0 or less, kill the player
	// drop the corpse, make the player disabled until respawn timer is up
	// let other players loot the corpse
	public void CheckForDeath() {
		if (MyStats.GetStat ("Health") <= 0 && !MyStats.IsDead) {
			Die();
		}
	}


	public void Die() {
		SoundSource.PlayOneShot (MyDeathSound);
		// drop item
		if (ItemDropPrefab != null) {
			Instantiate (ItemDropPrefab, transform.position, Quaternion.identity);
		}

		MyStats.LivesRemaining--;
		// check if out of lives
		if (MyStats.LivesRemaining <= 0) 
		{
			Destroy(gameObject, DeathAnimationTime);
		}
		// if not add to respawn list
		else 
		{
			GetManager.GetCharacterManager().AddToRespawn(this);	// respawn the character if extra lives
		}
		
		
		MyStats.IsDead = true;
		TotalDeaths++;	// statistics
		
		if (LastHitPlayer) 
		{
			BaseCharacter MyHittingPlayer = LastHitPlayer.GetComponent <BaseCharacter>();
			if (MyHittingPlayer) {
				MyHittingPlayer.IncreaseKills ();	// player who last hits wins!
				MyHittingPlayer.MyStats.IncreaseExperiencePoints (MyStats.MyLevel.Level);
				int GoldIncrease = UnityEngine.Random.Range(0,2) + Mathf.RoundToInt(MyStats.MyLevel.Level)*UnityEngine.Random.Range(1,3);
				if (MasterCharacter) 
				{
					MasterCharacter.GetComponent<BaseCharacter>().Gold += GoldIncrease;
				} else {
					MyHittingPlayer.Gold += GoldIncrease;
				}
				GetManager.GetCharacterManager().GetLocalPlayer().Gold += GoldIncrease;
				CreateTextPopup(gameObject, GoldIncrease, 60, new Color32(255,195,0,255));
			} else {
				Debug.LogError ("Player dead but no hitting player...");
			}
		}
		//DeathAnimationTime = TimeToGetDestroyed;

		int IsExplode = UnityEngine.Random.Range (1, 100);
		bool PewPew = (IsExplode >= 95);
		if (gameObject.GetComponent<ConvertBody> ()) 	// ragdoll body has to be converted anyhow
		{
			float TimeToGetDestroyed = 30f + UnityEngine.Random.Range (0f, 60f);
			gameObject.GetComponent<ConvertBody> ().ConvertBodyParts(PewPew, TimeToGetDestroyed);
		}

		{
			if (gameObject.GetComponent<GetVoxelModel> ()) 
			{
				gameObject.GetComponent<GetVoxelModel> ().Explode();
			}
		}
	}
	public bool IsAlive() {
		if (MyStats.GetStat ("Health") > 0)
			return true;
		else
			return false;
	}

	// :: Taking Hits (to the face) ::
	public bool IsSamePlayer(int HittingPlayerIndex, int HittingClanIndex) {
		if (HittingPlayerIndex == PlayerIndex) {// check which player hits who - or has bounced ? then can shoot self
			return true;
		}
		return false;
	}
	// returns true if they are allies
	public bool IsFriend(int HittingPlayerIndex, int HittingClanIndex) {
		if (MyGameManager.IsFriendlyFire())
			return false;
		// special case as any one of clan 0 is hostile to all
		if (HittingPlayerIndex == 0 || ClanIndex == 0) 
			return false;
		if (!MyGameManager.IsFriendlyFire() && HittingClanIndex != ClanIndex) // for when its not friendly fire, can only hit people of different clans
			return false;
		return true;
	}
	// used to determine alliances
	public bool IsAbleToHit(int HittingPlayerIndex, int HittingClanIndex) {
		// if players are not of the same index (theoritically, all indexes are unique, so it should stop the character hurting themselves)
		// set bulletindex to -1 if you want to burn your face off
		if (HittingPlayerIndex != PlayerIndex) {// check which player hits who - or has bounced ? then can shoot self
			if (MyGameManager.IsFriendlyFire())
				return true;
			if (HittingPlayerIndex == 0 || ClanIndex == 0) // special case as any one of clan 0 is hostile to all
				return true;
			if (!MyGameManager.IsFriendlyFire() && HittingClanIndex != ClanIndex) // for when its not friendly fire, can only hit people of different clans
				return true;
		}
		return false;
	}
	
	void OnTriggerEnter(Collider other) {
		TakeHit(other.gameObject);
	}
	// with physics rigidbody on it
	void OnCollisionEnter(Collision collision) {
		TakeHit(collision.gameObject);
	}

	// by aoe
	public void TakeHitAOE(GameObject HittingObject) {
		TakeHitBase (HittingObject);
	}
	public bool TakeHitBase(GameObject HittingObject) {
		MeshRenderer MyMeshRenderer = GetComponent<MeshRenderer> (); 
		if (!MyStats.IsDead) {
			BaseProjectile BaseProj = (BaseProjectile) HittingObject.gameObject.GetComponent ("BaseProjectile");
			if (BaseProj != null) {
				if (!BaseProj.DoneDamage ())
			//Debug.Log ("Damage: " + BaseProj.Damage + " Is being DELT! pewpew");
				if (!IsSamePlayer (BaseProj.GetPlayerIndex(), BaseProj.GetClanIndex())) { 	// check which player hits who - or has bounced ? then can shoot self
					BaseProj.HasHit ();
					bool IsItFriend = IsFriend (BaseProj.GetPlayerIndex(), BaseProj.GetClanIndex());
					// statistics - i should make visualizers for this
					HitStats HitStat = new HitStats ();
					HitStat.PlayerHitIndex = BaseProj.GetPlayerIndex();
					HitStat.DamageDone = BaseProj.GetDamage();
					HitStat.IsFriend = IsItFriend;
					HitStat.HitTime = Time.time;
					HitList.Add (HitStat);
					// update stats
					if (!(IsItFriend && BaseProj.GetDamage() > 0)) {
						MyStats.IncreaseState ("Health", -BaseProj.GetDamage());
						CreateDamagePopup (BaseProj.GetCharacterThatSpawned(), gameObject, BaseProj.GetDamage());
					}
					// add bullet effect - ie bleeding / burning / frozen

					BaseProj.ApplyOnHitEffects(this);

					LastHitPlayer = BaseProj.GetCharacterThatSpawned();
					// Change combat mode based on being hit
					BotPerception MyMovement = gameObject.GetComponent <BotPerception> ();
					if (MyMovement != null) {
						MyMovement.TakeHit (Time.time, BaseProj.GetCharacterThatSpawned().GetComponent<BaseCharacter> ());
					}

					BeginAnimateColor();	// begin damage animation
					return true;
				}
			} 
		}
			return false;
	}
	
	public bool TakeDamage(GameObject HittingPlayer, int AttackingPlayerIndex, int AttackingClanIndex, float DamageAmount) {
			if (!IsSamePlayer (AttackingPlayerIndex, AttackingClanIndex)) { 	// check which player hits who - or has bounced ? then can shoot self
				// statistics - i should make visualizers for this
				HitStats HitStat = new HitStats ();
				HitStat.PlayerHitIndex = AttackingClanIndex;
				HitStat.DamageDone = DamageAmount;
				HitStat.HitTime = Time.time;
				HitList.Add (HitStat);
				// update stats
			bool IsItFriend = IsFriend(AttackingPlayerIndex, AttackingClanIndex);
			if (!(IsItFriend && DamageAmount > 0)) {
				MyStats.IncreaseState ("Health", -DamageAmount);
				CreateDamagePopup( HittingPlayer,gameObject, DamageAmount);
			}
			LastHitPlayer = HittingPlayer;
			// Change combat mode based on being hit
			BotPerception MyMovement = gameObject.GetComponent <BotPerception>();
			if (MyMovement != null) {
				MyMovement.TakeHit (Time.time, HittingPlayer.GetComponent<BaseCharacter>());
			}
			BeginAnimateColor();	// begin damage animation
			return true;
		}
		return false;
	}
	
	void CreateDamagePopup(GameObject LookAtPlayer, GameObject SpawnPosition, float Damage) {
		CreateDamagePopup(LookAtPlayer, SpawnPosition, Damage, 120);
	}
	void CreateDamagePopup(GameObject LookAtPlayer, GameObject SpawnPosition, float Damage, int FontSize) {
		GameObject NewText = new GameObject();
		NewText.transform.position = SpawnPosition.transform.position;
		NewText.name = "Damage Popup " + Time.time.ToString ();
		TextMesh MyText = (TextMesh) NewText.AddComponent<TextMesh>();
		MyText.text = Damage.ToString();
		MyText.fontStyle = FontStyle.Bold;
		MyText.fontSize = FontSize/4;
		NewText.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		if (Damage > 0) {
			MyText.color = Color.red;
		} else {
			MyText.color = Color.green;
		}
		OnHitText MyOnHit = NewText.AddComponent<OnHitText>();
		MyOnHit.LookAtPlayer = GetManager.GetMainCamera ().gameObject;//LookAtPlayer;
		MyOnHit.PositionAtPlayer = SpawnPosition;
	}
	
	void CreateTextPopup(GameObject SpawnPosition, float Damage, int FontSize, Color32 DamageColor) {
		GameObject NewText = new GameObject();
		NewText.transform.position = SpawnPosition.transform.position;
		NewText.name = "Damage Popup " + Time.time.ToString ();
		TextMesh MyText = (TextMesh) NewText.AddComponent<TextMesh>();
		MyText.text = Damage.ToString();
		MyText.fontStyle = FontStyle.Bold;
		MyText.fontSize = FontSize;
		NewText.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		MyText.color = DamageColor;
		OnHitText MyOnHit = NewText.AddComponent<OnHitText>();
		MyOnHit.LookAtPlayer = GetManager.GetMainCamera ().gameObject;//LookAtPlayer;
		MyOnHit.PositionAtPlayer = SpawnPosition;
	}
	// by bullet
	public void TakeHit(GameObject HittingObject) {
		if (!PickUpItem (HittingObject)) 
		{
			BaseProjectile BaseProj = (BaseProjectile)HittingObject.GetComponent ("BaseProjectile");
			if (BaseProj != null) {
				bool WasHit = TakeHitBase (HittingObject);
				if (WasHit)
					BaseProj.WasHit();
				//if (IsAbleToHit(BaseProj.PlayerIndex, BaseProj.ClanIndex)) //bullet.PlayerIndex != PlayerIndex && 	// check which player hits who - or has bounced ? then can shoot self
			}
		}
	}
	// for when character collides with a pickup item
	public bool PickUpItem(GameObject PickupObject) {
		if (!IsAlive ())
			return false;

		ItemPickup MyPickup = PickupObject.GetComponent<ItemPickup> ();
		if (MyPickup) 
		if (MyPickup.CanPickup()) {
			if (MyPickup.DropType != IconType.None) {
				if (MyPickup.DropType == IconType.Item) {
					// add in the item index of type block - need to update this to take in a whole item data set rather then just type
					MyInventory.AddItem (MyPickup.DropItem);
				}
			}
			// pickup stat increase
			if (MyPickup.IsStatIncrease)
				MyStats.IncreaseState (MyPickup.StatName, MyPickup.StatIncrement);
			// pickup buff
			if (MyPickup.IsAddEffect) {
				MyStats.AddEffect (MyPickup.PickupEffect);
			}
			// pickup sound
			if (SoundSource)
				SoundSource.PlayOneShot (MyPickup.PickUpSound, SoundVolume);
			MyPickup.PickUp ();
			return true;
		}
		return false;
	}

	// ----Commands------
	
	public void CommandBot(int CommandType) {
		Debug.Log (gameObject.name + " is Command Move to.");
		if (SelectedPlayer)
			if (SelectedPlayer.GetComponent<BaseCharacter>().ClanIndex == ClanIndex)	// only command if in same clan
		{
			Vector3 NewMoveToPosition = MouseHit.point + MouseHit.normal * 1.0f;	// spell should be positioned based on its size, outside of the ray hit point
			if (Input.GetKey(KeyCode.LeftShift))
				SelectedPlayer.gameObject.GetComponent<BotMovement>().CommandAddTargetPoint (NewMoveToPosition);
			else if (Input.GetKey(KeyCode.LeftAlt)) {
				if (IsMouseHit && IsHitBlocks)
					SelectedPlayer.gameObject.GetComponent<BotMovement>().CommandMineBlock (MouseHit.point - MouseHit.normal * 0.5f);
			}
			else if (SelectedPlayer.gameObject.GetComponent<BotMovement>())
				SelectedPlayer.gameObject.GetComponent<BotMovement>().CommandMoveToPosition (NewMoveToPosition);
			//else if (SelectedPlayer.gameObject.GetComponent<BotGroundMove>())
			//	SelectedPlayer.gameObject.GetComponent<BotGroundMove>().MoveToPosition (NewMoveToPosition);
		}
	}

}
