using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class TextureManager : MonoBehaviour {
	public List<Material> MyTerrainMaterial = new List<Material>();
	public Material MyTerrainMaterial2;
	public bool IsBumpMapping = false;
	public bool IsEmissive = false;
	public List<Texture2D> BlockTextures = new List<Texture2D>();
	public Texture2D TileMap;
	public List<Texture> BlockNormalMapTextures = new List<Texture>();
	public Texture2D NormalsTileMap;
	public List<Texture> EmmissiveMapTextures = new List<Texture>();
	public Texture2D EmmissiveTileMap;
	public int TileLength = 8;
	public int PixelResolution = 32;
	public bool DoGenerateTileMap = false;
	public bool IsRawImage = false;
	public string TextureName = "NewTexture";
	public bool IsSaveToPNG = false;		// does not save on phone
	// Use this for initialization
	void Start () {
		if (DoGenerateTileMap) {
			DoGenerateTileMap = false;
			GenerateTileMap();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (DoGenerateTileMap) {
			DoGenerateTileMap = false;
			GenerateTileMap();
		}
	}

	public void GenerateTileMap() {
		TileMap = CreateTileMap (TextureName, BlockTextures, TileMap);
		for (int i = 0; i < MyTerrainMaterial.Count; i++) {
			MyTerrainMaterial[i].SetTexture (0, TileMap);
		}
		if (MyTerrainMaterial2 != null)
			MyTerrainMaterial2.SetTexture (0, TileMap);
		if (IsBumpMapping) {
			//NormalsTileMap = CreateTileMap (TextureName + "2", BlockNormalMapTextures, NormalsTileMap);
			//MyTerrainMaterial.SetTexture ("_BumpMap", NormalsTileMap);
		}
		if (IsEmissive) {
			//EmmissiveTileMap = CreateTileMap (TextureName + "3", EmmissiveMapTextures, EmmissiveTileMap);
			//MyTerrainMaterial.SetTexture ("_Emissive", NormalsTileMap);
		}
		if (IsRawImage) 
		{
			gameObject.GetComponent<RawImage> ().texture = TileMap;
		}
	}

	// converts the textures to a lower size
	public void LowerTextureQuality(int TextureDivision) {

	}
	// grabs all the textures and chucks them into a tile map
	public Texture2D CreateTileMap(string FileName, List<Texture2D> TiledTextures, Texture2D NewTileMap) {
		NewTileMap = new Texture2D(TileLength*PixelResolution,TileLength*PixelResolution);
		NewTileMap.filterMode = FilterMode.Point;
		//Debug.Log ("Created a new tilemap texture:");
		Debug.Log ("Created a new tilemap texture - Resolution of Pixels: " + PixelResolution.ToString() + " with a tile length of " + TileLength.ToString());
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
				tileLocationX = (z / TileLength);
				// Every Column
				tileLocationY = (z % TileLength);
				Texture2D BlockTexture = TiledTextures[z];
				Color32[] BlockColors = BlockTexture.GetPixels32 (0);
				for (int i = 0; i < PixelResolution; i++)
				for (int j = 0; j < PixelResolution; j++) {
					int i2 = i+tileLocationX*Mathf.RoundToInt(PixelResolution);	// the size fot the tilemap
					int j2 = j+tileLocationY*Mathf.RoundToInt(PixelResolution);	// the size fot the tilemap

					int PixelIndex1 = Mathf.RoundToInt (i * PixelResolution + j);
					int PixelIndex2 = Mathf.RoundToInt (i2 * TileLength*PixelResolution + j2);
					if (PixelIndex1 < BlockColors.Length && PixelIndex2 < TileColors.Length)
					TileColors [PixelIndex2] = BlockColors [PixelIndex1];	//new Color32(BlockColors [PixelIndex1];
					//Debug.Log (PixelIndex2 + " :Colors: " + BlockColors [PixelIndex1].ToString ());
					//Debug.Log (PixelIndex2 + " :Colors2: " + BlockTexture.GetPixel(i,j).ToString());
				}
			}
		}
		
		NewTileMap.SetPixels32 (TileColors, 0);
		NewTileMap.Apply( true );
		if (IsSaveToPNG &&  Application.platform != RuntimePlatform.WindowsWebPlayer) {
			//byte[] data = NewTileMap.EncodeToPNG ();
			//File.WriteAllBytes (FileName + ".png", data);
		}
		return NewTileMap;
		//}
		//Debug.Log ("=============================");
		//Debug.Log ("Created TileMap.");
		//Debug.Log ("Dimensions of- x: " + TileMap.texelSize.ToString());
	}

	/*
	public static bool GetImageSize(Texture2D asset, out int width, out int height) {
        if (asset != null) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
     
            if (importer != null) {
                object[] args = new object[2] { 0, 0 };
                MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(importer, args);
     
                width = (int)args[0];
                height = (int)args[1];
     
                return true;
            }
        }
     
        height = width = 0;
        return false;
    }
*/
}
