using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DialogueSystem {
	public static class DialogueConditions {
		public static string[] MyCommands = new string[]{"first", "noquest", "unfinishedquest", "hascompletedquest", "handedinquest", "next", "options" };

		public static bool IsCondition(DialogueData MyDialogue, string MyLine) {
			//	===-----=== Conditions ===-----===
			// if the first time talking to the npc, it will give a different route
			if (MyLine.Contains("/" + MyCommands[0] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				try {
					int NewIndex = int.Parse(MyLine);
					MyDialogue.AddCondition("first", NewIndex-1);	
				} catch(System.FormatException e) {
					
				}
				return true;
			}
			else if (MyLine.Contains ("/" + MyCommands[1] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				try {
					int NewIndex = int.Parse(MyLine);
					MyDialogue.AddCondition("noquest", NewIndex-1);
				} catch(System.FormatException e) {
					Debug.LogError(e.ToString() + " Error on line: " + MyLine);
				}
				return true;
			}
			else if (MyLine.Contains ("/" + MyCommands[2] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				try {
					int NewIndex = int.Parse(MyLine);
					MyDialogue.AddCondition("unfinishedquest", NewIndex-1);
				} catch(System.FormatException e) {
					Debug.LogError(e.ToString() + " Error on line: " + MyLine);
				}
				return true;
			}
			else if (MyLine.Contains ("/" + MyCommands[3] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				try {
					int NewIndex = int.Parse(MyLine);
					MyDialogue.AddCondition("hascompletedquest", NewIndex-1);
				} catch(System.FormatException e) {
					Debug.LogError(e.ToString() + " Error on line: " + MyLine);
				}
				return true;
			}
			else if (MyLine.Contains ("/" + MyCommands[4] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				try {
					int NewIndex = int.Parse(MyLine);
					MyDialogue.AddCondition("handedinquest", NewIndex-1);
				} catch(System.FormatException e) {
					Debug.LogError(e.ToString() + " Error on line: " + MyLine);
				}
				return true;
			}
			// can specify a different route for the dialogue tree
			else if (MyLine.Contains ("/" + MyCommands[5] + " "))
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				List<int> MyInts = SpeechUtilities.GetInts(MyLine);
				
				if (MyInts.Count == 1) 
				{
					//Debug.LogError("Adding Next variable");
					MyDialogue.AddCondition("default", MyInts[0]-1);
					//Debug.LogError(": " + MyDialogue.MyConditions[MyConditions.Count-1].Command);
				} else {
					//Debug.LogError("Problem reading next variable");
				}
			}
			else if (MyLine.Contains ("/" + MyCommands[6] + " "))	// || MyLine.Contains ("/next"))
			{
				MyDialogue.ActivateOptionsCondition(MyLine);
				return true;
			}
			return false;
		}
	}
}