using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Mostly handles item gui

// swapping items
// picking up items
// stacking them
// makes item follow mouse
// tooltips for items - when mouse over item
// dropping items by clicking, when holding an item, on something that isn't an item

// need to seperate the data of the ItemGuis from the 'is player swapping etc'
//   Crafting bench	(The 10th item is pickup only, and is determined by the 9 items and a recipe class, the recipe items are deducted from the others when its picked up!)
//   Treasure Chest (Inventory is stored in a block)
//	 hmmm... Perhaps things like Furnace?

[System.Serializable]
public class MyInventoryGuiWindow {
	// data sets
	public List<MyItemGui> MyItemGuis = new List<MyItemGui>();
	public bool IsSpellbook = false;
};

public class InventoryGui : MonoBehaviour {
	// references
	public BaseCharacter MyPlayer;					// set by character manager
	public Inventory MyInventory;					// set by character manager
	//public Inventory MyInventory = null;		// should link the inventory to the iton slots
	public GameObject ToolTipGui;

	// states
	public bool IsSwapItemMode = true;
	//public bool IsItemMoving = false;
	public bool IsItemSelected = false;
	public int OldSelectedItem = 0;	// index if icon selected
	public int NewSelectedItem = 0;	// index if icon selected

	public int SelectedWindowIndex;
	public List<MyInventoryGuiWindow> MyInventoryGuiWindows = new List<MyInventoryGuiWindow>();

	// textures
	public Texture ItemEmptyImage;
	public GameObject ItemOver;
	public Icon SelectedIcon = new Icon(IconType.None);
	public MyItemGui SelectedItemGui;
	public bool IsMouseOverItem = false;
	public int MouseOverIndex = 0;
	public Vector3 ToolTipOffset = new Vector3(50,50,0);

	int PreviousSelection;
	int PreviousWindowIndex;

	public void Update() {
		UpdateToolTip ();

		ItemFollowMouse ();
		
		if (MyPlayer && MyInventoryGuiWindows.Count > 0) {
			if (MyInventory == null)
				MyInventory = MyPlayer.MyInventory;

			CheckInventoryForUpdates ();	// updates textures, and text of items
			UpdateCoolDownAnimations ();
			
			if (MyInventory.HasSwitchedItem) {	// for item bar
				RepositionSelectedItemGraphic (0);	// only action bar uses selected item graphic
				MyInventory.HasSwitchedItem = false;
			}
		}
	}

	public void UpdateCoolDownAnimations() {
		for (int i = 0; i < 5; i++) {// MyPlayer.MyInventory.IconList.Count; i++) {
			if (i < MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis.Count) {
				float CoolDownLeft = MyPlayer.MyInventory.GetCoolDownAsPercent(i);	
				// this affects items for some reason
				if (MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis[i].ItemCoolDownAnimation != null)
					if (CoolDownLeft > 0)
				{
					//Debug.Log (i + " CoolDown: " + CoolDownLeft);
					MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis[i].ItemCoolDownAnimation.GetComponent<AnimatingTiledTexture>().Activate();
					MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis[i].ItemCoolDownAnimation.GetComponent<AnimatingTiledTexture>().SetIndexByPercent(CoolDownLeft);
				} else {		
					MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis[i].ItemCoolDownAnimation.GetComponent<AnimatingTiledTexture>().Stop();
				}
			}
		}
	}
	public void ItemFollowMouse() {
		if (SelectedItemGui.ItemImage.gameObject.activeSelf) {
				SelectedItemGui.ItemImage.gameObject.transform.position =
					new Vector3(Input.mousePosition.x,Input.mousePosition.y, 0);
		}
	}

	public void UpdatePlayer(BaseCharacter NewPlayer) {
		MyPlayer = NewPlayer;
		MyInventory = MyPlayer.MyInventory;
		CheckInventoryForUpdates ();
	}

	public void HideMovingItem() {
		SelectedItemGui.ItemImage.gameObject.SetActive (false);
	}

