using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CharacterSystem {
	[ExecuteInEditMode]
	public class CharacterManager : MonoBehaviour 
	{
		[Header("Debug")]
		[SerializeField] private bool IsDebugMode;
		
		[Header("Actions")]
		[Tooltip("Gathers all the characters in the scene.")]
		[SerializeField] private bool IsGatherCharacters = false;
		[Tooltip("Saves all the positions, quest logs and inventory data.")]
		[SerializeField] private bool IsSaveCharacters = false;
		[Tooltip("RLoads all the character temporary data")]
		[SerializeField] private bool IsLoadCharacters = false;

		[Header("Data")]
		[SerializeField] private string SaveFileName = "";
		public List<GameObject> MyCharacters = new List<GameObject>();
		
		void OnGUI()
		{
			if (IsDebugMode)
			{
				GUILayout.Label (MyCharacters.Count + " Characters in map.");
			}
		}

		// Update is called once per frame
		void Update () 
		{
			HandleActions ();
		}

		private void HandleActions() 
		{
			if (IsGatherCharacters) 
			{
				IsGatherCharacters = false;
				MyCharacters = GatherCharacters ();
			}
			if (IsSaveCharacters) 
			{
				IsSaveCharacters = false;
				Debug.Log ("Saving: " + MyCharacters.Count + " - at [" + VoxelEngine.ChunkUpdater.GetCurrentTime () + "]");
				for (int i = 0; i < MyCharacters.Count; i++)
				{
					//CharacterSaver MySaver = MyCharacters[i].GetComponent<CharacterSaver>();
					//if (MySaver) 
					{
						CharacterSaver.SaveCharacter (MyCharacters [i].transform);
					}
				}
			}
			if (IsLoadCharacters)
			{
				IsLoadCharacters = false;
				for (int i = 0; i < MyCharacters.Count; i++)
				{
					CharacterSaver MySaver = MyCharacters [i].GetComponent<CharacterSaver> ();
					if (MySaver) 
					{
						MySaver.LoadCharacter ();
					}
				}
			}
		}

		public void Add(Transform NewCharacter) 
		{
			for (int i = 0; i < MyCharacters.Count; i++) 
			{
				if (NewCharacter.name == MyCharacters[i].name) 
				{
					return;
				}
			}
			if (NewCharacter.GetComponent<Character> ()) 
			{
				MyCharacters.Add (NewCharacter.gameObject);
			}
		}

		public static string GetCharacterSaveFilePath(string WorldName) 
		{
			return VoxelEngine.VoxelSaver.GetWorldFolderPath (WorldName);
		}

		public static void SaveCharactersInChunks(VoxelEngine.World MyWorld, string WorldName) 
		{
			List<GameObject> MyCharacters = GatherCharacters ();
			for (int i = 0; i < MyCharacters.Count; i++)
			{
				Vector3 ChunkPosition = VoxelEngine.World.GetChunkPosition(MyWorld, MyCharacters[i].transform.position);
				string SaveFileName = GetCharacterSaveFilePath(WorldName) + ChunkPosition.ToString();
				CharacterSaver.SaveCharacter(MyCharacters[i].transform, SaveFileName);
			}
		}

		public static void Clear() 
		{
			List<GameObject> MyCharacters = GatherCharacters ();
			for (int i = MyCharacters.Count-1; i >= 0; i--) {
				CharacterSystem.CharacterPrefabLoader.DestroyThing (MyCharacters[i]);
			}
		}

		public static List<GameObject> GatherCharacters() 
		{
			List<GameObject> MyGatheredOnes = new List<GameObject> ();
			GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
			foreach (GameObject MyObject in AllObjects) 
			{
				if (MyObject.activeSelf && MyObject.GetComponent<Character> ()) 
				{
					#if UNITY_EDITOR
					if (UnityEditor.EditorApplication.isPlaying) 
					{
						if (!MyObject.transform.IsPrefab())
							MyGatheredOnes.Add(MyObject);
					}
					else 
					{
						if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None) 
						{
							MyGatheredOnes.Add(MyObject);
						}
					}
					#else
					if (!MyObject.transform.IsPrefab())
						MyGatheredOnes.Add(MyObject);
					#endif
				}
			}
			return MyGatheredOnes;
		}
	}
}

public static class ZeltexGameObjectExtensions
{
	public static bool IsPrefab(this Transform This)
	{
		var TempObject = new GameObject();
		try
		{
			TempObject.transform.parent = This.parent;
			var OriginalIndex = This.GetSiblingIndex();
			This.SetSiblingIndex(int.MaxValue);
			if (This.GetSiblingIndex() == 0) return true;
			This.SetSiblingIndex(OriginalIndex);
			return false;
		}
		finally
		{
			Object.DestroyImmediate(TempObject);
		}
	}
}