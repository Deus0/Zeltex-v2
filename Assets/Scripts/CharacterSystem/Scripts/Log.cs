using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using CustomEvents;
/*
 * 	With custom Conditions
	Conditions used in dialogue tree and quests
		(dialogue tree: /first - /questcomplete etc
	Used for tracking of conditions:
		Talk to npc - Speech handler count
		Has Items:	- on Inventory.OnAddItem
			A has picked up an item
			A has dropped an item
			B has given A an item
			B has give A money
			A has given B an item
			A has given B money
		Stats Check:	- on CharacterStats.OnAddStats
			A Has has gained 5 strength
			A has lost 5 health
		Zone interaction:	- on Zone.On(Enter/Leave)Zone
			A has entered zone B
			A has left zone B
		Time of day:	- on Clock.OnChangeState
			It has become night time
			It has become sunrise
		Quest Status:	- on Quest.OnQuestConditionChange
			A has Completed a quest
			A has handed in a quest to B
			A has recieved a quest off B
			A has given a quest to B
*/


namespace CharacterSystem {
	[System.Serializable]
	public class LogEvent 
	{
		public float TimeHappened = 0f;
		public string EventType = "";

		public LogEvent(string NewEvent, float TimeHappened_) {
			EventType = NewEvent;
			TimeHappened = TimeHappened_;
		}
		public string GetLabelText() {
			float MyTime = ((int)(TimeHappened * 100f)) / 100f;
			TimeHappened = TimeHappened % 60f;
			string MyTimeLabel = MyTime.ToString();
			string MyTabs = "\t";
			if (MyTimeLabel.Length == 1)
				MyTabs += "\t";
			if (MyTimeLabel.Length >= 4)
				MyTabs = "";
			return "[" + MyTimeLabel + "] : " + MyTabs + EventType;
		}
	}

	public class Log : MonoBehaviour 
	{
		public bool IsDebugMode = false;
		public UnityEvent OnAddLog;	// check the conditions here - based on event type
		public MyEventString OnAddLogString;	// check the conditions here - based on event type
		public List<LogEvent> MyLogs = new List<LogEvent>();	// logs that have happaned
		// conditions to check

		void OnGUI() {
			if (IsDebugMode) {
				for (int i = 0; i < MyLogs.Count; i++) {
					GUILayout.Label(MyLogs[i].GetLabelText());
				}
			}
		}

		public void AddLogEvent(string NewEventType) {
			MyLogs.Add (new LogEvent (NewEventType, Time.time));
			OnAddLogString.Invoke (MyLogs[MyLogs.Count-1].GetLabelText());
		}
		public void AddLogEvent(GameObject MyTriggerObject, string NewEventType) {
			MyLogs.Add (new LogEvent (gameObject.name + " " + NewEventType + " " + MyTriggerObject.name, Time.time));
			OnAddLogString.Invoke (MyLogs[MyLogs.Count-1].GetLabelText());
		}
	}
}