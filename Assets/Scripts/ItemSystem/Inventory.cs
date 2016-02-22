using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using QuestSystem;
using CustomEvents;

namespace ItemSystem {
	/*	Attach to same object as the character or treasurechest
	 * 		-Used to hold items generally
	 * 		
	 * 	-Now has trading in it. I need to change this..
	*/
	public class Inventory : MonoBehaviour {
		public bool IsDebugMode = false;
		public UnityEvent OnAddItem;
		public MyEvent3 OnPickupItem = new MyEvent3();
		public MyEvent3 OnExchangeItem = new MyEvent3();
		public UnityEvent OnExchangeCurrency;
		public List<Item> MyItems = new List<Item>();
		[SerializeField] private float Value;	// money player has
		[SerializeField] private bool CanBuyAllItems = false;	// for player
		[SerializeField] private bool CanSellAllItems = false;	// for merchants

		void Awake() {
			HandleAddItemEvent();
		}
		void Update() {
			if (name == "Lotus" && Input.GetKeyDown (KeyCode.H)) {
				Debug.LogError("Invoking: " + (OnAddItem != null));
				HandleAddItemEvent();
			}
		}
		void OnGUI() {
			if (IsDebugMode) {
				GUILayout.Label("Inventory of [" + gameObject.name + "]");
				GUILayout.Label("Cash [" + Value + "]");
				GUILayout.Label("My Items [" + MyItems.Count + "]");
				for (int i = 0; i < MyItems.Count; i++)
					GUILayout.Label("\t" + i + " - Item[" + MyItems[i].Name + "]");
			}
		}
		public string GetScript() {
			string MyScript = "";
			for (int i = 0; i < MyItems.Count; i++) {
				MyScript += MyItems[i].GetScript() + "\n";
			}
			return MyScript;
		}
		public float GetValue() {
			return Value;
		}
		public string GetValueText() {
			return "#" + Value;
		}
		public Item GetItem(int ItemIndex) {
			if (MyItems.Count == 0)
				return null;
			ItemIndex = Mathf.Clamp (ItemIndex, 0, MyItems.Count-1);
			return MyItems [ItemIndex];
		}
		public Item GetItem(string name) {
			for (int i = 0; i < MyItems.Count; i++) {
				if (name == MyItems[i].Name) {
					return  MyItems[i];
				}
			}
			return null;
		}
		public bool ContainsItem(string name) {
			for (int i = 0; i < MyItems.Count; i++) {
				if (name == MyItems[i].Name) {
					return true;
				}
			}
			return false;
		}
		public int GetItemQuantity(string ItemName) {
			for (int i = 0; i < MyItems.Count; i++) {
				if (MyItems [i].Name == ItemName) {
					return MyItems[i].GetQuantity();
				} else {
					//Debug.LogError(ItemName + " != " + MyItems[i].Name);
					//Debug.LogError(ItemName.Length + " != " + MyItems[i].Name.Length);
				}
			}

			return 0;
		}
		/*public int HasItems(string ItemName, int Quantity) {
			//if (MyInventory.MyItems.Count > 0) return true;
			//MyCondition.ItemName = SpeechFileReader.CheckStringForLastChar(MyCondition.ItemName);
			for (int i = 0; i < MyInventory.MyItems.Count; i++) {
				if (MyInventory.MyItems [i].Name == MyCondition.ItemName) {
					if (MyCondition.ItemQuantityState >= MyCondition.ItemQuantity) {
						return true;
					}
				}
			}
		}*/
		// returns true if removes item
		public bool RemoveItem(Item MyItem, int Quantity) {
			for (int i = 0; i < MyItems.Count; i++) {
				if (MyItem.Name == MyItems[i].Name) {
					//Debug.LogError("Item found in inventory");
					if (MyItems[i].GetQuantity() >= Quantity) 
					{
						//Debug.LogError("Item: " + MyItems[i].Name + " is " + MyItems[i].GetQuantity());
						MyItems[i].IncreaseQuantity(-Quantity);
						//Debug.LogError("Decreased Item: " + MyItems[i].Name + " by " + Quantity);
						//Debug.LogError("Item: " + MyItems[i].Name + " is " + MyItems[i].GetQuantity());
						HandleAddItemEvent();
						//Debug.LogError("Item: " + MyItems[i].Name + " is " + MyItems[i].GetQuantity());
						return true;
					} 
					else 
					{
						Debug.LogError("Item has insufficient quantity");
						return false;
					}
				}
			}
			return false;
		}

		public void PickupItem(GameObject MyObject) {
			ItemObject MyItemObject = MyObject.GetComponent<ItemObject> ();
			if (MyItemObject) {
				AddItem (MyItemObject.GetItem ());
				OnPickupItem.Invoke(MyObject, "Picked Up");
			}
		}
		public void HandleAddItemEvent() {
			//Debug.LogError(name + " HandleAddItemEvent");
			//if (gameObject.GetComponent<QuestLog>())
			//	gameObject.GetComponent<QuestLog>().OnAddItem();	// checks the quests?
			if (OnAddItem != null) {
				OnAddItem.Invoke ();
			} else {
				Debug.LogError(name + " Has a OnAddItem null");
			}
		}

