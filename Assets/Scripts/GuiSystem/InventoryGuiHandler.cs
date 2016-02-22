using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CharacterSystem;
using ItemSystem;

namespace GuiSystem {
	public class InventoryGuiHandler : GuiListHandler {
		[Header("Events")]
		public CustomEvents.MyEventString MyOnClickEvent = new CustomEvents.MyEventString();
		[Header("References")]
		public CharacterSystem.Character MyCharacter;
		public ItemSystem.Inventory MyInventory;
		private Text MyValueText;
		public ItemHold MyItemHold;
		public GameObject SelectedItem;
		[Header("Audio")]
		public AudioClip OnBuyItemSound;
		public AudioClip OnNonBuyItemSound;
		public AudioClip OnNotSellingSound;
		public AudioClip OnSellItemSound;
		public AudioSource MyAudioSource;

		public void UpdateValueText()
		{
			if (MyValueText == null && transform.parent.FindChild ("ValueLabel")) 
			{
				MyValueText = transform.parent.FindChild ("ValueLabel").GetComponent<Text> ();
			}
			UpdateValuesText ();
		}
		[PunRPC]
		public void IncreaseSelectedIcon() {
			OnChangeSelectedItem(MyCharacter.IncreaseSelectedIcon( 1,MyGuis.Count));
		}
		[PunRPC]
		public void DecreaseSelectedIcon() {
			OnChangeSelectedItem(MyCharacter.IncreaseSelectedIcon(-1,MyGuis.Count));
		}
		public void OnChangeSelectedItem(int NewSelectedItem)
		{
			if (SelectedItem) 
			{
				NewSelectedItem = Mathf.Clamp (NewSelectedItem, 0, MyGuis.Count-1);
				SelectedItem.transform.position = MyGuis [NewSelectedItem].transform.position;
			}
		}

		private void UpdateValuesText() 
		{
			if (MyValueText) {
				MyValueText.text = MyInventory.GetValueText();
			}
		}
		//int SelectedItemIndex = 0;
		// Update is called once per frame
		void Update () {
			base.Update();
			if (transform.parent.name.Contains ("Skill")) {
				float MouseScrollWheel = Input.GetAxis ("Mouse ScrollWheel");
				if (MouseScrollWheel > 0) {
					gameObject.GetComponent<PhotonView> ().RPC ("IncreaseSelectedIcon",PhotonTargets.All);
					//IncreaseSelectedIcon();
				} else if (MouseScrollWheel < 0) {
					gameObject.GetComponent<PhotonView> ().RPC ("DecreaseSelectedIcon",PhotonTargets.All);
					//DecreaseSelectedIcon();
				}
			}
		}

		// Handles updating and only update what has changed!
		public void UpdateInventoryGui() 
		{
			//Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			//Clear ();
			for (int i = MyGuis.Count-1; i >= 0; i--) {
				bool DoesContain = MyInventory.ContainsItem(MyGuis[i].name);
				if (!DoesContain || (DoesContain && MyInventory.GetItem(MyGuis[i].name).HasUpdated())) 
				{
					Destroy (MyGuis[i]);
					MyGuis.RemoveAt (i);
				}
			}
			//if (MyInventory.MyItems.Count == MyGuis.Count) 
			// Create the guis
			{
				for (int i = 0; i < MyInventory.MyItems.Count; i++) 
				{
					if (!Contains (MyInventory.MyItems [i].Name))
					{
						TooltipData MyData = new TooltipData ();
						MyData.LabelText = MyInventory.MyItems [i].Name;
						MyData.DescriptionText = MyInventory.MyItems [i].GetDescription ();
						AddGui ("x" + MyInventory.MyItems [i].GetQuantity (), MyData, i);
						if (MyGuis.Count > i) 
						{
							GameObject CreatedItem = MyGuis [i];
							// set the texture
							if (CreatedItem.transform.GetChild (0).GetComponent<RawImage> ())	// if child has texture
								CreatedItem.transform.GetChild (0).GetComponent<RawImage> ().texture = MyInventory.MyItems [i].GetTexture ();

							ItemHandler MyItemHandler = CreatedItem.AddComponent<ItemHandler> ();
							MyItemHandler.MyCharacter = MyCharacter;
							MyItemHandler.MyInventory = MyInventory;
							MyItemHandler.MyItem = MyInventory.MyItems [i];
							MyItemHandler.OnBuyItemSound = OnBuyItemSound;
							MyItemHandler.OnSellItemSound = OnSellItemSound;
							MyItemHandler.OnNonBuyItemSound = OnNonBuyItemSound;
							MyItemHandler.OnNotSellingSound = OnNotSellingSound;
							MyItemHandler.MyAudioSource = MyAudioSource;
							MyItemHandler.MyItemHold = MyItemHold;
							MyItemHandler.MyOnClickEvent = MyOnClickEvent;
						} 
						else 
						{
							Debug.LogError("Failure to add item gui");
						}
					}
				} 
			}
			//else 
			{
				//Debug.LogError("Improper amount of guis in inventory gui");
			}
			UpdateValueText ();
		}
	}
}