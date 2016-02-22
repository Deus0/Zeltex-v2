using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace OldCode {
// once selected a mode, this will add the right 3d ui onto it
public enum ModelEditMode {
	Move,
	Rotate,
	Scale,
	Voxels,
	Verticies,
	Edges,
	Faces,
	Bones
};

// handles the models created
public class ModelEditorManager : MonoBehaviour {
	public ModelTextureHandler MyModelTextureHandler;

	// Data management
	public List<GameObject> MyLoadedModels = new List<GameObject>();
	int CurrentBlockStructure = 0;
	public GameObject MyModel;			// Selected Model	- Can Select Others!
	
	public List<LineRenderer> MyBoneRenderers = new List<LineRenderer>();
	
	public bool IsModelHighlighted = false;
	private Vector3 ModelOriginalPosition;
	private Vector3 ModelOriginalUpDirection = new Vector3(0,1,0);	// when intiially pressed
	private Vector3 ModelOriginalLeftDirection = new Vector3(1,0,0);
	private Quaternion ModelOriginalRotation;	// when intiially pressed
	public float RotateSpeed = 0.05f;
	private bool IsRepositioning = false;
	private float ZoomSpeed = 1.1f;
	
	public Material StandardMaterial;
	public Material SelectedMaterial;
	public Vector2 TestMousePosition = new Vector2(16,16);
	// To add bone
	public Vector3 FirstAddBonePosition;
	public Vector3 SecondAddBonePosition;
	public ModelEditMode CurrentMode;
	LineRenderer MyLineRenderer;
	// For drag functions
	public int BlockPlaceType = 1;
	// Bone stuff
	public bool IsAddingBone = false;
	
	public InputField MyModelLabelText;
	public Text MyModeLabelText;
	public Text MyModelNameText;
	public RawImage MyModelImage;

	bool IsMoveTool = false;
	public bool IsClearBlockStructures = true;
	public bool IsLoadModels = true;

	// Use this for initialization
	void Start () 
	{
		SelectModel(GetManager.GetDataManager ().ModelViewer);
		StandardMaterial = GetManager.GetDataManager().MyGUIModelMaterials[0];
		SelectedMaterial = GetManager.GetDataManager().MyGUIModelMaterials[1];
		// add line render for creating bone
		InitiateBoneEditing ();
		SetLabelsActive(false);
	}

	void Awake() 
	{
		LoadAllFiles ();
		LoadNextBlockStructure();
	}

	// Update is called once per frame
	void Update () 
	{
		if (MyModelTextureHandler == null) {
			GameObject MyObject = GameObject.Find ("Model Texture(Clone)");
			if (MyObject) {
				MyModelTextureHandler = MyObject.GetComponent<ModelTextureHandler>();
				
				MyModelTextureHandler.MyCamera = GameObject.Find ("ModelEditorCamera");
			}
		} else {
			if (!MyModelLabelText.isFocused && MyModelTextureHandler.IsRunning)
				HandleInput();
			RenderBones ();
			UpdateGrid ();
			HandleRayTrace ();
			if (!MyModelTextureHandler.IsRunning)
				MyModelTextureHandler.CheckExtraMouseHits();
			SetLabelsActive(MyModelTextureHandler.IsMouseHitModelEditorGui || MyModelTextureHandler.IsRunning);
		}
	}
	public void SetLabelsActive(bool IsActive) 
	{
		MyModelLabelText.gameObject.SetActive (IsActive);
		MyModeLabelText.gameObject.SetActive (IsActive);
		MyModelImage.gameObject.SetActive (IsActive);
	}

	public void HandleRayTrace() 
	{
		if (MyModelTextureHandler.IsRayHitObject) {
			if (MyModelTextureHandler.MyRayTraceHit.collider.gameObject.tag == "ModelInScene") {
				HighLightModel ();
				if (MyModelTextureHandler.MyRayTraceHit.collider.gameObject != MyModel) {	// if not selected
					if (Input.GetMouseButton(0)) {
						UnSelectModel ();
						SelectModel (MyModelTextureHandler.MyRayTraceHit.collider.gameObject);
					}
				}
			} else {
				UnHighlightModel ();
			}
		} else {
			UnHighlightModel();
		}
	}

	// In-Game Data

	public void ClearScene() 
	{
		for (int i = 0; i < MyLoadedModels.Count; i++) {
			Destroy (MyLoadedModels[i]);
		}
		MyLoadedModels.Clear ();
	}

	// Input

	public void HandleFirstClick() 
	{
		IsRepositioning = !Input.GetKey (KeyCode.LeftAlt);
		if (MyModel != null) {
			ModelOriginalPosition = MyModel.transform.position;
			ModelOriginalRotation = MyModel.transform.rotation;
			// if local
			ModelOriginalUpDirection = MyModel.transform.up;
			ModelOriginalLeftDirection = -MyModel.transform.right;
		}
	}

	public void HandleMouseDrag() 
	{
		//Debug.LogError ("OnDrag MapTexture: " + Time.time);
		Vector3 MouseDeltaPosition = MyModelTextureHandler.MouseDeltaPosition;
		if (MyModel != null) {
			if (CurrentMode == ModelEditMode.Rotate || !IsRepositioning) {
				MyModel.transform.Rotate(new Vector3(MouseDeltaPosition.y * RotateSpeed,-MouseDeltaPosition.x * RotateSpeed, 0), Space.World);
				if (Input.GetKey(KeyCode.LeftShift))
				{
					MyModel.transform.eulerAngles = new Vector3(0f,MyModel.transform.eulerAngles.y,0f);
				}
				MyModelTextureHandler.MouseOriginalPosition = MyModelTextureHandler.MouseCurrentPosition;
			} else if (CurrentMode == ModelEditMode.Move ) { // move the model
				Vector2 StretchedModelTextureSize = MyModelTextureHandler.GetStretchedModelTextureSize ();
				Ray MyRay = MyModelTextureHandler.GetRayInModelTexture ();
				Vector3 Current3DPosition = MyRay.origin + MyRay.direction * 5.0f;
				Vector3 Difference3DPosition = Current3DPosition-MyModelTextureHandler.Original3DPosition;
				if (MyModelTextureHandler.IsMoveToolHit) 
				{
					MyModel.transform.position = Current3DPosition;
					//MyModel.transform.position = ModelOriginalPosition+Difference3DPosition;
				}
				else if (MyModelTextureHandler.IsMoveToolHitX) {
					//MyModel.transform.position = ModelOriginalPosition+new Vector3(Difference3DPosition.x, 0, 0);
					MyModel.transform.position = ModelOriginalPosition+Difference3DPosition;
					//MyModel.transform.position = new Vector3(MyModel.transform.position.x, ModelOriginalPosition.y, ModelOriginalPosition.z);
				}
				else if (MyModelTextureHandler.IsMoveToolHitY) {
					//MyModel.transform.position = ModelOriginalPosition+new Vector3(0, Difference3DPosition.y,0);
				}
				else if (MyModelTextureHandler.IsMoveToolHitZ) {
					//MyModel.transform.position = ModelOriginalPosition+new Vector3(0, 0, Difference3DPosition.z);
				}
				MyModel.transform.position = Current3DPosition;
			}
		}
	}

	public void ChangeMode(ModelEditMode NewMode) 
	{
		CurrentMode = NewMode;
		MyModeLabelText.text = "Mode: " + CurrentMode.ToString();
	}
	public void HandleInputModes() {
		
		// Different Model Editing Modes
		if (CurrentMode == ModelEditMode.Voxels) {
			//if (IsModelHighlighted) 
			{
				if (Input.GetMouseButtonDown (0)) {
					MyModelTextureHandler.RayTraceToModel();
					if (MyModelTextureHandler.IsRayHitObject && MyModelTextureHandler.MyRayTraceHit.collider.gameObject == MyModel) {
						GetSelectedBlockStructure().MyBlocks.UpdateBlockWithRay (MyModelTextureHandler.MyRayTraceHit, BlockPlaceType);
					}
				}
				else if (Input.GetMouseButtonDown (1)) {
					MyModelTextureHandler.RayTraceToModel();
					if (MyModelTextureHandler.IsRayHitObject&& MyModelTextureHandler.MyRayTraceHit.collider.gameObject == MyModel) {
						GetSelectedBlockStructure().MyBlocks.UpdateBlockWithRay2 (MyModelTextureHandler.MyRayTraceHit, 0);
					}
				}
			}
			
			if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
				SwitchBlockModel(-1);
			} else if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
				SwitchBlockModel(1);
			}
		} else if (CurrentMode == ModelEditMode.Bones) {
			HandleInputBoneMode ();
		} else if (CurrentMode == ModelEditMode.Move) {
			if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
				ScaleModel(false);
			} else if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
				ScaleModel(true);
			}
		}
	}

	public void HandleInput() 
	{
		// Load / Save
		if (Input.GetKeyDown (KeyCode.U)) {
			LoadModel ();
		}
		if (Input.GetKeyDown (KeyCode.Y)) {
			SaveModel();
		}
		if (Input.GetKeyDown (KeyCode.T)) {
			CurrentBlockStructure = GetManager.GetDataManager().BlockStructuresList.Count;
			SaveModelToFile("Model" + CurrentBlockStructure.ToString());
			GetManager.GetDataManager().BlockStructuresList.Add(MyModel.GetComponent<MyMeshHolder>().MyBlockStructure);
		}
		// Change Modes
		if (Input.GetKeyDown (KeyCode.W)) 
		{
			ChangeMode(ModelEditMode.Move);
		} 
		else if (Input.GetKeyDown (KeyCode.R)) 
		{
			ChangeMode(ModelEditMode.Rotate);
		} 
		else if (Input.GetKeyDown (KeyCode.E)) 
		{
			ChangeMode(ModelEditMode.Voxels);
		} 
		else if (Input.GetKeyDown (KeyCode.B)) 
		{
			ChangeMode(ModelEditMode.Bones);
		} 

		else if (Input.GetKeyDown (KeyCode.K)) 
		{
			MyModel.GetComponent<MyMeshHolder>().MyBlockStructure.MyBlocks.IncreaseSubDivision();
			//MyModel.GetComponent<TerrainGenerator> ().IncreaseSubDivision();
		}  
		else if (Input.GetKeyDown (KeyCode.F)) 
		{
			MyModelTextureHandler.ToggleFullScreen();
		}

		else if (Input.GetKeyDown (KeyCode.C)) 
		{
			LoadNextBlockStructure();
		}

		else if (Input.GetKeyDown (KeyCode.V)) 
		{
			//MyModel.GetComponent<CustomBones>().MySkinnedMesh.GetSkinnedDataFromMesh();
			//SetTextureToWebcam();
		}
		if (Input.GetKeyDown (KeyCode.G)) 
		{
			ToggleRotation();
		}
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			//TogglePhysics();
			ShowGrid();
		}
		HandleInputModes ();
	}

	public void SwitchBlockModel(int Increment) 
	{
		BlockPlaceType += Increment;
		if (BlockPlaceType > 16)
			BlockPlaceType = 1;
		if (BlockPlaceType < 1)
			BlockPlaceType = 16;
		if (BlockPlaceType < GetManager.GetTextureManager ().BlockTextures.Count) {
			MyModelImage.texture = GetManager.GetTextureManager ().BlockTextures [BlockPlaceType];
		} else {
			MyModelImage.texture = null;
		}
	}

	public void ScaleModel(bool IsMultiple) 
	{
		Vector3 CurrentScale = MyModel.transform.localScale;
		if (!IsMultiple)
			CurrentScale = CurrentScale / ZoomSpeed;
		else
			CurrentScale = CurrentScale * ZoomSpeed;
		MyModel.transform.localScale = CurrentScale;
	}

	public void ToggleRotation() 
	{
		MyModel.GetComponent<Rotator>().enabled = !MyModel.GetComponent<Rotator>().enabled;
	}

	public void UnHighlightModel() 
	{
		if (!MyModelTextureHandler.IsMousePressed) {
			IsModelHighlighted = false;
			//Debug.LogError ("Missed Model");
			/*Texture MyModelTexture = MyModel.GetComponent<MeshRenderer> ().material.mainTexture;
			StandardMaterial.mainTexture = MyModelTexture;
			MyModel.GetComponent<MeshRenderer> ().material = StandardMaterial;*/
			Texture MyModelTexture = null;
			if (MyModel.GetComponent<MeshRenderer> ()) {
				MyModelTexture = MyModel.GetComponent<MeshRenderer> ().material.mainTexture;
				StandardMaterial.mainTexture = MyModelTexture;
				MyModel.GetComponent<MeshRenderer> ().material = StandardMaterial;
			}
			else if (MyModel.GetComponent<SkinnedMeshRenderer> ()) 
			{
				//MyModelTexture = MyModel.GetComponent<SkinnedMeshRenderer> ().material.mainTexture;
				//StandardMaterial.mainTexture = MyModelTexture;
				//MyModel.GetComponent<SkinnedMeshRenderer> ().material = StandardMaterial;
			}
		}
	}
	public void HighLightModel() 
	{
		IsModelHighlighted = true;
		Texture MyModelTexture = null;
		if (MyModel.GetComponent<MeshRenderer> ()) {
			MyModelTexture = MyModel.GetComponent<MeshRenderer> ().material.mainTexture;
			SelectedMaterial.mainTexture = MyModelTexture;
			MyModel.GetComponent<MeshRenderer> ().material = SelectedMaterial;
		}
		else if (MyModel.GetComponent<SkinnedMeshRenderer> ()) 
		{
			//MyModelTexture = MyModel.GetComponent<SkinnedMeshRenderer> ().material.mainTexture;
			//SelectedMaterial.mainTexture = MyModelTexture;
			//MyModel.GetComponent<SkinnedMeshRenderer> ().material = SelectedMaterial;
		}
	}

	public void SelectModel(GameObject ModelToSelect) 
	{
		MyModel = ModelToSelect;
		MyModel.transform.FindChild ("MoveTool").gameObject.SetActive (IsMoveTool);
	}
	public void UnSelectModel() 
	{
		if (MyModel) 
		{
			Transform MyMoveTool = MyModel.transform.FindChild("MoveTool");
			if (MyMoveTool) 
				MyMoveTool.gameObject.SetActive(false);
		}
	}

	// File IO
	public void LoadNextBlockStructure() 
	{
		if (!MyModel.GetComponent<MyMeshHolder> ().MyBlockMesher.IsUpdatingMesh) {
			if (MyModel == null) {
				// create new model
			}
			if (MyModel) {
				bool IsGridShown = (MyModel.transform.FindChild ("Grid") != null);
				if (IsGridShown)
					DestroyGrid ();
				CurrentBlockStructure++;
				if (CurrentBlockStructure >= GetManager.GetDataManager ().BlockStructuresList.Count)
					CurrentBlockStructure = 0;
				MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure = GetManager.GetDataManager ().BlockStructuresList [CurrentBlockStructure];
				MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure.MyBlocks.HasUpdated = true;
				if (IsGridShown)
					ShowGrid ();
				UpdateNameLabel ();
			} else {
				Debug.LogError ("Trying to load model, but no model selected");
				Debug.Break ();
			}
		}
	}

	public void UpdateNameWithGui() {
		if (MyModelNameText.text != "")
		UpdateName (MyModelNameText.text);
	}
	public void UpdateName(string NewName) 
	{
		MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure.Name = NewName;
		UpdateNameLabel();
	}
	public void UpdateNameLabel() 
	{
		MyModelLabelText.text = MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure.Name;
	}
	public void LoadAllFiles() {
		if (IsLoadModels) {
			if (IsClearBlockStructures) {
				GetManager.GetDataManager ().BlockStructuresList.Clear ();
			}

			String FileName = FileLocator.SaveLocation (GetManager.GetGameManager ().GameName, "", "Models/", "");
			Debug.Log ("Reading from: " + FileName);
			var info = new DirectoryInfo (FileName);
			FileInfo[] fileInfo = info.GetFiles ("*.vmd");
			for (int i = 0; i < fileInfo.Length; i++) {
				FileName = fileInfo [i].Name; 
				FileName = FileName.Replace (".vmd", "");
				//Debug.LogError("Loading: " + FileName);
				GetManager.GetDataManager ().BlockStructuresList.Add (LoadModelFromFile2 (FileName));
				GetManager.GetDataManager ().BlockStructuresList [GetManager.GetDataManager ().BlockStructuresList.Count - 1].Name = FileName;
				GetManager.GetDataManager ().BlockStructuresList [GetManager.GetDataManager ().BlockStructuresList.Count - 1].HasInitiated = true;
			}
		}
	}
	public void DeleteModel() {
		DeleteModel (MyModel.GetComponent<MyMeshHolder>().MyBlockStructure.Name);
	}
	
	public void DeleteModel(string FileName) 
	{
		
	}

	public void CopyModel() {
		LoadModelFromFile2 (MyModel.GetComponent<MyMeshHolder>().MyBlockStructure.Name);
	}
	public void LoadModel() {
		LoadModelFromFile2 (MyModel.GetComponent<MyMeshHolder>().MyBlockStructure.Name);
	}
	public void SaveModel() {
		//SaveModelToFile ("Model" + CurrentBlockStructure.ToString ());
		SaveModelToFile (MyModel.GetComponent<MyMeshHolder>().MyBlockStructure.Name);
	}
	public void SaveModelToFile(string FileName) {
		IFormatter BinaryFile = new BinaryFormatter ();
		FileName = FileLocator.SaveLocation (GetManager.GetGameManager().GameName, FileName, "Models/", ".vmd");
		Debug.Log("Saving Model: " + FileName);
		Stream MyFile = new FileStream (FileName, FileMode.Create, FileAccess.Write, FileShare.None);
		Blocks MyBlocks = MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure.MyBlocks;
		BlocksSerial MyBlocksSerial = new BlocksSerial( MyBlocks.GetData (), MyBlocks.Size);

		BinaryFile.Serialize (MyFile,MyBlocksSerial);
		MyFile.Close ();
	}
	
	public void LoadModel(string FileName) {
		MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure = LoadModelFromFile2 (FileName);
	}

	public void LoadModelFromFile(string FileName) {
		MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure = LoadModelFromFile2 (FileName);
	}

	public BlockStructure LoadModelFromFile2(string FileName) {
		BlockStructure MyBlockStructure = new BlockStructure();
		FileName = FileLocator.SaveLocation (GetManager.GetGameManager ().GameName, FileName, "Models/", ".vmd");
		Debug.Log (".... Loading Model from: " + FileName + " At time: " + Time.time);
		if (File.Exists (FileName)) {
			IFormatter BinaryFile = new BinaryFormatter ();
			FileStream MyFile = new FileStream (FileName, FileMode.Open);
			
			BlocksSerial MyBlocksSerial = (BlocksSerial)BinaryFile.Deserialize (MyFile);
			
			MyBlockStructure.MyBlocks.SetData(MyBlocksSerial.GetData());
			MyBlockStructure.MyBlocks.Size = MyBlocksSerial.GetSize();
			MyBlockStructure.MyBlocks.ReCreateBlocks();
			MyBlockStructure.MyBlocks.HasUpdated = true;
		}
		return MyBlockStructure;
	}

	public void CopyModel(string OldFileName, string NewFileName) {
		
	}

	// Utility Functions
	
	
	public void SetTextureToWebcam() {
		WebCamTexture webcamTexture = new WebCamTexture();
		Renderer renderer = MyModel.GetComponent<Renderer>();
		renderer.material.mainTexture = webcamTexture;
		webcamTexture.Play();
	}
	public void TogglePhysics() {
		MyModel.GetComponent<Rigidbody>().isKinematic = !MyModel.GetComponent<Rigidbody>().isKinematic;
		MeshCollider MyCollider = MyModel.GetComponent<MeshCollider>();
		if (MyCollider != null) {
			MyCollider.convex = !MyModel.GetComponent<Rigidbody>().isKinematic;
		}
	}
	public Blocks GetSelectedBlocks() {
		return MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure.MyBlocks;
		//return MyModel.GetComponent<TerrainGenerator> ().MyBlocks;
	}
	public BlockStructure GetSelectedBlockStructure() {
		return MyModel.GetComponent<MyMeshHolder> ().MyBlockStructure;
		/*if (CurrentBlockStructure >= GetManager.GetDataManager().BlockStructuresList.Count)
			CurrentBlockStructure = 0;
		return GetManager.GetDataManager().BlockStructuresList[CurrentBlockStructure];
		return null;*/
	}
	// Render Functions
	void RenderBones() {
		CustomBones MyBones = MyModel.GetComponent<CustomBones>();
		if (MyBones)
		for (int i = 0; i < MyBones.MySkinnedMesh.Bones.Count; i += 2) {
			if (i < MyBones.MySkinnedMesh.Bones.Count)
				MyBoneRenderers[i/2].SetPosition(0,MyBones.MySkinnedMesh.Bones[i].position);
			if (i+1 < MyBones.MySkinnedMesh.Bones.Count)
				MyBoneRenderers[i/2].SetPosition(1,MyBones.MySkinnedMesh.Bones[i+1].position);
		}
		MyLineRenderer.enabled = IsAddingBone;
		if (!IsAddingBone) 
		{
			/*
			// Now render bones
			DebugDraw.DrawSphere (FirstAddBonePosition, 0.1f, Color.blue);
			//Quaternion MyBoneRotation = Quaternion.FromToRotation (FirstAddBonePosition, SecondAddBonePosition);
			MyLineRenderer.SetPosition (0, FirstAddBonePosition);
			MyLineRenderer.SetPosition (1, SecondAddBonePosition);
			//DebugDraw.DrawCubeStretched (FirstAddBonePosition, MyBoneRotation, (FirstAddBonePosition - SecondAddBonePosition), Color.red);
			DebugDraw.DrawSphere (SecondAddBonePosition, 0.1f, Color.blue);*/
		} else {
			Vector3 TempSecondPosition = MyModelTextureHandler.GetPositionInViewPortWithDepth(5.0f);
			// Now render bones
			DebugDraw.DrawSphere (FirstAddBonePosition, 0.1f, Color.blue);
			//Quaternion MyBoneRotation = Quaternion.FromToRotation (FirstAddBonePosition, SecondAddBonePosition);
			MyLineRenderer.SetPosition (0, FirstAddBonePosition);
			MyLineRenderer.SetPosition (1, TempSecondPosition);
			//DebugDraw.DrawCubeStretched (FirstAddBonePosition, MyBoneRotation, (FirstAddBonePosition - SecondAddBonePosition), Color.red);
			DebugDraw.DrawSphere (TempSecondPosition, 0.1f, Color.blue);
		}
	}
	// render the grid
