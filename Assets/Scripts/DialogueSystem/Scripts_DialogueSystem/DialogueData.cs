using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using CharacterSystem;
using MyCustomDrawers;
using QuestSystem;	// atm conditions use the quest log, need to abstract this outside of the system
using CustomEvents;

// conditions programmed in
//	/first
//	/noquest
//	/unfinishedquest
//	/hascompletedquest
//	/handedinquest
//	/options

/*
	note
	/questcheck 5,8,9
	goes to
	/noquest 5
	/unfinishedquest 8
	/completedquest 9

	and
	/questname [quest_name]
	
 */

/*
	Stores data of the dialogue lines
	Has a function to convert command lines into dialogue data
*/


namespace DialogueSystem {
	[System.Serializable]
	public class DialogueCondition {
#if UNITY_EDITOR
		[Enum(typeof(MyCustomDrawers.ConditionTypes))] 
#endif
		[SerializeField] public string Command;	// /first , /questcheck
		[SerializeField] public int NextIndex;	// An index for 'MyNext' List
		public DialogueCondition(string NewCommand, int NextPointer) {
			Command = NewCommand;
			NextIndex = NextPointer;
		}
	}
	// ramblings
	// if character has completed a condition, move to MyNext 0, if not move to 1
	
	[System.Serializable]
	public class SpeechLine {
		public string Speaker = "";
		public string Speech = "";
		public SpeechLine() {

		}
		public SpeechLine(string Speaker_, string Speech_) {
			Speaker = Speaker_;
			Speech = Speech_;
		}
		public string GetLabelText() {
			return Speaker + ": " + Speech;
		}
	}

	[System.Serializable]
	public class DialogueData {
		public string Name = "";
		[Header("Functions")]
		// a function to activate	- used for mostly preset functions like exiting the chat
		[Tooltip("Preset function calls! Just need their name!")]
		[SerializeField] public UnityEvent OnNextLine = new UnityEvent ();
		[SerializeField] public MyEvent3 OnNextLine2 = new MyEvent3 ();
		//public bool IsQuestGive = false;
		//public bool IsCompleteQuest = false;
		public string InputString = "";			// this should just be first parameter for function

		// All pointers
		[Header("Pointers")]
		public List<int> MyNext = new List<int>();		// index for each response	- use string names for these soon

		// All conditions for next dialogue data
		[Header("Conditions")]
		public List<DialogueCondition> MyConditions = new List<DialogueCondition>();		// index for each responsee

		// All Dialogue entries
		[Header("Speech")]
		public int SpeechIndex = 0;	// index of speech it is up to
		public List<SpeechLine> SpeechLines = new List<SpeechLine>();							// maybe the npc has different speech he can chose as well

		public DialogueData() {}

		// when there is options it will forcefully end the speech
		public void End() {
			SpeechIndex = SpeechLines.Count-1;
		}

