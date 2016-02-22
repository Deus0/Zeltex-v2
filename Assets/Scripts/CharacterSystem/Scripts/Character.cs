using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using AISystem;
using GuiSystem;
using ItemSystem;
using CustomEvents;
using UnityEngine.EventSystems;

/*
	NPC's class
		Contains movement State
		Raytrace for other characters and quest items
	Other npc classes:
		Stats (dynamic)
		Inventory
		QuestLog
 */

namespace CharacterSystem {
	// used for interactions - using raycasts
	[System.Serializable]
	public class SelectionType {
		public LayerMask MyLayer;
		public List<MyEvent> OnSelect;
		private GameObject HitObject;
		public string ComponentName = "";
		public string FunctionName = "";

		public void HandleOnSelect(int Type, GameObject HitObject_) {
			HitObject = HitObject_;
			if (Type >= 0 && Type < OnSelect.Count) 
			{
				OnSelect[Type].Invoke (HitObject);
			}
		}
	}
	[SelectionBase]
	public class Character : MonoBehaviour {
		public bool IsDebugMode = false;
		[Header("Raycasting")]
		[Tooltip("Length of ray used for interaction with environment")]
		public float RaycastRange = 5;
		[Tooltip("Used for interaction between character and other objects. Keep blank if using it's own transform.forward vector for ray casts.")]
		public Transform MyRayObject = null;
		public bool IsUseMainCamera = false;
		[Tooltip("Chose the layers that you want to be able to interact with.")]
		public LayerMask MyLayer;

		public List<SelectionType> MySelections = new List<SelectionType> ();

		private int SelectedIconIndex = 0;
		private Item SelectedIcon;

		public bool IsPositionClamped = true;
		private Vector3 PositionMinimum = new Vector3(0,14,0);
		private Vector3 PositionMaximum = new Vector3(48*4-21,40,48*4-21);
		// Interfacing with dialogue/speech handler

		// position limiter
		void OnDrawGizmosSelected() 
		{
			if (IsDebugMode) {
				Gizmos.color = new Color (1, 0, 0, 0.5F);
				Gizmos.DrawCube ((PositionMinimum + PositionMaximum) / 2f, 
			                PositionMaximum - PositionMinimum);
			}
		}

		void FixedUpdate() 
		{
			if (IsPositionClamped)
			transform.position = new Vector3 (Mathf.Clamp (transform.position.x, PositionMinimum.x, PositionMaximum.x), 
			                                 Mathf.Clamp (transform.position.y, PositionMinimum.y, PositionMaximum.y), 
			                                 Mathf.Clamp (transform.position.z, PositionMinimum.z, PositionMaximum.z));
		}

		void Start() 
		{
			HasUpdatedIcon ();
		}

		// for icon selection
		public int IncreaseSelectedIcon(int IncreaseAmount, int MaxIcons) 
		{
			SelectedIconIndex += IncreaseAmount;
			SelectedIconIndex = Mathf.Clamp (SelectedIconIndex, 0, MaxIcons);
			//if (SelectedIconIndex >= MaxIcons || SelectedIconIndex < 0)	// if outside of range
			//	SelectedIconIndex = SelectedIconIndex % MaxIcons;
			HasUpdatedIcon ();
			return SelectedIconIndex;
		}

		private void HasUpdatedIcon()
		{
			if (gameObject.GetComponent<Inventory> ()) {
				SelectedIcon = gameObject.GetComponent<Inventory> ().GetItem (SelectedIconIndex);
				if (SelectedIcon == null)
					return;
				VoxelEngine.VoxelBrush MyBrush = gameObject.GetComponent<VoxelEngine.VoxelBrush> ();
				if (MyBrush) {
					if (SelectedIcon.Name == "Dirt") {
						MyBrush.enabled = true;
					} else {
						MyBrush.enabled = false;
					}
				}
				CombatSystem.Shooter MyShooter = gameObject.GetComponent<CombatSystem.Shooter> ();
				if (MyShooter) {
					if (SelectedIcon.Name == "Marz's Bracelet") {
						MyShooter.enabled = true;
					} else {
						MyShooter.enabled = false;
					}
				}
			}
		}
		// just need to remove player stuff
		// can be added to normal characters later
		// also any ai should be disabled
		// so things like reviving enemies into zombies is possible
		public void OnDeath() 
		{
			if (GetComponent<RotateTowardsObject> ())
				Destroy (GetComponent<RotateTowardsObject> ());

			if (GetComponent<CharacterSystem.MyController> ())
				Destroy (GetComponent<CharacterSystem.MyController> ());
			if (GetComponent<Rigidbody> ())
				Destroy (GetComponent<Rigidbody> ());
			if (GetComponent<CapsuleCollider> ())
				Destroy (GetComponent<CapsuleCollider> ());
			if (GetComponent<SphereCollider> ())
				Destroy (GetComponent<SphereCollider> ());

			if (GetComponent<MouseLocker> ())
				Destroy (GetComponent<MouseLocker> ());

			if (GetComponent<CombatSystem.Shooter> ())
				Destroy (GetComponent<CombatSystem.Shooter> ());

			if (GetComponent<VoxelEngine.VoxelBrush> ())
				Destroy (GetComponent<VoxelEngine.VoxelBrush> ());
			
			if (GetComponent<Player> ())
				Destroy (GetComponent<Player> ());

			if (GetComponent<GuiManager> ())
				GetComponent<GuiManager> ().DestroyAllGuis ();
			
			//gameObject.transform.parent.name += "'s Corpse Holder";	// burnt, sliced, crushed, chocolified, decapitated, exploded
			gameObject.name += "'s Corpse";	// burnt, sliced, crushed, chocolified, decapitated, exploded
			//Destroy (this);
		}
		
