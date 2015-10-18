using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;


public class SpellManager : MonoBehaviour {
	public List<Spell> SpellsList;
	public List<Texture> SpellTextures;
	// data viewing mode
	public bool bIsDebugSpellData = false;
	public GameObject MySpellDataGui;
	public int SelectedSpellIndex = 0;
	public string GameName = "NewGame";

	// Use this for initialization
	void Awake () {
		LoadAllSpells ();
		if (GetManager.GetGameManager ())
			GameName = GetManager.GetGameManager ().GameName;
		UpdateSpellDataGui();
	}
	
	// Update is called once per frame
	void Update () {
		HandleSpellDataMode ();
	}
	
	public void ToggleDebugSpellData() {
		bIsDebugSpellData = !bIsDebugSpellData;
		if (MySpellDataGui == null)
			MySpellDataGui = GameObject.Find ("SpellsDebugData");
		if (MySpellDataGui != null)
			MySpellDataGui.SetActive (bIsDebugSpellData);
	}
	public void HandleSpellDataMode() {
		if (bIsDebugSpellData) {
			//if (Input.GetKeyDown (KeyCode.K)) {
			//	//SaveAllSpells ();
			//}
			//if (Input.GetKeyDown (KeyCode.L)) {
				//LoadAllSpells ();
			//}
			//if (Input.GetKeyDown (KeyCode.C)) {
				//ClearSpells ();
			//}
			/*if (Input.GetKeyDown (KeyCode.N)) {
				PreviousSpell();
			}
			if (Input.GetKeyDown (KeyCode.M)) {
				NextSpell();
			}*/
		}
	}
	public void SaveSelectedSpell() {
		SaveAllSpells ();
	}
	public void CloneSelectedSpell() {
		Spell NewSpell = new Spell (SpellsList[SelectedSpellIndex]);
		SpellsList.Insert (SelectedSpellIndex+1,NewSpell);
		UpdateSpellDataGui();
	}
	public void DeleteSelectedSpell() {
		SpellsList.RemoveAt (SelectedSpellIndex);
		UpdateSpellDataGui();
	}
	public void NewSpell() {
		Spell NewSpell = new Spell ();
		SpellsList.Insert (SelectedSpellIndex+1,NewSpell);
		UpdateSpellDataGui();
	}
	public void NextSpell() {
		SelectedSpellIndex++;
		if (SelectedSpellIndex > SpellsList.Count-1) 
			SelectedSpellIndex = 0;
		UpdateSpellDataGui();
	}
	public void PreviousSpell() {
		SelectedSpellIndex--;
		if (SelectedSpellIndex < 0)
			SelectedSpellIndex = SpellsList.Count - 1;
		UpdateSpellDataGui();
	}
	public void UpdateSpellDataGui() {
		if (MySpellDataGui.transform.FindChild ("SpellCountText")) 
			MySpellDataGui.transform.FindChild ("SpellCountText").GetComponent<Text> ().text = "[" + (SelectedSpellIndex+1) + "/" + SpellsList.Count.ToString () + "]";
		if (MySpellDataGui.transform.FindChild ("SelectedSpellText")) 
			MySpellDataGui.transform.FindChild ("SelectedSpellText").GetComponent<Text> ().text = SpellsList[SelectedSpellIndex].ConvertToText();

		if (MySpellDataGui.transform.FindChild ("SpellTexture"))
			if (SpellsList[SelectedSpellIndex].MyTextureId >= 0 && SpellsList[SelectedSpellIndex].MyTextureId < SpellTextures.Count)
			MySpellDataGui.transform.FindChild ("SpellTexture").GetComponent<RawImage> ().texture =  SpellTextures[SpellsList[SelectedSpellIndex].MyTextureId];
	}
	
	public void SaveAllSpells() {
		Debug.LogError("Saving " + SpellsList.Count + " Spells");
		for (int i = 0; i < SpellsList.Count; i++) {
			string SaveFileName = FileLocator.SaveLocation (GetManager.GetGameManager().GameName, "Spell" + i.ToString(), "Spells/", ".spe");
			SpellData MySpellData = new SpellData(SpellsList[i]);
			SaveSpellToText(SaveFileName, MySpellData);
		}
	}
	public void SaveSpellToText(string SaveFileName, SpellData MySpellData) {
		var MyFile = File.CreateText(SaveFileName);
		List<string> MyData = MySpellData.ConvertToTextData ();
		for (int i = 0; i < MyData.Count; i++) {
			MyFile.WriteLine (MyData[i]);
		}
		MyFile.Close();
	}
	public SpellData LoadSpellFromText(string SaveFileName) {
		if (File.Exists (SaveFileName)) {
			//Debug.Log ("File found. " + SaveFileName);
			//Debug.Log ("     v3 Loading spell from: " + SaveFileName.ToString () + " : " + (Directory.Exists (SaveFileName)).ToString ());
			var MyFile = File.OpenText (SaveFileName);
			//Debug.Log ("File opened.");
			SpellData MySpellData = null;
			List<string> SavedData = new List<string> ();
			var line = MyFile.ReadLine ();
			while (line != null) {
				SavedData.Add (line);
				line = MyFile.ReadLine ();
			} 
			MySpellData = new SpellData (SavedData);
			MyFile.Close ();
			return MySpellData;
		} else {
			Debug.Log ("File not found.");
		}
		return null;
	}

	public void LoadAllSpells() {
		SpellsList.Clear ();
		string FolderName = FileLocator.SaveLocation (GameName, "", "Spells/", "");
		Debug.Log ("Loading spells from: " + FolderName + " At Time: " + Time.time);
		DirectoryInfo info = new DirectoryInfo(FolderName);
		FileSystemInfo[] MyFiles = info.GetFileSystemInfos();
		for (int i = 0; i < MyFiles.Length; i++) {
			//Debug.LogError ("     Loading spell from: " + MyFiles[i].FullName);
			if (MyFiles[i].Name.Contains(".spe")) {
				string SaveFileName = FileLocator.SaveLocation (GameName, "Spell" + i.ToString (), "Spells/", ".spe");
				Debug.Log ("    Loading spell from: " + SaveFileName);
				if (File.Exists (SaveFileName)) {
					SpellData NewSpell = LoadSpellFromText(SaveFileName);
					if (NewSpell != null) {
						SpellsList.Add (new Spell (NewSpell));
					}
				} else {
					Debug.Log (SaveFileName + " does not exist :/aerioajer" );
				}
			} else {
				Debug.Log ("     Spell doesn't contain .spe! " );
			}
		}
	}
	public void ClearSpells() {
		SpellsList.Clear ();
	}
}