		// Normal list handling
		public void AddItem(Item NewItem) {
			AddItem (NewItem, -1);
		}
		public void AddItem(Item NewItem, int Quantity) {
			if (Quantity == -1)
				Quantity = NewItem.GetQuantity ();
			// first check to stack item
			if (NewItem != null)
			for (int i = 0; i < MyItems.Count; i++) {
				if (MyItems[i].Name == NewItem.Name) {
					MyItems[i].IncreaseQuantity(Quantity);
					HandleAddItemEvent();
					return;
				}
			}
			// if no item of type, add to list
			Item NewItem2 = new Item (NewItem);
			if (Quantity != -1)
				NewItem2.SetQuantity (Quantity);
			MyItems.Add (NewItem2);
			HandleAddItemEvent();
		}

		// Trade Mechanics - maybe seperate from inventory
		
		public bool CanBuy(Item MyItem)
		{
			if (CanBuyAllItems)
				return true;
			
			Item MyItemFound = GetItem (MyItem.Name);
			if (MyItemFound == null)
				return false;
			return MyItemFound.IsBuyable ();
		}
		
		public bool CanSell(Item MyItem)
		{
			if (CanSellAllItems)
				return true;
			Item MyItemFound = GetItem (MyItem.Name);
			if (MyItemFound == null)
				return false;
			return MyItemFound.IsSellable ();
		}

		public float GetAverageValue(Item Item1, Item Item2) {
			float AverageValue = 0f;
			if (Item2 != null && Item1 != null)
				AverageValue = (Item1.GetMidValue ()+Item2.GetMidValue())/2f;
			else if (Item1 != null && Item2 == null)
				AverageValue = Item1.GetMidValue ();
			else if (Item1 == null && Item2 != null)
				AverageValue = Item2.GetMidValue ();
			return AverageValue;
		}
		
		public void IncreaseValue(float AdditionValue) {
			Value += AdditionValue;
			if (OnExchangeCurrency != null)
				OnExchangeCurrency.Invoke ();
		}
		public void GiveItem(GameObject OtherInventory2, string ItemName) {
			GiveItem (OtherInventory2, ItemName, 1);
		}
		public bool GiveItem(GameObject OtherInventory2, string ItemName, int Quantity) {
			Inventory OtherInventory = OtherInventory2.GetComponent<Inventory> ();
			if (OtherInventory == null) {
				Debug.LogError(OtherInventory2.name + " has no inventory attached!");
				return false;
			}
			if (ItemName == "Money") 
			{
				GiveValue(gameObject, OtherInventory2, Quantity);
				//IncreaseValue(-Quantity);
				//OtherInventory.IncreaseValue(Quantity);
				return true;
			}
			OnExchangeItem.Invoke (OtherInventory2, "Gave " + ItemName);
			return Inventory.ExchangeItems (this, OtherInventory, ItemName, Quantity, false);
		}

		public bool BuyItem(Inventory OtherInventory, string ItemName, int BuyQuantity) 
		{
			return Inventory.ExchangeItems (OtherInventory, this, ItemName, BuyQuantity, true);
		}
		public bool SellItem(Inventory OtherInventory, string ItemName, int BuyQuantity) 
		{
			return Inventory.ExchangeItems (this, OtherInventory, ItemName, BuyQuantity, true);
		}
		public static bool GiveValue(GameObject MyCharacterGiver, GameObject MyCharacterTaker, float ExchangeValue)
		{
			Debug.LogError (MyCharacterGiver.name + " is giving value " + ExchangeValue + " to " + MyCharacterTaker.name);
			Inventory InventoryGive = MyCharacterGiver.GetComponent<Inventory> ();
			Inventory InventoryTake = MyCharacterGiver.GetComponent<Inventory> ();
			if (InventoryTake && InventoryGive) 
			if (InventoryGive.Value >= ExchangeValue) {
				InventoryGive.IncreaseValue (-ExchangeValue);
				InventoryTake.IncreaseValue (ExchangeValue);
				return true;
			}
			return false;
		}
		// assuming A(this) is buying off B(OtherInventory)
		// this class is selling to the other class
		public static bool ExchangeItems(Inventory InventoryGive, Inventory InventoryTake, string ItemName, int ItemQuantity, bool IsValueExchanged) 
		{
			Debug.LogError (InventoryGive.name + " is giving value " + ItemName + " to " + InventoryTake.name);
			// get item by name from each inventory :3
			Item MyItem = InventoryGive.GetItem (ItemName);
			Item MyItem2 = InventoryTake.GetItem (ItemName);
			if (MyItem == null) {
				Debug.LogError (InventoryGive.name + " Does not have: " + ItemName + " to give to " + InventoryTake.name);
				return false;
			} else if (MyItem.GetQuantity () < ItemQuantity) {
				Debug.Log (InventoryGive.name + "has " + ItemName + " to give to " + InventoryTake.name + " but not enough quantity.");
				return false;
			}
			// haggling step here!
			float BuyValue = InventoryGive.GetAverageValue (MyItem, MyItem2)*ItemQuantity;
			if (InventoryTake.Value < BuyValue)
				return false;
			// exchange currency
			if (IsValueExchanged) 
			{
				bool IsExchangeValue = GiveValue(InventoryGive.gameObject, InventoryTake.gameObject, BuyValue);
				if (!IsExchangeValue)
					return false;
			}
			// the item switching here!
			Debug.LogError (InventoryGive.name + " giving " + ItemName + " to " + InventoryTake.name);
			bool IsRemoveItem = InventoryGive.RemoveItem (MyItem, ItemQuantity);
			if (IsRemoveItem)
				InventoryTake.AddItem (MyItem, ItemQuantity);
			//InventoryTake.HandleAddItemEvent();
			//InventoryGive.HandleAddItemEvent();
			return IsRemoveItem;
		}
	}
}