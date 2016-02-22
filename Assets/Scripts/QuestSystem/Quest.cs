using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DialogueSystem;
using CharacterSystem;

namespace QuestSystem {

	// other rewards might be stats
	[System.Serializable]
	public class Reward 
	{
		public bool IsInventory;
		public int ItemQuantity;
		public string ItemName;

		public string GetDescriptionText() {
			if (IsInventory)
			{
				return ItemName + " x" + ItemQuantity;
			}
			return "";
		}
	}

	[System.Serializable]
	public class Quest 
	{
		public static string QuestNameColor = "#d1d9e1";
		public string Name;
		public string Description;		// 
		//public string Condition;		// a command code for the quests condition
		public int MyConditionIndex = 0;
		public List<Condition> MyConditions = new List<Condition>();
		public List<Reward> MyRewards = new List<Reward>();

		[SerializeField] private bool IsCompleted = false;
		public float TimeCompleted = 0f;
		public bool IsOrderedConditions = false;
		// reference to quest partipants
		public QuestLog QuestGiver = null;
		public QuestLog QuestTaker = null;
		public bool IsHandedIn = false;
		public float TimeHandedIn = 0f;

		public string GetLabelText() 
		{
			string MyLabelText = "";
			if (HasCompleted ())
				MyLabelText += "[X] ";
			else
				MyLabelText += "[O] ";
			MyLabelText += Name;
			return MyLabelText;
		}
		public string GetDescriptionText() 
		{
			string MyDescription = "";
			string QuestGiverName = "Invalid";
			if (QuestGiver)
				QuestGiverName = QuestGiver.name;
			MyDescription += "Quest Giver <color=" + QuestNameColor + ">[" + QuestGiverName + "]</color>\n";
			MyDescription += Description;
			if (MyConditions.Count > 0) {
				MyDescription += "\nConditions";
				for (int i = 0; i < MyConditions.Count; i++) {
					MyDescription += "\n\t";
					MyDescription += MyConditions [i].GetDescriptionText ();
				}
			} else {
				MyDescription += "\nNo Conditions.";
			}
			if (MyRewards.Count > 0) {
				MyDescription += "\nRewards:";
				for (int i = 0; i < MyRewards.Count; i++) {
					MyDescription += "\n\t";
					MyDescription += MyRewards [i].GetDescriptionText ();
				}
			} else {
				MyDescription += "\nNo Rewards.";
			}
			if (IsHandedIn) {
				MyDescription += "\nHanded in on: " + TimeHandedIn;
			}
			return MyDescription;
		}
		// conditions
		public bool HasGivenQuestOut()
		{
			if (QuestTaker == null)
				return false;
			else
				return true;
		}

		public void HandIn() {
			if (!IsHandedIn) 
			{
				CompleteQuest ();	// incase it isn't complete
				IsHandedIn = true;
				TimeHandedIn = Time.time;
			}
		}
		public void CompleteQuest() {
			if (!IsCompleted) 
			{
				IsCompleted = true;
				TimeCompleted = Time.time;
			}
		}
		public Condition GetCurrentCondition() {
			if (MyConditionIndex < MyConditions.Count)
				return MyConditions [MyConditionIndex];
			else
				return MyConditions [MyConditions.Count-1];
		}

		public Quest() {}

		public bool HasGivenToSomeone() {
			return (QuestTaker != null);
		}

		public bool HasCompleted() {
			return IsCompleted;
		}

		public bool OnZone(string CheckZone, string Action) 
		{
			bool DidTrigger = false;
			//Debug.Log (Name + " -Checking Zone for: " + CheckZone);
			for (int i = 0; i < MyConditions.Count; i++) 
			{
				if (!MyConditions[i].HasCompleted() && MyConditions[i].ConditionType.Contains("Zone")) 
				{
					DidTrigger = MyConditions[i].OnZone(CheckZone, Action);
				}
			}
			if (DidTrigger) 
			{
				IncreaseConditionIndex();
				CheckCompleted ();
			}
			return DidTrigger;
		}
		public void IncreaseConditionIndex() {
			if (GetCurrentCondition ().HasCompleted ()) {
				MyConditionIndex++;
				if (MyConditionIndex >= MyConditions.Count)
					MyConditionIndex = MyConditions.Count-1;
			}
		}

		// tells all the conditions that items have changed, so a check is done
		public bool OnAddItem(GameObject MyCharacter)
		{
			if (IsHandedIn)
				return false;
			bool DidTrigger = false;
			//ItemSystem.Inventory MyInventory = MyCharacter.gameObject.GetComponent<ItemSystem.Inventory> ();
			for (int i = 0; i < MyConditions.Count; i++) 
			{
				if (MyConditions[i].ConditionType.Contains("Inventory"))  
				{

					if (CheckInventory(MyConditions[i], MyCharacter))
						DidTrigger = true;
				}
			}

			if (DidTrigger) 
			{
				IncreaseConditionIndex();
				CheckCompleted ();
			}
			return DidTrigger;
		}

