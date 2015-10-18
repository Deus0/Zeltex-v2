using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class RenderingDebugGUI : MonoBehaviour {
	float deltaTime = 0.0f;
	public Text MyFPSText;
	public Text MyFPSAverageText;
	public Text MyChunksToLoadText;
	public Text MyChunksLoadedText;
	public Text MyPolygonCountText;
	public List<float> FpsRates = new List<float>();
	public LoadChunks MyChunkLoader;

	// Use this for initialization
	void Start () {
		MyFPSText = gameObject.transform.FindChild ("FPSText").GetComponent<Text>();
		MyFPSAverageText = gameObject.transform.FindChild ("FPSAverageText").GetComponent<Text>();
		MyChunksToLoadText = gameObject.transform.FindChild ("ChunksToLoadText").GetComponent<Text>();
		MyChunksLoadedText = gameObject.transform.FindChild ("ChunksLoadedText").GetComponent<Text>();
		MyPolygonCountText = gameObject.transform.FindChild ("PolygonCountText").GetComponent<Text>();
	}
	
	void Update()
	{
		UpdateFPSTexts ();
		if (MyChunkLoader == null) {
			MyChunksToLoadText.text = "No Player";
			if (GameObject.Find ("Camera(Clone)"))
			MyChunkLoader = GameObject.Find ("Camera(Clone)").GetComponent<LoadChunks> ();
		} else 
		{
			MyChunksToLoadText.text = "Updates: " + MyChunkLoader.DebugUpdateListSize + " - Building: " + MyChunkLoader.DebugBuildListSize;
			int MaxToLoadChunks = (MyChunkLoader.MaxLoadingDistance*2-1)*(MyChunkLoader.MaxLoadingDistance*2-1)*((MyChunkLoader.MaxLoadHeight+1)*2-1);
			//if (MyChunkLoader.MaxLoadingDistance == 1) {
				//MaxToLoadChunks = (MyChunkLoader.MaxLoadingDistance)*(MyChunkLoader.MaxLoadingDistance)*(MyChunkLoader.MaxLoadHeight+1);
			//}
			if (MyChunkLoader.world) {
			MyChunksLoadedText.text = "ChunksLoaded: " + MyChunkLoader.world.DebugChunksLoadedCount  + "/" + MaxToLoadChunks;
			//MyChunksToLoadText.text = "Chunks To Load: " + (MaxToLoadChunks-MyChunkLoader.world.DebugChunksLoadedCount) + "/" + MaxToLoadChunks;
			MyPolygonCountText.text = "Polygons: " + (MyChunkLoader.world.DebugPolygonCount);
			}
		}
	}
	void UpdateFPSTexts() {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		MyFPSText.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		if (fps < 10)
			MyFPSText.color = Color.red;
		else if (fps < 30)
			MyFPSText.color = Color.yellow;
		else
			MyFPSText.color = Color.green;
		FpsRates.Add (fps);
		if (FpsRates.Count > 100)
			FpsRates.RemoveAt (0);	// remove first added one
		
		float FPSAverage = 0;
		for (int i = 0; i < FpsRates.Count; i++) {
			FPSAverage += FpsRates[i];
		}
		FPSAverage /= FpsRates.Count;
		MyFPSAverageText.text = "Average: " + (Mathf.RoundToInt(FPSAverage)).ToString ();
	}
}
