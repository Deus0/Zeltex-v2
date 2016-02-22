using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
//using CharacterSystem;
using QuestSystem;

/*
	Handles just the gui's of quests
		
*/

namespace GuiSystem {
	public class QuestLogGuiHandler : GuiListHandler {
		public GameObject MyCharacter;
		public Color32 MyQuestColour = new Color (0, 22, 66);
		public Color32 MyQuestCompleteColour = new Color (0, 55, 155);
		public Color32 MyHandedInQuestColour = new Color (0, 55, 155);
		public bool IsCompletedOnly = false;
		public bool IsNonCompletedOnly = false;
		
		// Update is called once per frame
		void Update () {
			base.Update ();
		}
		
		public void ResetFilters() {
			IsCompletedOnly = false;
			IsNonCompletedOnly = false;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}
		public void FilterNonCompletedOnly() {
			IsCompletedOnly = false;
			IsNonCompletedOnly = true;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}
		public void FilterCompletedOnly() {
			IsCompletedOnly = true;
			IsNonCompletedOnly = false;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}

		public void UpdateQuestGuis() {
			QuestLog MyQuestLog = MyCharacter.GetComponent<QuestLog> ();
			Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			if (MyQuestLog) {
				Clear ();
				for (int i = 0; i < MyQuestLog.MyQuests.Count; i++) {
					if (IsRenderQuest (MyQuestLog.MyQuests [i])) {
						TooltipData MyData = new TooltipData ();
						MyData.LabelText = MyQuestLog.MyQuests [i].GetLabelText();
						MyData.DescriptionText = MyQuestLog.MyQuests [i].GetDescriptionText ();
						AddGui (MyData.LabelText, MyData);
					}
				}
				CheckQuestCompletitions ();	// always keep colours up to date!
			}
		}

		public bool IsRenderQuest(QuestSystem.Quest MyQuest) {
			if ((!IsCompletedOnly && !IsNonCompletedOnly) || 
			    (IsCompletedOnly && MyQuest.HasCompleted()) || 
			    (IsNonCompletedOnly && !MyQuest.HasCompleted())) 
				return true;
			return false;
		}
		public void CheckQuestCompletitions() {
			QuestLog MyQuestLog = MyCharacter.GetComponent<QuestLog> ();
			if (MyQuestLog) {
				int j = 0;
				for (int i = 0; i < MyQuestLog.MyQuests.Count; i++) {
					if (IsRenderQuest(MyQuestLog.MyQuests[i]))
					{
						if (MyQuestLog.MyQuests [i].IsHandedIn) {
							MyGuis [j].GetComponent<RawImage> ().color = MyHandedInQuestColour;
						} 
						else if (MyQuestLog.MyQuests [i].HasCompleted()) {
							MyGuis [j].GetComponent<RawImage> ().color = MyQuestColour;
						} else {
							MyGuis [j].GetComponent<RawImage> ().color = MyQuestCompleteColour;
						}
						j++;
					}
				}
			}
		}
	}
}