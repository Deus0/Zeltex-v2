using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace OldCode {
public class ItemButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
	public bool IsEnabled = false;
	private GuiManager MyGui;
	public int ItemButtonIndex = 0;
	public int WindowIndex = 0;

	// Use this for initialization
	void Start () {
		IsEnabled = false;
		MyGui = GetManager.GetGuiManager ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void OnPointerEnter(PointerEventData eventData) {
		//Debug.Log ("Mouse Entering ItemButton");
		GetManager.GetInventoryGui().HandleMouseOverItem (ItemButtonIndex,WindowIndex);
	}
	public void OnPointerExit(PointerEventData eventData) {
		//Debug.Log ("Mouse Exiting ItemButton");
		GetManager.GetInventoryGui().HandleMouseNotOverItem ();
	}
	public void OnDeselect(BaseEventData eventData) {
		IsEnabled = false;
		Debug.Log ("UnselectedItem");
		//MyGui.HandleIconDeselected ();
	}

	public void OnSelect(BaseEventData eventData) {
		/*IsEnabled = true;
		GetManager.GetInventoryGui().HandleIconSelected ();
		//IsEnabled = false;
		//gameObject.GetComponent<Button> ().CancelInvoke ();
		//eventData.currentInputModule. ();
		Debug.Log ("SelectedItem");
		//eventData.Reset ();*/
	}
	public void OnPointerDown (PointerEventData data) 
	{
		Debug.Log ("SelectedItem: " + gameObject.name);
		IsEnabled = true;
		GetManager.GetInventoryGui().HandleItemPressed (this);
	}
}
}
