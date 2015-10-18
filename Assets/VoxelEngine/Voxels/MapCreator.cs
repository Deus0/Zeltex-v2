using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// don't worry about the character on the map
// my position, and any other characters will be texture overlays

// takes the terrain top most blocks
// rends a pixel colour for each block
// can probaly do this while rendering the textures of each block later
// needs to load in the pixels of each chunk

public class MapCreator : MonoBehaviour {
	public int MaxMipMapLevel;
	public RawImage MyRawImage;
	public RawImage MyRawImage2;
	public List<Color32> BlockColors = new List<Color32> ();
	public World InWorld;
	public Vector3 InChunkPosition;
	public Vector2 MapSize;
	public Vector3 MyStartLocation;
	
	public float CoolDown = 0;
	public float LastUpdate = 0;
	Blocks InsideBlocks = new Blocks();
	public bool IsCircular = true;
	int MipMapLevel = 0;
	public Color32[] NewColors;
	public Texture2D MyTexture2D;
	public bool CanUploadTexture = false;

	public GameObject MyCharacterImage;
	// references
	public GameObject MyPlayer;
	//MyStartLocation = GetManager.GetCharacterManager().GetLocalPlayer().gameObject;

	// Use this for initialization
	void Start () {
		SetDefaultColors ();
		InsideBlocks.Size = new Vector3(MapSize.x,1,MapSize.y);
		InsideBlocks.InitilizeData();
		MyTexture2D = new Texture2D (Mathf.RoundToInt (InsideBlocks.Size.x), Mathf.RoundToInt (InsideBlocks.Size.z));
		//MyTexture2D.Resize (Mathf.RoundToInt (InsideBlocks.Size.x), Mathf.RoundToInt (InsideBlocks.Size.z));
		MaxMipMapLevel = MyTexture2D.mipmapCount;
		MyTexture2D.filterMode = FilterMode.Point;
		MyRawImage2.gameObject.SetActive (true);
		MyCharacterImage.SetActive (true);
		UpdateMapOnThread ();
	}
	
	// Update is called once per frame
	void Update () {
		if (MyPlayer == null) {
			if (GetManager.GetCharacterManager ().GetLocalPlayer ())
			MyPlayer = GetManager.GetCharacterManager ().GetLocalPlayer ().gameObject;
		}
		if (InWorld == null) {
			InWorld = GetManager.GetWorld ();
		}
		if (MyPlayer && InWorld) {
			if (!CanUploadTexture && (CoolDown > 0 && Time.time - LastUpdate > CoolDown)) {
				LastUpdate = Time.time;
				UpdateMapOnThread();
			}
			UploadNewTexture ();
		}
	}
	public void UpdateMapOnThread() {
		if (UpdateMapPosition ()) {
			NewColors = MyTexture2D.GetPixels32 (MipMapLevel);
			UnityThreading.ActionThread NewThread = UnityThreadHelper.CreateThread (() => {
				UpdateMap ();
				ConvertBlocksToColors(InsideBlocks, 0);
				// thread processing
				CanUploadTexture = true;
			});
		}
	}

