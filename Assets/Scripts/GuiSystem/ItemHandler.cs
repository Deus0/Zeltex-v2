using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using CharacterSystem;
using ItemSystem;

/*	Implemented for -=Trade Mechanic=-
 * 
 * 	Holds onto the Inventory data
 * 	Assumes second player is player, and first is npc
 * 	Plays sound effects depending on result of the transaction
 * 	
 */

namespace GuiSystem {
	public class ItemHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
		public GameObject ToolTipGui;
		public TooltipData MyTooltipData;
		public CharacterSystem.Character MyCharacter;
		public ItemSystem.Inventory MyInventory;
		public ItemSystem.Item MyItem;
		public AudioClip OnBuyItemSound;
		public AudioClip OnSellItemSound;
		public AudioClip OnNonBuyItemSound;
		public AudioClip OnNotSellingSound;
		public AudioSource MyAudioSource;
		public ItemHold MyItemHold;
		public CustomEvents.MyEventString MyOnClickEvent = new CustomEvents.MyEventString();

		public void OnPointerDown(PointerEventData eventData) 
		{
			if (MyAudioSource == null) 
			{
				MyAudioSource = gameObject.AddComponent<AudioSource>();
			}
			if (MyOnClickEvent != null) 
			{
				MyOnClickEvent.Invoke(MyItem.Name);
			}
			bool DidBuyItem = false;
			GameObject MyPlayer = GameObject.Find ("Player");	// assuming the player is the only one that clicks..!
			if (MyPlayer) 
			{
				if (MyPlayer != MyInventory.gameObject) { 	// if different player
					//Debug.LogError ("Player is different then inventory!");
					ItemSystem.Inventory MyPlayerInventory = MyPlayer.GetComponent<ItemSystem.Inventory> ();
					if (MyPlayerInventory != null) {

						if (Input.GetMouseButton (0)) { // player buys item off npc
							//if (MyInventory.IsSelling(MyItem.Name) && MyPlayerInventory.IsBuying(MyItem.Name))
							if (MyPlayerInventory.CanBuy (MyItem) && MyInventory.CanSell (MyItem)) 
							{
								if (MyPlayerInventory.BuyItem (MyInventory, MyItem.Name, 1)) 
								{
									//MyPlayerInventory.BuyItem (MyItem);
									DidBuyItem = true;
									//Debug.LogError ("Baught Item.");
								} else {
									Debug.LogError ("Failure to remove item");
								}
								Debug.LogError("Baught Item " + DidBuyItem);
								if (DidBuyItem)
									MyAudioSource.PlayOneShot (OnBuyItemSound);
								else
									MyAudioSource.PlayOneShot (OnNonBuyItemSound);
							} else {
								MyAudioSource.PlayOneShot (OnNotSellingSound);
							}
						} else if (Input.GetMouseButton (1)) { // player sells item to npc
							//if (MyItem.IsBuying ())
							{
								if (MyInventory.CanBuy (MyItem) && MyPlayerInventory.CanSell (MyItem)) {
									if (MyInventory.BuyItem (MyPlayerInventory, MyItem.Name, 1)) {
										DidBuyItem = true;
										//Debug.LogError ("Baught Item.");
									} else {
										Debug.LogError ("Failure to remove item");
									}

									if (DidBuyItem)
										MyAudioSource.PlayOneShot (OnSellItemSound);
									else
										MyAudioSource.PlayOneShot (OnNonBuyItemSound);
								}
								else 
								{
									MyAudioSource.PlayOneShot (OnNotSellingSound);
								}
							}
						} 
					}
				}
				else {
					if (Input.GetMouseButton (2))  // pickup item
					{	// lerp item from inventory to crosshair
						MyItemHold.BeginHoldItem(MyPlayer, this, MyItem);
					}
				}
			}
		}


		public void OnPointerUp(PointerEventData eventData) {
		}
	}
}
