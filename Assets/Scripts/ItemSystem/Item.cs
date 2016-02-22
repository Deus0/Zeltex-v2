using System.Collections.Generic;
using UnityEngine;
using DialogueSystem;
using CharacterSystem;	// for the stats

namespace ItemSystem {
	[System.Serializable]
	public class Item {
		[Tooltip("A unique identifier for the Item")]
		public string Name;
		[Tooltip("The icon image")]
		public Texture MyTexture;
		[Tooltip("Used in the tooltip to describe the item")]
		[SerializeField] private string Description;
		//public bool IsPickup = false;
		[Tooltip("How many of that item there is.")]
		[SerializeField] private int Quantity = 1;
		[Tooltip("How much the item is worth, Selling at the highest")]
		[SerializeField] private float SellValue = -1;
		[Tooltip("How much the item is worth, Buying at the lowest")]
		[SerializeField] private float BuyValue = -1;
		
		[Tooltip("The stats the item contains")]
		public CharacterSystem.Stats MyStats;

		public void LoadTexture(string TextureName) {
			MyTexture = null;
			GetTexture(TextureName);
		}
		// default get texture
		public Texture GetTexture() {
			return GetTexture ("");
		}
		public Texture GetTexture(string TextureName) {
			if (MyTexture == null) {
				GameObject MyManager = GameObject.Find ("Manager");
				if (MyManager != null) {
					ItemManager MyItemManager = MyManager.GetComponent<ItemManager>();
					if (MyItemManager != null) {
						MyTexture = MyItemManager.FindTexture(TextureName);
					} else {
						Debug.LogError("Manager was found but set to null.");
					}
				} else {
						Debug.LogError("Could not find Manager object. Please make sure there is a gameObject called Manager in the scene.");
				}
			}
			return MyTexture;
		}
		// Set to true if it needs gui updating
		private bool NeedsUpdating = false;
		
		public Item() {
			MyStats = new CharacterSystem.Stats();
		}
		public Item(Item OldItem) {
			CloneItem (OldItem);
		}
		public Item(Item OldItem, bool IsSingle) {
			CloneItem (OldItem);
			if (IsSingle)
				Quantity = 1;
		}
		public void CloneItem(Item OldItem) {
			Name = OldItem.Name;
			Description = OldItem.Description;
			SellValue = OldItem.SellValue;
			BuyValue = OldItem.BuyValue;
			MyTexture = OldItem.MyTexture;
			MyStats = OldItem.MyStats;
			Quantity = OldItem.Quantity;
		}
		public bool HasUpdated() {
			if (NeedsUpdating) {
				NeedsUpdating = false;
				return true;
			}
			return false;
		}

		public int GetQuantity() {
			return Quantity;
		}
		public void SetQuantity(int NewQuantity) {
			if (NewQuantity != Quantity) {
				Quantity = NewQuantity;
				NeedsUpdating = true;
			}
		}
		public bool IncreaseQuantity(int Addition) {
			if (Addition != 0) {
				NeedsUpdating = true;
				Quantity += Addition;
				if (Quantity < 0)
				{
					Quantity = 0;
				}
				return true;
			}
			return false;
		}

		public string GetDescription() {
			string DescriptionText = "";
			DescriptionText += "Quantity x" + Quantity + "\n";
			float MidValue = GetMidValue ();
			if (MidValue != -1)
				DescriptionText += "Value #" + MidValue + "\n";
			if (!IsBuying ())
				DescriptionText += "Not Buying\n";
			//else
				//DescriptionText += "Buying $" + BuyValue + "\n"; 
			if (!IsSelling())
				DescriptionText += "Not Selling\n";
			//else
			//	DescriptionText += "Selling $" + SellValue + "\n";

			DescriptionText += Description;
			for (int j = 0; j < MyStats.GetSize(); j++) {
				Stat MyStat = MyStats.GetStat (j);
				DescriptionText += "\n   " + MyStat.Name;
				if (MyStat.GetValue() > 0) {
					DescriptionText += ": +" + MyStat.GetValue();
				} else if (MyStat.GetValue() < 0) {
					DescriptionText += ": -" + Mathf.Abs (MyStat.GetValue()).ToString ();
				}
			}
			return DescriptionText;
		}

