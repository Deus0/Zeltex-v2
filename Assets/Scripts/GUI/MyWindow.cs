using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


// going to store windows in this class with their components
[System.Serializable]
public class MyWindow {
	public string Name;				// has the text of the title
	// variables
	// for the window
	public Vector3 Position;				// intiial position of window created
	public Vector3 Size;					// intiial position of window created
	public Vector3 Scale;					// initial scale of window
	// for the title
	public int TitleFontSize;
	public Color32 TitleFontColor;
	public Color32 TitleBackgroundColor;
	public float HeaderHeight;
	public Color32 WindowColor;
	public bool IsDefaultWindowColours;		// sets it to hte GuiCreators defaults
	// states
	public bool HasCloseButton;
	public bool HasDragZone;
	public bool IsResizable;
	public bool IsToggler;
	public bool IsStats;
	public bool IsMap;
	public bool IsDunegonCreator;
	public bool IsInputText;
	
	public bool IsCreateModelTexture;
	public bool IsCreateCells;
	public bool IsCellsItems;
	public bool IsCellsText;				// for things like stats or other database things
	public bool IsCellsInputText;
	// prefabs
	//public GameObject Panel;
	
	// references
	public GameObject WindowReference;
	public GameObject DragZone;
	public Text Title;						// has the text of the title
	public List<Text> CellTexts;			// for quick access to Cells Texts
	public List<MyItemGui> CellItemGuis;	// for quick access to Cells ItemGuis
	public List<GameObject> CellsReference;	// for quick access to Cells GameObjects

	public Texture2D DisplayTexture;
	public bool IsDestroyOnClose = false;

	// should probaly get the defaults from a style sheet stored somewhere else
	public void SetDefaults() {
		Name = "NewWindow";
		TitleFontSize = 28;
		HeaderHeight = 32;
		TitleFontColor = new Color32 (0, 255, 0, 255);
		TitleBackgroundColor = new Color32 (0, 0, 0, 255);
		WindowColor = new Color32 (0, 0, 0, 255);
		IsToggler = true;
		HasDragZone = true;
		HasCloseButton = true;
		Scale = new Vector3 (1, 1, 1);
	}

	public MyWindow() {
		SetDefaults ();
	}

	// new window just for viewing an image!
	public MyWindow(Texture2D NewTexture) {
		SetDefaults ();
		Size.x = NewTexture.width+10f;	// + the margins
		Size.y = NewTexture.height + HeaderHeight+10f;
		DisplayTexture = NewTexture;
		IsDestroyOnClose = true;
	}

	public void Disable() {
		//WindowReference
		for (int i = 0; i < WindowReference.transform.childCount; i++) {
			WindowReference.transform.GetChild (i).gameObject.SetActive(false);
		}
	}
	public void Enable() {
		for (int i = 0; i < WindowReference.transform.childCount; i++) {
			WindowReference.transform.GetChild (i).gameObject.SetActive(true);
		}
	}
};