using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


namespace GuiSystem {

	[System.Serializable]
	public class TooltipData 
	{
		public string LabelText;
		public string DescriptionText;
		public CustomEvents.MyEventString OnClick = new CustomEvents.MyEventString();

		public TooltipData() 
		{
			LabelText = "???";
			DescriptionText = "???";
		}
	}

	public class GuiTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		public GameObject ToolTipGui;
		public TooltipData MyTooltipData;
		
		public void OnPointerClick(PointerEventData eventData) 
		{
			MyTooltipData.OnClick.Invoke (MyTooltipData.LabelText);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (ToolTipGui) {
				//Debug.Log ("Pointering entering a gui element: " + name);
				ToolTipGui.SetActive (true);
				RectTransform MyRect = ToolTipGui.GetComponent<RectTransform> ();
				ToolTipGui.transform.GetChild (0).GetComponent<Text> ().text = MyTooltipData.LabelText;
				ToolTipGui.transform.GetChild (1).GetComponent<Text> ().text = MyTooltipData.DescriptionText;
			}
		}

		public void OnPointerExit(PointerEventData eventData) 
		{
			if (ToolTipGui) {
				ToolTipGui.SetActive (false);
			}
		}

		void OnDisable() 
		{
			if (ToolTipGui) {
				ToolTipGui.SetActive (false);
			}
		}
	}
}
