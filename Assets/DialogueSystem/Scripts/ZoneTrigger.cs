using UnityEngine;
using System.Collections;

namespace DialogueSystem {
	public class ZoneTrigger : MonoBehaviour {
		public Character MyCharacter;
		void OnTriggerExit(Collider other) {
			//Debug.LogError ("I have gotten away from seth! " + other.gameObject.name);
			Character MyLeavingCharacter = other.gameObject.GetComponent<Character> ();
			if (MyLeavingCharacter) {
				for (int i = 0; i < MyCharacter.MyQuests.Count; i++) 
				{
					//Debug.LogError("Completing: " + MyCharacter.MyQuests[i].Name);
					MyLeavingCharacter.CompleteQuest(MyCharacter.MyQuests[i].Name);
				}
			}
		}
	}
}