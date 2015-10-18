using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/* Speech Handler - Attach to bot
 * Link it to the Speech Gameobject(with Text on it)
	Once initialized, a certain player will start the talking
	MainCharacter -> Lotus etc
	(the idea is that characters will initialize their own dialogue with anyone)
	One of the Speech Handlers are used as the client, and the other is the server

	Depends on scripts:
		Character
		SpeechAnimator	(GuiUtilities)
		DialogueLine	(Data Format)
		SpeechFileReader(Static Reader)
		List			(Unity)
*/

namespace DialogueSystem {
	public class SpeechHandler : MonoBehaviour {
		public Text MyDialogueText;		// used to display the text in the chat - and toggle it on/off
		public bool IsOpen = false;	// whether the dialogue is currently opened or not
		public bool HasLoaded = false;	// used to check if the file has loaded - io
		public int ChattedCount = 0;			// counts how many times someone has talked to the reciever


		// all the dialogue data
		private string MyFile = "";
		private int DialogueIndex = 0;
		public List<DialogueLine> MyDialogues = new List<DialogueLine> ();
		// animates the text
		private SpeechAnimator MySpeechAnimator;
		// true = text is animating - false = text is not animating
		private bool CanUpdateDialogue = true;	
		// text file name! without the .txt extension
		// the 2 characters involved in the conversation! (more in the future)
		private Character MyCharacter;	// set in /awake - attached to same gameobject as this class
		private Character MyCharacter2;

		private bool IsFirstTalker = false;		// used to understand what speech handler is dictating the conversation - one initiator and one reciever

		// Quests
		// names of quests completed and given
		public List<string> QuestsCompleted = new List<string>();	// indexes of quests completed
		public List<string> QuestsGiven = new List<string>();	// indexes of quests completed

		public bool IsTalking() {
			return IsOpen;
		}
		public Character GetMainTalker() {
			return MyCharacter;
		}
		public void SetMainTalker() {
			IsFirstTalker = true;
		}
		public void SetSecondaryTalker() {
			IsFirstTalker = false;
		}
		public void SetCharacter(Character MyCharacter_) {
			//MyCharacter = MyCharacter_;
		}
		public void SetCharacter2(Character MyCharacter2_) {
			MyCharacter2 = MyCharacter2_;
		}

		// empties the data
		public void Clear() {
			MyDialogues.Clear ();
		}
		public void AddDialogue(DialogueLine NewDialogue) {
			MyDialogues.Add (NewDialogue);
		}
		public int DialogueSize() {
			return MyDialogues.Count;
		}
		public bool CanTalk() {
			return !IsOpen;
		}

		// Use this for initialization
		void Awake () {
			MyFile = gameObject.name;
			MyCharacter = gameObject.GetComponent<Character> ();
			if (MyFile != "") {
				//SpeechData.PrintText("Loading from: " + MyFile);
				HasLoaded = SpeechFileReader.ReadDialogue(MyFile, this);
			}
		}
		// begins the chat, resets everything
		public void Activate() {
			DialogueIndex = 0;
			IsOpen = true;
			MyDialogueText.gameObject.GetComponent<Text> ().text = "";
			CheckForEmptyDialogue();
			UpdateSpeech ();
		}
		// ends the chat
		public void ExitChat() {
			IsOpen = false;
			ToggleSpeech (false);
			ResetAll ();
			ChattedCount++;
			MyCharacter.OnEndDialogue();
			MyCharacter2.OnEndDialogue();
		}

		void Update() {
			if (IsOpen) {
				if (Input.GetKeyDown (KeyCode.Space)) {
					NextLine ();
				}
				if (Input.GetKeyDown (KeyCode.E)) {
					ExitChat ();
				}
			}
		}
		public void BeginSpeech(SpeechHandler MySpeech2, bool IsFirstSpeaker) {

		}


		public void ResetAll() {
			DialogueIndex = 0;
			MyDialogueText.text = "";
		}
		public void NextLine() {
			NextLine (0, false);
		}
		public void NextLine(int Value) {
			NextLine (Value, true);
		}
		public void NextLine(int Value, bool IsOption) {
			//Debug.LogError ("Next line beg : " + IsActive + " : " + gameObject.name);
			if (!IsFirstTalker) {
				MyCharacter2.MySpeechBubble.CanUpdateDialogue = CanUpdateDialogue;
				MyCharacter2.MySpeechBubble.NextLineDo(Value, IsOption);
			} else {
				NextLineDo(Value, IsOption);
			}
		}
		
		public void NextLineDo(int Value, bool IsOption) 
		{
			if (CanUpdateDialogue)
			if (MyDialogues.Count > 0 && DialogueIndex < MyDialogues.Count) {
				{
					DialogueLine MyDialogueLine = GetCurrentDialogue ();
					if (MyDialogueLine.RespondType != ResponseType.Next && !IsOption)
						return;
					//Debug.LogError ("Nextline doing : " + DialogueIndex);
					// quests
					//Debug.LogError("Adding quest: " + QuestIndex);
					if (IsFirstTalker) 
					{
						// reciever gives the quest to the initiator - i.e. player recieves the quest off the npc
						if (MyDialogueLine.IsQuestGive) 
						{
							if (MyCharacter)
							{
								//int QuestIndex = MyDialogueLine.QuestIndex;
								string QuestName = MyCharacter.GiveCharacterQuest(MyDialogueLine.QuestName, MyCharacter2);
								if (QuestName != "") 
								{
									QuestsGiven.Add(QuestName);
								}
							}
						}
						// checking from recieving speech handler - handing the quest in
						else if (MyDialogueLine.IsQuestCheck)
						{
							if (MyCharacter2) 
							{
								//string QuestName = MyCharacter.MyQuests[MyDialogueLine.QuestIndex].Name;
								int MyQuestCompletedIndex = MyCharacter2.RemoveQuest(MyDialogueLine.QuestName, MyCharacter);
								if (MyQuestCompletedIndex != -1) 
								{
									QuestsCompleted.Add(MyDialogueLine.QuestName);
								}
							}
						}
						
						if (MyDialogueLine.IsExitChat) 
						{
							ExitChat ();
							return;
						}
						QuestsGiven.Clear();
						for (int i = 0; i < MyCharacter2.MyQuests.Count; i++) {
							QuestsGiven.Add (MyCharacter2.MyQuests[i].Name);
						}
					}
					DialogueIndex = MyDialogueLine.GetNextLine ((ChattedCount == 0), Value, QuestsGiven, QuestsCompleted);
					CheckForEmptyDialogue(Value);
					UpdateSpeech ();
				}
			}
		}

