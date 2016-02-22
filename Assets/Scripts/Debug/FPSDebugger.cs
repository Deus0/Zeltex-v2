using UnityEngine;
using System.Collections;

public class FPSDebugger : MonoBehaviour 
{
	public KeyCode MyClearKey;
	float deltaTime = 0.0f;
	float LowestTime = -1, HighestTime = -1;

	void Start() {
		Application.targetFrameRate = 300;
	}

	void Update()
	{
		if (Input.GetKeyDown (MyClearKey)) 
		{
			HighestTime = -1;
			LowestTime = -1;
		}
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}
	
	void OnGUI()
	{
		if (deltaTime == 0)
			return;
		int w = Screen.width, h = Screen.height;
		if (deltaTime > HighestTime || HighestTime == -1)
			HighestTime = deltaTime;
		if (deltaTime < LowestTime || LowestTime == -1)
			LowestTime = deltaTime;
		float fps = 1.0f / deltaTime;
		GUIStyle style = new GUIStyle();
		if (fps < 10)
			style.normal.textColor = Color.red;
		else if (fps < 30)
			style.normal.textColor = Color.yellow;
		else if (fps < 50)
			style.normal.textColor = Color.green;
		else
			style.normal.textColor = Color.cyan;
		
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperRight;
		style.fontSize = h * 2 / 100;
	//	style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
		rect.y += 20;
		style.normal.textColor = Color.black;
		float LowestTime2 = LowestTime*1000f;
		float HighestTime2 = HighestTime*1000f;
		GUI.Label (rect, string.Format ("Low: {0:0.0} ms", LowestTime2), style);
		rect.y += 20;
		GUI.Label (rect, string.Format ("High: {0:0.0} ms", HighestTime2), style);
	}
}