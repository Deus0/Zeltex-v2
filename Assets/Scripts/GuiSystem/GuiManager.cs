using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AISystem;
using GUI3D;
using DialogueSystem;

/*	Interfaces with all the guis
 *
 * */

namespace GuiSystem {
	[System.Serializable]
	public class PlayerControls {
		[Header("Settings")]
		public bool IsPlayer = false;
		public KeyCode SkillsKey = KeyCode.BackQuote;
		public KeyCode MyInventoryKey = KeyCode.Alpha1;
		public KeyCode QuestLogKey = KeyCode.Alpha2;
		public KeyCode StatsKey = KeyCode.Alpha3;
		public KeyCode LabelKey = KeyCode.Alpha4;
		public KeyCode CrosshairKey = KeyCode.Alpha5;
		public KeyCode ClockKey = KeyCode.Alpha6;
		public KeyCode LogGuiKey = KeyCode.Alpha7;
	}
	public class GuiManager : MonoBehaviour {
		public string GuiState = "Label";
		public bool CanTalk = true;
		public bool IsHiddenLabel = false;
		[SerializeField] private List<Toggler> MyGuis = new List<Toggler> ();
		// different states:
		//		Label
		//		Dialogue
		//		Inventory
		//		QuestLog
		//		QuestContract
		//		StatsGui
		private SpeechHandler MySpeechBubble;
		[Header("Sounds")]
		private AudioSource MySource;
		[Tooltip("Played when dialogue begins")]
		[SerializeField] private AudioClip OnBeginTalkingSound;
		[Tooltip("Played when dialogue ends")]
		[SerializeField] private AudioClip OnEndTalkingSound;
		
		[Header("Speech")]
		[Tooltip("These functions are called when dialogue begins!")]
		public UnityEngine.Events.UnityEvent OnBeginTalking;
		[Tooltip("These functions are called when dialogue ends!")]
		public UnityEngine.Events.UnityEvent OnEndTalking;

		// Use this for initialization
		void Start () 
		{
			// gather the togglers
			for (int i = 0; i < transform.childCount; i++) {
				Toggler MyToggler = transform.GetChild(i).GetComponent<Toggler>();
				if (MyToggler)
					AddToggler(MyToggler);
			}
			// Grab my speech handler!
			MySpeechBubble = gameObject.GetComponent <SpeechHandler> ();
			// Title gui
			UpdateLabel ();
			MySource = gameObject.GetComponent<AudioSource> ();
			if (MySource == null) {
				MySource = gameObject.AddComponent<AudioSource> ();
			}
			DisableAllGuis ();
			EnableCurrentGui ();
		}

		public void TogglePlayerControls(bool IsPlayer) {
			if (IsPlayer)
				UpdatePlayerControls ();
			else
				RemovePlayerControls ();
		}
		public void RemovePlayerControls() 
		{
			for (int i = 0; i < MyGuis.Count; i++) 
			{
				MyGuis [i].MyToggleKey = KeyCode.None;
			}
		}
		public void UpdatePlayerControls() {
			UpdateControls (new PlayerControls ());
		}
		public void UpdateControls(PlayerControls MyControls) {
			for (int i = 0; i < MyGuis.Count; i++) {
				if (MyGuis[i].name == "ClockGui") {
					MyGuis[i].MyToggleKey = MyControls.ClockKey;
				} else if (MyGuis[i].name == "Label") {
					MyGuis[i].MyToggleKey = MyControls.LabelKey;
				} else if (MyGuis[i].name == "Dialogue") {
					//MyGuis[i].MyToggleKey = MyControls.;
				} else if (MyGuis[i].name == "Inventory") {
					MyGuis[i].MyToggleKey = MyControls.MyInventoryKey;
				} else if (MyGuis[i].name == "Stats") {
					MyGuis[i].MyToggleKey = MyControls.StatsKey;
				} else if (MyGuis[i].name == "QuestLog") {
					MyGuis[i].MyToggleKey = MyControls.QuestLogKey;
				} else if (MyGuis[i].name == "Log") {
					MyGuis[i].MyToggleKey = MyControls.LogGuiKey;
				} else if (MyGuis[i].name == "SkillBar") {
					MyGuis[i].MyToggleKey = MyControls.SkillsKey;
				} else if (MyGuis[i].name == "Crosshair") {
					MyGuis[i].MyToggleKey = MyControls.CrosshairKey;
				}
			}
		}

		public void DestroyAllGuis() {
			for (int i = 0; i < MyGuis.Count; i++) {
				if (MyGuis[i])
					Destroy(MyGuis[i].gameObject);
			}
			MyGuis.Clear ();
		}

		public void DisableAllGuis() 
		{
			for (int i = 0; i < MyGuis.Count; i++) 
			{
				if (MyGuis[i]) {
					if (!MyGuis[i].name.Contains("Crosshair"))
						MyGuis[i].TurnOff();
					else
						MyGuis[i].TurnOn();
				}
			}
		}

		public Toggler GetToggler(string GuiName) {
			
			//Debug.LogError ("Enabling: " + GuiName);
			for (int i = 0; i < MyGuis.Count; i++) 
			{
				if (MyGuis[i]) {
					if (MyGuis[i].gameObject.name == GuiName) 
					{
						//Debug.LogError ("Found: " + GuiName);
						return MyGuis[i];
					}
				}
			}
			return null;
		}

