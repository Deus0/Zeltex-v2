using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using DialogueSystem;

namespace CharacterSystem 
{
	public class CharacterSaver : MonoBehaviour 
	{
		[Tooltip("Used to test functions")]
		public bool IsDebugMode = false;
		[Tooltip("In Debugmode, loads character")]
		public KeyCode LoadKey = KeyCode.L;
		[Tooltip("In Debugmode, saves character")]
		public KeyCode SaveKey = KeyCode.K;
		
		[Tooltip("Loads from checkpoint when starts game.")]
		public bool IsLoadOnStart = false;

		void Awake() 
		{
			if (IsLoadOnStart) {
				LoadCharacter();
			}
		}

		void Update() 
		{
			if (IsDebugMode)
			{
				if (Input.GetKeyDown (LoadKey))
				{
					LoadCharacter();
				}
				else if (Input.GetKeyDown (SaveKey)) 
				{
					SaveCharacter(transform);
				}
			}
		}
		
		public void SaveOnCollide(GameObject ItemCollided, GameObject MyCharacter)
		{
			//if (ItemCollided.GetComponent<ItemObject> ())
			//	ItemCollided.GetComponent<ItemObject> ().GetPickedUp ();
			SaveCharacter (transform);
		}
		
		public void LoadOnCollide(GameObject ItemCollided, GameObject MyCharacter)
		{
			//if (ItemCollided.GetComponent<ItemObject> ())
			//	ItemCollided.GetComponent<ItemObject> ().GetPickedUp ();
			LoadCharacter ();
		}

		public static void SaveCharacter(Transform MyTransform) 
		{
			SaveCharacter (MyTransform, FileUtil.GetCharacterSaveLocation (MyTransform));
		}
		public static string GetFileName(Transform MyTransform) {
			return MyTransform.name + ".chr";
		}
		public static void SaveCharacter(Transform MyTransform, string SaveLocation) 
		{
			var MySaveFile = File.CreateText(SaveLocation+GetFileName(MyTransform));	//
			MySaveFile.WriteLine (MyTransform.position.x);
			MySaveFile.WriteLine (MyTransform.position.y);
			MySaveFile.WriteLine (MyTransform.position.z);
			MySaveFile.Close ();
		}
		public void LoadCharacter() {
			LoadCharacter (transform);
		}
		public static void LoadCharacter(Transform MyTransform) 
		{
			LoadCharacter (MyTransform, FileUtil.GetCharacterSaveLocation (MyTransform));
		}
		public static void LoadCharacter(Transform MyTransform, string SaveLocation) 
		{
			try {
				string FileName = SaveLocation;	//GetSaveLocation (MyTransform);	//+GetFileName(MyTransform)
				Vector3 PreviousPosition = MyTransform.position;
				var MySaveFile = File.ReadAllLines (FileName);
				if (MySaveFile.Length == 3) {
					try {
						MyTransform.position = new Vector3 (
							float.Parse(MySaveFile [0]), float.Parse(MySaveFile [1]), float.Parse(MySaveFile [2]));
					} catch(System.FormatException e) { }
				} else {
					Debug.Log("File was not in the right format.");
				}
			Vector3 DifferencePosition = MyTransform.position - PreviousPosition;
				if (MyTransform.parent)
			for (int i = 0; i < MyTransform.parent.childCount; i++) {
				GameObject SiblingObject = MyTransform.parent.GetChild(i).gameObject;
				if (SiblingObject != MyTransform.gameObject) {
					SiblingObject.transform.position += DifferencePosition;
				}
			}
			} catch (FileNotFoundException e) {
				Debug.Log("No file for character to load at " + MyTransform.name);
			}
		}
	}
}