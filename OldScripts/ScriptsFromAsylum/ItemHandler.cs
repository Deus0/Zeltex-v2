using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public class ItemHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,IPointerUpHandler, IDragHandler {
	public bool IsHighlighted;
	public int ItemIndex = 0;
	public InventoryManager MyInventoryManager;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnPointerUp(PointerEventData data) {

	}

	public void OnPointerDown (PointerEventData data) {
		MyInventoryManager.AttemptSelect(ItemIndex);
	}

	public void OnDrag (PointerEventData data) {

	}
		
	public void OnPointerEnter(PointerEventData eventData) {
		IsHighlighted = true;
		MyInventoryManager.HighlightedItemIndex = ItemIndex;
	}
	public void OnPointerExit(PointerEventData eventData) {
		IsHighlighted = false;
		MyInventoryManager.HighlightedItemIndex = -1;
	}
	public void OnSelect(BaseEventData eventData) {
	}
	public void OnDeselect(BaseEventData eventData) {
		//Debug.LogError ("Unselect MapTexture: " + Time.time);
	}

}
