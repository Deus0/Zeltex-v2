using UnityEngine;
using System.Collections;
using QuestSystem;

namespace DialogueSystem {
	public static class DialogueFunctions {
		public static string[] MyCommands = new string[]{"givequest", "giveitem", "completequest", "exit", "trade", "openstats", "openquestlog" , "execute"};

		public static bool IsFunction(DialogueData MyDialogue, string MyLine, GameObject MyCharacter) {
			string Other = SpeechUtilities.RemoveCommand(MyLine);
			if (MyLine.Contains ("/givequest "))	// gives a quest to the player
			{
				//IsQuestGive = true;
				MyDialogue.InputString = Other;
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<SpeechHandler>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine2,
					                                                         MyCharacter.GetComponent<QuestLog>().GiveCharacterQuest);
				#else
				//if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
				//	MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<QuestLog>().SignOffQuest(); });
				#endif
				return false;
			}  
			else if (MyLine.Contains ("/giveitem "))	// gives a quest to the player
			{
				//IsQuestGive = true;
				MyDialogue.InputString = Other;
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<SpeechHandler>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine2,
					                                                         MyCharacter.GetComponent<ItemSystem.Inventory>().GiveItem);
				#else
				//if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
				//	MyDialogue.OnNextLine2.AddListener(delegate{ MyCharacter.GetComponent<ItemSystem.Inventory>().GiveItem();});
				#endif
				return false;
			}  
			// if player has finished a quest, rewards them and removes the quest here!
			else if (MyLine.Contains ("/completequest "))
			{
				//AddCondition("completequest");
				MyDialogue.InputString = Other;
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<SpeechHandler>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine2,
					                                                         MyCharacter.GetComponent<QuestLog>().SignOffQuest);
				#else
				//if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
				//	MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<QuestLog>().SignOffQuest(QuestName);});
				#endif
				return true;
			}
			// ends the dialogue
			else if (MyLine.Contains("/exit"))
			{
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<SpeechHandler>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine,
					                                                         MyCharacter.GetComponent<SpeechHandler>().ExitChat
					                                                         );
				#else
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<SpeechHandler>().ExitChat();});
				#endif
				return false;
			}
			else if (MyLine.Contains("/trade"))
			{
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine,
					                                                         MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToInventory
					                                                         );
				#else
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToInventory();});
				#endif
			}
			else if (MyLine.Contains("/openstats"))
			{
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine,
					                                                         MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToStats
					                                                         );
				#else
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToStats();});
				#endif
			}
			else if (MyLine.Contains("/openquestlog"))
			{
				#if UNITY_EDITOR
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine,
					                                                         MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToQuestLog
					                                                         );
				#else
				if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
					MyDialogue.OnNextLine.AddListener(delegate{ MyCharacter.GetComponent<GuiSystem.GuiManager>().SwitchToQuestLog();});
				#endif
			}
			else if (MyLine.Contains ("/execute "))	// I guess this is the main one!
			{
				MyLine = SpeechUtilities.RemoveCommand(MyLine);
				AddExecuteCommand(MyDialogue, MyLine, MyCharacter);
				return true;
			}
			return false;
		}

		
		//Syntax - GameObject.ComponentName.FunctionName !!
		public static void AddExecuteCommand(DialogueData MyDialogue, string ExecuteCommand, GameObject MyCharacter) {
			string[] MyCommands = ExecuteCommand.Split('.');
			//QuestIndex = int.Parse(SavedData[i])-1;
			//Debug.LogError(FunctionName);
			if (MyCommands.Length == 3 || MyCommands.Length == 3) 
			{
				//Debug.LogError("Object: " + MyCommands[0] + " : " + MyCommands[0].Length);
				//Debug.LogError("ComponentName: " + MyCommands[1] + " : " + MyCommands[1].Length);
				//Debug.LogError("FunctionName: " + MyCommands[2] + " : " + MyCommands[2].Length);
				GameObject MyTargetObject = GameObject.Find (MyCommands[0]);
				if (MyCommands.Length == 3)
					MyTargetObject = GameObject.Find (MyCommands[0]);
				else 
					MyTargetObject = MyCharacter;
				if (MyTargetObject) {
					//Debug.LogError("Found Target GameObject: " + MyTargetObject.name);
					Component MyComponent;
					if (MyCommands.Length == 3)
						MyComponent = MyTargetObject.GetComponent(MyCommands[1]);
					else
						MyComponent = MyTargetObject.GetComponent(MyCommands[0]);
					if (MyComponent) {
						//Debug.LogError("Found Target Component: " + MyComponent.name);
						//FunctionName = MyCommands[2];
						string MyFunctionName;
						if (MyCommands.Length == 3)
							MyFunctionName = MyCommands[2];
						else
							MyFunctionName = MyCommands[1];
						#if UNITY_EDITOR
						UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyDialogue.OnNextLine,
						                                                               MyComponent.BroadcastMessage,
						                                                               MyFunctionName);
						#else
						MyDialogue.OnNextLine.AddListener(delegate{
							//Debug.LogError("Trying to: " + FunctionName + " to " + MyComponent.name);
							if (MyComponent != null)
								MyComponent.BroadcastMessage(MyCommands[2]);
						});
						#endif
					} else {
						Debug.LogError("Cannot find target Component: " + MyCommands[1] + " for function in dialogue.");
					}
				} else {
					Debug.LogError("Cannot find target object for function in dialogue.");
				}
			} 
			if (MyCommands.Length == 1) {
				#if UNITY_EDITOR
				UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyDialogue.OnNextLine,
				                                                               MyCharacter.BroadcastMessage,
				                                                               MyCommands[0]);
				#else
				MyDialogue.OnNextLine.AddListener( delegate{
					//Debug.LogError("Trying to: " + FunctionName + " to " + MyComponent.name);
					MyCharacter.BroadcastMessage(MyCommands[0]);
				});
				#endif
			} else {
				Debug.LogError("Dialogue Execute MyCommands.Length: " + MyCommands.Length);
			}
		}

	}
}