//	public GameObject GridObject;
	public void ShowGrid() {
		GameObject GridObject = null;
		if (MyModel.transform.FindChild ("Grid")) 
			GridObject = MyModel.transform.FindChild ("Grid").gameObject;
		if (GridObject == null) {
			CreateGrid();
		} else {
			GridObject.SetActive(!GridObject.activeSelf);
		}
	}
	void CreateGrid() {
		GameObject GridObject = new GameObject ();
		GridObject.name = "Grid";
		GridObject.transform.position = MyModel.transform.position;
		GridObject.transform.rotation = MyModel.transform.rotation;
		GridObject.transform.localScale = MyModel.transform.localScale;
		GridObject.transform.SetParent (MyModel.transform);
		Vector3 BlockStructureSize = GetSelectedBlocks().Size;
		float GridX = BlockStructureSize.x;
		float GridZ = BlockStructureSize.z;
		Vector3 Offset =  GridObject.transform.TransformPoint(MyModel.GetComponent<MyMeshHolder>().MeshData.DifferenceBounds);
		
		for (int i = 0; i <= GridX; i++)
		{
			GameObject GridLine = CreateGridLine(GridObject, i, 0);
			GridLine.transform.position =  GridObject.transform.position - Offset + GridObject.transform.TransformPoint(new Vector3 (i-GridX, -BlockStructureSize.y, -GridZ/2f));
		}
		
		for (int i = 0; i <= GridZ; i++) {
			GameObject GridLine2 = CreateGridLine(GridObject, 0, i);
			GridLine2.transform.position = GridObject.transform.position - Offset + GridObject.transform.TransformPoint(new Vector3 (-GridX/2, -BlockStructureSize.y, i-GridZ));
		}
	}
	public void DestroyGrid() {
		GameObject GridObject = null;
		if (MyModel.transform.FindChild ("Grid")) 
			GridObject = MyModel.transform.FindChild ("Grid").gameObject;
		if (GridObject)
		Destroy (GridObject);
	}
	public void RefreshGrid() {
		DestroyGrid();
		CreateGrid();
	}
	public void CheckGridForRefresh() {
		GameObject GridObject = null;
		if (MyModel.transform.FindChild ("Grid")) 
			GridObject = MyModel.transform.FindChild ("Grid").gameObject;
		if (GridObject) {
			Vector3 BlockStructureSize = GetSelectedBlocks().Size;
			if (GridObject.transform.childCount != Mathf.RoundToInt(BlockStructureSize.x+2+BlockStructureSize.z))
				RefreshGrid();
			else {

			}
		}
	}
	public void UpdateGrid() {
		CheckGridForRefresh ();
		GameObject GridObject = null;
		if (MyModel.transform.FindChild ("Grid")) 
			GridObject = MyModel.transform.FindChild ("Grid").gameObject;
		if (GridObject && GridObject.activeSelf) {
			Vector3 BlockStructureSize = GetSelectedBlocks().Size;
			float GridX = BlockStructureSize.x;
			float GridZ = BlockStructureSize.z;
			Vector3 Offset =  GridObject.transform.TransformPoint(MyModel.GetComponent<MyMeshHolder>().MeshData.DifferenceBounds);
			for (int i = 0; i <= GridX; i++)
			{
				int ChildIndex = i;
				/*if (ChildIndex !=  GridObject.transform.childCount)
				{
					RefreshGrid();
					return;
				}*/
				GameObject GridLine = GridObject.transform.GetChild(ChildIndex).gameObject;
				Vector3 Position1 = (new Vector3 (i-GridX, -BlockStructureSize.y, -GridZ));
				Vector3 Position2 = (new Vector3 (i-GridX, -BlockStructureSize.y, 0));
				Position1 = GridObject.transform.position - Offset + GridObject.transform.TransformPoint(Position1);
				Position2 = GridObject.transform.position - Offset + GridObject.transform.TransformPoint(Position2);
				GridLine.GetComponent<LineRenderer>().SetPosition (0, Position1);
				GridLine.GetComponent<LineRenderer>().SetPosition (1, Position2);
			}
			for (int i = 0; i <= GridZ; i++)
			{
				int ChildIndex = Mathf.RoundToInt(GridX+1)+i;
				/*if (ChildIndex !=  GridObject.transform.childCount)
				{
					RefreshGrid();
					return;
				}*/
				GameObject GridLine2 = GridObject.transform.GetChild(ChildIndex).gameObject;
				Vector3 Position3 = (new Vector3 (-GridX,  -BlockStructureSize.y, i-GridZ));
				Vector3 Position4 = (new Vector3(0,  -BlockStructureSize.y,  i-GridZ));
				Position3 = GridObject.transform.position - Offset + GridObject.transform.TransformPoint(Position3);
				Position4 = GridObject.transform.position - Offset + GridObject.transform.TransformPoint(Position4);
				GridLine2.GetComponent<LineRenderer>().SetPosition (0, Position3);
				GridLine2.GetComponent<LineRenderer>().SetPosition (1, Position4);
			}
		}
	}
	public GameObject CreateGridLine(GameObject GridObject, int i, int j) {
		
		GameObject GridLine = new GameObject ();
		GridLine.name = "GridLine: " + i.ToString () + " - " + j.ToString ();
		GridLine.transform.position = MyModel.transform.position;
		GridLine.transform.SetParent (GridObject.transform);
		LineRenderer GridLineRenderer = GridLine.AddComponent<LineRenderer> ();
		GridLineRenderer.SetVertexCount (2);
		GridLineRenderer.SetWidth (0.01f, 0.01f);
		//GridLineRenderer.material = GetManager.GetCharacterManager ();
		return GridLine;
	}

	// =====Bone Functions=====
	
	// create bones
	// select bones
	// destroy bones
	// rotate bones
	// position bones
	public void InitiateBoneEditing() {
		GameObject NewLineRenderer = new GameObject ();
		NewLineRenderer.name = "BoneAddingRenderer";
		NewLineRenderer.AddComponent<LineRenderer> ();
		MyLineRenderer = NewLineRenderer.GetComponent<LineRenderer> ();
		MyLineRenderer.SetWidth (0.1f, 0.1f);
		NewLineRenderer.transform.SetParent (MyModel.transform);
		NewLineRenderer.transform.localPosition = new Vector3 ();
		NewLineRenderer.GetComponent<LineRenderer> ().material = GetManager.GetCharacterManager ().MyBotPathMaterial;
	}
	public void HandleInputBoneMode() {
		if (Input.GetKeyDown (KeyCode.T)) 
		{
			CustomBones MyBoneMesh = MyModel.GetComponent<CustomBones>();
			if (MyBoneMesh) 
			{
				MyBoneMesh.IsAnimating = !MyBoneMesh.IsAnimating;
			}
		}
		if (Input.GetMouseButtonDown (0))
		{
			MyModelTextureHandler.GetViewportPositionInsideModelTexture();
			if (!IsAddingBone) 
			{
				FirstAddBonePosition = MyModelTextureHandler.GetPositionInViewPortWithDepth(5);
				IsAddingBone = true;
			} 
			else 
			{
				SecondAddBonePosition = MyModelTextureHandler.GetPositionInViewPortWithDepth(5);
				IsAddingBone = false;
				CustomBones MyBoneMesh = MyModel.GetComponent<CustomBones>();
				if (MyBoneMesh) 
				{
					MyBoneMesh.MySkinnedMesh.AddBone(FirstAddBonePosition, SecondAddBonePosition);
					AddBoneLine(FirstAddBonePosition, SecondAddBonePosition);
				}
			}
		}
	}
	
	public void AddBoneLine(Vector3 BoneBegin, Vector3 BoneEnd) {
		GameObject NewLineRenderer = new GameObject ();
		NewLineRenderer.name = "BoneLineRender";
		NewLineRenderer.AddComponent<LineRenderer> ();
		LineRenderer NewBoneRenderer = NewLineRenderer.GetComponent<LineRenderer> ();
		NewBoneRenderer.SetWidth (0.1f, 0.1f);
		NewLineRenderer.transform.SetParent(MyModel.transform);
		NewLineRenderer.transform.localPosition = new Vector3 ();
		NewBoneRenderer.material = GetManager.GetCharacterManager ().MyBotPathMaterial;
		NewBoneRenderer.SetPosition (0, BoneBegin);
		NewBoneRenderer.SetPosition (1, BoneEnd);
		NewLineRenderer.transform.SetParent (MyModel.transform);
		MyBoneRenderers.Add (NewBoneRenderer);
	}
}
}