	/*public void ShowMovingItem() {
		if (SelectedItemGui != null) {
			SelectedItemGui.ItemImage.gameObject.SetActive (IsItemMoving);
		} else {
			Debug.Log ("No SelectedItemGui Inside InventoryGui Class");
		}
	}*/
	public void AddNewWindow(MyInventoryGuiWindow NewWindow) {
		MyInventoryGuiWindows.Add (NewWindow);
		SetItemGuiIndexes (MyInventoryGuiWindows.Count - 1);
	}
	public void SetItemGuiIndexes(int Index) {
		for (int i = 0; i < MyInventoryGuiWindows[Index].MyItemGuis.Count; i++) {
			//Selectable MyButton = MyItemGuis [i].ItemImage.gameObject.GetComponent <Selectable> ();
			ItemButton MyButtonScript = MyInventoryGuiWindows[Index].MyItemGuis [i].ItemImage.gameObject.GetComponent <ItemButton>();
			MyButtonScript.ItemButtonIndex = i;
			MyButtonScript.WindowIndex = Index;
		}
	}


	public void RepositionSelectedItemGraphic(int WindowIndex) {
			if (MyPlayer != null) {
				// relocate the selected item texture
				RectTransform ItemOverRec = GetManager.GetInventoryGui ().ItemOver.gameObject.GetComponent <RectTransform> ();
				if (MyInventory.SelectedItem >= 0 && MyInventory.SelectedItem < MyInventoryGuiWindows [WindowIndex].MyItemGuis.Count) {
					if (MyInventoryGuiWindows [WindowIndex].MyItemGuis [MyInventory.SelectedItem].ItemImage != null) {
						RectTransform ItemRec = MyInventoryGuiWindows [WindowIndex].MyItemGuis [MyInventory.SelectedItem].ItemImage.gameObject.GetComponent  <RectTransform> ();
						ItemOverRec.SetParent (ItemRec);
						ItemOverRec.anchoredPosition = new Vector2 ();
						ItemOverRec.localScale = new Vector2 (1, 1);
						ItemOverRec.sizeDelta = ItemRec.sizeDelta;
					}
				} else if (MyInventory.SelectedItem == -1) {
					GameObject ItemOverRec2 = GameObject.Find ("StatsBackground");
					if (ItemOverRec2) {
						ItemOverRec.SetParent (ItemOverRec2.transform);
						ItemOverRec.anchoredPosition = new Vector2 ();
						ItemOverRec.localScale = new Vector2 (1, 1);
						ItemOverRec.sizeDelta = ItemOverRec2.GetComponent<RectTransform> ().sizeDelta;
					} else {
						ItemOverRec.localScale = new Vector2 (0, 0);
					}
				}
			}
	}
	
	// this is called in player?? Maybe.. in the update thread
	public void CheckInventoryForUpdates() {
		for (int i = 0; i < MyInventoryGuiWindows.Count; i++) {
			UpdateInventoryGuiImages(MyPlayer, i);
			UpdateItemTexts(i);
		}
		// seperating these, lets the icons update on multiple windows first!
		for (int i = 0; i < MyPlayer.MyInventory.IconsList.Count; i++) {
			MyPlayer.MyInventory.GetIcon (i).NeedsUpdating = false;
			Item MyItem = MyPlayer.MyInventory.GetItem (MyPlayer.MyInventory.GetIcon(i));
			if (MyItem != null)
				MyItem.NeedsUpdateText = true;
		}
	}

	public void UpdateInventoryGuiImages(BaseCharacter MyPlayer, int WindowIndex) {
			// reset all the inventorygui's textures with inventory data
		for (int i = 0; i < MyInventoryGuiWindows[WindowIndex].MyItemGuis.Count; i++) {
			if (MyPlayer.MyInventory.GetIcon (i).NeedsUpdating) {
				Debug.Log ("Updating icon at: " + Time.time);
				UpdateIconsTexture (MyPlayer, i, WindowIndex);
			}
		}
	}

