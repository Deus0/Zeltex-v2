using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using DialogueSystem;
using GUI3D;
using GuiSystem;
using MyCustomDrawers;
using ItemSystem;

namespace CharacterSystem {
	[ExecuteInEditMode]
	public class CharacterPrefabLoader : MonoBehaviour {		
		[Header("Prefab Loading")]
		[Tooltip("Reloads the character prefabs.")]
		[SerializeField] bool IsReloadCharacter;
		[Tooltip("Spawns a character and links up the prefabs together")]
		[HideInInspector] [SerializeField] bool IsLoadCharacter;
		[Tooltip("Removes character.")]
		[HideInInspector] [SerializeField] bool IsDeleteCharacter;
		[Tooltip("Saves Character as a prefab")]
		[HideInInspector] [SerializeField] bool IsSaveAsAsset;
		
		[Header("Prefabs")]
		public GameObject CharacterPrefab;
		public GameObject DialoguePrefab;
		public GameObject InventoryPrefab;
		public GameObject LabelPrefab;
		public GameObject QuestLogPrefab;
		public GameObject StatsPrefab;
		public GameObject CrosshairPrefab;
		public GameObject ClockGuiPrefab;
		public GameObject LogPrefab;
		public GameObject SkillsPrefab;

		// utility
		public static void DestroyThing(GameObject ObjectToDestroy) {
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
				Destroy (ObjectToDestroy);
			else
				DestroyImmediate (ObjectToDestroy);
			#else
			Destroy (ObjectToDestroy);
			#endif
		}
		public static void DestroyThing(Component ObjectToDestroy) {
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
				Destroy (ObjectToDestroy);
			else
				DestroyImmediate (ObjectToDestroy);
			#else
			Destroy (ObjectToDestroy);
			#endif
		}

		private GameObject MyCharacterSpawn;
		
		public static GameObject SpawnCharacter() 
		{
			return SpawnCharacter ("New_Character", new Vector3(), Quaternion.identity);
		}
		public static GameObject SpawnCharacter(string Name) 
		{
			return SpawnCharacter (Name, new Vector3(), Quaternion.identity);
		}
		public static GameObject SpawnCharacter(string Name, Vector3 Position) 
		{
			return SpawnCharacter (Name, Position, Quaternion.identity);
		}
		public static GameObject GetDefaultBodyPrefab() {
			return  Resources.Load ("Prefabs/Player") as GameObject;
		}

