using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
[System.Serializable]
public class Trigger {
	public char KeyPressed;
};

[System.Serializable]
public class Inventory {
	public int SelectedItem = 0;		// points to a icon in the inventory
	public int MaxActionBarItems = 5;
	public bool HasSwitchedItem = true;
	// Triggers
	public List<Trigger> TriggerList = new List<Trigger>();
	public bool IsShoot;
	public bool IsShoot2;
	public bool IsShoot3;

		int MaxIconSlots = 41;

	// Database
	public List<Icon> IconsList = new List<Icon>();
	public List<Spell> SpellsList = new List<Spell>();
	public List<Item> ItemsList = new List<Item>();
	public List<Gear> GearList = new List<Gear>();
	public List<Macro> MacroList = new List<Macro>();

	// clears all the data from inventory
	public void Clear() {
		IconsList.Clear ();
		for (int i = 0; i < MaxIconSlots; i++) {
			IconsList.Add (new Icon());
		}
		SpellsList.Clear ();
		ItemsList.Clear ();
		GearList.Clear ();
		MacroList.Clear ();
	}
	public BlockStructure GetSelectedBlockStructure() {
		Item ItemInUse = GetItem (SelectedItem);
		return GetManager.GetDataManager().BlockStructuresList[ItemInUse.BlockIndex];
	}
	public bool SetIcon(int IconIndex, Icon NewIcon) {
		if (IconIndex >= 0 && IconIndex < IconsList.Count) {
			IconsList[IconIndex] = NewIcon;
			return true;
		}
		return false;
	}
	public int FindEmptySlot() {
		//Debug.LogError ("Finding empty Icon Slot: " + IconsList.Count);
		for (int i = 0; i < IconsList.Count; i++) {
			if (IconsList[i].MyIconType == IconType.None) {
				return i;
			}
		}
		return -1;
	}
	
	public bool SelectItem(int NewItemSelected) {
		if (NewItemSelected >= 0 && NewItemSelected < IconsList.Count) {
			SelectedItem = NewItemSelected;
			return true;
		} else 
			return false;
	}
	
	public bool CheckItemInActionBar() {
		if (SelectedItem < 0)
			SelectedItem = MaxActionBarItems - 1;
		else if (SelectedItem >= MaxActionBarItems)
			SelectedItem = 0;
		else
			return false;
		return true;	// if has changed the position
	}
	public void SwitchItem(int Direction) {
		SelectedItem += Direction;
		CheckItemInActionBar ();
		HasSwitchedItem = true;
	}
	public void SetTriggerListDefaults() 
	{
		Trigger NewTrigger = new Trigger ();
		NewTrigger.KeyPressed = '1';
		TriggerList.Add(NewTrigger);
	}

	// finds an item with the same type to stack them
	public int FindStackItemIndex(string MyItemName) {
		//Debug.LogError ("Finding empty Icon Slot: " + IconsList.Count);
		for (int i = 0; i < IconsList.Count; i++) {
			if (IconsList[i].MyIconType == IconType.Item) {
				Item NewItem = GetItem (i);
				if (NewItem.Name == MyItemName && !NewItem.IsMax())
					return i;
			}
		}
		return -1;
	}
	public void ResetIconsTextures() {
		for (int i = 0; i < IconsList.Count; i++) {
			IconsList[i].NeedsUpdating = true;
		}
		HasSwitchedItem = true;
	}
	public Icon GetSelectedIcon() {
		return GetIcon (SelectedItem);
	}
	public Item GetSelectedItem() {
		return GetItem (SelectedItem);
	}
	public Spell GetSelectedSpell() {
		return GetSpell (SelectedItem);
	}

	//returns true if successfully adds item to inventory
	public bool AddItem(string ItemName) {
		Item NewItem = null;
		for (int i = 0; i < GetManager.GetDataManager().ItemsList.Count; i++) {
			if (GetManager.GetDataManager().ItemsList[i].Name == ItemName) {
				NewItem = new Item (GetManager.GetDataManager().ItemsList[i]);	// because itemindex is just blockindex, not the actual item!
				break;
			}
		}
		if (NewItem == null) {
			return false;
		}
		return AddItem (NewItem);
	}
	//returns true if successfully adds item to inventory
	