	public bool UpdateMapPosition() {
		if (MyPlayer == null)
			return false;
		Vector3 NewInChunkPosition = MyPlayer.transform.position;
		InChunkPosition = MyStartLocation;
		NewInChunkPosition.x = Mathf.RoundToInt(NewInChunkPosition.x);
		NewInChunkPosition.y = Mathf.RoundToInt(NewInChunkPosition.y);
		NewInChunkPosition.z = Mathf.RoundToInt(NewInChunkPosition.z);
		if (NewInChunkPosition != InChunkPosition) {
			InChunkPosition = NewInChunkPosition;
			return true;
		}
		return false;
	}
	public void UpdateMap() {
		for (int i = 0; i < MapSize.x; i++) {
			for (int j = 0; j < MapSize.y; j++) {
				Vector2 InBlockLocation = new Vector2(Mathf.RoundToInt(i+InChunkPosition.x-MapSize.x/2f),
				                                      Mathf.RoundToInt(j+InChunkPosition.z-MapSize.y/2f));
				int HighestBlock = 0;
				
				if (!IsCircular || (IsCircular && Vector2.Distance(new Vector2(i,j), new Vector2(InsideBlocks.Size.x/2f,InsideBlocks.Size.z/2f)) <= InsideBlocks.Size.x/2f)) {
					//HighestBlock = Mathf.RoundToInt(InChunkPosition.y);
					HighestBlock = Terrain.GetTopBlock(InBlockLocation, InWorld);
				}
				Block NewBlock = InWorld.GetBlock2 (Mathf.RoundToInt(InBlockLocation.x),
				                                    HighestBlock,
				                                    Mathf.RoundToInt(InBlockLocation.y));
				int BlockType = 62;
				if (NewBlock != null)
					BlockType = NewBlock.GetBlockIndex();
				InsideBlocks.UpdateBlock(new Vector3(i,0,j), BlockType);
			}
		}
	}
	public float outlineSize = 3;
	public void ConvertBlocksToColors(Blocks MyBlocks, float MapHeight) {
		for (int i = 0; i < MyBlocks.Size.x; i++)
		for (int k = 0; k < MyBlocks.Size.z; k++) {
			int PixelIndex = Mathf.RoundToInt (i * MyBlocks.Size.x + k);
			NewColors[PixelIndex] = BlockColors[MyBlocks.GetBlockType (new Vector3 (i, MapHeight, k))];
			if (IsCircular) {
				float DistanceToMid = Vector2.Distance(new Vector2(i,k), new Vector2(MyBlocks.Size.x/2f,MyBlocks.Size.z/2f));
				if (DistanceToMid > MyBlocks.Size.x/2f+outlineSize) {
					NewColors[PixelIndex] = new Color32(0,0,0,0);
				} else if (DistanceToMid > MyBlocks.Size.x/2f) {
					NewColors[PixelIndex] = new Color32(0,0,0,255);
				}
			}
		}
	}
	public void UploadNewTexture() {
		if (CanUploadTexture) {
			if (MyRawImage)
			MyRawImage.texture = (MyTexture2D);
			if (MyRawImage2)
			MyRawImage2.texture = (MyTexture2D);
			MyTexture2D.SetPixels32 (NewColors, MipMapLevel);
			MyTexture2D.Apply( true );
			CanUploadTexture = false;
		}
	}
	
	public void SetDefaultColors() {
		BlockColors.Clear ();
		BlockColors.Add (new Color32(125,165,165,255));
		BlockColors.Add (new Color32(70,50,15,255));	// dirt
		BlockColors.Add (new Color32(200,200,200,255));	// cobblestone
		BlockColors.Add (new Color32(55,255,255,255));	// grass
		BlockColors.Add (new Color32(255,255,0,255));		// sand - 4
		BlockColors.Add (new Color32(66,66,66,255));	// brick - 3 
		BlockColors.Add (new Color32(150,100,28,255));		// redSand? - 6
		BlockColors.Add (new Color32(255,0,0,255));		// redbrick? - 4
		BlockColors.Add (new Color32(69,10,100,255));		// Obsidian 8
		BlockColors.Add (new Color32(55,155,55,255));		// Hexgon brick - 4
		BlockColors.Add (new Color32(255,255,55,255));		// 1,0 brick - 4
		BlockColors.Add (new Color32(155,155,55,255));		// 1,0 brick - 4
		
		// Now fill in the rest with random colours
		for (int i = BlockColors.Count; i < 64; i++) {
			BlockColors.Add (new Color32((byte)(i*25),(byte)(i*15),(byte)(i*5),255));
		}
		BlockColors[62] = new Color32(15,15,15,255);		// Not loaded yet
		BlockColors[63] = new Color32(155,0,0,255);		// Where I am
	}
}

//int MeSize = 0;
//for (int i = -MeSize; i < MeSize+1; i++) 
//	for (int j = -MeSize; j < MeSize+1; j++) 
//		InsideBlocks.UpdateBlock(new Vector3(MapSize.x/2f+i,0,MapSize.y/2f+j), 63);