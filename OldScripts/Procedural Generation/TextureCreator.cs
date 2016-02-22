using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureCreator : MonoBehaviour {
	List<string> Commands = new List<string>();
	Color32 Tint;
	Vector3 TexSize;
	Vector3 MousePosition;
	bool bAddToMaterial;
	bool bIsFill;
	float GradientScale;
	Color32 OutlineColor;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Initialize() {
		TexSize = new Vector3(512, 512, 0);
		//TexData.Refresh(TexSize);
		Tint = new Color32(255, 255, 255, 255);
		//MouseSize = FVector(6, 6, 6);
		bAddToMaterial = true;
		//RefreshTexData(TexData);
		bIsFill = true; GradientScale = 10;
		OutlineColor = new Color32(0, 0, 0, 255);
		Commands.Add(("Fill"));
		Commands.Add(("Ecclipse"));
		Commands.Add(("Rectangle"));
		Commands.Add(("Triangle"));
		Commands.Add(("GradientPoint"));
		Commands.Add(("Offset"));
		Commands.Add(("Bricks"));
		Commands.Add(("Noise"));	// perlin noise or midplace
		Commands.Add(("Outline"));	// uses flood fill until it finds solid cells, then it adds a layer around all the solid ones
	}
}
/*
// the overall settings for the textures construction
USTRUCT(BlueprintType)
struct FTextureConstructor {
	GENERATED_USTRUCT_BODY()
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	TArray <FString> Instructions;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	TArray <FString> Commands;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bHasChanged;
	// the core of our texture
	FTexData2 TexData;
	// the dimensions of our texture
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector TexSize;
	// this determines whether it is added for material, else it can be used to create other textures
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bAddToMaterial;
	// whether a premade texture is laoded in
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsLoad;
	// if true, the texture renders gui items, and updates them when gui is udpated
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsGui;
	// Offsets the texture from the corner - used to make tiling better
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsOffset;
	// are bricks generated
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsBricks;
	
	// fills the texture with tint colour
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsFill;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsEllipse;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsCreateFace;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	AZeltexCharacter* DrawCharacter;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsGradient;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsPerlin;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsOutline;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FColor Tint;	// gets multiplied by every pixel
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector GradientPoint;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	float GradientScale;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FColor OutlineColor;	// gets multiplied by every pixel
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector OutlineSize;	// gets multiplied by every pixel
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector OffsetAmount;
	// the dimensions of each brick
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector BrickSize;
	// How far apart the bricks are spaced
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FVector BrickOffset;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FColor BrickColor;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsBlendEdges;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsGreyScale;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	bool bIsPixelated;
	// index of the loading textures list for when a texture is loaded in
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	int32 LoadTextureIndex;
	// These can be used for many things, but a texture will be saved as a combination of them
	// 0 for empty
	// 1 for add colour
	// 2 for
	// 3 for Add perlin noise
	// 
	//UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	//	TArray<FTextureInstruction> Instructions;
	// Buttons for our 3d gui
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	int32 GuiIndex;
	//UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	//	TArray<FGuiButton> Buttons;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	TArray<FPercentageBar> Bars;
	
	// Data for the Terrain Generation
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	FBiome BiomeData;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	int32 AnimationCount;
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Texture)
	int32 MaxAnimations;
	// how often the animation should update
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Animation)
	float UpdateTime;
	
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Gui3d)
	FVector MouseSize;

	void FillTexture(FTexData2& TexData, FColor Tint) {
		for (int32 x = 0; x < TexData.Data.Num(); x++) {
			for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
				//TexData[x].Data[y].RGBA = Tint;
				TexData.Update(Tint, x, y);
			}
		}
	}
	void CreateEllipse(FTexData2& TexData, FColor Tint, FVector BeginPosition, FVector Size) {
		for (int32 x = 0; x < TexData.Data.Num(); x++) {
			for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
				//if (FVector::Dist(FVector(BeginPosition.X, 0, 0), FVector(x, 0, 0)) <= Size.X && FVector::Dist(FVector(0, BeginPosition.Y, 0), FVector(0, y, 0)) <= Size.Y)
				if (FVector::Dist(BeginPosition, FVector(x,y*1.25f,0)) <= Size.X/2.0f)
					TexData.Update(Tint, x, y);
			}
		}
	}
	void CreateFace(FTexData2& TexData, TArray<float> genes) {
		
		float r = FMath::GetMappedRangeValue(FVector2D(genes[0], 0), FVector2D(1, 0), 70);
		FColor c = FColor(genes[1], genes[2], genes[3]);
		float eye_y = FMath::GetMappedRangeValue(FVector2D(genes[4], 0), FVector2D(1, 0), 5);
		float eye_x = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, 0), 10);
		float eye_size = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, 0), 10);
		FColor eyecolor = FColor(genes[4], genes[5], genes[6]);
		FColor mouthColor = FColor(genes[7], genes[8], genes[9]);
		float mouth_y = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, 0), 25);
		float mouth_x = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, -25), 25);
		float mouthw = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, 0), 50);
		float mouthh = FMath::GetMappedRangeValue(FVector2D(genes[5], 0), FVector2D(1, 0), 10);
		
		FillTexture(TexData, FColor(255,255,255));
		// Draw the head
		CreateEllipse(TexData, c, FVector(TexSize.X / 2, TexSize.Y / 2, 0), FVector(TexSize.X / 2, TexSize.Y / 2, 0));
		
		// draw the eyes
		
		// draw the mouth
		
		// draw the bounding box
		
		// display fitness value
		
	}
	void FillPerlinNoise(FTexData2& TexData) {
		GeneratedPerlinNoise perlin = GeneratedPerlinNoise(BiomeData.Persistence, BiomeData.Frequency, BiomeData.Amplitude, 
		                                                   BiomeData.Octaves, BiomeData.RandomSeed, TexSize, FVector(AnimationCount, AnimationCount, 0));
		for (int32 x = 0; x < TexData.Data.Num(); x++) {
			for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
				FColor NewColor = perlin.GetHeight(x, y) * TexData.GetColor(x, y);
				//TexData[x].Data[y].RGBA = perlin.GetHeight(x, y) * TexData[x].Data[y].RGBA;
				TexData.Update(NewColor, x, y);
			}
		}
	}
	
	void OffsetTexture(FTexData2& TexData) {
		//FVector TexSize;
		FTexData2 TempTexData;
		TempTexData.Refresh(TexSize);
		//RefreshTexData2(TempTexData);
		// Offset all our pixels by half of the size, so they tile
		FVector OffsetTex = OffsetAmount;
		//FVector OffsetTex = FVector(TexSize.X / 2.0f, TexSize.Y / 2.0f, 0);
		for (int32 x = 0; x < TexSize.X; x++) {
			for (int32 y = 0; y < TexSize.Y; y++) {
				int32 x2 = x + OffsetTex.X;
				int32 y2 = y + OffsetTex.Y;
				if (x2 >= TexSize.X) x2 -= TexSize.X;
				if (y2 >= TexSize.Y) y2 -= TexSize.Y;
				//TempTexData[x].Data[y].RGBA = TexData[x2].Data[y2].RGBA;
				TempTexData.Update(TexData.GetColor(x2, y2), x, y);
				//TempTexData[x].Data[y].RGBA = FColor(0, 0, 0, 255);
			}
		}
		// now replace our tex data with our temporary array
		for (int32 x = 0; x < TexSize.X; x++) {
			for (int32 y = 0; y < TexSize.Y; y++) {
				//TexData[x].Data[y].RGBA = TempTexData[x].Data[y].RGBA;
				TexData.Update(TempTexData.GetColor(x,y), x, y);
			}
		}
	}
	
	void GradientPoint2(FTexData2& TexData) {
		for (int32 x = 0; x < TexData.Data.Num(); x++) {
			for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
				//FVector DistFromMidPoint = FVector(FMath::Abs(x - TexSize.X / 2), FMath::Abs(y - TexSize.Y / 2), 0);
				FVector DistFromMidPoint = FVector(FMath::Abs(x - GradientPoint.X), FMath::Abs(y - GradientPoint.Y), 0);
				float AverageDimension = (TexData.Data.Num() + TexData.Data[x].Data.Num()) / 2.0f;
				float Dist = FVector::Dist(GradientPoint, DistFromMidPoint) / AverageDimension;
				Dist *= GradientScale;
				FColor NewColor = FColor(
					TexData.GetColor(x,y).R + Dist,
					TexData.GetColor(x, y).G + Dist,
					TexData.GetColor(x, y).B + Dist,
					TexData.GetColor(x, y).A);
				TexData.Update(NewColor, x, y);
			}
		}
	}
	
	void OutlineTexture(FTexData2& TexData) {
		for (int32 x = 0; x < TexData.Data.Num(); x++) {
			for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
				if (x <= OutlineSize.X || x >= TexSize.X - 1 - OutlineSize.X ||
				    y <= OutlineSize.Y || y >= TexSize.Y - 1 - OutlineSize.Y)
					//TexData[x].Data[y].RGBA = OutlineColor;
					TexData.Update(OutlineColor, x, y);
			}
		}
	}
	// so far all i've done is create tint textures
	void CreateTexture() {
		TexData.Refresh(TexSize);
		//RefreshTexData(TexData);
		if (bIsFill) {
			FillTexture(TexData, Tint);
		}
		if (bIsEllipse) {
			CreateEllipse(TexData, Tint, FVector(TexSize.X / 2, TexSize.Y / 2, 0), FVector(TexSize.X / 2, TexSize.Y / 2, 0));
		}
		if (bIsGradient) {
			GradientPoint2(TexData);
		}
		if (bIsCreateFace || DrawCharacter) {
			//CreateFace(TexData, DrawCharacter->genes);
		}
		if (bIsBricks) {
			int32 BricksCountX = TexSize.X / BrickSize.X;
			if (((int32)(TexSize.X) % (int32)(BrickSize.X)) != 0)	// if brick is not exactly fitting into the grid
				BricksCountX++;
			int32 BricksCountY = TexSize.Y / BrickSize.Y;
			if (((int32)(TexSize.Y) % (int32)(BrickSize.Y)) != 0)	// if brick is not exactly fitting into the grid
				BricksCountY++;
			for (int32 a = 0; a < BricksCountX; a++) {
				for (int32 b = 0; b < BricksCountY; b++) {
					for (int32 x = 0; x < BrickSize.X; x++) {
						for (int32 y = 0; y < BrickSize.Y; y++) {
							int32 x2 = x + a*BrickSize.X;
							int32 y2 = y + b*BrickSize.Y;
							if (x2 >= 0 && x2 < TexSize.X && y2 >= 0 && y2 < TexSize.Y)
								//TexData[x2].Data[y2].RGBA = BrickColor;
								TexData.Update(BrickColor, x2, y2);
							if (x == 0 || x == BrickSize.X - 1 || y == y || y == BrickSize.Y - 1)
								//TexData[x2].Data[y2].RGBA = FColor(BrickColor.R / 2.0f, BrickColor.G / 2.0f, BrickColor.B / 2.0f, BrickColor.A);
								TexData.Update(FColor(BrickColor.R / 2.0f, BrickColor.G / 2.0f, BrickColor.B / 2.0f, BrickColor.A), x2, y2);
						}
					}
				}
			}	
		}
		if (bIsPerlin){
			FillPerlinNoise(TexData);
		}
		
		if (bIsOutline) {
			OutlineTexture(TexData);
		}
		if (bIsBlendEdges) {
			FVector EdgeLimit = FVector(10, 10, 0);
			for (int32 x = 0; x < TexSize.X; x++) {
				for (int32 y = 0; y < TexSize.Y; y++) {
					FColor NewColor = TexData.GetColor(x,y);
					if (x <= EdgeLimit.X || x >= TexSize.X - 1 - EdgeLimit.X) {
						NewColor += TexData.Data[FMath::Abs(x - TexSize.X + 1)].Data[y].RGBA;
						NewColor = FColor(NewColor.R / 2, NewColor.R / 2, NewColor.R / 2, NewColor.A / 2);
					}
					if (y <= EdgeLimit.Y || y >= TexSize.Y - 1 - EdgeLimit.Y) {
						NewColor += TexData.Data[x].Data[FMath::Abs(y - TexSize.Y + 1)].RGBA;
						NewColor = FColor(NewColor.R / 2, NewColor.R / 2, NewColor.R / 2, NewColor.A / 2);
					}
					if (x <= EdgeLimit.X || x >= TexSize.X - 1 - EdgeLimit.X || y <= EdgeLimit.Y || y >= TexSize.Y - 1 - EdgeLimit.Y) {
						//TexData[x].Data[y].RGBA = NewColor;
						TexData.Update(NewColor, x, y);
					}
				}
			}
		}
		if (bIsOffset) {
			OffsetTexture(TexData);
		}
		
		bool bIsGlow = false;
		if (bIsGlow) {
			float GlowRate = 0.5f;
			FVector multColor = FVector(1 - AnimationCount*GlowRate, 1 - AnimationCount*GlowRate, 1 - AnimationCount*GlowRate);
			for (int32 x = 0; x < TexData.Data.Num(); x++) {
				for (int32 y = 0; y < TexData.Data[x].Data.Num(); y++) {
					//TexData[x].Data[y].RGBA = FColor(TexData[x].Data[y].RGBA.R*multColor.X, TexData[x].Data[y].RGBA.G*multColor.Y, TexData[x].Data[y].RGBA.B*multColor.Z, 255);
					FColor NewColor = FColor(TexData.GetColor(x, y).R*multColor.X, TexData.GetColor(x, y).G*multColor.Y, TexData.GetColor(x, y).B*multColor.Z, 255);
					TexData.Update(NewColor, x, y);
				}
			}
		}
		
		// add noise
		// Mult noise?
		//MyColor = FColor(FMath::Rand() * 255, FMath::Rand() * 255, FMath::Rand() * 255, 255);
		//MyColor = FColor(255 * ColorTint[i].X, 255 * ColorTint[i].Y, 255 * ColorTint[i].Z, 255);
	}
	
	void InputGui(FVector VirtualMouse, bool MouseLeft, bool MouseRight, TArray<FGuiButton>& Buttons) {
		if (bIsGui) {
			bHasChanged = false;
			FVector MouseRelativPosition = FVector(VirtualMouse.X*TexSize.X, VirtualMouse.Y*TexSize.Y, VirtualMouse.Z*TexSize.Z);
			for (int32 i = 0; i < Buttons.Num(); i++) {
				bool PreviousIsSwitched = Buttons[i].IsSwitched;
				bool PreviousIsMouseOver = Buttons[i].IsMouseOver;
				//Buttons[i].IsMouseOver = false;
				if (VirtualMouse.X != -1 && VirtualMouse.Y != -1) {
					if (MouseRelativPosition.X > Buttons[i].Position.X - Buttons[i].Size.X && MouseRelativPosition.X < Buttons[i].Position.X + Buttons[i].Size.X &&
					    MouseRelativPosition.Y > Buttons[i].Position.Y - Buttons[i].Size.Y && MouseRelativPosition.Y < Buttons[i].Position.Y + Buttons[i].Size.Y) {
						
						if (MouseLeft) {
							Buttons[i].IsSwitched = true;// !Buttons[i].IsSwitched;
						}
						//else {
						//	Buttons[i].IsSwitched = false;
						//}s
						Buttons[i].IsMouseOver = true;
					}
					else {
						Buttons[i].IsMouseOver = false;
					}
				}
				else {
					Buttons[i].IsMouseOver = false;
				}
				if (PreviousIsSwitched != Buttons[i].IsSwitched
				    || (PreviousIsMouseOver != Buttons[i].IsMouseOver)) {	// if button is switched, mouseover doesnt matter
					bHasChanged = true;
				}
				
			}
		}
	}
	
	void UpdateTexture(FVector VirtualMouse, bool MouseLeft, bool MouseRight, TArray<FTextureConstructor> TexConstructors, TArray<FLetter> Letters, TArray<FGuiButton>& Buttons) {
		if (bIsGui) {
			FVector MouseRelativPosition = FVector(VirtualMouse.X*TexSize.X, VirtualMouse.Y*TexSize.Y, VirtualMouse.Z*TexSize.Z);
			// Render all the buttons
			// Render them differently if mouse is over
			for (int32 i = 0; i < Buttons.Num(); i++) {
				Buttons[i].RenderTexture(TexData, TexConstructors, Letters);
				if (Buttons[i].bIsTexture)
				if (Buttons[i].TextureId >= 0 && Buttons[i].TextureId < TexConstructors.Num()) {
					for (int32 x = Buttons[i].Position.X - Buttons[i].Size.X; x < Buttons[i].Position.X + Buttons[i].Size.X; x++) {
						if (x > -1 && x < TexData.Data.Num())	// if X is within parameters
						for (int32 y = Buttons[i].Position.Y - Buttons[i].Size.Y; y < Buttons[i].Position.Y + Buttons[i].Size.Y; y++) {
							if (y > -1 && y < TexData.Data[x].Data.Num()) {// if Y is within parameters
								int32 a = x - Buttons[i].Position.X + Buttons[i].Size.X;
								int32 b = y - Buttons[i].Position.Y + Buttons[i].Size.Y;
								if (a >= 0 && b >= 0 && a < TexConstructors[Buttons[i].TextureId].TexData.Data.Num())
									if (b < TexConstructors[Buttons[i].TextureId].TexData.Data[a].Data.Num())
										//TexData[x].Data[y].RGBA = TexConstructors[Buttons[i].TextureId].TexData[a].Data[b].RGBA;
										TexData.Update(TexConstructors[Buttons[i].TextureId].TexData.GetColor(a, b), x, y);
							}
						}
					}
				}
			}
		}
	}
};*/