using System.Collections.Generic;
using UnityEngine;

namespace CharacterSystem {
	[System.Serializable]
	public class Stats 
	{
		public List<Stat> Data = new List<Stat>();
		public Stats() {
			
		}
		public string GetScript() {
			string MyScript = "";
			for (int i = 0; i < GetSize(); i++)
			{
				MyScript += (GetStat(i).GetScript()) + "\n";
				//GUILayout.Label(MyStats.TempStats.GetStat(i).GetScript());
			}
			return MyScript;
		}
		public Stats(List<string> StringData) {
			Debug.LogError ("Reading stats!");
			Data.Clear ();
			if (StringData.Count == 0)
				return;
			bool IsReadingStats = false;
			{
				Debug.Log ("Loading " + StringData.Count + " stats.");
				for (int i = 0; i < StringData.Count; i++) {
					Debug.LogError("Reading line: " + StringData[i]);
					if (StringData [i].Contains ("/endstats"))
					{
						IsReadingStats = false;
					}
					if (IsReadingStats) 
					{
						StringData[i] = SpeechUtilities.RemoveWhiteSpace(StringData[i]);
						if (StringData[i] != "") 
						{
							if (!SpeechUtilities.IsCommand(StringData[i]))
							{
								Debug.LogError("NewStat! " + StringData[i]);
								Stat NewStat = new Stat(StringData[i]);
								if (NewStat.GetDescription() != "/Destroy")
									Data.Add (NewStat);
							}
							else 
							{	// if has / at the front!
								Data[Data.Count-1].ActivateCommand(StringData[i]);
							}
						}
					}
					if (StringData [i].Contains ("/stats") || StringData [i].Contains ("/characterstats")) 
						IsReadingStats = true;
				}
			}
		}
		public void ReplaceStatData(Stat NewStat) {
			for (int i = 0; i < Data.Count; i++) {	
				if (Data[i].Name == NewStat.Name) // if in list, increase the value of the stat
				{
					Data[i].SetDescription(NewStat.GetDescription());
					Data[i].SetTexture(NewStat.GetTexture());
					return;
				}
			}
		}
		public bool HasStat(Stat MyStat) {
			for (int i = 0; i < Data.Count; i++) {	
				if (Data[i].Name == MyStat.Name) // if in list, increase the value of the stat
				{
					return true;
			 	}
			 }
			return false;
		}
		public void Clear() {
			Data.Clear ();
		}

		public Stat GetStat(int i)
		{
			return Data [i];
		}
		public Stat GetStat(string StatName) {
			for (int i = 0; i < Data.Count; i++) {
				if (StatName == Data[i].Name)
					return Data[i];
			}
			return null;
		}

		public void SetStat(string StatName, float NewValue) {
			for (int i = 0; i < Data.Count; i++) {
				if (StatName == Data[i].Name) {
					Data[i].SetState(NewValue);
					return;
				}
			}
		}

		public int GetSize() {
			return Data.Count; 
		}
		// returns true if list expands, false if already in list
		public bool Add(Stat MyStat) {
			for (int i = 0; i < Data.Count; i++) {
				if (Data[i].Name == MyStat.Name)
				//if (Data[i].GetStatType() == MyStat.GetStatType())// if in list, increase the value of the stat
				{
					Data[i].Add (MyStat.GetValue());
					return false;
				}
			}
			// if not using the new variable, it will override the base stats when creating temp ones
			Data.Add (new Stat(MyStat));	// if not in list, add to list
			return true;
		}
		
		public bool Contains(Stat MyStat) {
			for (int i = 0; i < Data.Count; i++) {
				if (Data[i].Name == MyStat.Name) {
					return true;
				}
			}
			return false;
		}
	}
}