		public bool HasEnded() {
			if (SpeechIndex >= SpeechLines.Count-1)
				return true;
			else
				return false;
		}
		// if no speech!
		public string GetSpeaker() {
			return GetSpeechSpeaker(SpeechIndex);
		}
		public string[] GetOptionsLines(int OptionsCount) {
			string[] NewLine = new string[OptionsCount];
			for (int i = 0; i < OptionsCount; i++) {
				NewLine[i] = GetSpeechLine(SpeechIndex);
				SpeechIndex++;
			}
			return NewLine;
		}
		public string GetSpeechLine() {
			return GetSpeechLine (SpeechIndex);
		}
		public string GetSpeechLine(int Index)	// if its the first line  
		{
			if (Index >= 0 && Index < SpeechLines.Count) {
				return SpeechLines [Index].Speech;
			} else {
				return "Invalid Index";
			}
		}
		public string GetSpeechSpeaker(int Index) {
			if (Index >= 0 && Index < SpeechLines.Count) {
				return SpeechLines [Index].Speaker;
			} else {
				return "Invalid Index";
			}
		}
		public void Reset() {
			SpeechIndex = 0;
		}
		public void NextLine() 
		{
			SpeechIndex++;
		}
		public string GetAllSpeech(string SpeakerName, bool IsIndexed) {
			string NewLines = "";
			for (int i = 0; i < SpeechLines.Count; i++) {
				if (GetSpeechSpeaker(i) == SpeakerName) {
					if (IsIndexed)
						NewLines += "[" + i + "] ";
					NewLines += GetSpeechLine(i) + "\n";
				}
			}
			return NewLines;
		}
		public int GetOptionsCount() {
			int MyOptionsCount = 0;
			for (int i = 0; i < SpeechLines.Count; i++) {
				if (GetSpeechSpeaker(i) == "Player") {
					MyOptionsCount++;
				}
			}
			return MyOptionsCount;
		}
		public bool ListContains(List<string> MyList, string Blarg) {
			for (int i = 0; i < MyList.Count; i++) {
				if (MyList[i] == Blarg) return true;
			}
			return false;
		}
		public void HandleNextLine(GameObject MyInvoker, GameObject MySecondaryTalker) {
			OnNextLine.Invoke ();
			OnNextLine2.Invoke (MySecondaryTalker, InputString);
		}
		public void AddSpeechLine() 
		{
			SpeechLine NewSpeechLine = new SpeechLine ();
			SpeechLines.Add (NewSpeechLine);
		}

		// conditions
		public void AddCondition(string NewCommand) {
			AddCondition (NewCommand, MyNext.Count, MyConditions.Count);
		}
		
		public void AddCondition(string NewCommand, int MyNextIndex) 
		{
			AddCondition (NewCommand, MyNextIndex, MyNext.Count);
		}
		public void AddCondition(string NewCommand, int MyNextIndex, int MyNextPointer) 
		{
			// first check if its already in list
			for (int i = 0; i < MyConditions.Count; i++)
			{
				if (MyConditions[i].Command == NewCommand)
					return;
			}
			DialogueCondition MyCondition = new DialogueCondition (NewCommand, MyNextPointer);
			MyNext.Add (MyNextIndex);
			MyConditions.Add (MyCondition);
		}
		public void RemoveCondition(int ConditionIndex) {
			MyNext.RemoveAt (ConditionIndex);
			MyConditions.RemoveAt (ConditionIndex);
		}

		public int GetNextDialogueIndex(bool IsFirst, QuestLog MyQuestLog, QuestLog MyQuestLog2, int OptionsIndex) {
			if (MyQuestLog2 == null) {
				Debug.Log("Quest log 2 is null..");
				return 0;
			}
			// Then check Conditions!
			// if first time talking condition
			for (int i = 0; i < MyConditions.Count; i++) {
				switch(MyConditions[i].Command) {
					case("first"):
						if (IsFirst)
						return MyNext[MyConditions[i].NextIndex];
						break;
					case("noquest"):
						//Debug.LogError("Checking for NoQuest!");
					if (!MyQuestLog2.HasQuest(InputString))
						return MyNext[MyConditions[i].NextIndex];
						break;
					case("unfinishedquest"):
					if (MyQuestLog2.HasUnfinishedQuest(InputString))
						return MyNext[MyConditions[i].NextIndex];
						break;
					case("hascompletedquest"):	// hand it in
						if (MyQuestLog2.HasCompletedQuest(InputString)) 
						{
							MyQuestLog2.HandInQuest(InputString, MyQuestLog);
							return MyNext[MyConditions[i].NextIndex];
						}
						break;
					case("handedinquest"):
						if (MyQuestLog2.HasHandedInQuest(InputString))
						return MyNext[MyConditions[i].NextIndex];
						break;
					case("options"):
						return MyNext[MyConditions[i].NextIndex+OptionsIndex];
						break;
				}
			}
			for (int i = 0; i < MyConditions.Count; i++) {
				if (MyConditions[i].Command == "default") {
					return MyNext[i];
				}
			}
			return ListIndex+1;
		}
		private int ListIndex = 0;	// just used to
		
