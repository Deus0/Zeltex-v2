using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

// The Map Texture Handler class
// Description:	this is attached to the RawImage of the map texture
//				It extends handler classes, so it will take in any mouse data when the user interacts with it, and call their respective functions
//					*See the unity documentation for the handler functions

namespace OldCode {
public class MapTextureHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {
	private MapCreator MyMap;

	// Use this for initialization
	void Start () {
		MyMap = GetManager.GetMapCreator ();
	}

	// Update is called once per frame
	void Update () {
	
	}
	public void OnPointerEnter(PointerEventData eventData) {
		//Debug.LogError ("Mouse Entering MapTexture: " + Time.time);
	}
	public void OnPointerExit(PointerEventData eventData) {
		//Debug.LogError ("Mouse Exiting MapTexture: " + Time.time);
	}
	public void OnDeselect(BaseEventData eventData) {
		//Debug.LogError ("Unselect MapTexture: " + Time.time);
	}
	
	public void OnSelect(BaseEventData eventData) {
		//Debug.LogError ("Select MapTexture: " + Time.time);
		if (MyMap != null) {
			MyMap.UpdateMap();
		}
	}
}
}