	public void AddItemWithoutIcon(Item NewItem) {
		ItemsList.Add (NewItem);
	}
	public void AddSpellWithoutIcon(Spell NewSpell) {
		SpellsList.Add (NewSpell);
	}
	public bool AddItem(Item NewItem) {
		for (int i = 0; i < ItemsList.Count; i++) {
			if (NewItem.Name == ItemsList[i].Name) {
				ItemsList[i].StackItem(NewItem);
				return true;
			}
		}
		
		int NewIconSlot = FindEmptySlot(); // as there are no items of the same type
		if (NewIconSlot != -1) {
			Debug.Log ("Adding new item of type: " + NewItem.Name + " at "  + Time.time);
			//NewItem = ;
			ItemsList.Add (NewItem);
			IconsList[NewIconSlot].MyIconType = IconType.Item;
			IconsList[NewIconSlot].Index = ItemsList.Count - 1;
			IconsList[NewIconSlot].NeedsUpdating = true;
			HasSwitchedItem = true;
			return true;
		}
		return false;	// no empty slots, dont pickup item
	}
	//returns true if successfully adds spell to inventory
	public bool AddSpell(Spell NewSpell) {
		//Debug.LogError ("Attempting - Adding Item to inventory: " + NewSpell.Name);
		//if (IconsList.Count < MaxIconSlots) {
			int NewIconSlot = FindEmptySlot();
			if (NewIconSlot != -1) {
				SpellsList.Add (NewSpell);
				IconsList[NewIconSlot].MyIconType = IconType.Spell;
				IconsList[NewIconSlot].Index = SpellsList.Count - 1;
				IconsList[NewIconSlot].NeedsUpdating = true;
				HasSwitchedItem = true;
				return true;
			} else {
				SpellsList.Add (NewSpell);	// add spells anyway, can find it in the spellbook
			}
		//}
		return false;
	}
	public void UnSelectItem() {
		SelectedItem = -1;
		UpdateGui();
	}
	public void DropSelectedItem() {
		DropItem (SelectedItem);
	}
	public void UpdateGui() {
		HasSwitchedItem = true;
	}
	public void DropItem(int IconIndex) {
		if (IconIndex >= 0 && IconIndex < IconsList.Count) {
			int Index = IconsList[IconIndex].Index;
			if (IconsList[IconIndex].MyIconType == IconType.Item) {
				RemoveItem(Index);
			} else if (IconsList[IconIndex].MyIconType == IconType.Spell) {
				RemoveSpell(Index);
			}
			IconsList[IconIndex].Clear();	// sets it to none
			HasSwitchedItem = true;
		}
	}
	public void RemoveItem(int ItemIndex) {
		if (ItemIndex >= 0 && ItemIndex < ItemsList.Count) {
			ItemsList.RemoveAt (ItemIndex);
			// lower all the indexes in icons
			for (int i = 0; i < IconsList.Count; i++) {
				if (IconsList[i].MyIconType == IconType.Item) {
					if (IconsList[i].Index > ItemIndex) {
						IconsList[i].Index--;
					}
				}
			}
		}
	}
	// all that needs to happen is the removing of the icon 
	public void RemoveSpell(int SpellIndex) {
		if (SpellIndex >= 0 && SpellIndex < SpellsList.Count) {
			//SpellsList.RemoveAt (SpellIndex);
			// lower all the indexes in icons
			/*for (int i = 0; i < IconsList.Count; i++) {
				if (IconsList[i].MyIconType == IconType.Spell) {
					if (IconsList[i].Index > SpellIndex) {
						IconsList[i].Index--;
					}
				}
			}*/
		}
	}
	public void DecreaseSelectedItemQuantity() {
		DecreaseItemQuantity (SelectedItem);
	}
	public void DecreaseItemQuantity(int IconIndex) {
		//int ItemIndex = GetItemIndex(IconIndex);
		Item ItemInUse = GetItem (IconIndex);
		ItemInUse.DecreaseQuantity ();
		if (ItemInUse.IsEmpty()) {
			// there will be a problem as the indexes for icons won't match up past this point
			DropItem (IconIndex);
		}
		HasSwitchedItem = true;	// as quantity is changed, need to update the gui
	}
	public Texture GetSpellTexture(int Indexu) {
		//if (Indexu >= 0 && Indexu < SpellsList.Count)
		//return SpellsList [Indexu].MyTexture;
		return 
			new Texture ();
	}
	public Texture GetTexture(Icon Iconu) {
		if (Iconu.MyIconType == IconType.Spell) {
			//if (Iconu.Index >= 0 && Iconu.Index < SpellsList.Count)
			//	return SpellsList [Iconu.Index].MyTexture;
		} else if (Iconu.MyIconType == IconType.Item) {
			if (Iconu.Index >= 0 && Iconu.Index < ItemsList.Count)
				return ItemsList [Iconu.Index].MyTexture;
		}
		return 
			new Texture ();
	}
	
