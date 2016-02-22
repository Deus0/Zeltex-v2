using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using CharacterSystem;
using QuestSystem;

namespace DialogueSystem {
	[System.Serializable]
	public class DialogueTree {
		public UnityEvent OnEndTree = new UnityEvent();
		// all the dialogue data
		[Tooltip("The count how many times someone has talked to the character")]
		[SerializeField] public int ChattedCount = 0;							// 
		[Tooltip("The index that the speech is at.")]
		[SerializeField] private int DialogueIndex = 0;
		[Tooltip("A list of the Dialogue Tree Branches.")]
		[SerializeField] private List<DialogueData> MyDialogues = new List<DialogueData> ();
		public DialogueData GetDialogue(int index) {
			return (MyDialogues [index]);
		}
		void CheckForEmptyDialogue(QuestLog MainQuestLog, QuestLog SecondaryQuestLog) {
			int MaxChecks = 25;
			int ChecksNumber = 0;
			while (ChecksNumber < MaxChecks) 
			{
				if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count) {
					if (MyDialogues [DialogueIndex].IsEmptyChat ()) {
						Debug.Log ("Skipping Dialogue: " + DialogueIndex);
						GetNextDialogueLine(MainQuestLog, SecondaryQuestLog);
					}
					else 
					{
						ChecksNumber = MaxChecks;	// end checks
					}
					ChecksNumber++;
				} else {
					ChecksNumber = MaxChecks;
				}
			}
		}

		public void NextLine(int OptionsIndex, QuestLog MainQuestLog, QuestLog SecondaryQuestLog) {
			if (SecondaryQuestLog == null || MainQuestLog == null) {
				Debug.LogError("Quest logs are null..");
				return;
			}
			DialogueData CurrentDialogue = GetCurrentDialogue ();
			if (CurrentDialogue != null) {
				if (CurrentDialogue.HasEnded ()) {
					Debug.Log ("Line has Ended. Moving to next Line. At " + (DialogueIndex + 1) + " out of " + (MyDialogues.Count));
					// reciever gives the quest to the initiator - i.e. player recieves the quest off the npc
					//if (CurrentDialogue.IsQuestGive) {
						//MainQuestLog.GiveCharacterQuest (CurrentDialogue.QuestName, SecondaryQuestLog);
					//}
					CurrentDialogue.HandleNextLine (MainQuestLog.gameObject, SecondaryQuestLog.gameObject);	// runs unity event
				
					// inside dialogueline is where 
					//		-functions are activated
					// 		-conditions are checked for dialoguetree
					//DialogueIndex = CurrentDialogue.GetNextDialogueIndex ((ChattedCount == 0), GetMainQuestLog (), GetSecondaryQuestLog (), OptionsIndex);
					GetNextDialogueLine (OptionsIndex, MainQuestLog, SecondaryQuestLog);
					CheckForEmptyDialogue(MainQuestLog, SecondaryQuestLog);
					if (DialogueIndex >= MyDialogues.Count) 
					{
						OnEndTree.Invoke();
					}
				}
				else 
				{
					CurrentDialogue.NextLine ();
				}
			}
		}
		public void End() {
			ChattedCount++;
			//DialogueIndex = 0;
		}

		public void GetNextDialogueLine(QuestLog MainQuestLog, QuestLog SecondaryQuestLog) {
			GetNextDialogueLine (0, MainQuestLog, SecondaryQuestLog);
		}
		public void GetNextDialogueLine(int OptionsIndex, QuestLog MainQuestLog, QuestLog SecondaryQuestLog) {
			DialogueIndex = MyDialogues [DialogueIndex].GetNextDialogueIndex ((ChattedCount == 0), MainQuestLog, SecondaryQuestLog, OptionsIndex);
			if (GetCurrentDialogue () != null) {
				GetCurrentDialogue ().Reset ();
			}
		}







		public DialogueData GetCurrentDialogue() 
		{
			if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count)
				return MyDialogues [DialogueIndex];
			else
				return null;
		}
		public void Reset(QuestLog MainQuestLog, QuestLog SecondaryQuestLog) {
			DialogueIndex = 0;
			CheckForEmptyDialogue(MainQuestLog, SecondaryQuestLog);
			GetCurrentDialogue ().Reset ();
		}
		public int GetIndex() {
			return DialogueIndex;
		}
		public int GetSize() {
			return MyDialogues.Count;
		}
		public void IncreaseIndex() {

		}
		public void Add(DialogueData NewData) {
			MyDialogues.Add (NewData);
		}
		public void Clear() {
			MyDialogues.Clear ();
		}
	}
}