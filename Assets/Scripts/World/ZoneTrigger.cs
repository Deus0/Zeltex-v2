using UnityEngine;
using System.Collections;
using CharacterSystem;
using UnityEngine.Events;
using QuestSystem;

namespace WorldUtilities {
	[System.Serializable]
	public class ZoneTrigger : MonoBehaviour {
		public MyEvent OnEnterZone;
		public MyEvent OnLeaveZone;
		//public QuestLog MyCharacter;
		
		void OnTriggerEnter(Collider other) {
			//Debug.LogError ("I have gotten away from seth! " + other.gameObject.name);
			QuestLog MyLeavingCharacter = other.gameObject.GetComponent<QuestLog> ();
			if (MyLeavingCharacter)
			{
				Debug.Log ("Player: " + MyLeavingCharacter.name + " has entered zone " + name + " at " + Time.time);
				MyLeavingCharacter.OnZone(gameObject.name, "EnterZone");
			}
			OnEnterZone.Invoke (other.gameObject);
		}
		void OnTriggerExit(Collider other) {
			//Debug.LogError ("I have gotten away from seth! " + other.gameObject.name);
			QuestLog MyLeavingCharacter = other.gameObject.GetComponent<QuestLog> ();
			if (MyLeavingCharacter)
			{
				Debug.Log ("Player: " + MyLeavingCharacter.name + " has left zone " + name + " at " +Time.time);
				MyLeavingCharacter.OnZone(gameObject.name, "LeaveZone");
			}
			OnLeaveZone.Invoke (other.gameObject);
		}
	}
}