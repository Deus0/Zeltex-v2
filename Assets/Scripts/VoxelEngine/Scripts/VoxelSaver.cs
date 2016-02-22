using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VoxelEngine {
	[ExecuteInEditMode]
	[RequireComponent(typeof (World))]
	public class VoxelSaver : MonoBehaviour {
		private World MyWorld;
		[Header("Debug")]
		public bool IsDebugMode = false;
		public KeyCode SaveDataKey;
		public KeyCode LoadDataKey;
		public KeyCode DeleteDataKey;
		[Header("Actions")]
		[Tooltip("Saves the voxel data to a file")]
		[SerializeField] private bool IsSaveToFile = false;
		[Tooltip("Loads the voxel data from a File Path")]
		[SerializeField] private bool IsLoadFromFile = false;
		[Tooltip("Deletes the voxel data at the file path")]
		[SerializeField] private bool IsDeleteFile = false;
		[Tooltip("Deletes the voxel data that is loaded")]
		[SerializeField] private bool IsClear = false;
		[Tooltip("Saves the meshes in a combined mesh asset.")]
		[SerializeField] private bool IsSaveAssets;
		[Header("Options")]
		public bool IsSaveOnUpdate = false;
		[SerializeField] public string SaveFileName = "";
		void Start() 
		{
			MyWorld = gameObject.GetComponent<World> ();
			//LoadFromFile ();
		}
		// stops the chunks from updating
		// clears the chunks
		// clears the bots inside the chunks
		public void Clear() 
		{
			IsClear = false;
			// reset main camera
			//Camera.main.transform.parent = null;
			GetComponent<VoxelLoader>().RefreshWorld();
			// clear minions
			CharacterSystem.CharacterManager.Clear ();
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (IsDebugMode) 
			{
				if (IsSaveToFile || Input.GetKeyDown (SaveDataKey)) {
					IsSaveToFile = false;
					SaveToFile ();
				}
				if (IsLoadFromFile || Input.GetKeyDown (LoadDataKey)) {
					IsLoadFromFile = false;
					LoadFromFile ();
				}
				if (IsSaveAssets) {
					IsSaveAssets = false;
					SaveAsAsset ();
				}
				if (IsClear) {
					Clear ();
				}
			}
		}

		// Deletes all the files inside the map folder
		public void DeleteFile()
		{
			string MyFolderName = GetWorldFolderPath (SaveFileName);
			string[] MyFiles = Directory.GetFiles(MyFolderName);
			if (MyFiles.Length == 0)
			{
				Debug.LogError("No files in directory: " + MyFolderName);
			}
			for (int i = 0; i < MyFiles.Length; i++) 
			{
				string FileName = Path.GetFileName(MyFiles[i]);
				Debug.LogError("Deleting file: " + MyFolderName + FileName);
				//Directory.Delete (MyFolderName + FileName);
				File.Delete(MyFolderName + FileName);
			}
			Directory.Delete (MyFolderName);
			string MetaFileName = MyFolderName.Remove(MyFolderName.Length-1) + ".meta";
			if (File.Exists(MetaFileName))
				File.Delete(MetaFileName);
			//#if UNITY_EDITOR
//			UnityEditor.AssetDatabase.DeleteAsset(MyFolderName);
//#endif
			SaveFileName = "";
			Clear ();
		}
		// Clones the file
		public void CloneFile()
		{
			string NewMapName = SaveFileName + "(2)";
			string FolderName1 = GetWorldFolderPath (SaveFileName);
			string FolderName2 = GetWorldFolderPath (NewMapName);

			string[] MyFiles = Directory.GetFiles(FolderName1);
			if (MyFiles.Length == 0)
			{
				Debug.LogError("No files in directory: " + FolderName1);
			}
			for (int i = 0; i < MyFiles.Length; i++) 
			{
				string FileName = Path.GetFileName(MyFiles[i]);
				Debug.LogError("Moving file: " + FolderName1 + FileName + " - to - " + FolderName2 + FileName);
				//UnityEditor.AssetDatabase.MoveAsset (FolderName1 + FileName, FolderName2 + FileName);
				File.Copy (FolderName1 + FileName, FolderName2 + FileName);
			}
			
			SaveFileName = NewMapName;
		}
		// Renames the file
		public void Rename(string NewSaveFileName) 
		{
			string FolderName1 = GetWorldFolderPath (SaveFileName);
			string FolderName2 = GetWorldFolderPath (NewSaveFileName);
			//#if UNITY_EDITOR
				//UnityEditor.AssetDatabase.MoveAsset (FolderName1, FolderName2);
				string[] MyFiles = Directory.GetFiles(FolderName1);
				if (MyFiles.Length == 0)
				{
					Debug.LogError("No files in directory: " + FolderName1);
				}
				for (int i = 0; i < MyFiles.Length; i++) 
				{
					string FileName = Path.GetFileName(MyFiles[i]);
					Debug.LogError("Moving file: " + FolderName1 + FileName + " - to - " + FolderName2 + FileName);
					//UnityEditor.AssetDatabase.MoveAsset (FolderName1 + FileName, FolderName2 + FileName);
					Directory.Move (FolderName1 + FileName, FolderName2 + FileName);
				}
				Directory.Delete(FolderName1);
			//#else
			//	Directory.Move (FolderName1, FolderName2);
			//#endif
			SaveFileName = NewSaveFileName;
		}

		// creates a safe file for a new world
		public void CreateNewWorld(string NewWorld) 
		{
			// refresh
			VoxelLoader.LoadWorld (MyWorld, new Vector3 (8, 1, 8));
			// first load new world with VoxelTerrain
			VoxelTerrain.CreateTerrain (MyWorld);
			SaveFileName = NewWorld;
			if (SaveFileName == "")
				SaveFileName = "NewWorld";
			SaveToFile ();
			//VoxelSaver.SaveWorld (MyWorld);
		}
			
		public void SaveChunkToFile(ChunkPosition MyPosition) 
		{
			if (IsSaveOnUpdate) 
			{
				Chunk MyChunk = MyWorld.MyChunkData [MyPosition];
				if (MyChunk) {
					SaveToFile (MyChunk);
				}
			}
		}

		public void SaveToFile() 
		{
			Debug.LogError("Saving " + MyWorld.MyChunkData.Count + " Chunks.");
			//foreach (var MyElement in MyWorld.MyChunkData)
			for (int i = 0; i < MyWorld.MyChunkData.Count; i++)
			{
				SaveToFile(MyWorld.MyChunkData.GetValueAt (i));
			}
			
			CharacterSystem.CharacterManager.SaveCharactersInChunks (MyWorld, SaveFileName);
			//SaveAsAsset ();
		}
		
		public static string GetWorldLocalPath(string SaveFileName) 
		{
			string FolderPath = "VoxelData/"+ SaveFileName + "/";
			return FolderPath;
		}
		public static string GetWorldFolderPath(string SaveFileName) {
			string FolderPath = FileUtil.GetFolderPath () + GetWorldLocalPath(SaveFileName);
			if (!Directory.Exists (FolderPath)) {
				Directory.CreateDirectory (FolderPath);
			}
			return FolderPath;
		}
		public static string GetChunkFilePath(Vector3 Position, string SaveFileName) 
		{
			string MyChunkSaveFile = GetWorldFolderPath(SaveFileName) + "Data" + Position.ToString () + ".dat";
			return MyChunkSaveFile;
		}
		
		public void SaveToFile(Chunk MyChunk) 
		{
			string MyChunkSaveFile = GetChunkFilePath (MyChunk.Position.GetVector(), SaveFileName);
			string MyBytes = "";
			for (int i = 0; i < Chunk.ChunkSize; i++) 
			{
				for (int j = 0; j < Chunk.ChunkSize; j++) 
				{
					for (int k = 0; k < Chunk.ChunkSize; k++) 
					{
						MyBytes += ((MyChunk.GetVoxel(i,j,k).GetBlockIndex())+"\n");
					}
				}
			}
			File.WriteAllText (MyChunkSaveFile, MyBytes);
			// save all the characters into chunk formats
			// also save the mesh
			
			/*#if UNITY_EDITOR
			Mesh MyMesh = MyChunk.gameObject.GetComponent<MeshFilter> ().sharedMesh;
			MyMesh.name = MyChunk.name; 
			UnityEditor.AssetDatabase.CreateAsset(MyMesh,  UnityEditor.AssetDatabase.GenerateUniqueAssetPath(GetMeshAssetPath()));
			UnityEditor.AssetDatabase.SaveAssets();
			#endif*/
			/*   
			#if UNITY_EDITOR
			string MyMeshPath = GetWorldFolderPath() + "DataMesh" + MyChunk.Position.GetVector().ToString () + ".asset";
			UnityEditor.AssetDatabase.CreateAsset(MyMesh, MyMeshPath);
			UnityEditor.AssetDatabase.SaveAssets();
			#endif*/
		}
		public string GetMeshAssetPath() {
			return "Assets/Resources/" + GetWorldLocalPath (SaveFileName) + SaveFileName + "_Mesh.asset";
		}
		public void SaveAsAsset() {
			SaveAsAsset (GetMeshAssetPath());
			//SaveAsAsset ("Assets/" + SaveFileName + "_Mesh.asset");
		}
		public void SaveAsAsset(string MySavePath) {
			#if UNITY_EDITOR
			Matrix4x4 WorldTransform = MyWorld.transform.worldToLocalMatrix;
			Mesh NewMesh = new Mesh();
			// now add all the chunk meshes to it using their transform positions and such
			List<CombineInstance> MeshList = new List<CombineInstance>();
			for (int i = 0; i < MyWorld.MyChunkData.Count; i++)
			{
				Chunk MyChunk = MyWorld.MyChunkData.GetValueAt (i);
				CombineInstance NewCombineInstance = new CombineInstance();
				NewCombineInstance.mesh =  MyChunk.GetComponent<MeshFilter>().sharedMesh;
				//NewCombineInstance.transform = MyChunk.transform.localToWorldMatrix;
				NewCombineInstance.transform = WorldTransform*MyChunk.transform.localToWorldMatrix;
				//NewCombineInstance.transform
				MeshList.Add(NewCombineInstance);
			}
			NewMesh.CombineMeshes (MeshList.ToArray (), false, true);// limit of 64 for submeshes
			// not create the data in the database
			//UnityEditor.AssetDatabase.CreateAsset(NewMesh,  UnityEditor.AssetDatabase.GenerateUniqueAssetPath(MySavePath));
			Debug.LogError("Saving in: " + (MySavePath));
			//Debug.LogError("Saving in: " + UnityEditor.AssetDatabase.GenerateUniqueAssetPath(MySavePath));
			UnityEditor.AssetDatabase.CreateAsset(NewMesh,  UnityEditor.AssetDatabase.GenerateUniqueAssetPath(MySavePath));
			UnityEditor.AssetDatabase.SaveAssets();
			#endif
		}
		public void LoadFromFile() {
			if (SaveFileName != "")
				LoadFromFile (SaveFileName);
		}
		public void LoadFromFile(string WorldName) 
		{
			SaveFileName = WorldName;
			if (SaveFileName == "")
				return;
			Clear ();
			Debug.LogError("Loading " + MyWorld.MyChunkData.Count + " Chunks.");
			MyWorld.MyUpdater.Reset ();
			/*for (int i = 0; i < MyWorld.MyChunkData.Count; i++) {
				LoadFromFile( MyWorld.MyChunkData.GetValueAt(i), WorldName);
			}*/
			for (int i = 0; i < transform.childCount; i++)
			{
				LoadFromFile(transform.GetChild(i).GetComponent<Chunk>(), WorldName);
			}
			/*foreach (var MyElement in MyWorld.MyChunkData)
			 * {
				LoadFromFile(MyElement.Value);
			}*/
			//MyWorld.UpdateAllChunks ();
		}

		List<string> ExtraDebug = new List<string>();
		public void LoadFromFile(Chunk MyChunk, string WorldName) 
		{
			//Debug.LogError("Loading Chunk: " +MyChunk.name);
			string MyChunkSaveFile = GetChunkFilePath (MyChunk.Position.GetVector(), WorldName);
			try
			{
				//MyChunk.HasUpdated = true;
				string[] fs = File.ReadAllLines(MyChunkSaveFile);
				//Debug.LogError ("Debug LoadString: " + fs.ToString());
				int MyIndex = 0;
				for (int i = 0; i < Chunk.ChunkSize; i++) 
				{
					for (int j = 0; j < Chunk.ChunkSize; j++) 
					{
						for (int k = 0; k < Chunk.ChunkSize; k++) 
						{
							if (MyIndex < fs.Length) 
							{
								MyChunk.UpdateBlockType(i,j,k, 
								                        int.Parse(fs[MyIndex]));
								//MyChunk.SetVoxelType(i,j,k, 
								//                     int.Parse(fs[MyIndex]));
								MyIndex++;
							}
						}
					}
				}
			}
			catch(FileNotFoundException e) 
			{

			}
			// modify this for players character, except Player.Attach(SpawnedCharacter) - or something

			// now load characters
			var info = new DirectoryInfo(GetWorldFolderPath(WorldName));
			var MyFiles = info.GetFiles();
			for (int i = 0; i < MyFiles.Length; i++) 
			{
				if (MyFiles[i] != null)
					if (MyFiles[i].FullName.Contains(".chr") && !MyFiles[i].FullName.Contains(".dat") && !MyFiles[i].FullName.Contains(".meta") && MyFiles[i].FullName.Contains (MyChunk.Position.GetVector().ToString())) 
				{
					//Debug.LogError(MyChunkSaveFile + " - File: " + MyFiles[i].ToString());
					// Load Character data spawns here

					// Spawn basic prefabbed character
					GameObject MyCharacter = CharacterSystem.CharacterPrefabLoader.SpawnCharacter (); // new GameObject();// 
					// Retrieve characters name from file name
					string CharacterName = MyFiles[i].Name.Replace(".chr", "");								
					CharacterName = CharacterName.Replace(MyChunk.Position.GetVector().ToString(), "");
					MyCharacter.name = CharacterName;
					// set basic stats to the minions	- later this should just be loaded from file unless first time spawning
					bool IsScript = CharacterSystem.CharacterSpawner.RunScript2(MyCharacter.transform, "Minion");
					ExtraDebug.Add (CharacterName + " has stats of size: " + MyCharacter.GetComponent<CharacterSystem.CharacterStats>().BaseStats.GetSize());
					CharacterSystem.CharacterSaver.LoadCharacter(MyCharacter.transform, MyFiles[i].FullName);
					// then add to chunk, and it will set them active when the chunk loads
					if (MyChunk.HasUpdated)	// if didnt update the chunk, it doesn't need to load~
						MyCharacter.SetActive(false);
					MyChunk.MyCharacterSpawns.Add (MyCharacter);
				}
			}
		}
		
		void OnGUI() {
			for (int i = 0; i < ExtraDebug.Count; i++) {
				GUILayout.Label(ExtraDebug[i]);
			}
		}
	}
}
