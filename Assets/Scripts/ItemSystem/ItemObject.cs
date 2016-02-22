using UnityEngine;
using System.Collections;
using CustomEvents;
using UnityEngine.UI;

namespace ItemSystem {
	/*	Used for interaction with objects in the world
	 * 		a character ray traces the objects, and interacts with them
	*/
	public class ItemObject : MonoBehaviour {
		[Header("Pickup")]
		private bool HasActivated = false;
		[Tooltip("Is the item is picked up?")]
		public bool IsItemPickup = true;
		[Tooltip("Is the item destroyed when picked up?")]
		public bool IsDestroyedOnPickup = true;
		[Tooltip("Is the item destroyed when collided")]
		public bool IsDestroyOnCollide = false;
		[Tooltip("Added to the characters inventory when picked up")]
		public Item MyItem = new Item();
		[Tooltip("Functions are called when item is interacted with (mouseclick)")]
		public MyEvent OnItemInteract;
		[Tooltip("When character collides with the item")]
		public MyEvent2 OnItemCollide;
		[Tooltip("Used for the player to get information about an item")]
		public GameObject MyItemInspectPrefab;
		[Header("Sounds")]
		[Tooltip("Played when item is picked up")]
		public AudioClip MyPickupSound;
		
		[Header("SpecialEffects")]
		[Tooltip("The Particles created when item is picked up")]
		public GameObject ParticlesPrefab;

		void Awake() 
		{
			if (MyItemInspectPrefab) {
				GameObject NewGui = (GameObject) Instantiate(MyItemInspectPrefab, transform.position, Quaternion.identity);
				NewGui.transform.SetParent(transform);
				GUI3D.Follower MyFollower = NewGui.GetComponent<GUI3D.Follower>();
				MyFollower.UpdateTarget(transform);
				AnimationUtilities.AnimateLine MyLineThing = NewGui.GetComponent<AnimationUtilities.AnimateLine>();
				MyLineThing.Target = gameObject;
				NewGui.transform.localScale = new Vector3((1f/transform.localScale.x)*0.001f,
				                                          (1f/transform.localScale.y)*0.001f,
				                                          (1f/transform.localScale.z)*0.001f);
				Text MyLabelText = NewGui.transform.GetChild(1).GetComponent<Text>();
				MyLabelText.text = MyItem.Name;
				Text MyDescriptionText = NewGui.transform.GetChild(2).GetComponent<Text>();
				MyDescriptionText.text = MyItem.GetDescription();
			}
		}

		public Item GetItem() {
			return MyItem;
		}
		public bool GetPickedUp() {
			if (ParticlesPrefab) {
				GameObject ItemLeftOver = (GameObject)Instantiate (ParticlesPrefab, transform.position, transform.rotation);
				ItemLeftOver.transform.localScale = transform.localScale;
				if (MyPickupSound) {
					AudioSource MySource = ItemLeftOver.AddComponent<AudioSource> ();
					if (MyPickupSound != null)
						MySource.PlayOneShot (MyPickupSound);
				}
				ItemLeftOver.AddComponent<ParticlesEmmisionOverLifetime> ();
			}
			if (IsDestroyedOnPickup)
				Destroy (gameObject);
			return true;
		}
		public bool HasUsed() {
			return HasActivated;
		}
		public void Use() {
			HasActivated = true;
		}
		public void Pickup(GameObject Direction) 
		{
			if (OnItemInteract != null)
				OnItemInteract.Invoke (Direction);
			if (IsItemPickup)
				GetPickedUp ();
		}
		public void Reset() {
			HasActivated = false;
		}

		void OnTriggerEnter(Collider collision) {
			OnThing (collision.gameObject);
		}

		void OnCollisionEnter(Collision collision) {
			OnThing (collision.gameObject);
		}
		private void OnThing(GameObject TheThing) {
			if (TheThing.GetComponent<CharacterSystem.Character> ()) {
				if (OnItemCollide != null) {
					OnItemCollide.Invoke(gameObject, TheThing);
				}
			}
			if (IsDestroyOnCollide)
				Destroy (gameObject);
		}
	}
}
