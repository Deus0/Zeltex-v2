using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO; 
using ItemSystem;
using QuestSystem;
using DialogueSystem;

/*
	Class responsible for reading and writing from dialogue files

		rename - CharacterScriptHandler
*/

public static class FileUtil 
{
	public static string GetFolderPath() {
		string FolderPath = Application.dataPath;
		if (FolderPath.Length > 0) 
		{
			if (FolderPath [FolderPath.Length - 1] != '/')
				FolderPath += '/';
		}
		FolderPath +=  "Resources/";
		return FolderPath;
	}
	
	public static string GetVoxelFolderPath() 
	{
		string FolderPath = GetFolderPath ();
		FolderPath += "VoxelData/";
		if (!Directory.Exists (FolderPath))
		{
			Directory.CreateDirectory (FolderPath);
		}
		return FolderPath;
	}
	
	// used in character saver class
	public static string GetCharacterSaveLocation(Transform MyTransform) 
	{
		return FileUtil.GetFolderPath ();
	}
}

namespace CharacterSystem
{
	public static class SpeechFileReader
	{
		public static string[] MyMainTags = new string[] {	"id", 				// dialogue
															"quest",			// quests
															"item", 			// inventory items
															"characterstats", 	// character stats
															"player"};			// player controls

		public static List<string> ReadTextFileList(string FileName)
		{
			string MyFileText = ReadTextFile (FileName);
			//string MyFileText = File.ReadAllText (FileName);
			//Debug.LogError ("Read da file!" + MyFileText.Length);
			List<string> MyLines = ConvertToList(MyFileText);
			return MyLines;
		}
		
		public static string ReadTextFile(string FileName) 
		{
			if (File.Exists (FileName)) 
				return File.ReadAllText (FileName);
			else
				return "";
		}

		public static void SaveFile(List<string> MySections, string FileName) 
		{
			string MyData = ConvertToSingle (MySections);
			//string FileName = GetFolderPath () + MyCharacter.name;
			var MySaveFile = File.CreateText(FileName);
			string[] SplitData = MyData.Split('\n');
			for (int i = 0; i < SplitData.Length; i++) {
				SplitData[i] = SplitData[i].Replace("\\n", "");
				if (SplitData[i] != "" && !string.IsNullOrEmpty(SplitData[i]))
					MySaveFile.WriteLine (SplitData[i]);
			}
			MySaveFile.Close();
			Debug.Log ("Saved to: " + FileName);
		}

		public static void PrintText(string MyText) 
		{
			if (Application.isWebPlayer)
				Application.ExternalCall("console.log", MyText);
			else
				Debug.LogError(MyText);
		}
		public static List<string> ConvertToList(string MyInput) 
		{
			List<string> MyList = new List<string> ();
			
			string[] linesInFile = MyInput.Split('\n');
			for (int i = 0; i < linesInFile.Length; i++) {
				//Application.ExternalCall("console.log", "Loaded:" + i + ": " + linesInFile[i]);
				MyList.Add (linesInFile[i]);
			}
			return MyList;
		}
		
