using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

// to do
// save load textures in structures
// save load spell data, item data, block structures

namespace OldCode {
//[ExecuteInEditMode]
public class DataManager : MonoBehaviour {
	public Shader MyTextShader;
	//public bool DoSaveItemData = false;
	//public bool DoAddDungeon;
	// Art Assets
	public List<Texture> Crosshairs;

	// Meta Data
	public List<Icon> IconsList;
	public List<Item> ItemsList;
	public List<BlockData> BlocksList;
	public List<Clan> ClanList;
	public List<MyMesh> MeshesList;

	//public string DataFileName = "MyData";
	//public bool SaveData;
	//public bool LoadData;

	//public int Index;
	// blocks
	public bool IsDeformedLandscape = false;	// so i can turn off deformity
	public List<Texture> BlockTextures;
	public List<BlockMesher> BlockMeshers;
	public List<Mesh> BlockModelsMeshes;
	public List<BlockModel> BlockModels;
	// spells
	public AudioClip DefaultSpawnSound;
	public List<Texture> SpellTextures;
	public List<AudioClip> SpellSpawnSounds;
	public List<GameObject> SpellPrefabs;
	public List<Effect> EffectsList;
	public List<Texture> EffectTextures;
	// items
	public AudioClip DefaultRemoveBlockSound;
	public AudioClip DefaultPlaceBlockSound;
	public List<Texture> ItemTextures;
	// quests
	public List<Quest> QuestsList = new List<Quest> ();
	
	// block structures - holding of voxel data
	public List<BlockStructure> BlockStructuresList;
	public List<Maze> MazeList;
	// this is essentially just baked voxels right now
	public List<Mesh> ModelsList;
	public List<Material> MaterialsList;
	public List<Texture> ModelTextures;
	public List<Material> MyGUIModelMaterials;

	// set this to the spawn position
	public GameObject ModelViewer;
	// spawn a game object, and attach the model
	public GameObject SpawnedModel;
	public int SelectedMeshIndex = 0;
	public LayerMask GuiLayerMask;
	public Text MyModelViewerLabel;

	public TextureManager BlocksTextureManager;

