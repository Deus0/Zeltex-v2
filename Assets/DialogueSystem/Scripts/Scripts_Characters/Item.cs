using System.Collections.Generic;

namespace DialogueSystem {
	[System.Serializable]
	public class Item {
		public string Name;
		//public bool IsPickup = false;
		public int Quantity;
		public Stats MyStats;
	}

}
// maybe make item action as well, ie (open a door)

// give worldItem, a function, so i can have other scripts activate when they are selected - ie flip a car, open a door