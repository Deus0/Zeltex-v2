using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CharacterSystem;
using ItemSystem;
using System.Collections.Generic;
using System.IO;

namespace GuiSystem 
{
	public class MapSelectGui : GuiListHandler 
	{
		//public VoxelEngine.World MyWorld;
		// Update is called once per frame
		void Update () 
		{
			base.Update();
		}
		public static List<string> GetCharactersList(string MyVoxels) {
			List<string> MyCharacters = new List<string> ();
			
			var info = new DirectoryInfo (FileUtil.GetVoxelFolderPath () + MyVoxels + "/");
			//var MyFiles = info.GetFiles();
			var MyFiles = info.GetFiles ();
			for (int i = 0; i < MyFiles.Length; i++) 
			{
				if (MyFiles[i].Name.Contains(".chr") && !MyFiles[i].Name.Contains(".meta")) 
				{
					string NewCharacterName = MyFiles[i].Name.Replace(".chr", "");
					int MyChunkIndex = NewCharacterName.IndexOf(")")+1;
					NewCharacterName = NewCharacterName.Substring(MyChunkIndex);
					MyCharacters.Add (NewCharacterName);
				}
			}
			return MyCharacters;
		}

		public static List<string> GetMapsList() {
			List<string> MyMaps = new List<string> ();
			
			var info = new DirectoryInfo (FileUtil.GetVoxelFolderPath ());
			//var MyFiles = info.GetFiles();
			var MyDirectories = info.GetDirectories ();
			for (int i = 0; i < MyDirectories.Length; i++) {
				//if (MyFiles [i].\
				MyMaps.Add (MyDirectories [i].Name);
			}
			return MyMaps;
		}
		// Handles updating and only update what has changed!
		override public void RefreshList() 
		{
			if (OnClick.GetPersistentEventCount() == 0) 
			{
				OnClick.RemoveAllListeners();
				VoxelEngine.World MyWorld;
				//if (MyWorld == null)
				{
					MyWorld =  GameObject.FindObjectOfType<VoxelEngine.World>();
				}
				if (MyWorld != null)
					OnClick.AddListener(
						MyWorld.GetComponent<VoxelEngine.VoxelSaver>().LoadFromFile
					);
			}
			Debug.LogError ("Refreshing list.");
			Clear ();
			List<string> MyMaps = GetMapsList ();

			for (int i = 0; i < MyMaps.Count; i++) {
				AddGui(MyMaps[i]);
			}
		}
	}
}