	public void UpdateItemTexts(int WindowIndex) {
		if (MyInventoryGuiWindows.Count > 0) {
			for (int i = 0; i < MyInventoryGuiWindows[WindowIndex].MyItemGuis.Count; i++) {
				Item MyItem = MyPlayer.MyInventory.GetItem (MyPlayer.MyInventory.GetIcon(i));
				if (MyItem != null)
				if (MyItem.NeedsUpdateText) {
					UpdateIconsTextureText (MyPlayer, i, WindowIndex);
				}
				if (MyPlayer.MyInventory.GetIcon (i).NeedsUpdating) {
					UpdateIconsTextureText (MyPlayer, i, WindowIndex);
				}
			}
		}
	}
	public void UpdateIconsTextureText(BaseCharacter MyPlayer, int i, int WindowsIndex) {
		if (MyInventoryGuiWindows [WindowsIndex].IsSpellbook) {
			MyInventoryGuiWindows [WindowsIndex].MyItemGuis [i].ItemText.text = "";
		} else {
			if (i >= 0 && i < MyInventory.IconsList.Count) {
				MyInventoryGuiWindows [WindowsIndex].MyItemGuis [i].ItemText.text = "";	// get icons image
				if (MyInventory.IconsList [i].MyIconType != IconType.None) {
					if (MyInventory.IconsList [i].MyIconType == IconType.Item) {
						Item MyItem = MyInventory.GetItem (i);	// objects stored in memory, changes to the variables work
						if (MyItem != null) {
							if (MyItem.MaxQuantity > 1)
								MyInventoryGuiWindows [WindowsIndex].MyItemGuis [i].ItemText.text = (MyItem.Quantity.ToString ());	// get icons image
						}
					}
				}
			}
		}
	}

	ItemButton GetSelectedItemButton(int ItemButtonIndex) {
		if (ItemButtonIndex >= 0 && ItemButtonIndex < MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis.Count)
			return MyInventoryGuiWindows[SelectedWindowIndex].MyItemGuis [ItemButtonIndex].ItemImage.gameObject.GetComponent<ItemButton> ();
		return null;
	}

	public void UpdateIconsTexture(BaseCharacter MyPlayer, int i, int WindowIndex) {
		if (!MyInventoryGuiWindows[WindowIndex].IsSpellbook) {
			if (i >= 0 && i < MyInventory.IconsList.Count) {
				if (MyInventory.IconsList [i].MyIconType != IconType.None) {
					// now it gets the InventoryGui and replaces its textures with the players inventory icon textures
					MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.enabled = true;
					MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.texture = MyInventory.GetTextureWithIndex (i);
						//GetSpellTexture
					// GetManager.GetMyDataManager().GetTextureWithIndex(i);//MyPlayer.MyInventory.IconsList[i].TextureIndex);//(i);	// get icons image
				} else {
					MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.texture = ItemEmptyImage;	// get icons image
				}
			} else {
				//if (i >= 0 && i < MyInventoryGuiWindows[WindowIndex].MyItemGuis.Count)
				//	MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.gameObject.SetActive(false);	//texture = ItemEmptyImage;	// get icons image
			}
		} else {	// spell book just has spells only
			if (i >= 0 && i < MyInventory.SpellsList.Count)
				MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.texture = MyInventory.GetSpellTexture (i);
			else
				MyInventoryGuiWindows[WindowIndex].MyItemGuis [i].ItemImage.texture = ItemEmptyImage;	// get icons image
		}
	}
	int SelectedItem = 0;
	// these functions should be in inventory class
	// this goes through all the button scripts and finds the selected one
	public int GetSelectedItem() {
		return SelectedItem;
	}
	public void HandleIconDeselected() {
		int NewSelectedItem = GetSelectedItem ();
		if (NewSelectedItem == -1) {	// if an item is enabled
			IsItemSelected = false;
			if (SelectedItemGui != null)
				SelectedItemGui.ItemImage.gameObject.SetActive (IsItemSelected);
		}
	}
	public void SetSelectedWindowIndex(int NewIndex) {
		PreviousWindowIndex = SelectedWindowIndex;
		SelectedWindowIndex = NewIndex;
		if (SelectedWindowIndex < 0)
			SelectedWindowIndex = 0;
		else if (SelectedWindowIndex >= MyInventoryGuiWindows.Count)
			SelectedWindowIndex = MyInventoryGuiWindows.Count - 1;
	}

