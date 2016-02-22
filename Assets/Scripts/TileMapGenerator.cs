using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class TileMapGenerator : MonoBehaviour {
	public List<Texture2D> MyTextures = new List<Texture2D>();
	public Texture2D OuputTexture;
	public FilterMode MyFilterMode;
	public int MaxX = -1;
	public int MaxY = -1;

	// Use this for initialization
	public void Start () {
		OuputTexture = CreateTileMap (name, MyTextures, OuputTexture, 4);
		if (gameObject.GetComponent<RawImage> ())
			gameObject.GetComponent<RawImage> ().texture = OuputTexture;
	}

	// grabs all the textures and chucks them into a tile map
	
	public static Texture2D CreateTileMap(List<Texture2D> TiledTextures, Texture2D NewTileMap, int TileLength) {
		return CreateTileMap ("", TiledTextures, NewTileMap, TileLength);
	}
	public static Texture2D CreateTileMap(string FileName, List<Texture2D> TiledTextures, Texture2D NewTileMap, int TileLength) {
		int PixelResolution = TiledTextures [0].width;
		int TileLengthX = TiledTextures.Count;
		int MaxX = TileLength;
		int MaxY = TileLength;
		if (MaxX != -1)
			TileLengthX = MaxX;
		int TileLengthY = MaxY;

		NewTileMap = new Texture2D(PixelResolution*TileLengthX,PixelResolution*TileLengthY);
		NewTileMap.filterMode = FilterMode.Point;
		//Debug.Log ("Created a new tilemap texture:");
		//Debug.Log ("Created a new tilemap texture - Resolution of Pixels: " + PixelResolution.ToString() + " with a tile length of " + TileLengthX.ToString());
		//Debug.Log ("Dimensions of- x: " + TileMap.texelSize.ToString() + " CalcCheck: " + (TileLength*PixelResolution).ToString ());
		
		int tileLocationX = 0;
		int tileLocationY = 0;
		
		Color32[] TileColors = NewTileMap.GetPixels32 (0);
		//Debug.Log ("Textures Adding " + BlockTextures.Count + "  to tileMap. ");
		for (int z = 0; z < TiledTextures.Count; z++) {
			if (TiledTextures[z]) {
				//Debug.Log (z.ToString() + " BlockTextures: " + BlockTextures[z].name);
				//Debug.Log (z.ToString() + " Dimensions of- x: " + BlockTextures[z].texelSize.ToString());
				//Debug.Log ("ColorSize: " + BlockTextures[z].texelSize.ToString ());
				//BlockTextures[z] = new Texture2D (32, 32);	//MyRenderer.material.mainTexture);
				tileLocationX = (z / TileLengthX);
				// Every Column
				tileLocationY = (z % TileLengthX);
				Texture2D BlockTexture = TiledTextures[z];
				Color32[] BlockColors = BlockTexture.GetPixels32 (0);
				for (int i = 0; i < PixelResolution; i++)
				for (int j = 0; j < PixelResolution; j++) {
					int i2 = i+tileLocationX*Mathf.RoundToInt(PixelResolution);	// the size fot the tilemap
					int j2 = j+tileLocationY*Mathf.RoundToInt(PixelResolution);	// the size fot the tilemap
					
					int PixelIndex1 = Mathf.RoundToInt (i * PixelResolution + j);
					int PixelIndex2 = Mathf.RoundToInt (i2 * TileLengthX*PixelResolution + j2);
					if (PixelIndex1 < BlockColors.Length && PixelIndex2 < TileColors.Length)
						TileColors [PixelIndex2] = BlockColors [PixelIndex1];	//new Color32(BlockColors [PixelIndex1];
					//Debug.Log (PixelIndex2 + " :Colors: " + BlockColors [PixelIndex1].ToString ());
					//Debug.Log (PixelIndex2 + " :Colors2: " + BlockTexture.GetPixel(i,j).ToString());
				}
			}
		}
		
		NewTileMap.SetPixels32 (TileColors, 0);
		NewTileMap.Apply( true );
		if (FileName != "" &&  Application.platform != RuntimePlatform.WindowsWebPlayer) {

			byte[] data = NewTileMap.EncodeToPNG ();
			string MySaveFileName = FileName + ".png";
			//if (File.Exists(MySaveFileName))
			{
			//	File.Delete(MySaveFileName);
			}
			FileStream MyStream = System.IO.File.Create(MySaveFileName);
			MyStream.Close();
			//File.Create(MySaveFileName);
			File.WriteAllBytes (MySaveFileName, data);
			NewTileMap = Resources.Load(FileName, typeof(Texture2D)) as Texture2D;
		}
		return NewTileMap;
		//}
		//Debug.Log ("=============================");
		//Debug.Log ("Created TileMap.");
		//Debug.Log ("Dimensions of- x: " + TileMap.texelSize.ToString());
	}
}
