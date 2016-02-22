using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
	public class ItemGenerator : MonoBehaviour {
		public bool IsGenerateAllItems = false;
		public List<int> SpellsToAddToInventory = new List<int> ();
		public List<int> ItemsToAddToInventory = new List<int>();
		public List<string> SpellNamesToAddToInventory = new List<string> ();
		public List<string> ItemNamesToAddToInventory = new List<string>();
		private SpellManager MySpellsManager;
		public bool IsChance = false;
		//private ItemManager MyItemManager;

		// Use this for initialization
		void Start () {
			MySpellsManager = GameObject.Find ("MySpellsManager").GetComponent<SpellManager>();
			GenerateInventoryData();
			Destroy (gameObject.GetComponent<ItemGenerator> ());
		}

		public void GenerateInventoryData() {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			DataManager MyData = GetManager.GetDataManager ();
			if (MyData) {
				if (ItemsToAddToInventory.Count > SpellsToAddToInventory.Count) {
					AddItemsToInventory();
					AddSpellsToInventory();
				} else {
					AddSpellsToInventory();
					AddItemsToInventory();
				}
				//MyInventory.IconsList = MyData.IconsList;
			}
		}
		
		public void AddItemsToInventory() {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			DataManager MyData = GetManager.GetDataManager ();
			if (IsGenerateAllItems) {
				ItemsToAddToInventory.Clear ();
				for (int i = 0; i <  MyData.ItemsList.Count; i++) {
					ItemsToAddToInventory.Add (i);
				}
			} 
			int ItemsIconIndex = MyCharacter.MyInventory.IconsList.Count;
			for (int i = 0; i < ItemsToAddToInventory.Count; i++) {
				int NewItemIndex = ItemsToAddToInventory [i];
				AddItemToInventory(NewItemIndex);
			}
			for (int i = 0; i < ItemNamesToAddToInventory.Count; i++) {
				int NewItemIndex = ItemNameToItemIndex(ItemNamesToAddToInventory [i]);
				AddItemToInventory(NewItemIndex);
			}
		}
		public int ItemNameToItemIndex(string ItemName) {
			DataManager MyData = GetManager.GetDataManager ();
			for (int i = 0; i < MyData.ItemsList.Count; i++) {
				if ( MyData.ItemsList[i].Name == ItemName) {
					return i;
				}
			}
			return -1;
		}
		public void AddItemToInventory(int NewItemIndex) {
			DataManager MyData = GetManager.GetDataManager ();
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			if (NewItemIndex >= 0 && NewItemIndex < MyData.ItemsList.Count) {
				//MyData.ItemsList [NewItemIndex].MyTextureId = i;
				MyCharacter.MyInventory.AddItem (new Item (MyData.ItemsList [NewItemIndex]));
				//MyInventory.ItemsList[MyInventory.ItemsList.Count-1].MyTextureId = i;
				/*Icon NewIcon = new Icon ();	
				NewIcon.Index = NewItemIndex;
				NewIcon.MyIconType = IconType.Item;
				MyCharacter.MyInventory.IconsList.Add (NewIcon);*/
			}
		}
		
		public void AddSpellsToInventory() {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			if (IsGenerateAllItems) {
				SpellsToAddToInventory.Clear ();
				for (int i = 0; i <  MySpellsManager.SpellsList.Count; i++) {
					SpellsToAddToInventory.Add (i);
				}
			}
			int SpellsIconIndex = MyCharacter.MyInventory.IconsList.Count;
			for (int i = 0; i < SpellsToAddToInventory.Count; i++) {
				int NewSpellIndex = SpellsToAddToInventory [i];
				AddSpellToInventory(NewSpellIndex);
			}
			float Chance = Random.Range(0,100);
			for (int i = 0; i < SpellNamesToAddToInventory.Count; i++) {
				if (!IsChance || Chance > 100-i*(100/SpellNamesToAddToInventory.Count) || (IsChance && i == SpellNamesToAddToInventory.Count-1)) {
					int NewSpellIndex = SpellNameToSpellIndex(SpellNamesToAddToInventory [i]);
					AddSpellToInventory(NewSpellIndex);
					if (IsChance) 
						break;
				}
			}
		}
		public int SpellNameToSpellIndex(string SpellName) {
			for (int i = 0; i < MySpellsManager.SpellsList.Count; i++) {
				if (MySpellsManager.SpellsList[i].Name == SpellName) {
					return i;
				}
			}
			return -1;
		}
		public void AddSpellToInventory(int NewSpellIndex) {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter>();
			if (NewSpellIndex >= 0 && NewSpellIndex < MySpellsManager.SpellsList.Count) {
				MyCharacter.MyInventory.AddSpell (new Spell (MySpellsManager.SpellsList [NewSpellIndex]));
				/*Icon NewIcon = new Icon ();
				NewIcon.Index = MyCharacter.MyInventory.SpellsList.Count-1;
				NewIcon.MyIconType = IconType.Spell;
				MyCharacter.MyInventory.IconsList.Add (NewIcon);*/
			}
		}
	}
}
