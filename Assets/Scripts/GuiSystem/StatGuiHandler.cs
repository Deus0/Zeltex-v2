using UnityEngine;
using System.Collections;
using CharacterSystem;
using ItemSystem;

namespace GuiSystem {
	public class StatGuiHandler : GuiListHandler {
		public GameObject MyCharacter;
		//public CharacterSystem.Stats MyStats;
		// Update is called once per frame
		void Update () 
		{
			base.Update();
		}

		public void UpdateGuiStats() {
			/*{
				StatExt NewStatThing = new StatExt();
				NewStatThing.Name = "Blarg";
				Stat Blarg = (Stat)(NewStatThing);
				Debug.LogError("Testing " + Blarg.GuiString ());
			}*/
			Clear ();
			//MyStats.Data.Clear ();
			CharacterStats MyCharacterStats = MyCharacter.GetComponent<CharacterStats> ();
			//Debug.LogError("Testing " + MyCharacterStats.BaseStats.Data[0].GuiString ());
			if (MyCharacterStats) {
				for (int i = 0; i < MyCharacterStats.TempStats.GetSize(); i++) {
					AddStat(MyCharacterStats.TempStats.GetStat(i));
				}
			} else {
				ItemSystem.Inventory MyCharacterInventory = MyCharacter.gameObject.GetComponent<ItemSystem.Inventory> ();
				if (MyCharacterInventory != null) {
					CharacterSystem.Stats MyStats = new CharacterSystem.Stats();
					for (int i = 0; i < MyCharacterInventory.MyItems.Count; i++) {
						CharacterSystem.Stats ItemStats = MyCharacterInventory.MyItems [i].MyStats;
						for (int j = 0; j < ItemStats.GetSize(); j++) {
							CharacterSystem.Stat ItemStat = ItemStats.GetStat(j);
							MyStats.Add (ItemStat);
						}
					}
					for (int i = 0; i < MyStats.GetSize(); i++) {
						AddStat(MyStats.GetStat(i));
					}
				}
			}
		}
		private void AddStat(Stat NewStat) {
			TooltipData MyData = new TooltipData ();
			MyData.LabelText = NewStat.GetToolTipName();
			MyData.DescriptionText = NewStat.GetToolTipText ();
			/*try {
				//Debug.LogError("Attempting to add new stat ext " + NewStat.Name);
				StatExt MyStatExt = ((StatExt) NewStat);
				AddGui (MyStatExt.GuiString (), MyData);
			} catch (System.InvalidCastException e) {
				//Debug.LogError("Added new stat " + NewStat.Name);
				AddGui (NewStat.GuiString (), MyData);
			}*/
			AddGui (NewStat.GuiString (), MyData);
		}
	}
}
