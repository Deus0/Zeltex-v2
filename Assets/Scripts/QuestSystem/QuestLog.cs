using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using CharacterSystem;
using ItemSystem;

/*	Completed quests are now kept in the quest log
 * 	They will be in the same list, with the boolean IsComplete set to true
 * 		When they are handed in for rewards, their boolean IsHandedIn is set to true
 * 	Otherwise the are just incomplete
 * 		
 * 
*/
namespace QuestSystem {
	public class QuestLog : MonoBehaviour {
		[Header("Events")]
		[Tooltip("Called when a quest is added or removed.")]
		public UnityEvent OnAddQuest;
		[Tooltip("Called when a quest is completed.")]
		public UnityEvent OnCompletedQuest;

		[Header("Quests")]
		[Tooltip("Limits the amount of unfinished quests. Set to -1 for unlimited.")]
		public int QuestLimit = -1;
		[Tooltip("These quests can be given to other characters to complete.")]
		public List<Quest> MyQuests = new List<Quest>();
		[Tooltip("Link to QuestLogGuiHandler class. Empty if the character has no quest gui.")]
		//public QuestLogGuiHandler MyQuestLogGui;

		[Header("Sounds")]
		private AudioSource MySource;
		[Tooltip("Sound Plays when completing a quest")]
		public AudioClip OnCompleteQuestSound;
		[Tooltip("Sound Plays when handing in a quest")]
		public AudioClip OnHandInQuestSound;
		[Tooltip("Sound Plays when Beginning in a quest")]
		public AudioClip BeginQuestSound;

		// Use this for initialization
		void Start () 
		{
			MySource = gameObject.GetComponent<AudioSource> ();
			if (MySource == null)
				MySource = gameObject.AddComponent<AudioSource> ();
		}

		public Quest GetQuest(string QuestName) 
		{
			for (int i = 0; i < MyQuests.Count; i++) {
				// need to do this check when reading the quest from files - have not tested yet
				MyQuests [i].Name = SpeechUtilities.CheckStringForLastChar(MyQuests [i].Name);	
				QuestName = SpeechUtilities.CheckStringForLastChar(QuestName);
				if (MyQuests [i].Name == QuestName) {
					return MyQuests[i];
				}
			}
			return null;
		}
		
		// sets the quest to completed!
		public void SignOffQuest(GameObject MyFinishingCharacter, string QuestName) 
		{
			if (MyFinishingCharacter.GetComponent<QuestLog> ()) {
				MyFinishingCharacter.GetComponent<QuestLog>().CompleteQuest(QuestName);
			}
		}

		// sets the quest to completed!
		public bool CompleteQuest(string QuestName) 
		{
			Quest MyQuest = GetQuest (QuestName);
			if (MyQuest != null) {
				if (!MyQuest.HasCompleted() && !MyQuest.IsHandedIn) {
					MyQuest.CompleteQuest ();
					HandleAddQuest ();
					HandleCompletedQuest();
					return true;
				}
			} else {
				Debug.Log("Could not find Quest: " + QuestName + " to complete.");
			}
			return false;
		}

		// when loading quests
		public void AddQuest(Quest NewQuest) 
		{
			NewQuest.QuestGiver = this;
			if (!MyQuests.Contains(NewQuest))
				MyQuests.Add (NewQuest);
		}

		public int GetUncompletedQuests() 
		{
			int UncompletedQuestsCount = 0;
			for (int i = 0; i < MyQuests.Count; i++) {
				if (!MyQuests[i].IsHandedIn) {
					UncompletedQuestsCount++;
				}
			}
			return UncompletedQuestsCount;
		}

		public int GetQuestIndex(string QuestName) 
		{
			QuestName = SpeechUtilities.CheckStringForLastChar (QuestName);
			for (int i = 0; i < MyQuests.Count; i++) {
				MyQuests[i].Name = SpeechUtilities.CheckStringForLastChar (MyQuests[i].Name);
				if (MyQuests[i].Name == QuestName) {
					return i;
				}
			}
			return -1;
		}