	// Use this for initialization
	void Start () {
		GenerateBlockData ();
		GenerateBlockModels ();
		// i should save/load all data here
		GenerateItemsForBlocks ();
		for (int i = 0; i < BlockStructuresList.Count; i++) {
			if (!BlockStructuresList[i].HasInitiated) {
				BlockStructuresList[i].Reset();
				BlockStructuresList[i].UpdateBlockStructureWithType();
			}
			// for my models! - create a meshes for them in thread
			BlockMesher NewTerrain = new BlockMesher();
			BlockMeshers.Add (NewTerrain);

			MyMesh NewMesh = new MyMesh();
			MeshesList.Add (NewMesh);
			NewTerrain.UpdateMeshesOnThread(NewMesh, BlockStructuresList[i].MyBlocks);
		}
		for (int i = 0; i < MazeList.Count; i++) {
			MazeList [i].Reset ();
			//MazeList [i].GenerateDungeon ();

		}
		// only use voxel models from now on lel
		ModelsList.Clear ();	
	}
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < BlockMeshers.Count; i++) {
			if (BlockMeshers[i].CanUpdateMesh) {
				BlockMeshers[i].CanUpdateMesh = false;
				//Debug.LogError ("BlockMesher: " + i + " : Has finished creating a mesh." + Time.time);
				Mesh NewMesh = BlockMeshers[i].CreateMesh( MeshesList[i]);
				NewMesh.name = "Mesh " + i;
				ModelsList.Add (NewMesh);
			}
		}
	}

	public BlockStructure GetBlockStructure(string BlockStructureName) {
		for (int i = 0; i < BlockStructuresList.Count; i++) 
		{
			if (BlockStructuresList[i].Name == BlockStructureName)
				return BlockStructuresList[i];
		}
		return null;
	}

	public Mesh GetModel(string ModelName) {
		for (int i = 0; i < ModelsList.Count; i++) 
		{
			if (ModelsList[i].name == ModelName)
				return ModelsList[i];
		}
		return null;
	}
	public bool IsModel(string ModelName) {
		for (int i = 0; i < ModelsList.Count; i++) {
			if (ModelsList[i].name == ModelName)
				return true;
		}
		return false;
	}

	public void DecreaseModel() {
		SelectedMeshIndex--;
		if (SelectedMeshIndex < 0)
			SelectedMeshIndex = ModelsList.Count - 1;
		UnSpawnMesh ();
		SpawnMesh (SelectedMeshIndex);
	}
	public void IncreaseModel() {
		SelectedMeshIndex++;
		if (SelectedMeshIndex >= ModelsList.Count)
			SelectedMeshIndex = 0;
		UnSpawnMesh ();
		SpawnMesh (SelectedMeshIndex);
	}

	public void SaveItemData() {
		string FilePath = "";
		for (int i = 0; i < ItemsList.Count; i++) {
			ItemsList[i].Save(FilePath);
		}
	}

	public void SpawnMesh(int MeshIndex) {
		if (MeshIndex >= 0 && MeshIndex < ModelsList.Count) {
			SpawnedModel = new GameObject();
			SpawnedModel.name = "Model Viewer: " + MeshIndex.ToString ();
			Mesh NewMesh = ModelsList[MeshIndex];
			SpawnedModel.AddComponent<MeshFilter>();
			SpawnedModel.AddComponent<MeshRenderer>();
			SpawnedModel.GetComponent<MeshFilter>().sharedMesh = NewMesh;
			SpawnedModel.transform.SetParent (ModelViewer.transform);
			SpawnedModel.transform.position = ModelViewer.transform.position;
			SpawnedModel.layer = 11;//GuiLayerMask.value;
			Debug.Log ("Changing layer to: " + GuiLayerMask.ToString());
			if (MeshIndex >=0 && MeshIndex < MaterialsList.Count) {
				SpawnedModel.GetComponent<MeshRenderer>().material = (MaterialsList[MeshIndex]);
			}
		}
	}

	public void UnSpawnMesh() {
		Destroy (SpawnedModel);
	}

	public void AlterModel(GameObject MyModel, int ModelIndex, int TextureIndex, bool IsItem, ItemType MyItemType) {
		if (ModelIndex >= 0 && ModelIndex < ModelsList.Count) {
			if (MyModel.GetComponent<MeshFilter>())
				MyModel.GetComponent<MeshFilter>().mesh = ModelsList[ModelIndex];
			if (MyModel.GetComponent<MeshCollider>() != null) {
				MyModel.GetComponent<MeshCollider>().sharedMesh = null;
				MyModel.GetComponent<MeshCollider>().sharedMesh = ModelsList[ModelIndex];
			}
		}
		if (IsItem) {
			if (MyItemType == ItemType.Block) {
				TextureManager MyTextureManager = GetManager.GetTextureManager();
				if (TextureIndex >= 0 && TextureIndex < MyTextureManager.BlockTextures.Count) {
					//MyModel.GetComponent<MeshRenderer> ().material.mainTexture = MyTextureManager.BlockTextures [TextureIndex];
				}
			} else if (MyItemType == ItemType.BlockStructure) {
				//GetManager.GetMyDataManager().BlockStructuresList[CurrentBlockStructure].Reset();
				//GetManager.GetMyDataManager().BlockStructuresList[CurrentBlockStructure].UpdateBlockStructureWithType();	// this just generates the data
				//MyModel.GetComponent<TerrainGenerator>().MyBlocks = GetManager.GetDataManager().BlockStructuresList[ModelIndex].MyBlocks;
				//MyModel.GetComponent<TerrainGenerator>().HasUpdated = true;
			}
		} else {
			if (TextureIndex >= 0 && TextureIndex < ModelTextures.Count) {
				MyModel.GetComponent<MeshRenderer> ().material.mainTexture = ModelTextures [TextureIndex];
			}
		}
	}
	public BlockData GetBlockData(int Indexu) {
		if (Indexu >= 0 && Indexu < BlocksList.Count)
			return BlocksList [Indexu];
		else
			return new BlockData(Indexu);
	}
	// used when loading items/spells
	
	public Texture GetSpellTexture(int SpellTextureIndex) {
		if (SpellTextureIndex >= 0 && SpellTextureIndex < SpellTextures.Count) {
			return SpellTextures [SpellTextureIndex];
		}
		return null;
	}
	public AudioClip GetSpellSpawnSound(int SpellSpawnSoundIndex) {
		if (SpellSpawnSoundIndex >= 0 && SpellSpawnSoundIndex < SpellSpawnSounds.Count) {
			return SpellSpawnSounds [SpellSpawnSoundIndex];
		}
		return DefaultSpawnSound;
	}
	public Texture GetItemTexture(int ItemTextureId, bool IsBlock) {
		if (!IsBlock) {
			if (ItemTextureId >= 0 && ItemTextureId < ItemTextures.Count)
				return ItemTextures [ItemTextureId];
		} else {
			if (ItemTextureId >= 0 && ItemTextureId < GetManager.GetTextureManager().BlockTextures.Count)
				return GetManager.GetTextureManager().BlockTextures [ItemTextureId];
		}
		return null;
	}

	public GameObject GetSpellPrefab(int SpellIndex) {
		if (SpellIndex >= 0 && SpellIndex < SpellPrefabs.Count)
			return SpellPrefabs[SpellIndex];
		return null;
	}

	public BlockModel GetBlockModel(int BlockIndex) {
		BlockData MyBlockData = GetBlockData(BlockIndex);
		return BlockModels[MyBlockData.ModelId];
	}

	public void GenerateBlockModels() {
		BlockModels = new List<BlockModel> ();
		BlockModel CubeBlockModel = BlockModelGenerator.GetCubeModel();
		BlockModels.Add (CubeBlockModel);
		BlockModel SlopeBlockModel = BlockModelGenerator.GetSlopeModel(false);
		BlockModels.Add (SlopeBlockModel);
		BlockModel SlopeBlockModel2 = BlockModelGenerator.GetSlopeModel(true);
		BlockModels.Add (SlopeBlockModel2);
		for (int i = 0; i < BlockModelsMeshes.Count; i++) {
			BlockModels.Add (new BlockModel(BlockModelsMeshes[i]));
		}
	}
	
	public void GenerateItemsForBlocks() {
		//ItemsList.RemoveRange (6,ItemsList.Count);
		//potato
		//MyMesh.CreateCubeMesh (ModelsList [1]);
		//MyModelViewerLabel = GetManager.GetGuiCreator().ModelViewerLabel.GetComponent<Text>();
		for (int i = 1; i < BlocksTextureManager.BlockTextures.Count; i++) {
			BlockData MyBlockData = GetBlockData(i);
			Item NewItem = new Item();
			NewItem.Name = MyBlockData.Name;
			NewItem.BlockIndex = i;
			NewItem.SetTextureIndex(1);
			NewItem.MyItemType = ItemType.Block;
			NewItem.Quantity = 1;
			NewItem.MaxQuantity = 99;
			ItemsList.Add (NewItem);
		}

		for (int i = 0; i < BlockStructuresList.Count; i++) {
			//BlockData MyBlockData = GetBlockData(i);
			Item NewItem = new Item();
			NewItem.Name = BlockStructuresList[i].Name;
			NewItem.BlockIndex = i;
			NewItem.MyTexture = ItemTextures[1];			// building texture
			NewItem.SetTextureIndex(1);			// building texture
			NewItem.MyItemType = ItemType.BlockStructure;
			NewItem.Quantity = 50;
			NewItem.MaxQuantity = 99;
			ItemsList.Add (NewItem);
		}
		for (int i = 0; i < MazeList.Count; i++) {
			Item NewItem = new Item();
			NewItem.Name = MazeList[i].Name;
			NewItem.SetTextureIndex(1);			// building texture
			NewItem.BlockIndex = i;
			NewItem.MyTexture = ItemTextures[1];			// building texture
			NewItem.MyItemType = ItemType.Dungeon;
			NewItem.Quantity = 1;
			NewItem.MaxQuantity = 1;
			ItemsList.Add (NewItem);
		}
	}

	public void GenerateBlockData() {
		if (BlocksList.Count != GetManager.GetTextureManager ().BlockTextures.Count) {
			BlocksList.Clear();
			for (int i = 0; i < GetManager.GetTextureManager().BlockTextures.Count; i++) {
				BlockData NewBlockData = new BlockData ();
				NewBlockData.Name = "Block " + i;
				NewBlockData.DropIndex = i;
				if (i == 1)
					NewBlockData.Name = "Stone";
				else if (i == 2)
					NewBlockData.Name = "Netherbrick";
				else if (i == 3)
					NewBlockData.Name = "Dirt";
				else if (i == 4)
					NewBlockData.Name = "Water";
				else if (i == 5)
					NewBlockData.Name = "Sand";
				else if (i == 6)
					NewBlockData.Name = "Netherbrick2";
				else if (i == 7)
					NewBlockData.Name = "BuildingCore";
				else if (i == 8)
					NewBlockData.Name = "Bedrock";
				else if (i == 9)
					NewBlockData.Name = "TownHallCore";
				else if (i == 10)
					NewBlockData.Name = "IronOre";
				else if (i == 11)
					NewBlockData.Name = "Torch";

				if (i == 4 || i == 6 || i == 3 || i == 5) {		// sand and dirt
					NewBlockData.Health = 40;
					NewBlockData.IsDeformed = true;
				}
				if (i == 1) {	// cobblestone
					NewBlockData.Health = 250;
					NewBlockData.Name = "Cobblestone";
				}
				else if (i == 2 || i == 11 || i == 10) {	// stone or road or bronze
					NewBlockData.Health = 150;
					NewBlockData.IsDeformed = true;
				}
				if (i == 8) {	// bedrock
					NewBlockData.CanBeRemoved = false;
				}
				if (i == 7) {	// building core - destroy zone
					NewBlockData.OnDestroyFunctionId = 1;
					NewBlockData.Health = 800;
				}
				if (i == 9) {	// town hall core
					NewBlockData.OnDestroyFunctionId = 2;
					NewBlockData.Health = 10000;
				}
				if (i == 11) {
					NewBlockData.ModelId = 3;
				}
				if (!IsDeformedLandscape) {
					NewBlockData.IsDeformed = false;
				}
				NewBlockData.RemovedSoundEffect = DefaultRemoveBlockSound;
				NewBlockData.PlaceSoundEffect = DefaultPlaceBlockSound;
				BlocksList.Add (NewBlockData);
			}
		}
	}
}
}
