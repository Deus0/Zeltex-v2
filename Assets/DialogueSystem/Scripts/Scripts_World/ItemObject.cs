using UnityEngine;
using System.Collections;


namespace DialogueSystem {
	public class ItemObject : MonoBehaviour {
		public UnityEngine.Events.UnityEvent OnPickupFunction;
		public bool IsDestroyedOnPickup = true;
		public bool HasActivated = false;

		public bool IsItemPickup = true;
		public Item MyItem;

		public Item GetItem() {
			return MyItem;
		}

		public void Pickup() {
			if (!HasActivated) {
				HasActivated = true;

				OnPickupFunction.Invoke ();

				if (IsDestroyedOnPickup)
					Destroy (gameObject);
			}
		}

		public void Reset() {
			HasActivated = false;
		}
	}
}
