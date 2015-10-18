using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


/*
	Handles just the gui's of quests
		
*/

namespace DialogueSystem {
	public class QuestLogGuiHandler : MonoBehaviour {
		public Color32 MyQuestColour = new Color (0, 22, 66);
		public Color32 MyQuestCompleteColour = new Color (0, 55, 155);
		public List<GameObject> MyQuestGuis = new List<GameObject> ();
		public GameObject QuestGuiPrefab;
		public int ScrollPosition = 0;
		public Character MyCharacter;

		// Use this for initialization
		void Start () {
			for (int i = 0; i < transform.childCount; i++) {
				MyQuestGuis.Add (transform.GetChild (i).gameObject);
			}
		}
		
		// Update is called once per frame
		void Update () {
			//if (Input.GetKeyDown (KeyCode.V)) {
			//	AddQuestGui("Generic Quest");
			//}
			if (Input.GetKeyDown (KeyCode.U)) {
				CheckQuestCompletitions();
			}
			for (int i = 0; i < MyQuestGuis.Count; i++) {
				MyQuestGuis[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(MyQuestGuis[i].GetComponent<RectTransform>().anchoredPosition, 
				                                                 new Vector2 (0, -(i-ScrollPosition) * 100f-30),
				                                                 Time.deltaTime);
			}
		}

		public void CheckQuestCompletitions() {
			if (MyCharacter) {
				for (int i = 0; i < MyQuestGuis.Count; i++) {
					if (MyCharacter.MyQuests [i].HasCompleted()) {
						MyQuestGuis [i].GetComponent<RawImage> ().color = MyQuestColour;
					} else {
						MyQuestGuis [i].GetComponent<RawImage> ().color = MyQuestCompleteColour;
					}
				}
			}
		}

		public void Scroll(bool Direction) {
			if (Direction) {
				if (ScrollPosition < MyQuestGuis.Count-1-5)	// 5 is the size that it can hold
				ScrollPosition++;
			} else {
				if (ScrollPosition > 0)
				ScrollPosition--;
			}

		}

		public void RemoveQuest(int QuestIndex) {
			Destroy(MyQuestGuis[QuestIndex].gameObject);
			MyQuestGuis.RemoveAt (QuestIndex);
		}

		public void AddQuestGui(string NewQuestName) {
			string NewName = (MyQuestGuis.Count+1) + ": " + NewQuestName;
			GameObject NewQuestGui = (GameObject)Instantiate (QuestGuiPrefab, 
			                                                  new Vector3 (0, -MyQuestGuis.Count * 100f, 0), 
			                                                  Quaternion.identity);
			NewQuestGui.name = NewName;
			NewQuestGui.transform.GetChild(0).gameObject.GetComponent<Text>().text = NewName;

			//SpeechAnimator MySpeech = NewQuestGui.transform.GetChild (0).gameObject.GetComponent<SpeechAnimator> ();
			//MySpeech.MyAudioSource = gameObject.GetComponent<AudioSource> ();

			//MySpeech.NewLine (NewName);
			//MySpeech.ResetAnimation ();

			NewQuestGui.transform.SetParent (gameObject.transform, false);
			MyQuestGuis.Add (NewQuestGui);
			CheckQuestCompletitions ();
		}
	}
}