		public bool IsEmptyChat() {
			if (SpeechLines.Count == 0)
				return true;
			return false;
		}
		// need to add a variable for each response
		// or a unity function reference here, so one can be like, 'i would like to trade'->open trade window

		public bool HasOptions() 
		{
			for (int i = 0; i < MyConditions.Count; i++) {
				if (MyConditions[i].Command == "options")
					return true;
			}
			return false;
		}
		// String reading shit!!
		// MyCharacter will be used for function calls
		public DialogueData(List<string> SavedData, int NextCount, string CharacterName, GameObject MyCharacter) 
		{
			ListIndex = NextCount;
			for (int i = 0; i < SavedData.Count; i++) {
				string MyLine = SavedData[i];
				string Other = SpeechUtilities.RemoveCommand(SavedData[i]);
				//Debug.Log("Reading: " + SavedData[i]);
				
				if (MyLine.Contains("/id "))
				{
					Name = Other;
				}
				else if (!SpeechUtilities.IsCommand(MyLine)) 
				{
					string CleanLine = SpeechUtilities.RemoveWhiteSpace(SavedData[i]);
					if (!SpeechFileReader.IsEmptyLine(CleanLine))
					{
						SpeechLine NewSpeechLine = new SpeechLine("Player", CleanLine);
						SpeechLines.Add(NewSpeechLine);
					}
				} else {
					if (SavedData[i].Contains ("/"+CharacterName)) 	// adding new charactername
					{
						SpeechLine NewSpeechLine = new SpeechLine(CharacterName, Other);
						SpeechLines.Add (NewSpeechLine);
						// if (SpeechDialogue != "") MyDialogueGroup.CreateNewDialogueLine();	// original idea was to just split them into new dialogue lines
					}
		//	===-----=== Variables ===-----===
					// if player has finished a quest, rewards them and removes the quest here!
					else if (SavedData[i].Contains ("/questname "))
					{
						InputString = Other;
					}
					// conditions
					else if (DialogueConditions.IsCondition(this, SavedData[i])) {

					}
		//	===-----=== Preset Functions ===-----===
					else if (DialogueFunctions.IsFunction(this, SavedData[i], MyCharacter)) {

					}
				}
			}
			if (!HasDefault())
			{
				AddCondition("default", NextCount);
			}
		}
		private bool HasDefault() {
			for (int i = 0; i < MyConditions.Count; i++) {
				if (MyConditions[i].Command == "default")
					return true;
			}
			return false;
		}

		public void ActivateOptionsCondition(string MyLine) {
			Debug.Log ("Activating Options Condition");
			MyLine = SpeechUtilities.RemoveCommand(MyLine);
			//MyLine = SpeechFileReader.RemoveCommand(MyLine, "/options ");
			List<int> MyInts = SpeechUtilities.GetInts(MyLine);
			
			AddCondition("options",  MyNext.Count);
			MyNext.RemoveAt(MyNext.Count-1);	// remove the last added one, and replace with custom ones
			for (int j = 0; j < MyInts.Count; j++) 
			{
				MyNext.Add (MyInts[j]-1);
			}
		}
		// string utilities
	}
}

// old


/*else if (MyLine.Contains ("/checkquest "))
			{
				//checkquest Get away from Seth!
				//next 5, 8, 9
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				List<int> MyInts = SpeechUtilities.GetInts(MyLine);
				
				if (MyInts.Count >= 1) 
				{
					AddCondition("noquest", MyInts[0]-1);
					if (MyInts.Count >= 2)
						AddCondition("unfinishedquest", MyInts[1]-1);
					if (MyInts.Count >= 3)
						AddCondition("completedquest", MyInts[2]-1);
					if (MyInts.Count >= 4)
						AddCondition("handedinquest", MyInts[3]-1);
				}
				return true;
			}*/