	public Texture GetTextureWithIndex(int Indexu) {
		if (Indexu >= 0 && Indexu < IconsList.Count) {
			return GetTexture (IconsList [Indexu]);
		}
		return 
			new Texture ();
	}

	public float GetCoolDownAsPercent(int Index) {
		if (Index >= 0 && Index < IconsList.Count) {
			Spell CoolDownSpell = GetSpell (Index);	// do the same thing for items
			if (CoolDownSpell != null)
				return CoolDownSpell.TimeLeft() / ((float)CoolDownSpell.CoolDown);
		}
		return 0;
	}
	
	public Icon GetIcon(int Index) {
		if (Index >= 0 && Index < IconsList.Count) {
				return IconsList[Index];
			}
		return new Icon ();
		//return null;
	}
	public Spell GetSpell(int Index) {
		if (Index >= 0 && Index < IconsList.Count) 
			if (IconsList[Index].MyIconType == IconType.Spell)
			if (IconsList[Index].Index >= 0 && IconsList[Index].Index < SpellsList.Count) {
				return SpellsList[ IconsList[Index].Index];
			}
		return null;
	}
	public Spell GetSpell2(int Index) {
		if (Index >= 0 && Index < SpellsList.Count) {
			return SpellsList[Index];
		}
		return null;
	}
	public Item GetItem(Icon MyIcon) {
		if (MyIcon.Index >=0 && MyIcon.Index < ItemsList.Count) 
		if (MyIcon.MyIconType == IconType.Item) {
			return ItemsList[MyIcon.Index];
		}
		return null;
	}

	public Item GetItem(int IconIndex) {
		if (IconIndex >= 0 && IconIndex < IconsList.Count) 
			if (IconsList[IconIndex].Index >=0 && IconsList[IconIndex].Index < ItemsList.Count) 
			if (IconsList[IconIndex].MyIconType == IconType.Item) {
				return ItemsList[IconsList[IconIndex].Index];
		}
		return null;
	}
	
	public int GetItemIndex(int Index) {
		if (Index >= 0 && Index < IconsList.Count) 
		if (IconsList[Index].Index >=0 && IconsList[Index].Index < ItemsList.Count) {
			return IconsList[Index].Index;
		}
		return -1;
	}
	public Item GetItem2(int Index) {
		if (Index >= 0 && Index < ItemsList.Count) {
			return ItemsList[Index];
		}
		//return new Item ();
		return null;
	}
	public void SwapItems(int ItemIndex1, int ItemIndex2) {
		if (ItemIndex1 >= 0 && ItemIndex1 >= 0 &&
		    ItemIndex1 < IconsList.Count && ItemIndex2 < IconsList.Count) {
			Icon TempIcon = IconsList[ItemIndex1];
			IconsList[ItemIndex1] = IconsList[ItemIndex2];
			IconsList[ItemIndex2] = TempIcon;
		}
	}
};
}