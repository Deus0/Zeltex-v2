using UnityEngine;
using System.Collections;
using DialogueSystem;

namespace QuestSystem {
	// what makes the quest true or not
	[System.Serializable]
	public class Condition 
	{
		public static string ObjectNameColor = "#d1d9e1";
		private bool IsCompleted;
		public string ConditionType = "";	//
		public string ObjectName = "";

		public bool bIsTakeItems;
		public int ItemQuantity;
		public int ItemQuantityState = 0;
		//public string ItemName;

		public void Complete() {
			IsCompleted = true;
		}
		public void SetCompleted(bool IsTrue) {
			IsCompleted = IsTrue;
		}
		public bool HasCompleted() {
			if (IsInventory ()) {
				if (ItemQuantity == ItemQuantityState) {
					IsCompleted = true;
				} else {
					IsCompleted = false;
				}
				return IsCompleted;
			} else {
				return IsCompleted;
			}
		}
		// one for each condition type
		public bool IsInventory() {
			return (ConditionType == "Inventory");
		}
		public bool IsLeaveZone() {
			return (ConditionType == "LeaveZone");
		}
		public bool IsEnterZone() {
			return (ConditionType == "EnterZone");
		}
		public bool IsTalkTo() {
			return (ConditionType == "TalkTo");
		}

		public string GetDescriptionText() 
		{
			string MyDescriptionText = "";
			if (IsCompleted) {
				MyDescriptionText += "[X] ";
			} else {
				MyDescriptionText += "[O] ";
			}
			if (IsInventory()) {
				MyDescriptionText += ObjectName + " x<color="+ObjectNameColor+">["+ ItemQuantityState + "/" + ItemQuantity + "]</color>";
			}
			if (ConditionType == "LeaveZone") 
			{
				MyDescriptionText += "Leave zone <color="+ObjectNameColor+">[" + ObjectName + "]</color>";
			}
			else if (ConditionType == "EnterZone") 
			{
				MyDescriptionText +=  "Enter zone <color="+ObjectNameColor+">[" + ObjectName + "]</color>";
			}
			else if (ConditionType == "TalkTo") 
			{
				MyDescriptionText +=  "Talk to <color="+ObjectNameColor+ ">[" + ObjectName+ "]</color>";
			}
			return MyDescriptionText;
		}

		// returns true if its triggered
		public bool OnZone(string CheckZone, string Action) 
		{
			ObjectName = SpeechUtilities.CheckStringForLastChar (ObjectName);
			CheckZone = SpeechUtilities.CheckStringForLastChar (CheckZone);
			if (IsCompleted)
				return false;
			
			if (CheckZone == ObjectName) 
			{
				if (ConditionType == "LeaveZone" && Action == "LeaveZone") {
					IsCompleted = true;
					return true;
				} 
				if (ConditionType == "EnterZone" && Action == "EnterZone") {
					IsCompleted = true;
					return true;
				}
			}
			return false;
		}
	}
}