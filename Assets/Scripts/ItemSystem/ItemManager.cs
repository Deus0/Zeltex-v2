using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ItemSystem {
	[ExecuteInEditMode]
	public class ItemManager : MonoBehaviour {
		[Tooltip("Displays debug data in a simple list")]
		public bool IsDebugMode = false;
		[Tooltip("Updates the data in the scene at the start of runtime.")]
		public bool IsUpdateOnStart = false;
		// actions
		[Header("Actions")]
		[Tooltip("Using the all the data, updates all the objects in the scene.")]
		public bool IsUpdateAll = false;
		[Tooltip("Gets Items from Inventories, ItemObjects: and replaces them from the list")]
		private bool IsUpdateItems = false;
		[Tooltip("Gets Stats from Inventories, ItemObjects: and replaces them")]
		private bool IsUpdateStats = false;

		[Tooltip("Gets Items from Inventories, ItemObjects: and adds them to the list")]
		public bool IsGatherItems = false;	// 
		[Tooltip("Gets Stats from Inventories, ItemObjects: and adds them to the list")]
		public bool IsGatherStats = false;
		
		[Header("Database")]
		[Tooltip("All the possible stats in the game, will use the data to update any others with the same name")]
		public CharacterSystem.Stats MyStats = new CharacterSystem.Stats();
		
		[Tooltip("A list of the items that will be used in the game")]
		public List<Item> MyItems;

		public List<Texture> MyTextures;
		public Texture DefaultTexture;
		// for stat descriptions

		// Use this for initialization
		void Start () {
			if (IsUpdateOnStart) {
				IsUpdateItems = true;
				IsUpdateStats = true;
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (IsUpdateAll) {
				IsUpdateAll = false;
				IsUpdateItems = true;
				IsUpdateStats = true;
			}
			if (IsGatherItems) {
				IsGatherItems = false;
				GatherItemsFromScene();
				Debug.Log ("Gathered all items.");
			}
			if (IsGatherStats) {
				IsGatherStats = false;
				GatherStatsFromScene();
				Debug.Log ("Gathered all items.");
			}
			if (IsUpdateStats) {
				IsUpdateStats = false;
				UpdateItemsWithStatData();
				Debug.Log ("Updated all items.");
			}
			if (IsUpdateItems) {
				IsUpdateItems = false;
				UpdateAllItems();
				Debug.Log ("Updated all items.");
			}
		}

		
		public void UpdateAllStats() {
			UpdateItemsWithStatData ();
			UpdateAllItems ();
			
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) {
				CharacterSystem.CharacterStats MyCharacterStats = MyObject.GetComponent<CharacterSystem.CharacterStats> ();
				if (MyCharacterStats) {
					for (int i = 0; i < MyStats.GetSize(); i++) 
					{
						MyCharacterStats.BaseStats.ReplaceStatData(MyStats.GetStat(i));
						//CheckStatForReplacement (MyItem);
					}
				} 
			}
		}
		
		public void UpdateItemsWithStatData() {
			for (int i = 0; i < MyItems.Count; i++) {
				//for (int j = 0; j < MyItems[i].MyStats.Size (); j++) {
				for (int k = 0; k < MyStats.GetSize (); k++) {
						MyItems[i].MyStats.ReplaceStatData(MyStats.GetStat(k));
						//if (MyItems[i].MyStats.GetStat(j).Name == MyStats.GetStat(k).Name) {
							//MyItems[i].MyStats.GetStat(j).SetDescription(MyStats.GetStat(k).GetDescription());
						//}
					}
				//}
			}
		}

		public Texture FindTexture(string TextureName) 
		{
			for (int i = 0; i < MyTextures.Count; i++) 
			{
				if (MyTextures[i].name == TextureName)
					return MyTextures[i];
			}
			return DefaultTexture;
		}

		private void GatherStatsFromScene() 
		{
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) {
				if (MyObject.GetComponent<ItemObject> ()) {
					CheckForStatsAdd (MyObject.GetComponent<ItemObject> ().MyItem);	
				} else if (MyObject.GetComponent<Inventory> ()) {
					Inventory MyInventory = MyObject.GetComponent<Inventory> ();
					for (int j = 0; j < MyInventory.MyItems.Count; j++) {
						CheckForStatsAdd (MyInventory.MyItems [j]);
					}
				}
			}
		}

		private void CheckForStatsAdd(Item MyItem)
		{
			for (int i = 0; i < MyItem.MyStats.GetSize(); i++) 
			{
				if (!MyStats.HasStat(MyItem.MyStats.GetStat(i))) 
				{
					MyStats.Add (MyItem.MyStats.GetStat(i));
				}
			}
		}

		public static List<GameObject> GatherAllItemObjects(string ComponentName)
		{
			List<GameObject> MyItems = new List<GameObject> ();
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) {
#if UNITY_EDITOR
				if (MyObject.GetComponent(ComponentName) && MyObject.activeSelf)
				{
					//Debug.Log ("Found: " + MyObject.name + " : " + UnityEditor.PrefabUtility.GetPrefabType(MyObject).ToString());
					if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None ||
					    UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.DisconnectedPrefabInstance) 
					{
						MyItems.Add (MyObject);
					}
				}
#else
				if (MyObject.GetComponent(ComponentName) && MyObject.activeSelf)
					MyItems.Add (MyObject);
#endif
			}
			return MyItems;
		}

		private void GatherItemsFromScene() 
		{
			MyItems.Clear ();
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) {
				//if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None) 
				{
					if (MyObject.GetComponent<ItemObject> ()) {
						CheckForItemAdds (MyObject.GetComponent<ItemObject> ().MyItem);	
					} else if (MyObject.GetComponent<Inventory> ()) {
						Inventory MyInventory = MyObject.GetComponent<Inventory> ();
						for (int j = 0; j < MyInventory.MyItems.Count; j++) {
							CheckForItemAdds (MyInventory.MyItems [j]);
						}
					}
				}
			}
		}

		private void CheckForItemAdds(Item MyItem)
		{
			for (int i = 0; i < MyItems.Count; i++) 
			{
				if (MyItems[i].Name == MyItem.Name) 
				{
					return;
				}
			}
			MyItems.Add (MyItem);
		}

		public void UpdateAllItems() 
		{
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) {
				//if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None) 
				{
					if (MyObject.GetComponent<ItemObject> ()) {
						Item MyItem = MyObject.GetComponent<ItemObject> ().MyItem;
						CheckItemForReplace (MyItem);	
					} else if (MyObject.GetComponent<Inventory> ()) {
						Inventory MyInventory = MyObject.GetComponent<Inventory> ();
						
						for (int j = 0; j < MyInventory.MyItems.Count; j++) {
							Item MyItem = MyInventory.MyItems [j];
							CheckItemForReplace (MyItem);
						}
					}
				}
			}
		}
		private void CheckItemForReplace(Item NewItem) 
		{
			for (int i = 0; i < MyItems.Count; i++)
			{
				if (Item.ReplaceItem(NewItem, MyItems[i]))
					i = MyItems.Count;
			}
		}
	}
}
