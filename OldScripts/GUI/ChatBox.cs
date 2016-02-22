using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace OldCode {
public class Command {
	public int FunctionID;
	public string CommandText;

	public Command(int FunctionID_, string CommandText_) {
		FunctionID = FunctionID_;
		CommandText = CommandText_;
	}

};

public class ChatBox : MonoBehaviour {
	public Text ChatBoxText;
	public List<string> ChatTexts = new List<string> ();
	public int MaxLines = 3;
	public InputField ChatInput;
	public MonsterSpawner MyMonsterSpawner;
	public List<Command> Commands = new List<Command> ();


	// Use this for initialization
	void Start () {
		SetDefaultCommands ();
		//UpdateChatBox ();
		UpdateChatBoxText ();
		ChatInput.interactable = false;
	}

	void SetDefaultCommands() {
		AddNewCommand("help");
		AddNewCommand("monster");
		AddNewCommand("restart");
		AddNewCommand("saveplayer");		// selects item, then you can edit individual properties using commands
		AddNewCommand("loadplayer");
		AddNewCommand ("deleteplayer");
		AddNewCommand ("changename");
		AddNewCommand ("save");
		AddNewCommand ("load");
		AddNewCommand ("deleteworld");
		AddNewCommand ("loadgame");
		AddNewCommand ("savegame");
		AddNewCommand ("unstuck");
		AddNewCommand ("testfps");
		AddNewCommand ("playgame");
		AddNewCommand ("teleport");
		AddNewCommand ("unstuck");
		AddNewCommand ("debug");
		AddNewCommand ("modelname");
		AddNewCommand ("day");
		AddNewCommand ("night");
		// inventory
		AddNewCommand("giveitem");
		AddNewCommand("givespell");
		AddNewCommand("newitem");
		AddNewCommand("newspell");
		AddNewCommand("edititem");		// selects item, then you can edit individual properties using commands
		AddNewCommand("editspell");
		AddNewCommand ("editspells");	// oppens up spell editor
		AddNewCommand ("allitems");	// oppens up spell editor
	}
	public void AddNewCommand(string NewCommand) {
		Commands.Add (new Command (Commands.Count, NewCommand));
	}
	void Awake() {
		ChatTexts.Clear ();																																																				
		ChatTexts.Add ("Welcome to Zeltex!");
		ChatTexts.Add ("This is release version 0.1");
		ChatTexts.Add ("Enjoy your stay :3");
	}
	public void CheckForCommand(string NewLine) {
		string SecondCommandWord = "";
		int index = NewLine.IndexOf(" ")+1;
		if (index < 0)
			Debug.LogError ("No Spaces?"); 
		else
			SecondCommandWord = NewLine.Substring(index, NewLine.Length-index);

		BaseCharacter MyCharacter = GetManager.GetCharacterManager ().GetLocalPlayer ();
		if (NewLine [0] == '/') {
			for (int i = 0; i < Commands.Count; i++) {
				if (NewLine.Contains (Commands[i].CommandText)) {
					EnterText("Enabling Command: " + Commands[i].CommandText);


					switch(Commands[i].CommandText) {
					case "help":	
						for (int j = 0; j < Commands.Count; j++) {
							EnterText("Command " + j + ": " + Commands[j].CommandText);
						}
						break;
					case "monster":	
						//SecondCommandWord - find monster with name <><
						int MyMonsterIndex = Random.Range (0,MyMonsterSpawner.MonsterList.Count);
						MyMonsterSpawner.SpawnMonster (MyMonsterIndex, MyCharacter.transform.position + MyCharacter.ShootForwardVector*5);
						break;
					case "restart":
						Application.LoadLevel(Application.loadedLevelName);
						break;
					case "changename":
						//GetManager.GetCharacterManager().GetLocalPlayer().gameObject.GetComponent<Player>().ToggleMenuSystem();
						//GetManager.GetGameManager().SetCanToggleMenu(false);	// disables toggling of menu screen, this doesnt work rn
						//GetManager.GetGuiCreator().CreateInputWindow ("What is your name?");
						break;
					case "saveplayer":
						GetManager.GetCharacterManager().GetLocalPlayer().gameObject.GetComponent<CharacterSaver>().Save(GetManager.GetGameManager().GameName);
						break;
					case "loadplayer":
						GetManager.GetCharacterManager().GetLocalPlayer().gameObject.GetComponent<CharacterSaver>().Load(GetManager.GetGameManager().GameName);
						break;
					case "savegame":
						GetManager.SaveAllChunks();
						break;
					case "loadgame":
						GetManager.LoadAllChunks();
						break;
					case "save":
						GetManager.GetCharacterManager().GetLocalPlayer().gameObject.GetComponent<CharacterSaver>().Save(GetManager.GetGameManager().GameName);
						GetManager.SaveAllChunks();
						break;
					case "load":
						GetManager.GetCharacterManager().GetLocalPlayer().gameObject.GetComponent<CharacterSaver>().Load(GetManager.GetGameManager().GameName);
						GetManager.LoadAllChunks();
						break;
					case "deleteworld":
						break;
					case "deleteplayer":
						GetManager.GetGameManager().DeleteLocalCharacter();
						break;
					case "testfps":
						// maybe load a blood assault map here, with respawning and item pickups
						break;
					case "playgame":
						GetManager.GetGameManager().StartGame();
						break;
					case "teleport":
					case "unstuck":
						GetManager.GetCharacterManager().GetLocalPlayer().transform.position = GetManager.GetCharacterManager().SpawnPosition;
						break;
					case "debug":
						GetManager.GetGuiManager().DebugScreen.SetActive(true);
						break;
					case "modelname":
						{
						string OldName = GameObject.Find ("MyModelEditorManager").GetComponent<ModelEditorManager>().GetSelectedBlockStructure().Name;
						GameObject.Find ("MyModelEditorManager").GetComponent<ModelEditorManager>().UpdateName (SecondCommandWord);
						EnterText("Changed Model " + OldName + " _to_ " + SecondCommandWord);
						}
						break;
					case "day":
						GameObject.Find ("DayCycleManager").GetComponent<SunCycle>().DayTime();
						break;
					case "night":
						Debug.LogError("Attempting to make darkness");
						GameObject MyDayManager = GameObject.Find ("DayCycleManager");
						MyDayManager.GetComponent<SunCycle>().NightTime();
						break;
					case "editspells":
						GameObject MySpellManager = GameObject.Find ("MySpellsManager");
						MySpellManager.GetComponent<SpellManager>().ToggleDebugSpellData();
						break;
					case "allitems":
						ItemGenerator MyItemGen = GetManager.GetCharacterManager().GetLocalPlayer().gameObject.AddComponent<ItemGenerator>();
						MyItemGen.IsGenerateAllItems = true;
						MyItemGen.GenerateInventoryData();
						break;
					}
			}
			}
		}
	}
	public void EnterText(string NewChatLine) {
		ChatTexts.Add (NewChatLine);
		UpdateChatBoxText ();
		CheckForCommand (NewChatLine);
	}
	void UpdateChatBoxText() {
		if (ChatBoxText != null) {
			// first reset the chatbox's text
			ChatBoxText.text = "";
			// now for all our lines, add them
			for (int i = MaxLines-1; i >= 0; i--) {
				//if (i >= MaxLines)
				//	i = ChatTexts.Count;
				if (i < ChatTexts.Count && ChatTexts.Count - i - 1 >= 0) {
					ChatBoxText.text += ChatTexts [ChatTexts.Count - i - 1] + "\n";
				} else
					ChatBoxText.text += "\n";
			}
		} else {
			Debug.LogError ("No chat text linked up in chatbox script...");
		}
	}
	// adds the input text to the chatbox - later i will add channels
	// set to activate when player presses enter
	void UpdateChatBox() {
		DisableChatBox ();	// disable whether text was typed or not
		if (ChatInput.text != "") {
			string NewChatLine = ChatInput.text;
			ChatInput.text = "";
			ChatTexts.Add (NewChatLine);
			UpdateChatBoxText ();
			CheckForCommand (NewChatLine);
		}
	}
	// enables mouse input
	public void EnableChatBox() {
		ChatInput.interactable = true;
		ChatInput.Select();
		GetManager.GetCameraManager().EnableMouseGui();
	}
	// disables the chatbox input - and enables mouse fps against
	public void DisableChatBox() {
		ChatInput.interactable = false;
		GetManager.GetCameraManager().EnableMouseFPS();
	}
	public bool IsEnabled() {
		if (ChatInput == null)
			return false;
		return ChatInput.interactable;
	}
}
}
