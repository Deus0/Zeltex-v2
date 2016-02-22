using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler {
	
	public Vector2 originalLocalPointerPosition;
	public Vector3 originalPanelLocalPosition;
	public RectTransform panelRectTransform;
	public RectTransform parentRectTransform;
	
	void Awake () {
		panelRectTransform = transform.parent as RectTransform;
		//parentRectTransform = panelRectTransform.parent as RectTransform;
		parentRectTransform = GameObject.Find ("MyCanvas").GetComponent<RectTransform>();//	GetManager.GetCanvas ().gameObject.GetComponent<RectTransform>();
	}

	public void OnPointerDown (PointerEventData data) {
		gameObject.transform.parent.gameObject.transform.SetAsLastSibling();
		//Debug.Log ("Pointer is now on drag panel! at " + gameObject.name);
		originalPanelLocalPosition = panelRectTransform.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
	}
	
	public void OnDrag (PointerEventData data) {
		if (panelRectTransform == null || parentRectTransform == null)
			return;
		
		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition)) {
			Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
			panelRectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
		}
		
		ClampToWindow ();
	}
	
	// Clamp panel to area of parent
	void ClampToWindow () {
		Vector3 pos = panelRectTransform.localPosition;
		// scale it as well! possibly rotation later
		Vector2 PanelMinSize = new Vector2(panelRectTransform.rect.min.x * panelRectTransform.localScale.x,
		                                   panelRectTransform.rect.min.y * panelRectTransform.localScale.y);

		Vector2 PanelMaxSize = new Vector2(panelRectTransform.rect.max.x * panelRectTransform.localScale.x,
		                                   panelRectTransform.rect.max.y * panelRectTransform.localScale.y);

		Vector3 minPosition = parentRectTransform.rect.min - PanelMinSize;
		Vector3 maxPosition = parentRectTransform.rect.max - PanelMaxSize;
		
		pos.x = Mathf.Clamp (panelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
		pos.y = Mathf.Clamp (panelRectTransform.localPosition.y, minPosition.y, maxPosition.y);
		
		panelRectTransform.localPosition = pos;
	}
}