		// Checks to see if the mouse is hitting the gui
		public bool IsRayHitGui() 
		{
			var pointer = new PointerEventData(EventSystem.current);
			pointer.position = (Input.mousePosition);
			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointer, raycastResults);
			if (raycastResults.Count > 0) {
				return true;
			}
			return false;
		}

		public bool RayTraceSelections(int InteractType)
		{
			if (IsRayHitGui ())
				return false;
			if (MyRayObject == null) {
				if (IsUseMainCamera) {
					MyRayObject = Camera.main.transform;
				} else {
					MyRayObject = gameObject.transform;
				}
			}
			//Debug.LogError ("Trying to select!");
			for (int i = 0; i < MySelections.Count; i++)
			{
				RaycastHit MyHit;
				if (Physics.Raycast (MyRayObject.position, MyRayObject.forward, out MyHit, RaycastRange, MySelections[i].MyLayer)) 
				{
					//Debug.LogError ("HitObject! " + MyHit.collider.gameObject.name);
					//MySelections[i].HandleOnSelect(InteractType, MyHit.collider.gameObject);
					AnimationUtilities.BodyPart MyBodyPart = MyHit.collider.gameObject.GetComponent<AnimationUtilities.BodyPart>();
					if (MyBodyPart)
					{
						if (MyBodyPart.RagdollBrain.OnInteract.Count > InteractType  && InteractType >= 0)
							MyBodyPart.RagdollBrain.OnInteract[InteractType].Invoke(MyHit.collider.gameObject);
					}

					Character HitCharacter = MyHit.collider.gameObject.GetComponent<Character>();
					if (HitCharacter != null) {
						return CharacterToCharacter(HitCharacter, InteractType);
					}
					
					ItemObject HitItemObject = MyHit.collider.gameObject.GetComponent<ItemObject>();
					if (HitItemObject != null) {
						return CharacterToItem(HitItemObject, InteractType);
					}
					
					Door MyDoor = MyHit.collider.gameObject.GetComponent<Door>();
					if (MyDoor) 
					{
						Debug.Log("Toggling door!");
						MyDoor.ToggleDoor();
						return true;
					}

					if (MySelections[i].ComponentName != "" && MySelections[i].FunctionName != "") 
					{
						Component MyComponent = (Component) MyHit.collider.gameObject.GetComponent(MySelections[i].ComponentName);
						MyComponent.BroadcastMessage(MySelections[i].FunctionName);
					}
				}
			}
			return false;
		}

		// Responses
		public bool CharacterToCharacter(Character HitCharacter, int InteractType) 
		{
			//Debug.LogError("HitCharacter!");
			// now initialize that characters dialogue system with (MainCharacter->Lotus dialogue file)
			GuiSystem.GuiManager MyGuiManager = gameObject.GetComponent<GuiSystem.GuiManager>();
			GuiSystem.GuiManager MyGuiManager2 = HitCharacter.GetComponent<GuiSystem.GuiManager>();
			/*if (InteractType == 0) 
			{
				if (MyGuiManager && MyGuiManager2) {
					MyGuiManager.BeginTalk(MyGuiManager2);
				}
			}
			else if  (InteractType == 1) 
			{
				if (MyGuiManager && MyGuiManager2) {
					MyGuiManager.BeginTrade(MyGuiManager2);
				}
			}*/
			return true;
		}

		public bool CharacterToItem(ItemObject HitItemObject, int InteractType) 
		{
			if (InteractType == 0) {
				//Debug.LogError("HitItem!");
				if (!HitItemObject.HasUsed())
				{
					HitItemObject.Use ();
					ItemSystem.Inventory MyInventory = gameObject.GetComponent<ItemSystem.Inventory> ();
					if (MyInventory != null)
					{
						MyInventory.PickupItem(HitItemObject.gameObject);	// adds it to the inventory?
					}
					// passes in a ray object for any kind of picking action
					HitItemObject.Pickup(MyRayObject.gameObject);	// does things like destroy, activates the special function
				}
			}
			// add a value to statistics! for things 
			//if (gameObject.GetComponent<QuestLog>())
			//	gameObject.GetComponent<QuestLog>().RefreshQuestsGui();
			return true;
		}
	}
}
