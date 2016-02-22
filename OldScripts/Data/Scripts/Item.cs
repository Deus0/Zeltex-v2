using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
public enum ItemType {
	Block,
	BlockStructure,
	Dungeon,
	Tool,
	Ingredient
};

// just need to generate items now
// with textures
//public void GenerateItems(List<Texture> MyTextures, int MaxBlocks) {

//}

[System.Serializable]
public class Item {
	//public int Index;
	public string Name;
	public Texture MyTexture;
	private int MyTextureId;
	public float Durability = 100;
	public int Damage = 5;
	public int Quantity = 1;
	public int MaxQuantity = 64;
	public int BlockIndex = 1;
	public ItemType MyItemType = ItemType.Block;
	public int ModelId = 0;
	//public int TextureId = 0;
	//public bool IsBlockStructure;
	public bool NeedsUpdateTexture = false;
	public bool NeedsUpdateText = false;
	
	public Item() {
		SetDefaults ();
	}
	public Item(int Index_) {
		SetDefaults ();
	}
	public int GetTextureIndex() {
		return MyTextureId;
	}
	public void SetTextureIndex(int NewTextureId) {
		if (MyTextureId != NewTextureId) {
			NeedsUpdateTexture = true;
			MyTextureId = NewTextureId;
		}
	}
	public void StackItem(Item ItemToStack) {
		//Debug.LogError ("Quantity: " + Quantity + " - Stacking Item " + Name + " with " + ItemToStack.Quantity + " of " + ItemToStack.Name);
		IncreaseQuantity (ItemToStack.Quantity);
	}
	public void SetDefaults() {
		Name = "New Item ";
		Durability = 100;
	}
	public bool IsMax() {
		if (Quantity == MaxQuantity)
			return true;
		else 
			return false;
	}
	public Item(Item NewItem) {
		Name = NewItem.Name;
		Damage = NewItem.Damage;
		MyTexture = NewItem.MyTexture;
		Durability = NewItem.Durability;
		Quantity = NewItem.Quantity;
		MaxQuantity = NewItem.MaxQuantity;
		BlockIndex = NewItem.BlockIndex;
		MyItemType = NewItem.MyItemType;
		ModelId = NewItem.ModelId;
		MyItemType = NewItem.MyItemType;
		MyTextureId = NewItem.MyTextureId;

		if (MyItemType == ItemType.Block)
			MyTexture = GetManager.GetDataManager().GetItemTexture (BlockIndex, true);
		else
			MyTexture = GetManager.GetDataManager().GetItemTexture (MyTextureId, false);
	}
	public Item(ItemData NewItem) {
		Name = NewItem.Name;
		//MyTexture = NewItem.MyTexture;
		Durability = NewItem.Durability;
		Quantity = NewItem.Quantity;
		MaxQuantity = NewItem.MaxQuantity;
		BlockIndex = NewItem.BlockIndex;
		MyItemType = NewItem.MyItemType;
		ModelId = NewItem.ModelId;
		MyItemType = NewItem.MyItemType;
		MyTextureId = NewItem.MyTextureId;
		Damage = NewItem.Damage;

		if (MyItemType == ItemType.Block)
			MyTexture = GetManager.GetDataManager().GetItemTexture (BlockIndex, true);
		else
			MyTexture = GetManager.GetDataManager().GetItemTexture (MyTextureId, false);
	}
	public int IncreaseQuantity(int IncreaseAmount) {
		Quantity += IncreaseAmount;
		int Difference = Quantity - MaxQuantity;
		if (Difference < 0)
			Difference = 0;	// only return a number if there is left over items
		CheckQuantityBounds ();
		return Difference;
	}
	public void CheckQuantityBounds() {
		Quantity = Mathf.Clamp (Quantity, 0, MaxQuantity);
		NeedsUpdateText = true;
	}
	public void IncreaseQuantity() {
		Quantity++;
		CheckQuantityBounds ();
	}
	public void DecreaseQuantity() {
		Quantity--;
		CheckQuantityBounds ();
	}
	public bool IsEmpty() {
		if (Quantity == 0 || Durability <= 0)
			return true;
		else
			return false;
	}
	public void DecreaseDurability(int BlockIndex) {
		float DurabilityDecreaseAmount = 0.1f;
		Durability -= DurabilityDecreaseAmount;
	}
	public void Save(string FilePath) {

	}
};



// whenever I update the variables - make sure i update the copy constructor lel
[System.Serializable]
public class ItemData {
	//public int Index;
	public string Name;
	//public Texture MyTexture;
	public int MyTextureId;
	public float Durability = 100;
	public int Damage = 5;
	public int Quantity = 1;
	public int MaxQuantity = 64;
	public int BlockIndex = 1;
	public ItemType MyItemType = ItemType.Block;
	public int ModelId = 0;
	public int TextureId = 0;

	public ItemData(Item NewItem) {
		Name = NewItem.Name;
		//MyTexture = NewItem.MyTexture;
		Durability = NewItem.Durability;
		Quantity = NewItem.Quantity;
		MaxQuantity = NewItem.MaxQuantity;
		BlockIndex = NewItem.BlockIndex;
		MyItemType = NewItem.MyItemType;
		ModelId = NewItem.ModelId;
		MyItemType = NewItem.MyItemType;
		MyTextureId = NewItem.GetTextureIndex();
		Damage = NewItem.Damage;
	}
}
}