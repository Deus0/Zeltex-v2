using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace GuiSystem 
{
	[ExecuteInEditMode]
	public class GuiListHandler : MonoBehaviour 
	{
		// statics
		//public static string GuiCellPrefabPath = "Prefabs/GuiCells/GuiCellPrefab.prefab";
		public static string GuiCellPrefabPath = "Prefabs/GuiCells/GuiCellPrefab";
		// debug options
		[Header("Debug")]
		public bool IsDebugMode = false;
		public bool IsRefreshList = false;
		public bool IsClearList = false;
		public KeyCode AddCellKey = KeyCode.Z;
		public KeyCode RemoveLastCellKey = KeyCode.X;
		public KeyCode RefreshListKey = KeyCode.C;
		// databases
		protected List<GameObject> MyGuis = new List<GameObject> ();	// spawned guis
		private List<TooltipData> MyGuiTooltipDatas = new List<TooltipData> ();

		// auto check list size
		[Header("Prefabs")]
		[Tooltip("Prefab of the gui Cell")]
		public GameObject GuiCellPrefab;
		[Tooltip("Reference to the ToolTip Gui")]
		public GameObject TooltipGui;

		// grid options - for construction of grid
		[Header("Grid Options")]
		[Tooltip("Updates List at beginning of runtime")]
		public bool IsUpdateOnStart = false;
		[Tooltip("Initial margin of the cell positions")]
		public Vector2 GridMargin = new Vector2(25,25);
		[Tooltip("Distance between each cell")]
		public Vector2 CellMargin = new Vector2(25,25);
		[Tooltip("Keep at 0 if you want no limits.")]
		public Vector2 LimitGrid = new Vector2(0,0);
		private Vector2 ScrollPosition = new Vector2(0,0);
		private Vector2 MaxGrid;	// visible amount of cells showing
		private Vector2 CellSize;	// this depends on prefab
		
		bool IsVerticalButtons = false;
		// Animation Options
		[Tooltip("Time it takes to scroll from one position to the next")]
		public float ScrollAnimationTime = 2f;
		private List<Vector2> ScrollBeginPositions = new List<Vector2>();
		private float ScrollBeginTime;

		[Header("Events")]
		public CustomEvents.MyEventString OnClick = new CustomEvents.MyEventString();

		void OnGUI()
		{
			if (IsDebugMode)
			{
				// Debugging for the positioning variables. Incase I want to add more features and accidently break it.
				GUILayout.Label (name + " has " + MyGuis.Count + " Guis.");
				GUILayout.Label ("MaxGrid: " + MaxGrid.ToString());
				GUILayout.Label ("CellSize: " + CellSize.ToString());
				GUILayout.Label ("GridMargin: " + GridMargin.ToString());
				GUILayout.Label ("CellMargin: " + CellMargin.ToString());
				GUILayout.Label ("IsVerticalButtons: " + IsVerticalButtons.ToString());
			}
		}

		private void DebugHandles() 
		{
			if (IsDebugMode) 
			{
				if (Input.GetKeyDown (AddCellKey)) 
				{
					AddGui ("Test" + (MyGuis.Count - 1));
				}
				if (Input.GetKeyDown (RemoveLastCellKey)) 
				{
					RemoveAt (MyGuis.Count - 1);
				}
				if (IsRefreshList || Input.GetKeyDown (RefreshListKey)) 
				{
					IsRefreshList = false;
					RefreshList();
				}
				if (IsClearList) {
					IsClearList = false;
					Clear();
				}
			}
		}
		
		void OnEnable() 
		{
			//Debug.Log ("Enabling script.");
			ToggleVerticalButtons ();
		}

		void Start() 
		{
			if (IsUpdateOnStart)
			{
				RefreshList();
			}
			//Debug.Log ("Starting script.");
			ToggleVerticalButtons ();
		}

		// Update is called once per frame
		protected void Update () 
		{
			DebugHandles ();
			AnimateCells ();
		}

		// implement in the extended class
		virtual public void RefreshList() 
		{

		}

		private void AnimateCells() 
		{
			if (ScrollBeginTime != -1) {	// finished animating
				if (Time.time - ScrollBeginTime < ScrollAnimationTime) 
				{
					for (int i = 0; i < MyGuis.Count; i++)
					{
						if (MyGuis [i] != null) 
						{
							if (i < ScrollBeginPositions.Count)
								MyGuis [i].GetComponent<RectTransform> ().anchoredPosition = 
								Vector2.Lerp (ScrollBeginPositions [i], 
					              GetCellPosition (i),
					              (Time.time - ScrollBeginTime) / ScrollAnimationTime);
						} else {
							Debug.LogError("Problem: Null Gui in list: " + name);
						}
					}
				} else {
					// end animation
					for (int i = 0; i < MyGuis.Count; i++) 
					{
						MyGuis [i].GetComponent<RectTransform> ().anchoredPosition = GetCellPosition (i);
					}
					ScrollBeginTime = -1;
				}
			}
		}

		public bool Contains(string name) 
		{
			for (int i = 0; i < MyGuis.Count; i++)
			{
				if (MyGuis[i].name == name)
					return true;
			}
			return false;
		}
		public void Scroll(bool Direction) 
		{
			if (Direction)
			{
				int Max = Mathf.FloorToInt(ScrollPosition.y*MaxGrid.x + MaxGrid.y*MaxGrid.x);
				//MyGuis.Count/ListSizeX-ListSizeY*ListSizeX
				if (Max < MyGuis.Count-1)	// 5 is the size that it can hold
					ScrollPosition.y++;
			}
			else 
			{
				if (ScrollPosition.y > 0)
					ScrollPosition.y--;
			}
			ScrollBeginTime = Time.time;
			ScrollBeginPositions.Clear ();
			
			for (int i = 0; i < MyGuis.Count; i++) 
			{
				ScrollBeginPositions.Add (MyGuis [i].GetComponent<RectTransform> ().anchoredPosition);
			}
		}

		public void Clear() 
		{
			for (int i = 0; i < MyGuis.Count; i++)
			{
				if (MyGuis[i])
					DestroyImmediate(MyGuis[i]);
			}
			for (int i = transform.childCount-1; i >= 0; i--) 
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
			MyGuis.Clear ();
			MyGuiTooltipDatas.Clear ();
		}

		public void RemoveAt(int Index) 
		{
			if (Index >= 0 && Index < MyGuis.Count) 
			{
				Destroy (MyGuis [Index].gameObject);
				MyGuis.RemoveAt (Index);
				MyGuiTooltipDatas.RemoveAt (Index);
			}
		}
		public Vector3 GetCellPosition(int Index) 
		{
			if (Mathf.FloorToInt(MaxGrid.x) == 0 || 
			    Mathf.FloorToInt(MaxGrid.y) == 0)	// minimum grid should be 1
			{
				return new Vector3 ();
			}

			Vector3 CellPosition = new Vector3 ();
			int PositionX = Index % Mathf.FloorToInt(MaxGrid.x);
			int PositionY = Index / Mathf.FloorToInt(MaxGrid.x);
			PositionY -= Mathf.FloorToInt(ScrollPosition.y);
			CellPosition = new Vector3 ( GridMargin.x  + (CellSize.x/2f) + (PositionX) * (CellSize.x + CellMargin.x),	
			                            -(GridMargin.y + (CellSize.y/2f) +(PositionY) * (CellSize.y + CellMargin.y)),
			                            0);
			//if (MaxGrid.x > 1)
			//	CellPosition.x += -CellSize.x*MaxGrid.x/2f; 
			return CellPosition;
		}
		
		public void AddGui(string GuiLabel) 
		{
			TooltipData NewTooltip = new TooltipData ();
			NewTooltip.LabelText = GuiLabel;
			AddGui (GuiLabel, NewTooltip);
		}
		
		public void AddGui(string GuiLabel, TooltipData MyTooltipData) 
		{
			AddGui (GuiLabel, MyTooltipData, -1);
		}
		
		// returns true if size was updated - size is used to position cells
		private void UpdateSize() 
		{
			if (CellSize.x == 0 || CellSize.y == 0) {
				// Path only works in editor
				if (GuiCellPrefab == null) {
					GuiCellPrefab = Resources.Load (GuiCellPrefabPath, typeof(GameObject)) as GameObject;
				}
				if (GuiCellPrefab != null) {
					RectTransform MyGuiPrefabRect = GuiCellPrefab.GetComponent<RectTransform> ();
					if (MyGuiPrefabRect) {
						CellSize = MyGuiPrefabRect.GetSize ();
					}
					Vector2 GridSize = gameObject.GetComponent<RectTransform> ().GetSize ();
					MaxGrid.x = Mathf.FloorToInt((GridSize.x - GridMargin.x) / (CellSize.x + CellMargin.x));
					MaxGrid.y = Mathf.FloorToInt((GridSize.y - GridMargin.y) / (CellSize.y + CellMargin.y));
					if (MaxGrid.x < 1)
						MaxGrid.x = 1;
					if (MaxGrid.y < 1)
						MaxGrid.y = 1;
					if (LimitGrid.x != 0)
						if (MaxGrid.x > LimitGrid.x)
							MaxGrid.x = LimitGrid.x;
					if (LimitGrid.y != 0)
						if (MaxGrid.y > LimitGrid.y)
							MaxGrid.y = LimitGrid.y;
				}
			}
		}

		protected void ToggleVerticalButtons() 
		{
			ToggleVerticalButtons (IsVerticalButtons);
		}

		protected void ToggleVerticalButtons(bool IsOn)
		{
			Transform UpButton = transform.parent.FindChild ("UpButton");
			if (UpButton)
				UpButton.gameObject.SetActive (IsOn);
			Transform DownButton = transform.parent.FindChild ("DownButton");
			if (DownButton)
				DownButton.gameObject.SetActive (IsOn);
		}

		private void CheckVerticalButtons() 
		{
			int PositionY = (MyGuis.Count - 1) / Mathf.FloorToInt (MaxGrid.x);
			IsVerticalButtons = (PositionY >= MaxGrid.y);
		}
		// Add Click event to the GuiTooltip class

		// index use for insert the gui into a point in the list
		public void AddGui(string GuiLabel, TooltipData MyTooltipData, int Index) 
		{
			UpdateSize ();	// initiate size
			//if (GuiCellPrefab == null)	// only if guicellprefab is null do we not create prefab
			//	return;
			if (GuiCellPrefab) {
				GameObject NewGuiCell = (GameObject)Instantiate (GuiCellPrefab, 
			                                                  new Vector3 (0, 0, 0), 
			                                                  Quaternion.identity);
				NewGuiCell.name = MyTooltipData.LabelText;

				if (NewGuiCell.transform.childCount > 0) {
					if (NewGuiCell.transform.GetChild (0).gameObject.GetComponent<Text> ())
						NewGuiCell.transform.GetChild (0).gameObject.GetComponent<Text> ().text = GuiLabel;
				}
				//if (TooltipGui)
				{
					GuiTooltip MyGuiToolTip = NewGuiCell.AddComponent<GuiTooltip> ();
					MyGuiToolTip.ToolTipGui = TooltipGui;
					MyTooltipData.OnClick = OnClick;
					MyGuiToolTip.MyTooltipData = MyTooltipData;
				}

				NewGuiCell.transform.SetParent (gameObject.transform, false);
				//Debug.LogError ("Assigning new position: " + GetCellPosition (MyGuis.Count).ToString ());
				if (Index == -1) {
					MyGuis.Add (NewGuiCell);
					NewGuiCell.GetComponent<RectTransform> ().anchoredPosition = GetCellPosition (MyGuis.Count - 1);
					MyGuiTooltipDatas.Add (MyTooltipData);
				} else {
					NewGuiCell.GetComponent<RectTransform> ().anchoredPosition = GetCellPosition (Index);
					MyGuis.Insert (Index, NewGuiCell);
					MyGuiTooltipDatas.Insert (Index, MyTooltipData);
				}
				CheckVerticalButtons();
				if (gameObject.activeSelf)
					ToggleVerticalButtons();
			}
		}
	}
}