		void CheckForEmptyDialogue() {
			CheckForEmptyDialogue (0, 0);
		}
		void CheckForEmptyDialogue(int Value) {
			CheckForEmptyDialogue (Value, 0);
		}
		void CheckForEmptyDialogue(int Value, int ChecksNumber) {
			if (MyDialogues [DialogueIndex].IsEmptyChat ()) {
				Debug.Log ("Skipping Dialogue: " + DialogueIndex);
				DialogueIndex = MyDialogues [DialogueIndex].GetNextLine ((ChattedCount == 0), Value, QuestsGiven, QuestsCompleted);
				ChecksNumber++;
				if (ChecksNumber < 1000)
					CheckForEmptyDialogue();
			}
		}
		// 0 to 3, for the various options

		public DialogueLine GetCurrentDialogue() {
			if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count)
				return MyDialogues [DialogueIndex];
			else
				return new DialogueLine ();
		}
		// uses the dialogue index, and updates the gui with dialogue data
		private void UpdateSpeech() {
			if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count)
				UpdateDialogue (MyDialogues [DialogueIndex]);
		}


		private void UpdateDialogue(DialogueLine NewDialogue) {
			if (CanUpdateDialogue) {
				if (NewDialogue.IsReverseSpeech())
				{
					ToggleSpeech(false, true);
					MyCharacter2.MySpeechBubble.MyDialogueText.GetComponent<SpeechAnimator> ().NewLine (NewDialogue.GetReverseSpeechDialogue(ChattedCount == 0));
					MyCharacter2.MySpeechBubble.DeactivateChildren ();
					MyCharacter2.MySpeechBubble.AddAnimationListener();
					MyCharacter2.MySpeechBubble.CanUpdateDialogue = false;
				} else {
					ToggleSpeech(true, false);
					MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().NewLine (NewDialogue.GetSpeechDialogue(ChattedCount == 0));
					DeactivateChildren ();
					// this way the handler knowns when the text has finished animation - stops skipping to next line
					MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().OnFinishedAnimationFunction.AddListener(UpdateRespondType);
					CanUpdateDialogue = false;
				}
			}
		}

		public void AddAnimationListener() {
			MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().OnFinishedAnimationFunction.AddListener(UpdateRespondType);
		}

		public void ToggleSpeech(bool IsSpeech) {
			ToggleSpeech (IsSpeech, IsSpeech);
		}

		public void ToggleSpeech(bool IsSpeech, bool IsSpeech2) {
			MyDialogueText.gameObject.transform.parent.gameObject.SetActive (IsSpeech);
			Transform MyCharacterLabel = MyDialogueText.gameObject.transform.parent.parent.FindChild ("Label");	// a son of my parent is my brother, label is the brother of the speech text
			if (MyCharacterLabel != null) {
				MyCharacterLabel.gameObject.SetActive(!IsSpeech);
			}
			IsOpen = IsSpeech;
			
			if (MyCharacter2) {
				MyCharacter2.MySpeechBubble.IsOpen = IsSpeech2;
				MyCharacter2.MySpeechBubble.MyCharacter2 = MyCharacter;
				MyCharacter2.MySpeechBubble.MyDialogueText.gameObject.transform.parent.gameObject.SetActive (IsSpeech2);
				Transform MyCharacterLabel2 = MyCharacter2.MySpeechBubble.MyDialogueText.transform.parent.parent.FindChild ("Label");
				if (MyCharacterLabel2 != null) {
					MyCharacterLabel2.gameObject.SetActive (!IsSpeech2);
				}
			}
		}

		private void UpdateRespondType() {
			//Debug.LogError ("Finished animating text.");
			UpdateRespondType(GetCurrentDialogue());
		}

		// handles the various setups for responses
		private void UpdateRespondType(DialogueLine NewDialogue) {
			DeactivateChildren ();

			if (NewDialogue.ReverseDialogueLines.Count <= 1) 
			{
				MyDialogueText.gameObject.transform.FindChild("NextButton").gameObject.SetActive(true);
			}
			else
			{
				for (int i = 0; i < NewDialogue.ReverseDialogueLines.Count; i++) {
					Transform MyResponseTransform = MyDialogueText.gameObject.transform.FindChild("Option"+(i+1));
					if (MyResponseTransform) {
						MyResponseTransform.gameObject.SetActive(true);
						MyResponseTransform.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text =
																				NewDialogue.ReverseDialogueLines[i];
					}
				}
			}
			CanUpdateDialogue = true;
		}

		private void DeactivateChildren() {
			for (int i = 0; i < MyDialogueText.gameObject.transform.childCount; i++) {
				MyDialogueText.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
	}
}