		// turns on a specific gui
		public bool EnableGui(string GuiName) 
		{
			Toggler MyToggler = GetToggler (GuiName);	
			if (MyToggler)
			{
				MyToggler.TurnOn();
			}
			if (GuiName == "Label")
			{
				Toggler MySkillBar = GetToggler ("SkillBar");
				if (MySkillBar)
					MySkillBar.TurnOn();
			}
			//Debug.Log ("Error finding: " + GuiName);
			return false;
		}

		public void UpdateLabel() 
		{
			Transform MyCharacterLabel;// = transform.parent.FindChild ("Label");
			Toggler MyLabelToggler = GetToggler ("Label");
			if (MyLabelToggler == null)
				return;
			MyCharacterLabel = MyLabelToggler.transform;

			if (MyCharacterLabel != null) {
				if (!IsHiddenLabel)
					MyCharacterLabel.FindChild("LabelText").gameObject.GetComponent<Text> ().text = name;
				else {
					MyCharacterLabel.FindChild("LabelText").gameObject.GetComponent<Text> ().text = GenerateHiddenName ();
					
				}
			}
		}

		public string GenerateHiddenName() {
			string NewName = "";
			float RandomRange = name.Length+Random.Range(-2,2);
			if (RandomRange < 3) // min ?'s is 3
				RandomRange = 3;	
			for (int i = 0; i < RandomRange; i++)
				NewName += "?";
			return NewName;
		}
		public void AddToggler(Toggler NewToggler) {
			// if already exists in list, dont add
			for (int i = 0; i < MyGuis.Count; i++) {
				if (MyGuis[i] == NewToggler)
					return;
			}
			// otherwise add gui to list
			if (NewToggler != null)
				MyGuis.Add (NewToggler);
		}
		public void ClearTogglers() {
			MyGuis.Clear ();
		}
		public void EnableCurrentGui() {
			EnableGui (GuiState);
		}
		public void SwitchToInventory() {
			SwitchMode ("Inventory");
		}
		public void SwitchToStats() {
			SwitchMode ("Stats");
		}
		public void SwitchToQuestLog() {
			SwitchMode ("QuestLog");
		}
		public void SwitchMode(string NewMode) {
			if (GuiState != NewMode) 
			{
				/*if ((GuiState == "Dialogue" || GuiState == "") && NewMode == "Label") {
					gameObject.GetComponent<SpeechHandler>().ExitChat();
				}*/
				GuiState = NewMode;
				DisableAllGuis();
				EnableGui(NewMode);
			}
		}
		
		public void BeginTrade(GuiManager HitCharacter)
		{
			if (!MySpeechBubble.IsTalking ()) {
				if (HitCharacter.GuiState == "Label") {
					//SwitchMode ("Inventory");
					HitCharacter.SwitchMode ("Inventory");
				}
			}
		}

		public void EndTrade() {
			if (!MySpeechBubble.IsTalking ()) {
				if (GuiState == "Inventory") {
					SwitchMode ("Label");
				}
			}
		}
		public void BeginTalk(GuiManager HitCharacter) {
			if (CanTalk && HitCharacter.CanTalk) {
				if (!MySpeechBubble.IsTalking ()) {
					if (HitCharacter.GuiState == "Label") {
						SwitchMode ("Dialogue");
						HitCharacter.SwitchMode ("Dialogue");
						//Debug.LogError (gameObject.name + " -Beginning dialogue with: " + HitCharacter.name);
						HitCharacter.BeginDialogue (this);
					}
				}
			}
		}
		public void EndTalk() {
			Debug.Log ("Ending talk in gui manager! " + name);
			if (GuiState == "Dialogue" || GuiState == "") {
				SwitchMode ("Label");
				if (OnEndTalking != null)
					OnEndTalking.Invoke ();
				if (OnEndTalkingSound != null)
					MySource.PlayOneShot (OnEndTalkingSound);
			}
		}

		public SpeechHandler GetSpeechHandler() {
			return MySpeechBubble;
		}
		
		public void BeginDialogue(GuiManager ConversationStarter) {
				// Now have speech bubbles pop up
				if (!MySpeechBubble.IsTalking () && !ConversationStarter.GetSpeechHandler ().IsTalking ()) {
					if (gameObject.GetComponent<Wander> ()) {
						gameObject.GetComponent<Wander> ().ToggleWander (false);
					}
					if (gameObject.GetComponent<Movement> ()) {
						gameObject.GetComponent<Movement> ().LookAt (ConversationStarter.gameObject);
					}
					if (gameObject.GetComponent<RotateTowardsObject> ()) {
						//Debug.LogError("Doing the thing!");
						gameObject.GetComponent<RotateTowardsObject> ().RotateTowards (ConversationStarter.gameObject);
					}
					MySpeechBubble.Begin (ConversationStarter.gameObject);
					if (OnBeginTalking != null)
						OnBeginTalking.Invoke ();
					if (OnBeginTalkingSound != null)
						MySource.PlayOneShot (OnBeginTalkingSound);
				}
		}

		


	}
}