		public void CheckCompleted() {
			if (IsAllConditionsTrue())
				IsCompleted = true;
			else 
				IsCompleted = false;
		}

		public bool IsAllConditionsTrue() {
			for (int i = 0; i < MyConditions.Count; i++) 
			{
				if (MyConditions[i].HasCompleted() == false)
					return false;
			}
			return true;
		}

		// checks conditions
		// uses statistics, inventory classes
		public bool CheckInventory(Condition MyCondition, GameObject MyCharacter) {
			// inventory condition checks
			if (MyCondition.IsInventory()) 
			{
				ItemSystem.Inventory MyInventory = MyCharacter.gameObject.GetComponent<ItemSystem.Inventory> ();
				if (MyInventory) 
				{
					int MyQuantity = MyInventory.GetItemQuantity(MyCondition.ObjectName);
					if (MyQuantity != MyCondition.ItemQuantityState)
					{
						MyCondition.ItemQuantityState = MyQuantity;
						MyCondition.ItemQuantityState = Mathf.Clamp (MyCondition.ItemQuantityState, 0, MyCondition.ItemQuantity);
						return true;
					}
				}
			}
			return false;
		}

		//reading/writing section
		
		public static string[] MyCommands = new string[] {	"quest", 
															"description", 
															"leavezone", 
															"enterzone", 
															"reward", 
															"failure", 
															"items", 
															"talkto"
														 };
		public Quest(List<string> SavedData) {
			for (int i = 0; i < SavedData.Count; i++) {
				string Other = SpeechUtilities.RemoveCommand(SavedData[i]);
				if (SavedData[i].Contains ("/" + MyCommands[0] + " ")) 
				{
					string QuestName = Other;
					Name = QuestName;
				} 
				else if (SavedData[i].Contains ("/" + MyCommands[1] + " ")) // description
				{
					Description = Other;
				}
				else if (SavedData[i].Contains ("/" + MyCommands[2] + " "))	// leave zone
				{
					Condition NewCondition = new Condition();
					NewCondition.ConditionType = "LeaveZone";
					NewCondition.ObjectName = Other;
					MyConditions.Add(NewCondition);
				}  
				else if (SavedData[i].Contains ("/" + MyCommands[3] + " "))	// enter zone
				{
					Condition NewCondition = new Condition();
					NewCondition.ConditionType = "EnterZone";
					NewCondition.ObjectName = Other;
					MyConditions.Add(NewCondition);
				}
				else if (SavedData[i].Contains ("/" + MyCommands[7] + " "))	// talk to npc
				{

					Condition NewCondition = new Condition();
					NewCondition.ConditionType = "TalkTo";
					NewCondition.ObjectName = Other;
					MyConditions.Add(NewCondition);
				}  

				else if (SavedData[i].Contains ("/" + MyCommands[4] + " ")) // rewards
				{					
					List<string> MyRewardCommands = SpeechFileReader.SplitCommands(Other);
					
					if (MyRewardCommands.Count > 0) 
					{
						Reward NewReward = new Reward();
						NewReward.IsInventory = true;
						try { 
							string QuantityString = MyRewardCommands[MyRewardCommands.Count-1];
							NewReward.ItemQuantity  = int.Parse(QuantityString);
							string ItemName = Other.Remove(Other.IndexOf(QuantityString)-1, QuantityString.Length+1);
							NewReward.ItemName = //SpeechUtilities.CheckStringForLastChar
								(ItemName);
						} 
						catch(System.FormatException e) 
						{	
							NewReward.ItemName = //SpeechUtilities.CheckStringForLastChar
								(Other);
							NewReward.ItemQuantity = 1;
						}
						MyRewards.Add (NewReward);
					}
				}
				else if (SavedData[i].Contains ("/" + MyCommands[5] + " ")) // failure
				{
					SavedData[i] = Other;
					
				} 
				else if (SavedData[i].Contains ("/" + MyCommands[6] + " ")) // items needed by npc
				{
					SavedData[i] = Other;
					List<string> MyItems = SpeechFileReader.SplitCommands(SavedData[i]);

					if (MyItems.Count > 0) {
						Condition NewCondition = new Condition();
						NewCondition.ConditionType = "Inventory";
						try 
						{
							string QuantityString = MyItems[MyItems.Count-1];	// get last command
							NewCondition.ItemQuantity  = int.Parse(QuantityString);	// convert it to int
							SavedData[i] = SavedData[i].Remove(SavedData[i].IndexOf(QuantityString)-1, QuantityString.Length+1);
							NewCondition.ObjectName = SavedData[i];
						}
						catch(System.FormatException e) 
						{	
							NewCondition.ObjectName = SavedData[i];
							NewCondition.ItemQuantity = 1;
						}
						MyConditions.Add(NewCondition);
					}
				}
			}
		}
	}
}
