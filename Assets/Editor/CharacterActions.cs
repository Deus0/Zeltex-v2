using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using CharacterSystem;
using ItemSystem;
using QuestSystem;

public class CharacterActions : EditorWindow 
{
	private static Transform MyCharacter;
	private static bool IsEditScript = false;
	//private static List<string> MyScript = new List<string>();
	private static string MyScript = "";
	private static string MyFileName = "";

	[MenuItem("Marz/CharacterEditor")]
	public static void Enable()
	{
		//EditorVoxels.Begin();
		/*if (EditorVoxels.MyWorld == null) {
			EditorVoxels.MyWorld = GameObject.FindObjectOfType<VoxelEngine.World>();
		}*/
		//SceneView.onSceneGUIDelegate += OnScene;
		//Debug.Log("Scene GUI : Enabled");
		//UnityEditor.EditorApplication.update += Update;
		
		CharacterActions MyWindow = 
			(CharacterActions)EditorWindow.GetWindow(typeof(CharacterActions));
	}

	void OnGUI ()
	{
		GUILayout.Label ("Character Editor");
		// Player I want to update
		GUILayout.Label ("");
		Transform OldCharacter = MyCharacter;
		MyCharacter = ((Transform) EditorGUI.ObjectField(new Rect(3, GUILayoutUtility.GetLastRect().y, 300, 18), 
		                                              "Target Character", 
		                                              MyCharacter, 
		                                              typeof(Transform)));
		// if has selected new character
		if (MyCharacter != OldCharacter)
		{
			if (MyCharacter.GetComponent<Character> () == null)
				MyCharacter = null;
			else {
				GatherScript();
			}
		}

		if (MyCharacter) 
		{
			IsEditScript = GUILayout.Toggle(IsEditScript, "Edit Script");
			GUILayout.Label(MyCharacter.name);
			if (!IsEditScript) {
				CharacterStats MyStats = MyCharacter.GetComponent<CharacterStats>();
				if (MyStats) {
					GUILayout.Label("Stats");
					if (MyStats.TempStats.GetSize() == 0) 
					{
						GUILayout.Label("\tNone");
					} else {
						for (int i = 0; i < MyStats.TempStats.GetSize(); i++)
						{
							GUILayout.Label("\t" + MyStats.TempStats.GetStat(i).GetGuiString());
						}
					}
				}
				Inventory MyInventory = MyCharacter.GetComponent<Inventory>();
				if (MyInventory) {
					GUILayout.Label("Items");
					if (MyInventory.MyItems.Count == 0) 
					{
						GUILayout.Label("\tNone");
					} else {
						for (int i = 0; i < MyInventory.MyItems.Count; i++)
						{
							GUILayout.Label("\t" + MyInventory.GetItem(i).GetDescription());
						}
					}
				}
				QuestLog MyQuestLog = MyCharacter.GetComponent<QuestLog>();
				if (MyQuestLog) {
					GUILayout.Label("Quests");
					if (MyQuestLog.MyQuests.Count == 0) 
					{
						GUILayout.Label("\tNone");
					} else {
						for (int i = 0; i < MyQuestLog.MyQuests.Count; i++)
						{
							GUILayout.Label("\t" + MyQuestLog.MyQuests[i].GetDescriptionText());
						}
					}
				}
			} 
			else 
			{
				if (GUILayout.Button ("Load: " + MyFileName))
				{
					string OldFileName = MyFileName;
					MyFileName = EditorUtility.OpenFilePanel (
						"Select a script",
						"",
						"txt");
					if (MyFileName != OldFileName)
					{
						CharacterSystem.CharacterSpawner.RunScript(MyCharacter, MyFileName);
					}
				}

				if (GUILayout.Button("Activate Script"))
				{
					CharacterSystem.SpeechFileReader.ActivateScript(MyCharacter, MyScript);
				}
				if (GUILayout.Button("Refresh Script"))
				{
					GatherScript();
				}
				MyScript = GUILayout.TextArea(MyScript);
				/*for (int i = 0; i < MyScript.Count; i++) 
				{
					MyScript[i] = GUILayout.TextField(MyScript[i]);
				}*/
				/*string MyScript = CharacterSystem.SpeechFileReader.ReadTextFile(MyCharacter.name);
				GUILayout.Label(MyScript);*/
			}
		}
	}

	private static void GatherScript() 
	{
		MyScript = "";
		CharacterStats MyStats = MyCharacter.GetComponent<CharacterStats>();
		if (MyStats) 
		{
			if (MyStats.TempStats.GetSize() == 0) 
			{

			} else {
				MyScript += "/characterstats" + "\n";
				MyScript += MyStats.BaseStats.GetScript();
				MyScript += "/endstats" + "\n"+ "\n";
			}
		}
		
		Inventory MyInventory = MyCharacter.GetComponent<Inventory>();
		if (MyInventory) {
			MyScript += MyInventory.GetScript();
		}
	}
}
