using UnityEngine;
using System.Collections;

namespace DialogueSystem {
	public class StatGuiHandler : GuiListHandler {
		public Character MyCharacter;
		public Stats MyStats;
		// Update is called once per frame
		void Update () {
			UpdateGuis ();
		}

		public void UpdateGuiStats() {
			Clear ();
			MyStats.Data.Clear ();

			Inventory MyCharacterInventory = MyCharacter.gameObject.GetComponent<Inventory> ();
			if (MyCharacterInventory != null) {
				for (int i = 0; i < MyCharacterInventory.MyItems.Count; i++) {
					Stats ItemStats = MyCharacterInventory.MyItems[i].MyStats;
					for (int j = 0; j < ItemStats.Data.Count; j++) {
						Stat ItemStat = ItemStats.Data[j];
						MyStats.Add(ItemStat);
					}
				}

				for (int i = 0; i < MyStats.Data.Count; i++) {
					AddGui(MyStats.Data[i].GuiString());
				}
			}
		}
	}
}
