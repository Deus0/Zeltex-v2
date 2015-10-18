using UnityEngine;
using System.Collections;
using System.IO;

public static class FileLocator {
	public static string SaveLocation(string WorldName, string PlayerName)	{
		return SaveLocation (WorldName, PlayerName, "CharacterSaves/", ".chr");
	}
	public static string SaveLocation(string WorldName, string PlayerName, string FolderName, string FileExtension)	{
		string TemporarySaveFileName = (Application.dataPath.Replace("Assets", ""))+ Serialization.saveFolderName + "/" + WorldName + "/";

		if (!Directory.Exists(TemporarySaveFileName))
		{
			Directory.CreateDirectory(TemporarySaveFileName);
		}
		
		TemporarySaveFileName += FolderName;
		
		if (!Directory.Exists(TemporarySaveFileName))
		{
			Directory.CreateDirectory(TemporarySaveFileName);
		}
		TemporarySaveFileName += (PlayerName + FileExtension);
		return TemporarySaveFileName;
	}
}