		public static void ConvertToPlayer(GameObject NewCharacter)
		{
			AddPlayerComponents(NewCharacter);
			AddGuis (NewCharacter, true);
		}
		public static void AddPlayerComponents(GameObject MyPlayer) 
		{	
			// if not a player
			if (MyPlayer.GetComponent<Player> () == null) 
			{
				MyPlayer.AddComponent<Player> ();	// used for key input
				MyPlayer.AddComponent<TestMovement> ();	// used for key input
				CombatSystem.Shooter MyShooter = MyPlayer.AddComponent<CombatSystem.Shooter> ();
				MyShooter.HotSpotTransform = Camera.main.transform;
				VoxelEngine.VoxelBrush MyBrush = MyPlayer.AddComponent<VoxelEngine.VoxelBrush> ();
				MyBrush.enabled = false;
			}
		}
		public static void RemovePlayerComponents(GameObject NewCharacter)
		{
			if (NewCharacter.GetComponent<CharacterSystem.Player> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<CharacterSystem.Player> ());
			if (NewCharacter.GetComponent<TestMovement> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<TestMovement> ());

			if (NewCharacter.GetComponent<CharacterSystem.CharacterSaver> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<CharacterSystem.CharacterSaver> ());
			if (NewCharacter.GetComponent<CombatSystem.Shooter> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<CombatSystem.Shooter> ());
			if (NewCharacter.GetComponent<VoxelEngine.VoxelBrush> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<VoxelEngine.VoxelBrush> ());
			if (NewCharacter.GetComponent<QuestSystem.QuestHelper> ())
				CharacterPrefabLoader.DestroyThing  (NewCharacter.GetComponent<QuestSystem.QuestHelper> ());
		}
		public static void AddGuis(GameObject NewCharacter, bool IsPlayer)
		{
			// now add guis
			if (NewCharacter.transform.FindChild ("Label") == null)
			{
				string GuiPrefabName = "Prefabs/CharacterGuis/Label";
				GameObject LabelPrefab = Resources.Load ("Prefabs/CharacterGuis/Label") as GameObject;
				SpawnLabel (NewCharacter.transform, LabelPrefab, GuiPrefabName);
			}
			if (NewCharacter.transform.FindChild ("Dialogue") == null) 
			{
				string GuiPrefabName = "Prefabs/CharacterGuis/DialoguePrefab";
				GameObject DialoguePrefab = Resources.Load ("Prefabs/CharacterGuis/DialoguePrefab") as GameObject;
				SpawnDialogue (NewCharacter.transform, DialoguePrefab, GuiPrefabName);
			}
			if (IsPlayer) 
			{
				if (NewCharacter.transform.FindChild ("SkillBar") == null)
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/Skillbar";
					GameObject SkillsPrefab = Resources.Load ("Prefabs/CharacterGuis/Skillbar") as GameObject;
					SpawnSkillsBar (NewCharacter.transform, SkillsPrefab, GuiPrefabName);
				}
				if (NewCharacter.transform.FindChild ("Stats") == null)
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/Stats";
					GameObject StatsPrefab = Resources.Load (GuiPrefabName) as GameObject;
					SpawnStats (NewCharacter.transform, StatsPrefab, GuiPrefabName);
				}
				if (NewCharacter.transform.FindChild ("Inventory") == null) 
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/Inventory";
					GameObject InventoryPrefab = Resources.Load (GuiPrefabName) as GameObject;
					SpawnInventory (NewCharacter.transform, InventoryPrefab, GuiPrefabName);
				}
				if (NewCharacter.transform.FindChild ("QuestLog") == null) 
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/QuestLog";
					GameObject QuestLogPrefab = Resources.Load (GuiPrefabName) as GameObject;
					SpawnQuestLog (NewCharacter.transform, QuestLogPrefab, GuiPrefabName);
				}
				if (NewCharacter.transform.FindChild ("Log") == null) 
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/Log";
					GameObject LogPrefab = Resources.Load ("Prefabs/CharacterGuis/Log") as GameObject;
					SpawnLogGui (NewCharacter.transform, LogPrefab, GuiPrefabName);
				}
				if (NewCharacter.transform.FindChild ("Crosshair") == null) 
				{
					string GuiPrefabName = "Prefabs/CharacterGuis/Crosshair";
					GameObject CrosshairPrefab = Resources.Load ("Prefabs/CharacterGuis/Crosshair") as GameObject;
					SpawnCrosshair (NewCharacter.transform, CrosshairPrefab, GuiPrefabName);
				}
			}
			GuiManager MyManager = NewCharacter.GetComponent<GuiManager> ();
			if (MyManager) 
			{
				MyManager.TogglePlayerControls (IsPlayer);
				MyManager.DisableAllGuis();	// hide them all
			}
		}
		// default player spawn
		public static GameObject SpawnCharacter(string Name, Vector3 Position, Quaternion Rotation) 
		{
			GameObject MyBodyPrefab = GetDefaultBodyPrefab ();
			GameObject NewCharacter = SpawnBody (MyBodyPrefab, Name, Position, Rotation);
			RemovePlayerComponents (NewCharacter);
			AddGuis (NewCharacter, false);
			// destroy any previous guis
			/*for (int i = NewCharacter.transform.childCount-1; i >= 0 ; i--) 
			{
				if (NewCharacter.transform.GetChild(i).GetComponent<Toggler>())
				{
					CharacterPrefabLoader.DestroyThing (NewCharacter.transform.GetChild(i).gameObject);
				}
			}*/
			return NewCharacter;
		}
		
		// Update is called once per frame
		void Update () {
			UpdatePrefabActions ();
			if (IsLoadCharacter) {
				IsLoadCharacter = false;
				DeleteCharacter ();
				LoadCharacter ();
			}
		}
		private void UpdatePrefabActions() {
			if (IsReloadCharacter) 
			{
				IsReloadCharacter = false;
				IsLoadCharacter = true;
				IsDeleteCharacter = true;
			}
			if (IsDeleteCharacter) 
			{
				IsDeleteCharacter = false;
				DeleteCharacter();
			}
			if (IsLoadCharacter) 
			{
				IsLoadCharacter = false;
				LoadCharacter();
			}
			if (IsSaveAsAsset) {
				IsSaveAsAsset = false;
				/*#if UNITY_EDITOR
				string MySaveFolder = SaveFileName.Substring(0, SaveFileName.Length-4);	// remove .txt
				string MySavePath = MySaveFolder + "_Prefab.prefab";
				int IndexOf = MySavePath.IndexOf("Assets/");
				MySavePath = MySavePath.Remove(0, IndexOf);
				//MySavePath = MySavePath.Remove(0, "Assets/".Length+1);
				Debug.LogError("Saving " + name + " as a prefab in " + MySavePath);
				//UnityEditor.PrefabUtility.CreateEmptyPrefab("Assets/Resources/Characters/Test.prefab");
				//UnityEditor.PrefabUtility.CreateEmptyPrefab(MySavePath);
				UnityEditor.PrefabUtility.CreatePrefab(MySavePath, gameObject);
				//UnityEditor.AssetDatabase.CreateAsset(gameObject,  UnityEditor.AssetDatabase.GenerateUniqueAssetPath(MySavePath));
				//UnityEditor.AssetDatabase.SaveAssets();
				#endif*/
			}
		}
		public void DeleteCharacter() {
			for (int i = transform.childCount-1; i >= 0; i--) {
				DestroyImmediate (transform.GetChild(i).gameObject);
			}
		}
		public void LoadCharacter() {
			/*SpawnBody();
			SpawnLabel(transform, LabelPrefab);
			SpawnDialogue(transform, DialoguePrefab);
			SpawnInventory(transform, InventoryPrefab);
			SpawnStats(transform, StatsPrefab);
			SpawnQuestLog(transform, QuestLogPrefab);
			SpawnCrosshair(transform, CrosshairPrefab);
			if (ClockGuiPrefab) 
			{
				GameObject MyClockSpawn = SpawnNewGui(transform, ClockGuiPrefab);
			}
			SpawnLogGui (transform, LogPrefab);
			SpawnSkillsBar (transform, SkillsPrefab);*/
		}

		private void SpawnBody()
		{
			if (CharacterPrefab == null) 
			{
				CharacterPrefab = GetDefaultBodyPrefab ();
			}
			MyCharacterSpawn = SpawnBody (CharacterPrefab, name, transform.position, transform.rotation);
		}
		private static GameObject SpawnBody(GameObject CharacterPrefab, string Name, Vector3 Position, Quaternion Rotation)
		{
			GameObject MyCharacterSpawn = (GameObject) Instantiate(CharacterPrefab, Position, Rotation);
			//MyCharacterSpawn.transform.SetParent(transform);
			//if (!name.Contains ("Spawner"))
			//	name += "Spawner";
			//MyCharacterSpawn.name = name;
			MyCharacterSpawn.name = Name;
			//MyCharacterSpawn.name = MyCharacterSpawn.name.Replace ("Spawner", "");
			GuiSystem.GuiManager MyGuiManager =  MyCharacterSpawn.GetComponent<GuiSystem.GuiManager> ();
			MyGuiManager.ClearTogglers ();
			// Handlers
			// Stats will be used by:
			// Star Bar Manager - When it changes states
			// Stats Gui List - When updating the stats
			// Log Book - when gaining new stats

			CharacterStats MyStats = MyCharacterSpawn.GetComponent <CharacterStats> ();
			MyStats.OnNewStats = new UnityEvent ();
			MyStats.OnUpdateStats = new UnityEvent ();
			MyStats.OnDeath = new UnityEvent ();
			#if UNITY_EDITOR
				UnityEditor.Events.UnityEventTools.AddPersistentListener(MyStats.OnDeath,
			                                                         MyCharacterSpawn.GetComponent<Character>().OnDeath);
				UnityEditor.Events.UnityEventTools.AddPersistentListener(MyStats.OnDeath,
			                                                         // MyCharacterSpawn.transform.GetChild(0).GetComponent<AnimationUtilities.Ragdoll>().RagDoll);
			                                                         MyCharacterSpawn.transform.GetChild(0).GetComponent<AnimationUtilities.Ragdoll>().Explode);
			#else
				MyStats.OnDeath.AddListener(delegate{ MyCharacterSpawn.GetComponent<Character>().OnDeath();});
				MyStats.OnDeath.AddListener(delegate{ MyCharacterSpawn.transform.GetChild(0).GetComponent<AnimationUtilities.Ragdoll>().Explode();});
			#endif
			
			
			// Inventory
			// Log - for when picking up, dropping, trading items
			// Inventory Gui - when exchanging value or items - or picking up items - need to update that gui
			
			Inventory MyInventory = MyCharacterSpawn.GetComponent <Inventory> ();
			MyInventory.OnAddItem = new UnityEvent ();
			#if UNITY_EDITOR
			UnityEditor.Events.UnityEventTools.AddPersistentListener(MyInventory.OnAddItem,
			                                                         MyStats.UpdateState);
			#else
			MyInventory.OnAddItem.AddListener(delegate{ MyStats.UpdateState();});
			#endif
			
			// SpeechHandler
			// Log - When talking to a new character
			SpeechHandler MySpeech = MyCharacterSpawn.GetComponent <SpeechHandler> ();
			
			// Quest Log
			// QuestHelper - Refresh when When new quest added, When Completed a Quest
			QuestSystem.QuestLog MyQuestLog = MyCharacterSpawn.GetComponent <QuestSystem.QuestLog> ();
			Debug.LogError ("Adding OnAddItem for questlog");
			#if UNITY_EDITOR
			UnityEditor.Events.UnityEventTools.AddPersistentListener(MyInventory.OnAddItem,
			                                                         MyQuestLog.OnAddItem);
			#else
			MyInventory.OnAddItem.AddListener(delegate{ MyQuestLog.OnAddItem();});
			#endif
			
			// Log
			// On Add Log, refresh gui
			return MyCharacterSpawn;
		}
		public static GameObject SpawnNewGui(Transform MyParent, GameObject GuiPrefab, string GuiPrefabName) 
		{
			if (GuiPrefab && MyParent) 
			{
				GameObject NewGui;
				if (PhotonNetwork.connected) {
					NewGui = (GameObject)PhotonNetwork.Instantiate (GuiPrefabName, MyParent.position, MyParent.rotation, 0);
				} else {
					NewGui = (GameObject)Instantiate (GuiPrefab, MyParent.position, MyParent.rotation);
				}
				NewGui.transform.SetParent (MyParent);
				NewGui.name = GuiPrefab.name;
				
				Toggler MyToggler = NewGui.GetComponent<Toggler> ();
				if (MyToggler) 
				{
					MyToggler.MyToggleKey = KeyCode.None;
					MyToggler.TargetCharacter = MyParent.gameObject;
					GuiSystem.GuiManager MyGuiManager =  MyParent.GetComponent<GuiSystem.GuiManager> ();
					MyGuiManager.AddToggler(MyToggler);
					
					// set up the close button
					for (int i = 0; i < NewGui.transform.childCount; i++) {
						if (NewGui.transform.GetChild(i).name.Contains("CloseButton")) 
						{
							Button MyCloseButton = NewGui.transform.GetChild(i).GetComponent<Button>();
							MyCloseButton.onClick = new Button.ButtonClickedEvent();
							#if UNITY_EDITOR
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MyCloseButton.onClick,
							                                                         MyToggler.TurnOff);
							//if (!IsPlayer)
							{
							//	UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyCloseButton.onClick,
							//	                                                               MyGuiManager.SwitchMode,
							//	                                                               "Label");
							}
							#endif
							i = NewGui.transform.childCount;
						}
					}
				}
				// link these to the main body
				Orbitor MyOrbitor = NewGui.GetComponent<Orbitor> ();
				if (MyOrbitor) {
					if (MyParent.FindChild("CameraBone"))
						MyOrbitor.Target = MyParent.FindChild("CameraBone").gameObject;
					//MyOrbitor.IsFollowMainCamera = IsPlayer;
					MyOrbitor.IsFollowUserAngleAddition = true;
					/*if (IsPlayer)
						MyOrbitor.TargetOffset = new Vector3(0,0,0);
					else
						MyOrbitor.TargetOffset = new Vector3(0,0.5f,0);*/
				}
				Billboard MyBillboard = NewGui.GetComponent<Billboard> ();
				if (MyBillboard) {
					MyBillboard.TargetCharacter = MyParent.FindChild("CameraBone").gameObject;
				}
				Follower MyFollower = NewGui.GetComponent<Follower> ();
				if (MyFollower) {
					MyFollower.UpdateTarget(MyParent);
				}
				//if (IsPlayer) {
				//	NewGui.transform.localScale = NewGui.transform.localScale*0.5f;
				//}
				return NewGui;
			}
			return null;
		}
		
		// Add any handler functions that involve updating the gui list
		public static void SpawnQuestLog(Transform MyParent, GameObject QuestLogPrefab, string GuiPrefabName) 
		{
			if (MyParent && QuestLogPrefab) 
			{
				GameObject NewQuestLog = SpawnNewGui(MyParent, QuestLogPrefab, GuiPrefabName);
				NewQuestLog.name = "QuestLog";
				//if (!IsPlayer)
				//{
					//Orbitor MyOrbitor = NewQuestLog.GetComponent<Orbitor> ();
					//MyOrbitor.TargetCharacter2 = MyCharacterSpawn.transform.GetChild(0).FindChild("CameraBone").gameObject;	// Camera.main.gameObject;
					//MyOrbitor.TargetOffset = new Vector3(0, 0.6f, 0);
				//}
				// link toggler to gui manager
				for (int i = 0; i < NewQuestLog.transform.childCount; i++) {
					QuestLogGuiHandler MyQuestLogGuiHandler = NewQuestLog.transform.GetChild(i).GetComponent<QuestLogGuiHandler>();
					if (MyQuestLogGuiHandler)
					{
						MyQuestLogGuiHandler.MyCharacter = MyParent.gameObject;
						QuestSystem.QuestLog MyQuestLog = MyParent.GetComponent<QuestSystem.QuestLog>();
						
						#if UNITY_EDITOR
						UnityEditor.Events.UnityEventTools.AddPersistentListener(MyQuestLog.OnAddQuest,
						                                                         MyQuestLogGuiHandler.UpdateQuestGuis
						                                                         );
						QuestSystem.QuestHelper MyQuestHelper = MyParent.GetComponent<QuestSystem.QuestHelper>();
						/*if (MyQuestHelper) {
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MyQuestLog.OnAddQuest,
							                                                         MyQuestHelper.FindCurrentQuestTarget
							                                                         );
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MyQuestLog.OnCompletedQuest,
							                                                         MyQuestHelper.FindCurrentQuestTarget
							                                                         );
						}*/
						#endif
						i = NewQuestLog.transform.childCount;
					}
				}
			}
		}
		
		// only add listeners that involve updating the stats gui
		private static void SpawnStats(Transform MyParent, GameObject StatsPrefab, string GuiPrefabName) 
		{
			if (MyParent && StatsPrefab) {
				GameObject NewStatsGui = SpawnNewGui(MyParent, StatsPrefab, GuiPrefabName);
				
				if (NewStatsGui.transform.FindChild("StatsList")) {
					StatGuiHandler MyStatsGuiHandler = NewStatsGui.transform.FindChild("StatsList").GetComponent<StatGuiHandler>();
					MyStatsGuiHandler.MyCharacter = MyParent.gameObject;
					//GuiSystem.GuiManager MyGuiManager =  MyCharacterSpawn.GetComponent<GuiSystem.GuiManager> ();
					#if UNITY_EDITOR
					CharacterStats MyStats = MyParent.GetComponent<CharacterStats>();
					if (MyStats) {
						//UnityEditor.Events.UnityEventTools.AddPersistentListener(MyCharacterSpawn.GetComponent<ItemSystem.Inventory>().OnAddItem,
						//                                                         MyStats.UpdateState);
						
						UnityEditor.Events.UnityEventTools.AddPersistentListener(MyStats.OnUpdateStats,
						                                                         MyStatsGuiHandler.UpdateGuiStats
						                                                         );
					}
					#else
					// put delegates here
					#endif
				}
			}
		}
		private static void SpawnCrosshair(Transform MyParent, GameObject CrosshairPrefab, string GuiPrefabName) 
		{
			if (MyParent && CrosshairPrefab) 
			{
				GameObject NewCrosshair = SpawnNewGui(MyParent.transform, CrosshairPrefab, GuiPrefabName);
			}
		}
		public static void SpawnLabel(Transform MyParent, GameObject LabelPrefab, string GuiPrefabName) {
			if (MyParent && LabelPrefab) 
			{
				GameObject MyLabelSpawn = SpawnNewGui(MyParent, LabelPrefab, GuiPrefabName);
				Billboard MyBillBoard = MyLabelSpawn.GetComponent<Billboard>();
				if (MyBillBoard) 
				{
					MyBillBoard.IsLookAtPlayer = true;
				}
				StatBarManager MyStatBarManager = MyLabelSpawn.GetComponent<StatBarManager>();
				MyStatBarManager.TargetCharacter = MyParent.gameObject;
				CharacterStats MyStats = MyParent.GetComponent<CharacterStats>();
				if (MyStats) {
					#if UNITY_EDITOR
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyStats.OnUpdateStats,
					                                                         MyStatBarManager.OnUpdateStats
					                                                         );
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyStats.OnNewStats,
					                                                         MyStatBarManager.OnNewStats
					                                                         );
					#else
					// delegates
					MyStats.OnUpdateStats.AddListener(delegate{MyStatBarManager.OnUpdateStats();});
					MyStats.OnNewStats.AddListener(delegate{MyStatBarManager.OnNewStats();});
					#endif
				}
			}
		} 
		
		public static void SpawnInventory(Transform MyParent, GameObject InventoryPrefab, string GuiPrefabName) {
			if (MyParent && InventoryPrefab) {
				GameObject MyInventorySpawn = SpawnNewGui(MyParent, InventoryPrefab, GuiPrefabName);
				MyInventorySpawn.name = "Inventory";
				
				/*if (!IsPlayer) 
				{
					Orbitor MyOrbitor = MyInventorySpawn.GetComponent<Orbitor> ();
					MyOrbitor.TargetCharacter2 = Camera.main.gameObject;
					//MyOrbitor.TargetOffset = new Vector3(0, 0.6f, 0);
				}*/
				
				if (MyInventorySpawn.transform.FindChild("ItemsList")) {
					InventoryGuiHandler MyInventoryGuiHandler = MyInventorySpawn.transform.FindChild("ItemsList").GetComponent<InventoryGuiHandler>();
					MyInventoryGuiHandler.MyCharacter = MyParent.GetComponent<Character>();
					MyInventoryGuiHandler.MyInventory = MyParent.GetComponent<ItemSystem.Inventory>();
					GuiSystem.GuiManager MyGuiManager =  MyParent.GetComponent<GuiSystem.GuiManager> ();
					#if UNITY_EDITOR
					//MyCharacterSpawn.GetComponent<ItemSystem.Inventory>().OnAddItem = new UnityEvent();
					MyParent.GetComponent<ItemSystem.Inventory>().OnExchangeCurrency = new UnityEvent();
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyParent.GetComponent<ItemSystem.Inventory>().OnAddItem,
					                                                         MyInventoryGuiHandler.UpdateInventoryGui
					                                                         );
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyParent.GetComponent<ItemSystem.Inventory>().OnExchangeCurrency,
					                                                         MyInventoryGuiHandler.UpdateValueText
					                                                         );
					#endif
				}
			}
		}
		
		public static void SpawnDialogue(Transform MyParent, GameObject DialoguePrefab, string GuiPrefabName)
		{
			if (MyParent && DialoguePrefab) {
				GameObject MyDialogueSpawn = SpawnNewGui(MyParent, DialoguePrefab, GuiPrefabName);
				MyDialogueSpawn.name = "Dialogue";
				
				SpeechHandler MySpeech = MyParent.GetComponent<SpeechHandler> ();
				if (MyDialogueSpawn.transform.FindChild("SpeechText")) {
					MySpeech.MyDialogueText = MyDialogueSpawn.transform.FindChild("SpeechText").GetComponent<Text> ();
				}
				
				GuiSystem.GuiManager MyGuiManager =  MyParent.GetComponent<GuiSystem.GuiManager> ();
				if ( MyDialogueSpawn.transform.FindChild("CloseButton")) {
					Button MyCloseButton = MyDialogueSpawn.transform.FindChild("CloseButton").GetComponent<Button>();
					#if UNITY_EDITOR
					/*UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyCloseButton.onClick,
				                                                               MyGuiManager.SwitchMode,
					                                                               "Label");*/
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyCloseButton.onClick,
					                                                         MySpeech.ExitChat);
					#else
					//delegates here
					MyCloseButton.onClick.AddListener(delegate{MySpeech.ExitChat();});
					#endif
				}
			}
		}

		public static void SpawnSkillsBar(Transform MyParent, GameObject SkillsPrefab, string GuiPrefabName) {
			if (MyParent && SkillsPrefab) {
				GameObject MySpawn = SpawnNewGui(MyParent, SkillsPrefab, GuiPrefabName);

				if (MySpawn.transform.FindChild("ItemsList")) {
					InventoryGuiHandler MyInventoryGuiHandler = MySpawn.transform.FindChild("ItemsList").GetComponent<InventoryGuiHandler>();
					MyInventoryGuiHandler.MyCharacter = MyParent.GetComponent<Character>();
					MyInventoryGuiHandler.MyInventory = MyParent.GetComponent<ItemSystem.Inventory>();
					GuiSystem.GuiManager MyGuiManager =  MyParent.GetComponent<GuiSystem.GuiManager> ();
					#if UNITY_EDITOR
					//MyCharacterSpawn.GetComponent<ItemSystem.Inventory>().OnAddItem = new UnityEvent();
					MyParent.GetComponent<ItemSystem.Inventory>().OnExchangeCurrency = new UnityEvent();
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyParent.GetComponent<ItemSystem.Inventory>().OnAddItem,
					                                                         MyInventoryGuiHandler.UpdateInventoryGui
					                                                         );
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyParent.GetComponent<ItemSystem.Inventory>().OnExchangeCurrency,
					                                                         MyInventoryGuiHandler.UpdateValueText
					                                                         );
					#endif
				}
			}
		}

		public static void SpawnLogGui(Transform MyParent, GameObject LogPrefab, string GuiPrefabName) 
		{
			if (MyParent && LogPrefab) {
				GameObject LogGuiSpawn = SpawnNewGui(MyParent, LogPrefab, GuiPrefabName);
					
					Log MyLog = MyParent.GetComponent<Log>();
					
					if (MyLog) 
					{
						#if UNITY_EDITOR
						if (LogGuiSpawn.transform.FindChild("LogList")) 
						{
							GuiListHandler MyLogGuiList = LogGuiSpawn.transform.FindChild("LogList").GetComponent<GuiListHandler>();
							if (MyLogGuiList) {
								MyLog.OnAddLogString = new CustomEvents.MyEventString();
								UnityEditor.Events.UnityEventTools.AddPersistentListener(MyLog.OnAddLogString,
								                                                         MyLogGuiList.AddGui);
							}
						}
						//UnityEditor.Events.UnityEventTools.
						Inventory MyInventory = MyParent.GetComponent<Inventory>();
						if (MyInventory) 
						{
							MyInventory.OnPickupItem = new CustomEvents.MyEvent3();
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MyInventory.OnPickupItem,
							                                                         MyLog.AddLogEvent);
							MyInventory.OnExchangeItem = new CustomEvents.MyEvent3();
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MyInventory.OnExchangeItem,
							                                                         MyLog.AddLogEvent);
						}
						SpeechHandler MySpeech = MyParent.GetComponent<SpeechHandler>();
						if (MySpeech) 
						{
							MySpeech.OnBeginTalkTo = new CustomEvents.MyEvent3();
							UnityEditor.Events.UnityEventTools.AddPersistentListener(MySpeech.OnBeginTalkTo,
							                                                         MyLog.AddLogEvent);
						}
						#endif
				}
			}
		}
	}
}