using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace DialogueSystem {
	public class GuiListHandler : MonoBehaviour {
		public List<GameObject> MyGuis = new List<GameObject> ();
		public GameObject GuiPrefab;
		private int ScrollPosition = 0;
		
		// Update is called once per frame
		void Update () {
			UpdateGuis ();
		}
		public void UpdateGuis() {
			for (int i = 0; i < MyGuis.Count; i++) {
				MyGuis [i].GetComponent<RectTransform> ().anchoredPosition = 
					Vector2.Lerp (MyGuis [i].GetComponent<RectTransform> ().anchoredPosition, 
				   					new Vector2 (0, -(i - ScrollPosition) * 100f - 30),
				    				Time.deltaTime);
			}
		}
	
	
		public void Scroll(bool Direction) {
			if (Direction) {
				if (ScrollPosition < MyGuis.Count-1-5)	// 5 is the size that it can hold
					ScrollPosition++;
			} else {
				if (ScrollPosition > 0)
					ScrollPosition--;
			}
			
		}

		public void Clear() {
			for (int i = 0; i < MyGuis.Count; i++) {
				Destroy(MyGuis[i]);
			}
			MyGuis.Clear ();
		}

		public void RemoveAt(int QuestIndex) {
			Destroy(MyGuis[QuestIndex].gameObject);
			MyGuis.RemoveAt (QuestIndex);
		}
		
		public void AddGui(string NewQuestName) {
			if (GuiPrefab == null) {
				GuiPrefab = new GameObject();
			}
			string NewName = (MyGuis.Count+1) + ": " + NewQuestName;
			GameObject NewQuestGui = (GameObject)Instantiate (GuiPrefab, 
			                                                  new Vector3 (0, -MyGuis.Count * 100f, 0), 
			                                                  Quaternion.identity);
			NewQuestGui.name = NewName;
			NewQuestGui.transform.GetChild(0).gameObject.GetComponent<Text>().text = NewName;
			
			NewQuestGui.transform.SetParent (gameObject.transform, false);
			MyGuis.Add (NewQuestGui);
		}
	}
}