		// when another quest holder is issueing the quest out
		public void GiveCharacterQuest(GameObject QuestTaker2, string QuestName) 
		{
			QuestLog QuestTaker = QuestTaker2.GetComponent<QuestLog> ();
			if (QuestTaker == null)
				return;
			Debug.Log (name + " is giving ["+QuestName+"] to " + QuestTaker.name + " at " + Time.time);
			// First check if the SecondQuestLog(player) can recieve a quest
			if (GetUncompletedQuests () >= QuestLimit && QuestLimit != -1) 
			{
				Debug.Log ("When " + name + " was giving ["+QuestName+"] it failed. Limit Reached");
				return;
			}
			
			int QuestIndex = GetQuestIndex(QuestName);
			if (QuestIndex == -1) 
			{
				Debug.LogError ("When " + name + " was giving ["+QuestName+"] it failed. No Quest to give with that name.");
				//if (MyQuests.Count > 0) 
				{
					//Debug.LogError("Quest to find's length is[" + QuestName.Length + "] and QuestLog Quest's length is [" + MyQuests[0].Name.Length + "]");
					//Debug.LogError("Quest to find's name is [" + QuestName + "] and QuestLog Quest's name is: [" + MyQuests[0].Name + "]");
					//Debug.LogError("Number " +(QuestName.Length-2)+" [" + QuestName[QuestName.Length-2] + "] [" + ((int)QuestName[QuestName.Length-2]) + "]");
					//Debug.LogError("Number " +(QuestName.Length-1)+" [" + QuestName[QuestName.Length-1] + "] [" + ((int)QuestName[QuestName.Length-1]) + "]");
				}
				return;
			}
			
			if (QuestIndex < MyQuests.Count && QuestIndex >= 0) {
				Quest NewQuest = MyQuests [QuestIndex];
				if (NewQuest.HasGivenQuestOut()) {
					Debug.Log ("When " + name + " was giving ["+QuestName+"] it failed. Quest has already been given out.");
					return;
				}

				if (!QuestTaker.MyQuests.Contains (NewQuest)) 
				{
					QuestTaker.MyQuests.Add (NewQuest);
					QuestTaker.MyQuests[QuestTaker.MyQuests.Count-1].OnAddItem(QuestTaker.gameObject);	// basically make sure it's been checked at beginning of add
					if (QuestTaker.BeginQuestSound != null) {
						MySource.PlayOneShot (QuestTaker.BeginQuestSound);
						//Debug.LogError("PLaying sound");
					}
					//Debug.LogError("Adding Quest");
					//Debug.LogError("Adding quest: " + MyCharacter.MyQuests[QuestIndex].Name);
					NewQuest.QuestGiver = this;
					NewQuest.QuestTaker = QuestTaker;

					HandleAddQuest ();
					QuestTaker.HandleAddQuest();
					return;
				} else {
					Debug.LogError ("When " + name + " was giving ["+QuestName+"] it failed. " + QuestTaker.name + " already has that quest.");
				}
			}
		}

		// sets the variables of the quest to [given rewards out = true]
		public bool HandOutQuest(string QuestName) {
			Quest MyQuest = GetQuest (QuestName);
			if (MyQuest == null)
				return false;
			MyQuest.HandIn ();
			return true;
		}
		// when player is handing in a quest
		public bool HandInQuest(string QuestName, QuestLog MyQuestGiver) {
			Quest MyQuest = GetQuest (QuestName);

			if (MyQuest == null)	// if doesn't have the quest
				return false;

			if (MyQuest.HasCompleted() && !MyQuest.IsHandedIn)
			{
				//Debug.LogError (name + " Removeing quest: " + QuestIndex);
				// now trade rewards over
				
				if (!MyQuestGiver.HandOutQuest(QuestName)) 
				{
					//	if quest giver no longer has that quest contract
					//return false;
				}
				MyQuest.HandIn();
				HandleAddQuest ();
				
				if (OnHandInQuestSound != null)
					MySource.PlayOneShot (OnHandInQuestSound);
				
				for (int i = 0; i < MyQuest.MyConditions.Count; i++) {
					if (MyQuest.MyConditions[i].IsInventory()) {
						ItemSystem.Inventory MyInventory = gameObject.GetComponent<ItemSystem.Inventory>();
						ItemSystem.Inventory MyInventory2 = MyQuestGiver.gameObject.GetComponent<ItemSystem.Inventory>();
						if (MyInventory && MyInventory2) {
							MyInventory.GiveItem(MyInventory2.gameObject, MyQuest.MyConditions[i].ObjectName, MyQuest.MyConditions[i].ItemQuantity);
						}
					}
				}
				for (int i = 0; i < MyQuest.MyRewards.Count; i++) {
					Reward MyReward = MyQuest.MyRewards[i];
					if (MyReward.IsInventory) 
					{
						Debug.Log(MyQuestGiver.name + " is rewarding: " + name);
						ItemSystem.Inventory MyInventory = gameObject.GetComponent<ItemSystem.Inventory>();
						ItemSystem.Inventory MyInventory2 = MyQuestGiver.gameObject.GetComponent<ItemSystem.Inventory>();
						if (MyInventory && MyInventory2) {
							MyInventory2.GiveItem(MyInventory.gameObject, MyReward.ItemName, MyReward.ItemQuantity);
						}
					}
				}
				return true;
			}
			return false;
		}