	// activated from ItemButton class, when a button is selected
	public void HandleItemPressed(ItemButton MyPressedItem) {
		IsSwapItemMode = GetManager.GetGuiManager ().IsInventoryOpened ();
		SetSelectedWindowIndex (MyPressedItem.WindowIndex);
		PreviousSelection = GetSelectedItem();
		OldSelectedItem = NewSelectedItem;
		SelectedItem = MyPressedItem.ItemButtonIndex;
		NewSelectedItem = GetSelectedItem ();	// index if icon selected
		if (MyPlayer != null) {
			if (IsSwapItemMode) {
				if (Input.GetKey(KeyCode.LeftAlt)) {
					if (!MyInventoryGuiWindows[MyPressedItem.WindowIndex].IsSpellbook)
						MyInventory.DropItem(MyPressedItem.ItemButtonIndex);
				} else {
					SwapItems ();
				}
			} else {
				//MyInventory.SelectedItem = GetSelectedItem ();	// index if icon selected
				//int PreviousSelection = MyPlayer.SelectedItem;
				//RepositionSelectedItemGraphic(SelectedWindowIndex);
			}
		}
	}

	public void SwapItems() {
		// if previously no item was selected, it becomes a first select
		bool IsFirstSelect = !IsItemSelected;
		IsItemSelected = false;	// default to false
		//Debug.Log ("Item Selected!");
			//Player MyPlayerScript = (Player) MyPlayer.GetComponent ("Player");
			if (NewSelectedItem != -1) {	// if an item is enabled
				IsItemSelected = true;
					
				// Pickup Item
				if (SelectedIcon.MyIconType == IconType.None) {
					HandleFirstPickup();	// assumes no item currently selected
				} else {
					HandleSecondClick();
				}
				// if selected icon is nonexistant after switch, stop moving item
				if (SelectedIcon.MyIconType == IconType.None) {
					IsItemSelected = false;
				}
			}
	}
	public void HandleFirstPickup() {
		//IsItemMoving = true;
		// switches the items
		if (!MyInventoryGuiWindows[SelectedWindowIndex].IsSpellbook) {
			Icon TempIcon = new Icon (GetSelectedIcon ());
			MyPlayer.MyInventory.IconsList [SelectedItem] = new Icon (SelectedIcon);
			SelectedIcon = new Icon (TempIcon);
			UpdateIconsTexture (MyPlayer, SelectedItem, SelectedWindowIndex);
			UpdateIconsTextureText(MyPlayer, SelectedItem, SelectedWindowIndex);
			UpdateSelectedItemGui();
		} else {
			if (MyInventory.SelectedItem < 0 || MyInventory.SelectedItem >= MyInventory.SpellsList.Count) {
				SelectedIcon.Clear();
			} else {
				SelectedIcon = new Icon();
				SelectedIcon.MyIconType = IconType.Spell;
				SelectedIcon.Index = SelectedItem;
			}
			UpdateSelectedItemGui();
		}
	}
	public void HandleSecondClick() {
		if (!MyInventoryGuiWindows [SelectedWindowIndex].IsSpellbook) {
			Icon TempIcon = new Icon (MyPlayer.MyInventory.IconsList [(SelectedItem)]);
			MyPlayer.MyInventory.SetIcon (SelectedItem, new Icon (SelectedIcon));
			SelectedIcon = new Icon (TempIcon);
			UpdateIconsTexture (MyPlayer, PreviousSelection, PreviousWindowIndex);
			UpdateIconsTexture (MyPlayer, SelectedItem, SelectedWindowIndex);
			UpdateIconsTextureText (MyPlayer, PreviousSelection, PreviousWindowIndex);
			UpdateIconsTextureText (MyPlayer, SelectedItem, SelectedWindowIndex);

			UpdateSelectedItemGui();
		}
	}

