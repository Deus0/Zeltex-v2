using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class GuiItem {
	public string Name;
	public string ToolTip;
	public int Amount;
};

public class InventoryManager : MonoBehaviour {
	public List<GuiItem> MyItemSlots;
	public List<Texture2D> MyItemTextures;
	public Texture2D DefaultItemTexture;
	public GameObject ItemPrefab;
	public int Columns = 5;
	public int Rows = 5;
	public List<GameObject> MyItems;
	public int ItemWidth;
	public int ItemHeight;
	public int MarginX, MarginY;
	//public int TopAdd;
	//public int LeftAdd;
	public GameObject MyToolTip;
	public int HighlightedItemIndex = -1;	// -1 no item is highlighted
	public int SelectedItemIndex = -1;
	public bool IsTreatmentManager = false;
	public Font MyFont;
	public Color32 MyTreatmentTextColor;
	public int MyTreatmentFontSize;
	public Color32 ItemAmountColor;

	// Use this for initialization
	void Start () {
		SpawnInventory ();
	}

	public void AttemptSelect(int PossibleSelectIndex) {
		if (MyItemSlots [PossibleSelectIndex].Amount > 0) {
			MyItems[SelectedItemIndex].GetComponent<RawImage>().color = new Color32(255,255,255,255);
			SelectedItemIndex = PossibleSelectIndex;
			MyItems[SelectedItemIndex].GetComponent<RawImage>().color = new Color32(255,55,55,255);
		}
	}

	// Update is called once per frame
	void Update () {
		if (HighlightedItemIndex >= 0 && HighlightedItemIndex < MyItems.Count)
		{
			MyToolTip.SetActive(true);
			MyToolTip.transform.GetComponent<RectTransform>().anchoredPosition = MyItems [HighlightedItemIndex].GetComponent<RectTransform>().anchoredPosition 
				+ new Vector2(MyItems[HighlightedItemIndex].GetComponent<RectTransform>().sizeDelta.x/2f+MyToolTip.transform.GetComponent<RectTransform>().sizeDelta.x/2f,
				              - MyItems[HighlightedItemIndex].GetComponent<RectTransform>().sizeDelta.y/2f);// - MyToolTip.transform.GetComponent<RectTransform>().sizeDelta.y/2f);	// minus tool tips size
			MyToolTip.transform.GetChild(0).GetComponent<Text>().text = MyItemSlots[HighlightedItemIndex].Name + "\n" + MyItemSlots[HighlightedItemIndex].ToolTip;
			if (!IsTreatmentManager) {
				MyItems[HighlightedItemIndex].transform.GetChild(0).GetComponent<Text>().text = MyItemSlots[HighlightedItemIndex].Amount.ToString();
			}
		}
		else
			MyToolTip.SetActive(false);
	}

	public void SpawnInventory() {
		for (int j = 1; j <= Rows; j++)
		{
			for (int i = 1; i <= Columns; i++)
			{
				if (MyItems.Count < MyItemSlots.Count) {
					GameObject MyItem = (GameObject) Instantiate(ItemPrefab, gameObject.GetComponent<RectTransform>().anchoredPosition
					                                             + new Vector2(-gameObject.GetComponent<RectTransform>().sizeDelta.x/2f, 
					              									gameObject.GetComponent<RectTransform>().sizeDelta.y/2f)
					                                             , Quaternion.identity);
					Vector2 ItemSize = MyItem.GetComponent<RectTransform>().sizeDelta;
					MyItem.GetComponent<RectTransform>().anchoredPosition = MyItem.GetComponent<RectTransform>().anchoredPosition
																			+ new Vector2(i*(ItemSize.x+MarginX), 
					                                                                       -(j)*(ItemSize.y+MarginY))
																			+ new Vector2(-ItemSize.x/2f, ItemSize.y);

					MyItem.transform.SetParent(gameObject.transform, false);
					if (MyItems.Count < MyItemSlots.Count && MyItems.Count < MyItemTextures.Count)
					{
						MyItem.GetComponent<RawImage>().texture = MyItemTextures[MyItems.Count];
					} else {
						MyItem.GetComponent<RawImage>().texture = DefaultItemTexture;
					}
					if (!IsTreatmentManager) {
						MyItem.transform.GetChild(0).GetComponent<Text>().color = ItemAmountColor;
						// for items set the value of how much stock is there
						MyItem.transform.GetChild(0).GetComponent<Text>().text = MyItemSlots[MyItems.Count].Amount.ToString();	
					} else {
						Destroy (MyItem.transform.GetChild(0).gameObject);	// destroys the item text
						MyItem.transform.DetachChildren();
					}
					ItemHandler MyItemHandler = MyItem.AddComponent<ItemHandler>();
					MyItemHandler.ItemIndex = MyItems.Count;
					MyItemHandler.MyInventoryManager = gameObject.GetComponent<InventoryManager>();


					if (IsTreatmentManager) {
						//MyItem.transform.GetChild(0).GetComponent<Text>().text = "";
						GameObject NewTextLabel = new GameObject();
						NewTextLabel.name = "MyTreatmentLabel";
						Text MyTreatmentLabel = NewTextLabel.AddComponent<Text>();
						MyTreatmentLabel.text = "Room number: " + MyItems.Count.ToString();
						MyTreatmentLabel.font = MyFont;
						MyTreatmentLabel.alignment = TextAnchor.MiddleCenter;
						MyTreatmentLabel.color = MyTreatmentTextColor;
						MyTreatmentLabel.fontSize = MyTreatmentFontSize;
						// stretch it
						//NewTextLabel.GetComponent<RectTransform>().sizeDelta = MyItem.GetComponent<RectTransform>().sizeDelta;
						NewTextLabel.transform.SetParent(MyItem.transform, false);
						NewTextLabel.GetComponent<RectTransform>().sizeDelta = MyItem.GetComponent<RectTransform>().sizeDelta;
						//NewTextLabel.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
						//NewTextLabel.GetComponent<RectTransform>().anchorMax = new Vector2(1,1);

					}
					MyItems.Add (MyItem);
				}
			}
		}
	}
}