		// buy-sell stuff
		// assumong buy value is minimum, sell value is max
		public float GetMidValue() 
		{
			if (SellValue == -1 && BuyValue != -1)
				return BuyValue;
			else if (SellValue != -1 && BuyValue == -1)
				return SellValue;
			else if (SellValue == -1 && BuyValue == -1)
				return -1;
			else
				return BuyValue+(SellValue-BuyValue)/2f;
		}
		public float GetSellValue() {
			return SellValue;
		}
		public float GetBuyValue() {
			return BuyValue;
		}
		public bool IsBuyable() {
			if (BuyValue == -1)
				return false;
			else {
				return true;
			}
		}

		public bool IsSellable() {
			if (SellValue == -1)
				return false;
			//if (MyValue >= SellValue)
			if (Quantity == 0)
				return false;
			else
				return true;
		}
		public bool IsSelling() {
			if (SellValue == -1)
				return false;
			else
				return true;
		}

		public bool IsBuying() {
			if (BuyValue == -1)
				return false;
			else
				return true;
		}

		public string GetCommand(string Data) {
			if (Data.Length == 0)
				return "";
			for (int i = 0; i < Data.Length; i++) {
				if (Data [i] == '/') {
					Data = Data.Substring(i);
					//Debug.LogError(Data);
					i = Data.Length;
				}
			}
			if (Data [0] == '/') {
				string[] New = Data.Split(' ');
				return New[0];
			} else {
				return "";
			}
		}
		// used by item manager to update items
		public static bool ReplaceItem(Item Item1, Item Item2) {
			if (Item1 == null || Item2 == null)
				return false;
			if (Item1.Name != Item2.Name)
				return false;
			Item1.Description = Item2.Description;
			Item1.BuyValue = Item2.BuyValue;
			Item1.SellValue = Item2.SellValue;
			Item1.MyTexture = Item2.MyTexture;
			Item1.MyStats = Item2.MyStats;
			
			Item1.NeedsUpdating = true;
			Item2.NeedsUpdating = true;
			return true;
		}
		public string GetScript() {
			List<string> MyScript = GetScript2 ();
			string MyScriptSingle = "";
			for (int i = 0; i < MyScript.Count; i++) 
			{
				MyScriptSingle += MyScript[i] + "\n";
			}
			return MyScriptSingle;
		}
		public List<string> GetScript2()
		{
			List<string> MyScript = new List<string> ();
			MyScript.Add ("/item " + Name);
			if (Description != "")
				MyScript.Add ("/description " + Description);
			if (Quantity != 1)
				MyScript.Add ("/quantity " + Quantity);
			if (SellValue != -1)
				MyScript.Add ("/sellvalue " + SellValue);
			if (BuyValue != -1)
				MyScript.Add ("/buybalue " + BuyValue);
			//NewSaveData.Add ("/texture " + MyTexture.name);
			if (MyStats.GetSize () != 0) 
			{
				MyScript.Add ("/stats\n");
				MyScript.Add (MyStats.GetScript());
			}
			return MyScript;
		}
		public Item(List<string> Data) {
			ActivateScript (Data);
		}
		public void ActivateScript(List<string> Data) 
		{
			NeedsUpdating = true;
			Quantity = 1;	// default
			//Debug.LogError ("Loading Item");
			for (int i = 0; i < Data.Count; i++) {
				if (SpeechUtilities.IsCommand (Data [i])) {
					string Command = SpeechUtilities.GetCommand (Data [i]);
					string Other = SpeechUtilities.RemoveCommand (Data [i]);
					//Debug.Log ("CommandName [" + Command + "]");
					//Debug.Log ("Input [" + Other + "]");

					switch (Command) {
					case ("/item"):
						Name = Other;
						break;
					case ("/description"):
						Description = Other;
						break;
					case ("/quantity"):
						try {
							Quantity = int.Parse (Other);
						} catch (System.FormatException e) {
						}
						break;
					case ("/sellvalue"):
						try {
							SellValue = int.Parse (Other);
						} catch (System.FormatException e) {
						}
						break;
					case ("/buyvalue"):
						try {
							BuyValue = int.Parse (Other);
						} catch (System.FormatException e) {
						}
						break;
					case ("/texture"):
						LoadTexture (Other);
						break;
					case ("/stats"):
						Debug.Log ("Now loading stats!");
						break;
					}
					if (Command.Contains("/stats")) {
						Debug.Log ("Now loading stats!");
						MyStats = new CharacterSystem.Stats (Data);
					}
				}
			}
			if (MyStats == null) {
				MyStats = new CharacterSystem.Stats ();
			}

		}
	}

}
// maybe make item action as well, ie (open a door)

// give worldItem, a function, so i can have other scripts activate when they are selected - ie flip a car, open a door