	public void UpdateSelectedItemGui() {
		if (SelectedItemGui != null) {
			SelectedItemGui.ItemImage.gameObject.SetActive (SelectedIcon.MyIconType != IconType.None);
			if (SelectedIcon.MyIconType != IconType.None)  
			{
				SelectedItemGui.ItemImage.texture = MyPlayer.MyInventory.GetTexture (SelectedIcon);
				if (SelectedIcon.MyIconType == IconType.Item)
				{
					Item MyItem = MyPlayer.MyInventory.GetItem (SelectedIcon);
					if (MyItem != null) {
						SelectedItemGui.ItemText.text = MyItem.Quantity.ToString ();
					}
				}
				else {
					SelectedItemGui.ItemText.text = "";	// for gear, maybe durability?
				}
			}
		}
	}
	// will copy reference to spells rather then moving them

	public Icon GetSelectedIcon() {
		return MyPlayer.MyInventory.GetIcon (SelectedItem);
	}
	// doesnt work as it doesnt pass them by reference
	public void SwapIcons(Icon MyIcon1, Icon MyIcon2) {
		Icon TempIcon = new Icon(MyIcon1);
		MyIcon1 = new Icon(MyIcon2);
		MyIcon2 = new Icon(TempIcon);
	}

	// ------tool tip stuff for item gui-------
	//public GameObject MyModel;
	int MouseOverWindowIndex = 0;
	public void HandleMouseOverItem(int NewMouseOverIndex, int MouseOverWindowIndex2) {
		IsMouseOverItem = true;
		MouseOverIndex = NewMouseOverIndex;
		MouseOverWindowIndex = MouseOverWindowIndex2;
		if (!MyInventoryGuiWindows [MouseOverWindowIndex].IsSpellbook) {
			if (MyPlayer.MyInventory.GetIcon (MouseOverIndex).MyIconType != IconType.None) {
				EnableToolTips ();
				//ItemButton MouseOverButton = GetSelectedItemButton (MouseOverIndex);
				if (MyPlayer.MyInventory.GetIcon (MouseOverIndex).MyIconType == IconType.Spell) {
					HandleSpellMouseOver (false);
				} else if (MyPlayer.MyInventory.GetIcon (MouseOverIndex).MyIconType == IconType.Item) {
					HandleItemMouseOver ();
				}
			}
		} else {
			if (MouseOverIndex >= 0 && MouseOverIndex < MyInventory.SpellsList.Count) {
				EnableToolTips ();
				HandleSpellMouseOver(true);
			}
		}
		//SetItemGuiIndexes (MouseOverWindowIndex);	// just incase - update the indexes on gui items
	}
	public void HandleSpellMouseOver(bool IsSpells) {
		Text MyToolTipGuiText = ToolTipGui.transform.GetChild (0).GetComponent<Text> ();

		Spell MyMouseOverSpell;	
		if (IsSpells) 
			MyMouseOverSpell = MyInventory.SpellsList [MouseOverIndex];
		else 
			MyMouseOverSpell = GetSelectedSpell (MouseOverIndex);
		string DamageDealt = "Damage: " + MyMouseOverSpell.Damage;
		if (MyMouseOverSpell.Damage < 0)
			DamageDealt = "Heal: " + (-MyMouseOverSpell.Damage);
		
		string DamageKind = "Damage Type: ";
		if (MyMouseOverSpell.MyDamageType == DamageType.Peirce)
			DamageKind += "Peirce";
		else if (MyMouseOverSpell.MyDamageType == DamageType.Chaos)
			DamageKind += "Chaos";
		else if (MyMouseOverSpell.MyDamageType == DamageType.Splash)
			DamageKind += "Splash";
		else if (MyMouseOverSpell.MyDamageType == DamageType.Wave)
			DamageKind += "Wave";
		
		MyToolTipGuiText.text = "---" + MyMouseOverSpell.Name + "---\n" +
			DamageDealt + "\n" +
				MyMouseOverSpell.StatType + " Cost: " + MyMouseOverSpell.ManaCost + "\n" +
				"Range: " + MyMouseOverSpell.LifeTime + "\n" +
				"Speed: " + MyMouseOverSpell.TravelSpeed + "\n" +
				DamageKind + "\n"
				//MyMouseOverSpell.ToolTip
				;
	}
	public void HandleItemMouseOver() {
		DataManager MyData = GetManager.GetDataManager();
		Text MyToolTipGuiText = ToolTipGui.transform.GetChild (0).GetComponent<Text> ();
		Item MyMouseOverItem = MyPlayer.MyInventory.GetItem (MouseOverIndex);
		if (MyMouseOverItem != null)
			MyToolTipGuiText.text = MyMouseOverItem.Name + " : [" + MyMouseOverItem.Quantity + "/" + MyMouseOverItem.MaxQuantity + "]\n";// +MyMouseOverSpell.ToolTip;
		
		// if model viewer open

		/*if (MyMouseOverItem.ModelId >= 0 && MyMouseOverItem.ModelId < MyData.ModelsList.Count) {
			//if (MyModel.GetComponent<MeshFilter>())
			//	MyModel.GetComponent<MeshFilter>().mesh = MyData.ModelsList[MyMouseOverItem.ModelId];
		}
		if (MyMouseOverItem.MyItemType == ItemType.Block) {
			//MyData.AlterModel(MyModel,MyMouseOverItem.ModelId, MyMouseOverItem.BlockIndex, true, MyMouseOverItem.MyItemType);
			//MyModel.GetComponent<MeshRenderer> ().material.mainTexture = GetManager.GetTextureManager ().BlockTextures [MyMouseOverItem.BlockIndex];
		} else {
			//MyData.AlterModel(MyModel,MyMouseOverItem.ModelId,MyMouseOverItem.TextureId, false, MyMouseOverItem.MyItemType);
			//MyData.AlterModel(MyModel,MyMouseOverItem.ModelId,MyMouseOverItem.GetTextureIndex(), true, MyMouseOverItem.MyItemType);
			//MyModel.GetComponent<MeshRenderer> ().material = GetManager.GetMyDataManager ().MaterialsList [MyMouseOverItem.TextureId];
			//MyModel.GetComponent<MeshRenderer> ().material.mainTexture = GetManager.GetMyDataManager ().ModelTextures [MyMouseOverItem.TextureId];
		}*/
	}
	public void HandleMouseNotOverItem() {
		IsMouseOverItem = false;
		DisableToolTips ();
	}

	Spell GetSelectedSpell(int IconIndex) {
		return MyPlayer.MyInventory.GetSpell (IconIndex);
	}
	void UpdateToolTip() {
		if (ToolTipGui != null && MyPlayer != null) {
				if (!MyInventoryGuiWindows[MouseOverWindowIndex].IsSpellbook) {
					Icon MouseOverIcon = MyPlayer.MyInventory.GetIcon(MouseOverIndex);
					if (MouseOverIcon != null) {
						if (MouseOverIcon.MyIconType != IconType.None) {
							MoveToolTip();
						}
					}
				} else {
				if (MouseOverIndex >= 0 && MouseOverIndex < MyInventory.SpellsList.Count) {
						MoveToolTip();
					}
				}
		}
	}
	public void MoveToolTip() {	
		ToolTipGui.transform.position =	new Vector3(Input.mousePosition.x+ToolTipOffset.x, Input.mousePosition.y+ToolTipOffset.y, ToolTipGui.transform.position.z);
	}
	public void EnableToolTips() {
		ToolTipGui.SetActive(true);
	}
	public void DisableToolTips() {
		ToolTipGui.SetActive(false);
	}
}