		// Quest Condition Checking
		private void HandleCompletedQuest() {
			if (OnCompleteQuestSound != null)
				MySource.PlayOneShot (OnCompleteQuestSound);
			OnCompletedQuest.Invoke();
		}
		public void OnTalkTo(GameObject MyCharacter)
		{
			for (int i = 0; i < MyQuests.Count; i++) // maybe pass into the quest, Object name and Event Type (Seth PickedUp Item, Seth TalkedTo Lotus, etc)
			{
				if (MyQuests[i].GetCurrentCondition().IsTalkTo()) {
					if (MyQuests[i].GetCurrentCondition().ObjectName == MyCharacter.name) 
					{
						bool HasFinished = MyQuests[i].HasCompleted();
						MyQuests[i].GetCurrentCondition().Complete();
						MyQuests[i].CheckCompleted();
						
						if (!HasFinished && MyQuests[i].HasCompleted()) 
						{
							HandleCompletedQuest();
						}
					}
				}
			}
			HandleAddQuest ();
		}

		public void OnAddItem() 
		{
			// Check quests for item type conditions
			for (int i = 0; i < MyQuests.Count; i++) 
			{
				if (MyQuests[i].QuestTaker == this)
				{	// only check if i am the one doing the quest
					bool HasFinished = MyQuests[i].HasCompleted();

					bool DidTrigger = MyQuests[i].OnAddItem(gameObject);

					if (!HasFinished && MyQuests[i].HasCompleted()) 
					{
						HandleCompletedQuest();
					}
					
					if (DidTrigger) {
						OnAddQuest.Invoke();
					}
				}
			}
			HandleAddQuest ();
		}
		public void OnZone(string ZoneName, string Action) {
			//Debug.LogError ("Handing Leaving of: " + ZoneName);
			for (int i = 0; i < MyQuests.Count; i++) {
				bool HasFinished = MyQuests[i].HasCompleted();

				bool DidTrigger = MyQuests[i].OnZone(ZoneName, Action);

				if (!HasFinished && MyQuests[i].HasCompleted()) 
				{
					HandleCompletedQuest();
				}
				if (DidTrigger) {
					OnAddQuest.Invoke();
				}
			}
			HandleAddQuest ();
		}
		public void HandleAddQuest() {
			if (OnAddQuest != null) {
				OnAddQuest.Invoke();
			}
		}

		// Checks

		// returns true if the user has the quest
		public bool HasQuest(string QuestName) {
			Quest MyQuest = GetQuest (QuestName);
			return (MyQuest != null);
		}
		
		// returns true if the user has handed in the quest
		public bool HasHandedInQuest(string QuestName) {
			Quest MyQuest = GetQuest (QuestName);
			if (MyQuest == null)
				return false;
			return (MyQuest.IsHandedIn);
		}
		// returns true if the user has unfinishquest
		public bool HasUnfinishedQuest(string QuestName) 
		{
			Quest MyQuest = GetQuest (QuestName);
			if (MyQuest == null) {
				return false;
			} 
			else 
			{
				return !MyQuest.HasCompleted();
			}
		}
		// returns true if the user has the completed quest and not handed it in
		public bool HasCompletedQuest(string QuestName) 
		{
			Quest MyQuest = GetQuest (QuestName);
			if (MyQuest == null) {
				return false;
			} 
			else 
			{
				if (MyQuest.HasCompleted() && !MyQuest.IsHandedIn)
					return true;
				else 
					return false;
			}
		}
	}
}