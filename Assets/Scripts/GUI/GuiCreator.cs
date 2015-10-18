using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// creates all the windows by adding different classes and prefabs together
public class GuiCell {
	public GameObject ObjectReference;
	public Vector3 Position;
	public Vector3 Size;
	public int Index;
	public Vector2 GetCellPosition(int NewIndex, int ColumnsMax) {
		Index = NewIndex;
		return new Vector2(
			Index / ColumnsMax,
			Index % ColumnsMax);
	}
};

public class GuiCreator : MonoBehaviour {
	// Reference for updates
	public BaseCharacter MyStatsCharacter;
	// References to Spawned Objects
	public List<MyWindow> MyWindows;
	public List<Text> MySpawnedTexts;	// for quick access to text
	public List<GameObject> MySpawnedButtons;	// for quick access to text
	public GameObject ModelViewerLabel;
	// prefabs for creation
	public GameObject MyCanvas;			// where the newly created objects will be attached to
	public Font MyFont;					// font all objects will use
	public int MyFontSize = 14;			// size of the font
	public bool IsCentred = true;		// are the gui elements centred
	// window variables
	float DefaultHeaderSize = 20.0f;
	public Color32 WindowColor = new Color32 (0, 0, 0, 255);
	public Color32 BackgroundColor = new Color32 (255, 255, 255, 255);
	// item creation variables
	public int RowsMax = 3;
	public int ColumnsMax = 10;
	public Vector3 NewGuiSize = new Vector3 (375, 30, 0);	// size per cell
	public Color32 CellsColor = new Color32(0,0,0,255);
	public Color32 FontColor = new Color32 (0, 0, 0, 255);
	public Color32 ButtonColor = new Color32 (0, 0, 0, 255);
	// Stats Screen variables
	public float StatsPanelMarginX = 5f;	// can go to 2
	public float StatsPanelMarginY = 5f;	// can go to 2
	// In between cells
	public float MarginPercentageX = 1f;	// can go to 2
	public float MarginPercentageY = 1f;	// can go to 2
	public float MarginAdditionX = 0f;	// can go to 2
	public float MarginAdditionY = 0f;	// can go to 2

	// More prefebas
	public GameObject PanelPrefab;
	public GameObject CloseButtonPrefab;
	public GameObject DragZonePrefab;
	public GameObject ResizeZonePrefab;

	public GameObject ButtonPrefab;
	public Texture PlusTexture;
	public Texture MinusTexture;
	public GameObject ItemPrefab;
	public GameObject ModelTexturePrefab;
	public ResizePanel MyResizeablePanel;
	public GameObject MapTexturePrefab;
	public GameObject DungeonTexturePrefab;
	
	public Vector3 InventoryScreenPosition;
	//public Vector3 ModelScreenPosition = new Vector3(300,800,0);
	public Vector3 StatScreenPosition = new Vector3 (1300, 500, 0);
	GameObject StatsScreenLabel;
	private bool IsInitialUpdate = true;

	// reference used to turn them off and on
	List<GameObject> IncreaseStatsButtons = new List<GameObject>();

	// Input Window	- -1 for disabled
	public int ChatWindowIndex = -1;

	void Start () {
		//CreateModelScreen (ModelScreenPosition);
		MarginPercentageX = 1.25f;
		MarginPercentageY = 1.25f;
		RowsMax = 6;
		ColumnsMax = 6;

		//CreateItemsScreen (InventoryScreenPosition);
		for (int i = 0; i < MyWindows.Count; i++) {
			// create window
			CreateWindow (MyWindows[i], i);
		}
	}

