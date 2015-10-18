using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// GUI Manager class
// Description:
// 		Mainly handles the data of the GUI, and what is on and off
//		GUICreator class will create windows, and this will manaage whats open and closed

//	*Need to rewrite this

public enum ScreenType {
	PreGame,
	InGame,
	EndGame,
	MenuSystem
};


// Used to store the item's GUI data
// Items store an image and text, so they are stackable
// They also store a cooldown animation, for when the items are activated!
// At the top of their structure is their RawImage, can be used for toggling items on and off, or stretching the items scale

[System.Serializable]
public class MyItemGui {
	public RawImage ItemImage;				// icons image, normally a 64x64 pixel art drawing
	public Text ItemText;					// used to display how many items are stacked, its disabled if the max stackable item is 1
	public GameObject ItemCoolDownAnimation;// This is used to show how much time is left on the cooldown, it only shows a percentage now the floating point number

};

/*
	Mainly stores GUI data for other classes to update per frame - should be when updated
	Stats are updated per frame almost -> I can set them to only update per .1 seconds though to reduce lag
	Other things like Score List, won't need to be upgraded each frame, only when the data changes
	They should be called by handlers - this will involve lots of analyzing code

 * */

public class GuiManager : MonoBehaviour {
	public Text MyGoldText;
	private BaseCharacter MyCharacter;		// local player
	// references
	public Canvas MyCanvas;
	public GameManager MyGameManager;
	public DataManager MyData;

	public Texture2D MouseTextureDefault;
	public Texture2D MouseTextureAttack;
	public float GuiScale = 1f;
	// used to record the gui system currently being rendered
	public ScreenType CurrentScreen;

	// links to all the different screens - this should be done in an array list with a screen datastructure? maybe
	public GameObject PreGame;
	public GameObject PreGameMainScreen;
	public GameObject PreGameConnectScreen;
	public GameObject InGame;
	public GameObject EndGame;
	public GameObject MenuSystem;
	public GameObject DebugScreen;
	public Text WinText;

	//public Button RestartButton;

	public List<Text> PlayerScores;
	public Scrollbar MyScrollBar;

	public List<GameObject> MyMenuButtons = new List<GameObject> ();

	// Use this for initializationd
	void Start () {
		EnterNewState(CurrentScreen);
		// if mouse texture exists kinda thing
		Cursor.visible = true;
		if (MouseTextureDefault != null) {
			Cursor.SetCursor(MouseTextureDefault,Vector2.zero, CursorMode.Auto);// new Vector2(16,0), new CursorMode());
		}
		PreScreen ("MainScreen");
	}

	// Update is called once per frame
	void Update () {
		UpdateGuiWithPlayer ();
		if (MyCharacter == null) {
			MyCharacter = GetManager.GetCharacterManager().GetLocalPlayer ();
		}

		// if respawn
		if (MyCharacter) {
			EndGame.transform.FindChild ("EndGameLabel").gameObject.GetComponent<Text> ().text = 
			"Time To Respawn: " + GetManager.GetCharacterManager ().GetRespawnTime (MyCharacter);
		} else if (EndGame.activeSelf) {
			EndGame.transform.FindChild ("EndGameLabel").gameObject.GetComponent<Text> ().text = "Game Over";
			if (GetManager.GetWaveManager()) {
				EndGame.transform.FindChild ("EndGameLabel").gameObject.GetComponent<Text> ().text = "You Survived " + GetManager.GetWaveManager().WaveCount + " Waves!";
			}
		}
	}
	
	public void UpdatePlayer(BaseCharacter NewPlayerStats) {
		MyCharacter = NewPlayerStats;
	}
	public void UpdateGuiWithPlayer() {
		if (MyCharacter != null) {

		}
	}
	public void UpdateSpellData(GameObject SpellBook) {
		//GameObject SpellBookBackground = (GameObject) SpellBook.GetComponent ("SpellBackground") as GameObject;
		//GameObject SpellName =(GameObject) SpellBook.GetComponent ("SpellName") as GameObject;
		//GameObject SpelIcon = (GameObject)SpellBook.GetComponent ("SpelIcon") as GameObject;
		//GameObject Sp = SpellBook.Get
	}
	public void DisableAllScreens() {
		PreGame.transform.FindChild("MainScreen").gameObject.SetActive (false);
		PreGame.transform.FindChild("ConnectScreen").gameObject.SetActive (false);
		PreGame.transform.FindChild("DungeonScreen").gameObject.SetActive (false);
		PreGame.transform.FindChild("AdventureScreen").gameObject.SetActive (false);
	}
	public void PreScreen(string ChildName) {
		EnterNewState (ScreenType.PreGame);
		DisableAllScreens ();
		PreGame.transform.FindChild(ChildName).gameObject.SetActive (true);
	}

