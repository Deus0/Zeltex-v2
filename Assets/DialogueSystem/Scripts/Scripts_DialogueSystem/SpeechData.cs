using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO; 

/*
	Class responsible for reading and writing from dialogue files
	
*/

namespace DialogueSystem {
	public static class SpeechFileReader {
		public static bool ReadDialogue(string FileName, SpeechHandler NewSpeech) {
			bool HasLoaded = false;
			List<string> MyLines = new List<string>();
			NewSpeech.Clear ();
			string CharacterName = FileName;
			//if (!FileName.Contains (".txt"))
			//	FileName += ".txt";
			string TemporaryFileName = Application.dataPath;
			
			// i can see this going bad
			if (TemporaryFileName.Contains("Assets")) {
				TemporaryFileName = TemporaryFileName.Remove(TemporaryFileName.IndexOf ("Assets"), "Assets".Length);
			}
			
			if (TemporaryFileName [TemporaryFileName.Length - 1] != '/')
				TemporaryFileName += '/';
			TemporaryFileName =  TemporaryFileName+ "Resources/"+FileName;

			
			
			TextAsset MyText = (TextAsset)Resources.Load(CharacterName, typeof(TextAsset));
			if (MyText == null)
				return false;
			//PrintText("Done: " + MyText.text);
			string[] linesInFile = MyText.text.Split('\n');
			for (int i = 0; i < linesInFile.Length; i++) {
				//Application.ExternalCall("console.log", "Loaded:" + i + ": " + linesInFile[i]);
				MyLines.Add (linesInFile[i]);
			}

			//NewSpeech.MyFile = WWW.UnEscapeURL(TemporaryFileName);
			if (MyLines.Count > 0) {
				HasLoaded = true;
			}
			//NewSpeech.MyFile = TemporaryFileName;
			LoadFile (CharacterName, NewSpeech, MyLines);
			//NewSpeech.MyFile = CharacterName;
			return HasLoaded;
		}
		public static void PrintText(string MyText) {
			if (Application.isWebPlayer)
				Application.ExternalCall("console.log", MyText);
			else
				Debug.LogError(MyText);
		}
		public static void LoadFile(string CharacterName, SpeechHandler NewSpeech, List<string> MyLines) {
			List<string> SavedData = new List<string> ();//used to break up commands
			bool IsReadingID = false;
			bool IsReadingQuest = false;
			
			for (int i = 0; i < MyLines.Count; i++) {
				string line = MyLines[i];
				if (ContainsMainTag(line)) {
					if (IsReadingID) {
						//Debug.LogError("Adding new dialog! at line: " + line);
						DialogueLine NewDialogue = new DialogueLine(SavedData, NewSpeech.DialogueSize()+1, CharacterName);
						NewSpeech.AddDialogue (NewDialogue);
					} else if (IsReadingQuest) {
						Quest NewQuest = new Quest (SavedData);
						//Debug.LogError("Adding new quest: " + NewQuest.Name);
						if (NewSpeech.GetMainTalker())
							NewSpeech.GetMainTalker().AddQuest(NewQuest);
						else
							Debug.LogError("No main talker in conversation: " + NewSpeech.name);
					}
					SavedData.Clear();
					IsReadingID = (line.Contains ("/id"));
					IsReadingQuest =(line.Contains ("/quest"));
				}
				else {
					SavedData.Add (line);
				}
			}
			if (IsReadingID ) {
				DialogueLine NewDialogue = new DialogueLine(SavedData, NewSpeech.DialogueSize()+1, CharacterName);
				NewSpeech.AddDialogue (NewDialogue);
			}
			if (IsReadingQuest) {
				Quest NewQuest = new Quest (SavedData);
				NewSpeech.GetMainTalker().MyQuests.Add (NewQuest);
			}
		}

		public static bool ContainsMainTag(string Line) {
			return (Line.Contains ("/id") || Line.Contains ("/quest"));
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
	}
}
/*	Reading files from any URL

		TemporaryFileName = Application.dataPath;

			TemporaryFileName =  TemporaryFileName+WWW.EscapeURL("/"+FileName);
			//TemporaryFileName = WWW.EscapeURL("http://www.net-snmp.org/docs/mibs/NET-SNMP-EXAMPLES-MIB.txt");
			//TemporaryFileName = (@"file:///C:/DialogueSystemWeb/Lotus.txt");
			Application.ExternalCall("console.log", "Normal URL: "+ WWW.UnEscapeURL(TemporaryFileName));
			Application.ExternalCall("console.log", "Escaped URL: "+ (TemporaryFileName));

			//TemporaryFileName = WWW.EscapeURL("file:///C:/%23%20Project%20Urbem%20Mortourum/DialogueSystemWeb/Lotus.txt");
			WWW request;
			request = new WWW(TemporaryFileName);
			int CheckCount = 0;
			while(!request.isDone) {
				Debug.Log("Loading...");
				CheckCount++;
				if (CheckCount > 5000) {
					Application.ExternalCall("console.log", "Failed to load files..");
					break;
				}
			}
			if (request.isDone) {
				NewSpeech.HasLoaded = true;
				string TotalText = request.text;
				if (TotalText == "") {
					Application.ExternalCall("console.log", "Null text.." + TotalText);
				} else 
					Application.ExternalCall("console.log", "Success..." + TotalText);

				Application.ExternalCall("console.log", "Bytes Size: " + request.bytes.Length);
				if (!string.IsNullOrEmpty(request.error)) {
					Application.ExternalCall("console.log", "Error..." + request.error);
				}
				//TotalText = System.Text.Encoding.UTF8.GetString(request.bytes, 3, request.bytes.Length - 3);
				Application.ExternalCall("console.log", "Success..." + TotalText);
				string[] linesInFile = TotalText.Split('\n');
				for (int i = 0; i < linesInFile.Length; i++) {
					Application.ExternalCall("console.log", "Loaded:" + i + ": " + linesInFile[i]);
					MyLines.Add (linesInFile[i]);
				}
			}
			
			Application.ExternalCall("console.log", "Done: " + MyLines.Count);*/