	void Update () {
		ManageStatScreen ();
	}
	public void DisableAllWindows() {
		for (int i = 0; i < MyWindows.Count; i++) {
			// create window
			MyWindows[i].Disable();
		}
		for (int i = 0; i < GetManager.GetGuiManager().MyMenuButtons.Count; i++) {
			// create window
			GetManager.GetGuiManager().MyMenuButtons[i].GetComponent<Button>().interactable = false;
		}
	}
	public void EnableAllWindows() {
		for (int i = 0; i < MyWindows.Count; i++) {
			// create window
			MyWindows[i].Enable();
		}
		for (int i = 0; i < GetManager.GetGuiManager().MyMenuButtons.Count; i++) {
			// create window
			GetManager.GetGuiManager().MyMenuButtons[i].GetComponent<Button>().interactable = true;
		}
	}
	public GameObject NewInputTextPrefab;
	public GameObject CreateWindow(MyWindow NewWindow, int WindowIndex) {
		NewWindow.Position = new Vector3 (-	NewWindow.Position.x, NewWindow.Position.y, NewWindow.Position.z);
		NewWindow.Position -= new Vector3 (NewWindow.Size.x / 2f, -NewWindow.Size.y / 2f, 0);

		NewWindow.WindowReference =	(GameObject) Instantiate (PanelPrefab, new Vector3 (), Quaternion.identity);
		NewWindow.WindowReference.GetComponent<ActiveStateToggler> ().IsDestroyOnClose = NewWindow.IsDestroyOnClose;
		SetGuiSettings (NewWindow.WindowReference, NewWindow.Position, NewWindow.Size);
		NewWindow.WindowReference.GetComponent<RawImage> ().color = NewWindow.WindowColor;
		NewWindow.WindowReference.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0);
		NewWindow.WindowReference.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0);
		NewWindow.WindowReference.GetComponent<RectTransform> ().SetPivot(new Vector2 (0, 1));

		NewWindow.WindowReference.name = NewWindow.Name;
		
		if (NewWindow.HasDragZone) {
			NewWindow.DragZone = AddDragZoneToPanel (NewWindow.WindowReference, NewWindow.Position, NewWindow.HeaderHeight);
		}
		// title text
		GameObject NewGuiText;
		if (NewWindow.HasDragZone) {
			RectTransform DragRect = NewWindow.DragZone.GetComponent<RectTransform>();
			NewGuiText = CreateGuiText (NewWindow.Position, new Vector3(DragRect.sizeDelta.x,DragRect.sizeDelta.y, 0), null);
			NewGuiText.transform.SetParent (NewWindow.DragZone.transform);
		} else {
			NewGuiText = CreateGuiText (NewWindow.Position, new Vector3(NewWindow.Size.x,NewWindow.HeaderHeight, 0), null);
			NewGuiText.transform.SetParent (NewWindow.WindowReference.transform);
		}
		if (NewWindow.Name != "") {
			RectTransformExtensions.SetPivotAndAnchors (NewGuiText.GetComponent<RectTransform> (), new Vector2 (0.5f, 1f));
			NewWindow.Title = NewGuiText.GetComponent<Text> ();
			NewWindow.Title.text = NewWindow.Name;
			NewGuiText.GetComponent<Text> ().fontSize = NewWindow.TitleFontSize;
			NewGuiText.GetComponent<Text> ().color = NewWindow.TitleFontColor;
			NewGuiText.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
		}

		if (NewWindow.IsCreateModelTexture) {
			GameObject ModelTextureReference = (GameObject) Instantiate (ModelTexturePrefab, new Vector3 (), Quaternion.identity);
			Vector2 ModelTextureMargin = new Vector2(5,5);
			Vector3 ModelTexturePosition = new Vector3 (NewWindow.Position.x+ModelTextureMargin.x, NewWindow.Position.y - NewWindow.HeaderHeight -ModelTextureMargin.y, 0);
			Vector2 ModelTextureSize = new Vector2 (NewWindow.Size.x-ModelTextureMargin.x*2, NewWindow.Size.y-ModelTextureMargin.y*2-NewWindow.HeaderHeight);
			ModelTextureReference.GetComponent<RawImage>();
			SetGuiSettings (ModelTextureReference, ModelTexturePosition, ModelTextureSize);
			ModelTextureReference.transform.SetParent (NewWindow.WindowReference.transform);
		}
		if (NewWindow.IsDunegonCreator) {
			GameObject ModelTextureReference = (GameObject) Instantiate (DungeonTexturePrefab, new Vector3 (), Quaternion.identity);
			Vector2 ModelTextureMargin = new Vector2(5,5);
			Vector3 ModelTexturePosition = new Vector3 (NewWindow.Position.x+ModelTextureMargin.x, NewWindow.Position.y - NewWindow.HeaderHeight -ModelTextureMargin.y, 0);
			Vector2 ModelTextureSize = new Vector2 (NewWindow.Size.x-ModelTextureMargin.x*2, NewWindow.Size.y-ModelTextureMargin.y*2-NewWindow.HeaderHeight);
			ModelTextureReference.GetComponent<RawImage>();
			SetGuiSettings (ModelTextureReference, ModelTexturePosition, ModelTextureSize);
			ModelTextureReference.transform.SetParent (NewWindow.WindowReference.transform);
		}
		
		if (NewWindow.IsCellsItems) {
			Vector3 CellSize = new Vector3 (40, 40, 0);
			MarginPercentageX = 1.1f;
			MarginPercentageY = 1.1f;
			RowsMax = 6;
			ColumnsMax = 6;
			List<GuiCell> MyItemCells = CreateGuiCells(CellSize);
			CreateCells(MyItemCells);
			AttachCellsToParent(MyItemCells, NewWindow.WindowReference, 10f, NewWindow.HeaderHeight+10f);
			AddCellsToGuiInventory(MyItemCells, NewWindow.Name);
		}
		if (NewWindow.IsCellsText) {
			RowsMax = 2;
			ColumnsMax = 8;
			MarginPercentageX = 1;
			MarginPercentageY = 1;
			CreateCells (NewWindow.WindowReference, true, NewGuiSize, "Text", NewWindow.Position, NewWindow.HeaderHeight, NewWindow.CellsReference);
		}
		if (NewWindow.DisplayTexture != null) {
			GameObject NewTexture = new GameObject();
			RawImage MyImage = NewTexture.AddComponent<RawImage>();
			MyImage.texture = NewWindow.DisplayTexture;Vector2 ModelTextureMargin = new Vector2(5,5);
			Vector3 ModelTexturePosition = new Vector3 (NewWindow.Position.x+ModelTextureMargin.x, NewWindow.Position.y - NewWindow.HeaderHeight -ModelTextureMargin.y, 0);
			Vector3 ModelTextureSize = new Vector3 (NewWindow.DisplayTexture.width, NewWindow.DisplayTexture.height, 0);
			SetGuiSettings (NewTexture, ModelTexturePosition, ModelTextureSize);
			NewTexture.transform.SetParent (NewWindow.WindowReference.transform);
		}
		if (NewWindow.IsMap) {
			GameObject NewTexture = (GameObject) Instantiate (MapTexturePrefab, new Vector3 (), Quaternion.identity);
			Vector2 ModelTextureMargin = new Vector2(5,5);
			Vector3 ModelTexturePosition = new Vector3 (NewWindow.Position.x+ModelTextureMargin.x, NewWindow.Position.y - NewWindow.HeaderHeight -ModelTextureMargin.y, 0);
			Vector3 ModelTextureSize = new Vector3 (NewWindow.Size.x-ModelTextureMargin.x*2, NewWindow.Size.y-ModelTextureMargin.y*2-NewWindow.HeaderHeight, 0);
			SetGuiSettings (NewTexture, ModelTexturePosition, ModelTextureSize);
			NewTexture.transform.SetParent (NewWindow.WindowReference.transform);
			GetManager.GetMapCreator().MyRawImage = NewTexture.GetComponent<RawImage>();	// link up the map creator class with the created Raw Image
		}
		if (NewWindow.IsInputText) {
			GameObject NewInputText = (GameObject) Instantiate (NewInputTextPrefab, new Vector3 (), Quaternion.identity);
			Vector2 ModelTextureMargin = new Vector2(5,5);
			Vector3 ModelTexturePosition = new Vector3 (NewWindow.Position.x+ModelTextureMargin.x, NewWindow.Position.y - NewWindow.HeaderHeight -ModelTextureMargin.y, 0);
			Vector3 ModelTextureSize = new Vector3 (NewWindow.Size.x-ModelTextureMargin.x*2, NewWindow.Size.y-ModelTextureMargin.y*2-NewWindow.HeaderHeight, 0);
			SetGuiSettings (NewInputText, ModelTexturePosition, ModelTextureSize);
			NewInputText.transform.SetParent (NewWindow.WindowReference.transform);
			// add function to input text on press enter lol
			NewInputText.GetComponent<InputField>().onEndEdit.AddListener(delegate {DestroyInputWindow ();});
		}

		if (NewWindow.HasCloseButton) {
			AddCloseButtonToWindow (NewWindow.WindowReference, NewWindow.Size, NewWindow.Position);
		}
		if (NewWindow.IsResizable) {
			AddResizezoneToWindow (NewWindow.WindowReference, NewWindow.Position, NewWindow.HeaderHeight);
		}


		// now attach the window to the canvas - one for each set of windows - ie main window, or in game menu
		NewWindow.WindowReference.GetComponent<RectTransform>().SetParent (MyCanvas.GetComponent<RectTransform>(), false);

		if (NewWindow.IsToggler) {
			// add state toggler button
			GetManager.GetGuiManager ().MyMenuButtons [WindowIndex].GetComponent<Button> ().onClick.AddListener (
					NewWindow.WindowReference.GetComponent<ActiveStateToggler> ().ToggleActive
				);
			// link up the windows state toggler to the button that toggles it
			NewWindow.WindowReference.GetComponent<ActiveStateToggler> ().MyButton = GetManager.GetGuiManager ().MyMenuButtons [WindowIndex].GetComponent<Button> ();
			GetManager.GetGuiManager ().MyMenuButtons [WindowIndex].transform.GetChild (0).GetComponent<Text> ().text = NewWindow.Name;
		}
		NewWindow.WindowReference.transform.localScale = NewWindow.Scale;
		NewWindow.WindowReference.SetActive (false);	// set them inactive by default

		return NewWindow.WindowReference;
	}
	public void ToggleStatsScreen() {
		Debug.Log ("Toggling StatsScreen to: " + (!StatsScreenLabel.activeSelf).ToString());
		//StatsScreenLabel
		StatsScreenLabel.SetActive (!StatsScreenLabel.activeSelf);
	}
	public void UpdatePlayer(BaseCharacter NewPlayerStats) {
		MyStatsCharacter = NewPlayerStats;
	}
	void ManageStatScreen() {
		if (MyStatsCharacter != null && Time.timeScale != 0) {
			ToggleIncreaseStatsButtons();
			for (int i = 0; i < MyWindows.Count; i++)
			SetStats (MyWindows[i], MyStatsCharacter);
		}
	}
	public void ToggleIncreaseStatsButtons() {
		for (int i = 0; i < IncreaseStatsButtons.Count; i++) {
			if (MyStatsCharacter.MyStats.MyLevel.SkillPoints > 0) {
				IncreaseStatsButtons [i].SetActive (true);
			} else {
				IncreaseStatsButtons [i].SetActive (false);
			}
		}
	}
	// shouldn't need to do this every screen
	// if MyStats.HasUpdated
	// this should be able to be updated for any player -> ie check out a bots stats
	public void SetStats(MyWindow NewWindow, BaseCharacter MyCharacter) {
		if (NewWindow.IsStats) {
			if (MyCharacter != null)
			if (MyCharacter.MyStats.HasUpdated || IsInitialUpdate) {
				NewWindow.Title.text = "Stats of: " +MyCharacter.name;
				if (MyCharacter.MyStats.MyLevel.SkillPoints > 0)
					NewWindow.Title.text += " - SkillPoints [" + MyCharacter.MyStats.MyLevel.SkillPoints.ToString () + "]";
				if (MyCharacter.MyStats.MyLevel.SkillPoints > 0) {
					// enable +/- buttons
					for (int i = 0; i < IncreaseStatsButtons.Count; i++) {
						IncreaseStatsButtons [i].SetActive (true);
					}
				} else {
					// disable +/- buttons
					for (int i = 0; i < IncreaseStatsButtons.Count; i++) {
						IncreaseStatsButtons [i].SetActive (false);
					}
				}

				UpdateText ("Strength: " + MyCharacter.MyStats.GetAttributeFromList ("Strength").Base.ToString (), 0, 0, NewWindow);
				UpdateText ("Vitality: " + MyCharacter.MyStats.GetAttributeFromList ("Vitality").Base.ToString (), 0, 1, NewWindow);
				UpdateText ("Intelligence: " + MyCharacter.MyStats.GetAttributeFromList ("Intelligence").Base.ToString (), 0, 2, NewWindow);
				UpdateText ("Wisdom: " + MyCharacter.MyStats.GetAttributeFromList ("Wisdom").Base.ToString (), 0, 3, NewWindow);
				UpdateText ("Agility: " + MyCharacter.MyStats.GetAttributeFromList ("Agility").Base.ToString (), 0, 4, NewWindow);
				UpdateText ("Dexterity: " + MyCharacter.MyStats.GetAttributeFromList ("Dexterity").Base.ToString (), 0, 5, NewWindow);
				UpdateText ("Luck: " + MyCharacter.MyStats.GetAttributeFromList ("Luck").Base.ToString (), 0, 6, NewWindow);
				UpdateText ("Charisma: " + MyCharacter.MyStats.GetAttributeFromList ("Charisma").Base.ToString (), 0, 7, NewWindow);
		
				UpdateText ("Health: " + MyCharacter.MyStats.GetStatFromList ("Health").Max.ToString (), 1, 0, NewWindow);
				UpdateText ("HealthRegen: " + MyCharacter.MyStats.GetStatFromList ("Health").Regeneration.ToString (), 1, 1, NewWindow);
				UpdateText ("Mana: " + MyCharacter.MyStats.GetStatFromList ("Mana").Max.ToString (), 1, 2, NewWindow);
				UpdateText ("ManaRegen: " + MyCharacter.MyStats.GetStatFromList ("Mana").Regeneration.ToString (), 1, 3, NewWindow);
				UpdateText ("Energy: " + MyCharacter.MyStats.GetStatFromList ("Energy").Max.ToString (), 1, 4, NewWindow);
				UpdateText ("EnergyRegen: " + MyCharacter.MyStats.GetStatFromList ("Energy").Regeneration.ToString (), 1, 5, NewWindow);
				MyCharacter.MyStats.HasUpdated = false;
				IsInitialUpdate = false;
			}
		}
	}
	public void UpdateText(string NewText, int PositionX, int PositionY, MyWindow NewWindow) {
		int Index = PositionY + PositionX * ColumnsMax;
		NewWindow.CellsReference[Index].GetComponent<Text>().text = NewText;
	}

	public GameObject AddResizezoneToWindow(GameObject NewPanel, Vector3 NewPanelPosition, float HeaderSize) {	
		/*if (MyResizeablePanel != null) {
			GameObject ResizeZone = (GameObject)Instantiate (MyResizeablePanel, new Vector3 (), Quaternion.identity);
			ResizeZone.transform.SetParent (NewPanel.transform);
			SetGuiSettings (ResizeZone, NewPanelPosition, new Vector2 (NewPanel.GetComponent<RectTransform> ().GetSize ().x, HeaderSize));
			ResizeZone.GetComponent<DragPanel> ().panelRectTransform = NewPanel.GetComponent<RectTransform> ();
			return ResizeZone;
		}*/
		return null;
	}
	public GameObject AddDragZoneToPanel(GameObject NewPanel, Vector3 NewPanelPosition, float HeaderSize) {		
		if (DragZonePrefab != null) {
			GameObject DragZone = (GameObject)Instantiate (DragZonePrefab, new Vector3 (), Quaternion.identity);
			DragZone.transform.SetParent (NewPanel.transform);
			SetGuiSettings (DragZone, NewPanelPosition, new Vector2 (NewPanel.GetComponent<RectTransform> ().GetSize ().x, HeaderSize));
			DragZone.GetComponent<DragPanel> ().panelRectTransform = NewPanel.GetComponent<RectTransform> ();
			return DragZone;
		} else
			return null;
	}
	
	public void SetGuiSettings(GameObject GuiElement, Vector3 GuiElementPosition, Vector3 GuiElementSize) {
		SetGuiSettings (GuiElement, GuiElementPosition, GuiElementSize, new Vector2 (0, 1));
	}
	public void SetGuiSettings(GameObject GuiElement, Vector3 GuiElementPosition, Vector3 GuiElementSize, Vector2 Anchor) {
		RectTransform GuiTransform = (RectTransform) GuiElement.GetComponent ("RectTransform");
		RectTransformExtensions.SetDefaultScale (GuiTransform);
		RectTransformExtensions.SetAnchors (GuiTransform, Anchor);
		RectTransformExtensions.SetPivot (GuiTransform, new Vector2 (0.0f, 1f));// (GuiTransform, new Vector2 (0, 400));
		RectTransformExtensions.SetSize(GuiTransform, new Vector2(GuiElementSize.x,GuiElementSize.y));
		GuiTransform.position = new Vector2 (GuiElementPosition.x, GuiElementPosition.y);
	}
	public GameObject CreateNewWindow(Vector3 PanelSize, Vector3 NewGuiPosition) {
		GameObject NewPanel = (GameObject) Instantiate (PanelPrefab, new Vector3 (), Quaternion.identity);
		SetGuiSettings (NewPanel, NewGuiPosition, PanelSize);
		NewPanel.GetComponent<RawImage> ().color = WindowColor;
		//NewPanel.transform.GetChild (0).GetComponent<RawImage> ().color = WindowColor;
		NewPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0);
		NewPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0);
		NewPanel.GetComponent<RectTransform> ().SetPivot(new Vector2 (0, 1));
		return NewPanel;
	}
	public void AddCloseButtonToWindow(GameObject WindowReference, Vector3 WindowSize, Vector3 CloseButtonPosition) {
		GameObject NewCloseButton =(GameObject) Instantiate (CloseButtonPrefab, new Vector3 (), Quaternion.identity);
		CloseButtonPosition.x += WindowSize.x - CloseButtonPrefab.GetComponent<RectTransform>().sizeDelta.x;
		SetGuiSettings (NewCloseButton, CloseButtonPosition, new Vector3(CloseButtonPrefab.GetComponent<RectTransform>().sizeDelta.x,
		                                                                 CloseButtonPrefab.GetComponent<RectTransform>().sizeDelta.y,
		                                                                 0), new Vector2 (1, 1));
		NewCloseButton.transform.SetParent (WindowReference.transform);
		NewCloseButton.GetComponent<Button> ().onClick.AddListener (WindowReference.GetComponent<ActiveStateToggler> ().ToggleActive);
	}
	public List<GuiCell> CreateGuiCells(Vector3 CellSize) {
		List<GuiCell> MyCells = new List<GuiCell> ();

		for (int j = 0; j < ColumnsMax; j++)	
			for (int i = 0; i < RowsMax; i++)
		{
			GuiCell NewCell = new GuiCell();
			NewCell.Position = new Vector3(i*CellSize.x, j*CellSize.y, 0);
			NewCell.Position = new Vector3(NewCell.Position.x*MarginPercentageX+MarginAdditionX, 
			                               NewCell.Position.y*MarginPercentageY+MarginAdditionY, 0);
			NewCell.Size = CellSize;
			MyCells.Add (NewCell);
		}
		return MyCells;
	}
	
	public void CreateCells(List<GuiCell> MyGuiCells) {
		for (int i = 0; i < MyGuiCells.Count; i++) {
			CreateGuiCell (MyGuiCells[i]);
			MyGuiCells[i].ObjectReference.name = "Cell: " + MyGuiCells[i].Position.ToString();
		}
	}
	public void AddCellsToGuiInventory(List<GuiCell> MyGuiCells, string MyWindowName) {
		MyInventoryGuiWindow NewWindow = new MyInventoryGuiWindow ();
		for (int i = 0; i < MyGuiCells.Count; i++) {
			MyItemGui MyGuiItem = new MyItemGui ();
			MyGuiItem.ItemImage = MyGuiCells[i].ObjectReference.GetComponent<RawImage> ();
			MyGuiItem.ItemText = MyGuiCells[i].ObjectReference.transform.GetChild (0).GetComponent<Text> ();
			NewWindow.MyItemGuis.Add (MyGuiItem);
		}
		if (MyWindowName == "Spellbook") 
			NewWindow.IsSpellbook = true;
		GetManager.GetInventoryGui ().AddNewWindow(NewWindow);
	}
	public GameObject CreateGuiCell(GuiCell MyCell) {
		MyCell.ObjectReference = (GameObject)CreateGuiItem (MyCell.Position, MyCell.Size);
		return MyCell.ObjectReference;
	}
	public GameObject CreateGuiItem(Vector3 GuiElementPosition, Vector3 GuiElementSize) {
		GameObject NewButton = (GameObject) Instantiate (ItemPrefab, new Vector3 (), Quaternion.identity);
		
		Button MyButton = (Button)NewButton.GetComponent ("Button");
		if (MyButton != null) {
			MyButton.onClick.AddListener (ButtonPressed);
			MySpawnedButtons.Add (NewButton);
		} else {
			Debug.LogError("Button not added in GuiCreator...");
		}
		SetGuiSettings (NewButton, GuiElementPosition, GuiElementSize);
		return NewButton;
	}
	public void AttachCellsToParent(List<GuiCell> MyGuiCells, GameObject ParentObject, float MarginLeft, float MarginTop) {
		RectTransform MyParentTransform = ParentObject.gameObject.GetComponent<RectTransform>();
		for (int i = 0; i < MyGuiCells.Count; i++) {
			MyGuiCells[i].ObjectReference.transform.SetParent(ParentObject.transform);
			Vector3 NewPosition = MyGuiCells[i].ObjectReference.transform.position;
			NewPosition.y = -NewPosition.y;
			NewPosition -= new Vector3(-MarginLeft + MyParentTransform.sizeDelta.x/2f, MarginTop-MyParentTransform.sizeDelta.y/2f, 0);
			MyGuiCells[i].ObjectReference.transform.position = NewPosition;
		}
	}
	// Should be 'create cell positions' then from a returned array list of positions and sizes, create teh cells
	// need to rewrite this bit
	public void CreateCells(GameObject NewPanel, bool IsButtons, Vector3 CellSize, string CellType, Vector3 NewCellsPosition, float HeaderSize, List<GameObject> CellsReference) {
		// make a background for our cells
		GameObject CellImage = new GameObject ();
		CellImage.name = "CellImage";
		CellImage.AddComponent<RawImage> ();
		CellImage.GetComponent<RawImage> ().color = CellsColor;

		Vector3 StatsScreenPosition = new Vector3 (NewCellsPosition.x+StatsPanelMarginX, NewCellsPosition.y - HeaderSize -StatsPanelMarginY*2, 0);
		Vector3 StatsScreenSize = new Vector3 (CellSize.x*(RowsMax)*MarginPercentageX, CellSize.y*(ColumnsMax)*MarginPercentageY, 0);
		SetGuiSettings (CellImage, StatsScreenPosition, StatsScreenSize);

		// Create the cells
		for (float i = 0; i < RowsMax; i++) {
			GameObject NewColumn = (GameObject) CreateDataBaseColumn(i, StatsScreenPosition, new Vector3(), IsButtons, CellType, CellSize, CellsReference);
			NewColumn.transform.SetParent(CellImage.transform);
		}
		CellImage.transform.SetParent (NewPanel.transform);
	}

	public GameObject CreateDataBaseColumn(float ColumnNumber, Vector3 StatsScreenPosition, Vector3 ColumnsSize, bool IsButtons, string CellType, Vector3 CellSize, List<GameObject> CellsReference) {
		GameObject NewColumn = new GameObject ();
		NewColumn.AddComponent<RectTransform> ();
		NewColumn.name = "Column " + ColumnNumber.ToString ();
		SetGuiSettings (NewColumn, StatsScreenPosition, ColumnsSize);
		for (float i = 0; i < ColumnsMax; i++) {
			GameObject NewGuiBackground = CreateGuiCell (ColumnNumber, i, StatsScreenPosition,IsButtons, CellType, CellSize, CellsReference);
			NewGuiBackground.transform.SetParent(NewColumn.transform);
		}
		return NewColumn;
	}
	public GameObject CreateGuiCell(float ColumnNumber, float RowNumber, Vector3 StatsScreenPosition, bool IsButtons, string CellType, Vector3 CellSize, List<GameObject> CellsReference) {
		Vector3 GuiElementPosition = StatsScreenPosition + new Vector3 (CellSize.x * ColumnNumber * MarginPercentageX + MarginAdditionX, 
		                                                                -CellSize.y * RowNumber * MarginPercentageY + MarginAdditionY,  0);
		GameObject NewGuiBackground;	// highest point
		if (CellType == "Text") {
			NewGuiBackground = (GameObject)CreateGuiBackground (GuiElementPosition, CellSize);
		
			GameObject NewGuiText = CreateGuiText (GuiElementPosition, CellSize, CellsReference);
			NewGuiText.transform.SetParent (NewGuiBackground.transform);
		} else {
			NewGuiBackground = (GameObject) CreateGuiItem(GuiElementPosition, CellSize);
		}
		
		if (ColumnNumber == 0 && IsButtons) {	// add in the buttons
			Vector3 NewButtonSize = new Vector3(CellSize.y-2f, CellSize.y-2f, 0);

			GameObject PlusButton = CreateGuiButton(GuiElementPosition + new Vector3((CellSize.x)-(CellSize.y),1,0), NewButtonSize);
			PlusButton.transform.SetParent (NewGuiBackground.transform);
			PlusButton.GetComponent<RawImage>().texture = PlusTexture;
			NewButtonSize = new Vector3(NewGuiSize.y-2f, NewGuiSize.y-2f, 0);

			ColorBlock MyColorBlock = PlusButton.GetComponent<Button>().colors;
			MyColorBlock.highlightedColor = new Color(0,0,0);
			PlusButton.GetComponent<Button>().colors = MyColorBlock;

			IncreaseStatsButtons.Add (PlusButton);

			PlusButton.AddComponent<IncreaseStatButton>();
			PlusButton.GetComponent<IncreaseStatButton>().StatIndex = Mathf.RoundToInt(RowNumber);
			PlusButton.GetComponent<Button>().onClick.AddListener(
				PlusButton.GetComponent<IncreaseStatButton>().IncreaseStats
				);

			/*GameObject NewButton2 = CreateGuiButton(GuiElementPosition + new Vector3((CellSize.x)-2f*(CellSize.y),1,0), NewButtonSize);
			NewButton2.transform.SetParent (NewGuiBackground.transform);
			NewButton2.GetComponent<RawImage>().texture = MinusTexture;
			NewButton2.GetComponent<Button>().colors = MyColorBlock;*/
		}
		return NewGuiBackground;
	}
	public GameObject CreateGuiBackground(Vector3 GuiElementPosition, Vector3 GuiElementSize) {
		GameObject NewGuiBackground = new GameObject ();	//(GameObject) Instantiate (new GameObject (), new Vector3 (), Quaternion.identity);
		NewGuiBackground.name = "GuiElementBackgroundImage";
		NewGuiBackground.AddComponent <RawImage>();

		RawImage MyRawImage = (RawImage)NewGuiBackground.GetComponent ("RawImage");
		MyRawImage.color = BackgroundColor;

		SetGuiSettings (NewGuiBackground, GuiElementPosition, GuiElementSize);

		return NewGuiBackground;
	}
	public GameObject CreateGuiText(Vector3 GuiElementPosition, Vector3 GuiElementSize, List<GameObject> CellsReference) {
		GameObject NewGuiText = new GameObject ();	//(GameObject) Instantiate (new GameObject (), new Vector3 (), Quaternion.identity);
		NewGuiText.name = "GuiElementText";
		NewGuiText.AddComponent <Text>();
		NewGuiText.transform.SetParent (MyCanvas.transform);
		
		Text MyText = (Text) NewGuiText.GetComponent ("Text");
		MyText.font = MyFont;
		MyText.color = FontColor;
		MyText.fontSize = MyFontSize;
		MyText.text = "";
		if (CellsReference != null) 
			CellsReference.Add (MyText.gameObject);

		SetGuiSettings (NewGuiText, GuiElementPosition, GuiElementSize);
		return NewGuiText;
	}

	public GameObject CreateGuiButton(Vector3 GuiElementPosition, Vector3 GuiElementSize) {
		GameObject NewButton = new GameObject ();	//(GameObject) Instantiate (ButtonPrefab, new Vector3 (), Quaternion.identity);
		NewButton.name = "NewButton";
		NewButton.AddComponent <RawImage>();
		NewButton.AddComponent <Button>();
		
		Button MyButton = (Button)NewButton.GetComponent ("Button");
		if (MyButton != null) {
			MyButton.onClick.AddListener (ButtonPressed);
			MySpawnedButtons.Add (NewButton);
		} else {
			Debug.LogError("Button not added in GUICreator");
		}
		/*MyButton.onClick.AddListener(() => {
			//handle click here
			Debug.Log ("Button was pressed in GUICreator");
		});*/

		SetGuiSettings (NewButton, GuiElementPosition, GuiElementSize);
		return NewButton;
	}

	public void ButtonPressed() {
		Debug.Log ("Button was pressed in GUICreator");
	}


	// input window - things like text, or a yes or no, or a quest
	// stops the player from inputting anything else
	// used for urgent matters
	
	public void CreateInputWindow(string InputString) {
		if (ChatWindowIndex == -1) {
			DisableAllWindows ();
			MyWindow NewChatWindow = new MyWindow ();
			NewChatWindow.Size = new Vector3 (250, 75);
			NewChatWindow.Position.x = -125;
			NewChatWindow.Position.y = 375;
			NewChatWindow.HasDragZone = true;
			NewChatWindow.HasCloseButton = false;
			NewChatWindow.IsInputText = true;
			NewChatWindow.Name = InputString;
			MyWindows.Add (NewChatWindow);
			ChatWindowIndex = MyWindows.Count - 1;
			CreateWindow (NewChatWindow, MyWindows.Count - 1);
		}
	}
	public void DestroyInputWindow() {
		string WhatWasTyped = "";
		//Debug.LogError ("Destroying Input Window");
		if (ChatWindowIndex >= 0 && ChatWindowIndex < MyWindows.Count) {
			WhatWasTyped = MyWindows[ChatWindowIndex].WindowReference.transform.GetChild (1).gameObject.transform.GetChild (2).gameObject.GetComponent<Text>().text;
			//MyWindows [ChatWindowIndex].WindowReference.SetActive(false);
			Destroy (MyWindows [ChatWindowIndex].WindowReference,0.05f);
			// for some reason doing this immediately in a compiled version will crash it lel - probaly something to do with the handlers
			MyWindows.RemoveAt (ChatWindowIndex);
			EnableAllWindows ();
		}
		ChatWindowIndex = -1;
		GetManager.GetGameManager().SetCanToggleMenu(true);
		//Debug.LogError ("Closing Chat, text input was: " + WhatWasTyped);
		MyStatsCharacter.UpdateName(WhatWasTyped);
	}


	// Stats stuff
	// when increase stat button is pressed, increase the stat
	public void IncreasePlayerStats(int Index) {
		MyStatsCharacter.MyStats.IncreaseAttribute (Index, 1);
	}
}