		public static void ActivateScript(Transform MyCharacter, string MyScript) 
		{
			ActivateScript (MyCharacter, SplitSections(MyScript));
		}
		public static void ActivateScript(Transform MyCharacter, List<string> MySections) 
		{
			if (MyCharacter == null)
			{
				Debug.LogError("No Character to Activate script on.");
				return;
			}
			if (MySections.Count == 0) {
				Debug.LogError("No Script to Activate on Character.");
				return;
			}
			Debug.Log ("Reading file of " + MyCharacter.name + " with " + MySections.Count + " sections!");
			QuestLog MyQuestLog = MyCharacter.GetComponent<QuestLog> ();
			if (MyQuestLog == null)
			{
				MyQuestLog = MyCharacter.gameObject.AddComponent<QuestLog>();
			} else {
				MyQuestLog.MyQuests.Clear ();
			}
			Inventory MyInventory = MyCharacter.GetComponent<Inventory> ();
			if (MyInventory == null)
			{
				MyInventory = MyCharacter.gameObject.AddComponent<Inventory>();
			} else {
				MyInventory.MyItems.Clear ();
			}
			CharacterStats MyCharacterStats = MyCharacter.GetComponent<CharacterStats> ();
			if (MyCharacterStats == null)
			{
				MyCharacterStats = MyCharacter.gameObject.AddComponent<CharacterStats>();
			} else {
				MyCharacterStats.BaseStats.Clear ();
				MyCharacterStats.TempStats.Clear ();
			}
			SpeechHandler MySpeech = MyCharacter.GetComponent<SpeechHandler> ();
			if (MySpeech == null) {
				MySpeech = MyCharacter.gameObject.AddComponent<SpeechHandler>();
			} else {
				MySpeech.MyTree.Clear ();
			}

			for (int i = 0; i < MySections.Count; i++) 
			{
				List<string> MyList = ConvertToList(MySections[i]);	// convert the section text to a list of lines
				if (MyList.Count > 0) {
					if (MyList[0].Contains ("/" + MyMainTags[0])) {	// /id []
						DialogueData NewDialogue = new DialogueData(MyList, MySpeech.GetSize()+1, MyCharacter.name, MyCharacter.gameObject);
						MySpeech.AddDialogue (NewDialogue);
					} else if (MyList[0].Contains ("/" + MyMainTags[1])) {	// /quest []
						Quest NewQuest = new Quest (MyList);
						MyQuestLog.MyQuests.Add (NewQuest);
					} else if (MyList[0].Contains ("/" + MyMainTags[2])) {	// /item []
						Item NewItem = new Item (MyList);
						MyInventory.AddItem(NewItem);
					} else if (MyList[0].Contains ("/" + MyMainTags[3])) {	// /characterstats []
						MyCharacterStats.RunScript(MyList);
					} else if (MyList[0].Contains ("/" + MyMainTags[4])) {	// /player []
						//MyCharacterStats.Load(MyList);
					}
				}
			}
		}
		public static string ConvertToSingle(List<string> MyList) {
			string NewString = "";
			for (int i = 0; i < MyList.Count; i++) 
			{
				string NewLine = MyList[i];
				if (NewLine.Length > 0) {
					if (NewLine[NewLine.Length-1] != '\n')
						NewLine += '\n';
					NewString += NewLine;
				}
			}
			return NewString;
		}
		public static bool ContainsMainTag(string Line) {
			Line = Line.ToLower ();
			for (int i = 0; i < MyMainTags.Length; i++) {
				string CheckTag = "/" + MyMainTags[i];
				if (i != 3)
					CheckTag += " ";
				if (Line.Contains(CheckTag)) 
				{
					return true;
				}
			}
			return false;
		}
		// Splits up the sections using the main tags - /id - /quest - /item - /stats
		public static List<string> SplitSections(string FileText)
		{
			List<string> MyLines = new List<string>();
			if (FileText == "")
				return MyLines;
			string CurrentSection = "";
			string[] linesInFile = FileText.Split('\n');
			for (int i = 0; i < linesInFile.Length; i++) {
				if (ContainsMainTag(linesInFile[i])) {
					if (CurrentSection != "") {
						MyLines.Add (CurrentSection);
						CurrentSection = "";
					}
				}
				//linesInFile[i] = SpeechUtilities.RemoveWhiteSpace(linesInFile[i]);
				if (linesInFile[i] != "\n")
					CurrentSection += linesInFile[i] + '\n';	// adding the return /n as it is removed with split function
			}
			if (CurrentSection != "") {
				MyLines.Add (CurrentSection);
				CurrentSection = "";
			}
			return MyLines;
		}
		
		public static List<string> SplitCommands(string SavedData) {
			string[] MyCommandsArray = SavedData.Split(' ');
			
			for (int j = 0; j < MyCommandsArray.Length; j++) {
				if (MyCommandsArray [j].Contains (","))
					MyCommandsArray [j] = MyCommandsArray [j].Remove (MyCommandsArray [j].IndexOf (","));
			}
			List<string> MyCommands = new List<string> ();
			for (int j = 0; j < MyCommandsArray.Length; j++) {
				if (!IsEmptyLine(MyCommandsArray[j])) {
					MyCommands.Add(MyCommandsArray[j]);
				}
			}
			return MyCommands;
		}
		public static bool IsEmptyLine(string MyLine) {
			bool IsEmpty = true;
			for (int k = 0; k < MyLine.Length; k++) {
				if (MyLine[k] != ' ' && MyLine[k] != '\n' && (int)(MyLine[k]) != 13 && MyLine[k] != '\t')
				{
					return false;
				}
			}
			return IsEmpty;
		}
	}
}