	public void AnimateMenuButtons(bool IsForward) {
		for (int i = 0; i < MyMenuButtons.Count; i++) {
			AnimateButton MyAnimation = MyMenuButtons[i].GetComponent<AnimateButton>();
			if (MyAnimation != null) {
				MyAnimation.IsAnimating = true;
				MyAnimation.IsForward = IsForward;
			}
		}
	}
	public void EnterNewState(ScreenType NewState) {
		// if old screen is this -> make changes
		if (CurrentScreen == ScreenType.InGame) {
			InGame.SetActive (false);
		} else if (CurrentScreen == ScreenType.MenuSystem) {
			AnimateMenuButtons(true);
			MenuSystem.SetActive (false);
			//ActionBar.transform.position = new Vector3(ActionBar.transform.position.x, ActionBar.transform.position.y-450, ActionBar.transform.position.z);
			GetManager.GetInventoryGui().HideMovingItem();
			GetManager.GetInventoryGui().DisableToolTips();
			if (MyCharacter != null) {
				//Player MyPlayerScript = (Player) MyCharacter.gameObject.GetComponent ("Player");
				//MyPlayerScript.HasSwitchedItem = MyPlayerScript.CheckItemInActionBar();
			} else {
				Debug.LogError ("Gui Manager has no player!?!");
			}
		} else if (CurrentScreen == ScreenType.PreGame) {
			PreGame.SetActive (false);
		} else if (CurrentScreen == ScreenType.EndGame) {
			EndGame.SetActive (false);
		}

		CurrentScreen = NewState;
		// switch statement for new screen, if needed later on
		if (CurrentScreen == ScreenType.InGame) {
			InGame.SetActive (true);
			GetManager.GetCameraManager().EnableInGameControls();
			MyGameManager.UpdatePause (true);
		} else if (CurrentScreen == ScreenType.MenuSystem) {
			InGame.SetActive (true);
			AnimateMenuButtons(false);
			MenuSystem.SetActive (true);
			//GetManager.GetInventoryGui().ShowMovingItem();
		} else if (CurrentScreen == ScreenType.PreGame) {
			PreGame.SetActive (true);
			MyGameManager.UpdatePause (false);
		} else if (CurrentScreen == ScreenType.EndGame) {
			EndGame.SetActive (true);
		}
	}

	// score page manager class needed

	// summoned minions should not be on the scores list
	// if a summoned minion gets a kill, the kill will go to the summoned character
	// i need a window with a list of summond monsters
	public float OverRideScrollBarSize = 0;
	public float ScrollBarSizeDebug = 0;
	public int PositionIncrementer = 0;

	// if its 9 players, it should be (9-7)=2, so 1/2 size
	// this should be in a score list class
	// together with game managers function affecting it
	public void UpdateScoreList(List<string> ScoreList) {
		if (MyScrollBar != null) {
			float ScrollBarSize = ScoreList.Count;
			//if (ScrollBarSize < 1) ScrollBarSize = 1;
			ScrollBarSize =  7f/ScrollBarSize;
			if (ScoreList.Count == 7) ScrollBarSize = 1;
			if (OverRideScrollBarSize != 0) ScrollBarSize = OverRideScrollBarSize;
			ScrollBarSizeDebug = ScrollBarSize;
			MyScrollBar.size = ScrollBarSize;

			PositionIncrementer = (int)((ScoreList.Count - 7) * MyScrollBar.value);
			if (PositionIncrementer < 0) PositionIncrementer = 0;

			for (int i = 0; i < PlayerScores.Count; i++) {
				int NewPosition = i + PositionIncrementer;
				if (NewPosition >= 0 && NewPosition < ScoreList.Count)
					PlayerScores [i].text = NewPosition + "_ " + ScoreList [NewPosition];
				else
					PlayerScores [i].text = "";
			}
		}
	}
	
	public bool ToggleInventory() {
		if (CurrentScreen == ScreenType.MenuSystem) {
			EnterNewState (ScreenType.InGame);
			return true;
		} else if (CurrentScreen == ScreenType.InGame) {
			EnterNewState (ScreenType.MenuSystem);
			return false;
		}
		return false;
	}
	public bool IsInventoryOpened() {
		return (CurrentScreen == ScreenType.MenuSystem);
	}

}
