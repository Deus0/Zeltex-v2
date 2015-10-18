using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// currently used on cooldowns

// to be used on:
// breaking blocks
// hmm idk... any particle effects maybe?
// gui select thing, where it clones around the outside in a line

public class AnimatingTiledTexture : MonoBehaviour {
	//public List<Texture> AnimatingTextures;
	public int MaxRows = 8;
	public int MaxColumns = 8;
	public Vector2 TilePosition = new Vector2(0,0);
	public Vector2 TileSize = new Vector2(0.125f,0.125f);
	public float FramesPerSecond = 4f;
	public int MaxIndex = 10;
	public bool IsRawImage = false;
	public bool IsLoop = false;
	public int AnimationCount = 0;
	public int AnimationCountMax = 1;
	public bool IsReversed = false;
	public bool IsActive = false;
	public bool IsReset = false;
	public float TimeExisted = 0;
	public int Index = -1;
	public int OverrideIndex = -1;

	// Use this for initialization
	void Start () {
		TileSize = new Vector2(1f/MaxColumns, 1f/MaxRows);
	}
	public void Stop() {
		IsActive = false;
	}
	public void Activate() {
		IsActive = true;
	}
	// Update is called once per frame
	void Update () {
		if (IsActive) {
			TimeExisted += Time.deltaTime;
			//Debug.Log ("Wanting to update Animation.");
			if (IsLoop || (AnimationCount < AnimationCountMax) || OverrideIndex != -1) {
				ModifyUVs ();
			}
		} else {
			EndAnimation ();
		}
		if (IsReset) {
			AnimationCount = 0;
			TimeExisted = 0f;
			IsReset = false;
		}
	}
	public void SetIndexByPercent(float Percent) {
		//Percent = Percent / 100f;
		OverrideIndex = Mathf.RoundToInt (Percent * MaxIndex);
	}
	public void ModifyUVs() {
		Debug.Log ("Updating Animation.");
		// Calculate index
		Index = Mathf.RoundToInt (TimeExisted * FramesPerSecond);
		if (OverrideIndex != -1)
			Index = OverrideIndex;
		// repeat when exhausting all frames
		int MaxTileIndex = MaxRows * MaxColumns;
		if (MaxTileIndex > MaxIndex) {
			MaxTileIndex = MaxIndex;
		}
		
		//if (Index > -1)
			//TileIndex = Index;
		//else 
		{
			if (Index >= MaxTileIndex) {
				AnimationCount = Index / (MaxTileIndex);	// when it reaches the end of the animation
				// if it is not looping, and if animation count is undermax
				if (OverrideIndex == -1)
				if (!IsLoop && AnimationCount >= AnimationCountMax) {
					EndAnimation ();
					return;	// otherwise keep going xD
				}
			}
			Index = Index % (MaxTileIndex);
		}

		if (IsReversed)
			Index = MaxTileIndex - Index;
		// split into horizontal and vertical index
		int TemporaryMaxRows = MaxRows;
		int TemporaryMaxColumns = MaxColumns;
		//TemporaryMaxRows = 2;
		int TileX = Index % TemporaryMaxRows;
		//if (TileX == 1)
		//	TemporaryMaxColumns = 10 % MaxRows;
		int TileY = Index / TemporaryMaxColumns;
		// build offset
		// inverting as open gl and direct x different
		Vector2 Offset = new Vector2 (((float)TileX) * TileSize.x, ((float)TileY) * TileSize.y);
		if (!IsRawImage) {
			Renderer MyRenderer = (Renderer)gameObject.GetComponent ("Renderer");
			MyRenderer.material.SetTextureOffset ("_MainTex", Offset);
			MyRenderer.material.SetTextureScale ("_MainTex", TileSize);
			Debug.Log (Index.ToString () + "  Updating Animation~ " + " : " + MyRenderer.name);
		} else {
			RawImage MyRawImage = (RawImage)gameObject.GetComponent ("RawImage");
			Rect MyUvRect = MyRawImage.uvRect;
			MyUvRect.x = Offset.x;
			MyUvRect.y = Offset.y;
			MyUvRect.width = TileSize.x;
			MyUvRect.height = TileSize.y;
			MyRawImage.uvRect = MyUvRect;
		}
		//Debug.Log (Index.ToString () + "  Tiles " + " : " + TileX.ToString () + " : " + TileY.ToString ());
		//Debug.Log (Index.ToString () + "  Offset " + " : " + Offset.x.ToString () + " : " + Offset.y.ToString ());
	}
	public void EndAnimation() {
		//Debug.Log ("Ending Animation.");
		if (!IsRawImage) {	// if it is used a material
			Renderer MyRenderer = (Renderer)gameObject.GetComponent ("Renderer");
			MyRenderer.material.SetTextureOffset ("_MainTex", new Vector2(0,0));
			MyRenderer.material.SetTextureScale ("_MainTex", new Vector2(0,0));
		} else {
			RawImage MyRawImage = (RawImage)gameObject.GetComponent ("RawImage");
			Rect MyUvRect = MyRawImage.uvRect;
			MyUvRect.x = 0;
			MyUvRect.y = 0;
			MyUvRect.width = 0;
			MyUvRect.height = 0;
			MyRawImage.uvRect = MyUvRect;
		}
	}
}
