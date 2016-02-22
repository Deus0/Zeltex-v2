using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler {
	
	public Vector2 minSize = new Vector2 (100, 100);
	public Vector2 maxSize = new Vector2 (400, 400);
	public Vector2 minScale = new Vector2 (.1f, .1f);
	public Vector2 maxScale = new Vector2 (2, 2);
	
	private RectTransform panelRectTransform;
	private Vector2 originalLocalPointerPosition;
	private Vector2 OriginalSize;
	private Vector2 originalLocalScale;
	
	//void Awake () {
	//	if (transform.parent != null)
	//		panelRectTransform = transform.parent.GetComponent<RectTransform> ();
		//originalSizeDelta = panelRectTransform.sizeDelta;
	//}
	
	public void OnPointerDown (PointerEventData data) {
		OriginalSize = panelRectTransform.sizeDelta;
		originalLocalScale = panelRectTransform.localScale;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
	}
	
	public void OnDrag (PointerEventData data) {
		if (panelRectTransform == null)
			return;
		
		Vector2 localPointerPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, data.position, data.pressEventCamera, out localPointerPosition);
		Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;


		Vector2 NewSize = OriginalSize + new Vector2 (offsetToOriginal.x, -offsetToOriginal.y);
		NewSize = new Vector2 (
			Mathf.Clamp (NewSize.x, minSize.x, maxSize.x),
			Mathf.Clamp (NewSize.y, minSize.y, maxSize.y)
		);
		Vector2 SizeRatioDifference = new Vector2 ( NewSize.x / OriginalSize.x, NewSize.y / OriginalSize.y);
		Vector2 NewScale = new Vector2(originalLocalScale.x*SizeRatioDifference.x, originalLocalScale.y*SizeRatioDifference.y);
		
		NewScale = new Vector2 (
			Mathf.Clamp (NewScale.x, minScale.x, maxScale.x),
			Mathf.Clamp (NewScale.y, minScale.y, maxScale.y)
			);
		//panelRectTransform.sizeDelta = NewSize;
		panelRectTransform.localScale = NewScale; 
	}
}