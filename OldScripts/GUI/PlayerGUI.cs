using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// uses the data stored its accompanying gameobject(and its children) to update data slots -
//			like Player Health, Or enemy Health, or Selected Item position

// this class mostly deals with the players own gui - interfaces between player data and the gui elements
// it also now accompanies any character inventory/stats that are opened
namespace OldCode {
public class PlayerGUI : MonoBehaviour {
	// references to managers
	public BaseCharacter MyPlayer;
	public BaseCharacter MyOriginalPlayer;
	public GameObject EffectGuiPrefab;
	//private GameObject MyHealthBar;
	//private GameObject MyHealthBarBackground;
	public bool IsLocalPlayer = false;
	public bool IsTargetOfPlayer = false;
	private bool IsHiddenGui = false;

	// Use this for initialization
	void Start () {
		// first set references
		MyPlayer = gameObject.GetComponent <Player>();
	}

	// Update is called once per frame
	void Update () {
		if (IsTargetOfPlayer && MyPlayer) {
			if (MyOriginalPlayer.SelectedPlayer == null) {
				MyPlayer = null;
			}
		}
		if (MyPlayer != null) {
			ShowGui();
			if (!MyPlayer.IsAlive ()) {
				//MyGui.EndGame.SetActive (true);	// this should be done from EndGame in GameManager
			} else {
				UpdateEffectsGui ();
				UpdateTexts (MyPlayer);	// updates the player stats
			}
		} else {
			HideGui();
			if (IsLocalPlayer) 
			{
				UpdateCharacter(GetManager.GetCharacterManager().GetLocalPlayer());
			}
		}
	}

	public void ToggleChildren(bool IsActive) {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild(i).gameObject.SetActive(IsActive);
		}
	}

	public void ShowGui() {
		if (IsHiddenGui) {
			ToggleChildren(true);
			IsHiddenGui = false;
		}
	}

	public void HideGui() {
		if (!IsHiddenGui) {
			ToggleChildren(false);
			IsHiddenGui = true;
		}
	}
	public void UpdateCharacter(BaseCharacter NewCharacter) {
		MyPlayer = NewCharacter;
		MyOriginalPlayer = NewCharacter;
		if (IsTargetOfPlayer && MyOriginalPlayer) {
			if (MyOriginalPlayer.SelectedPlayer) {
				MyPlayer = MyOriginalPlayer.SelectedPlayer.GetComponent<BaseCharacter>();
			} else {
				MyPlayer = null;
			}
		}
	}
	
	public void OnRenderObject () {
		RenderSelectedCube ();
	}

	// Will be called after all regular rendering is done
	public void RenderSelectedCube ()
	{
		if (MyPlayer)
		if (MyPlayer.IsHitBlocks)
		if (MyPlayer.SelectedBlockModel != null) 
		if (MyPlayer.MyInventory.GetSelectedItem() != null)
		{
			float Direction = 1;
			float NormalScale = 0.5f;	// this should be half of block size not just .5
			if (MyPlayer.MyInventory.GetSelectedItem().MyItemType == ItemType.Tool) {
				Direction = -1;
			} else if (MyPlayer.MyInventory.GetSelectedItem().MyItemType == ItemType.Block) {
				Direction = 1;
			}
			int x = Mathf.RoundToInt (MyPlayer.MouseHit.point.x +  Direction*MyPlayer.MouseHit.normal.x * NormalScale);
			int y = Mathf.RoundToInt (MyPlayer.MouseHit.point.y +  Direction*MyPlayer.MouseHit.normal.y * NormalScale);
			int z = Mathf.RoundToInt (MyPlayer.MouseHit.point.z +  Direction*MyPlayer.MouseHit.normal.z * NormalScale);
			DebugShapes.DrawCube (new Vector3(x,y,z), new Vector3 (0.5f, 0.5f, 0.5f), Color.black);
		}
	}

	// modifies the positioning of the placement mesh
	public void SetBlockModelValues(bool IsActive, ItemType MyItemType) {
		if (MyPlayer.SelectedBlockModel != null) {
			MyPlayer.SelectedBlockModel.gameObject.SetActive (IsActive);
			if (IsActive) {
				float Direction = 1;
				Vector3 MySize = new Vector3(1,1,1);
				if (MyItemType == ItemType.Tool) {
					Direction = -1;
					MyPlayer.SelectedBlockModel.transform.localScale = new Vector3(1.1f,1.1f,1.1f);	// Select Around the block
				} else if (MyItemType == ItemType.Block) {
					Direction = 1;
					MyPlayer.SelectedBlockModel.transform.localScale = new Vector3(1f,1f,1f);
				} else if (MyItemType == ItemType.BlockStructure) {
					Direction = 1;
					MySize = MyPlayer.MyInventory.GetSelectedBlockStructure().MyBlocks.Size;
					//MySize *= 0.5f;
					MyPlayer.SelectedBlockModel.transform.localScale = MySize*1.025f;
				}
				float NormalScale = 0.5f;	// this should be half of block size not just .5
				int x = Mathf.RoundToInt (MyPlayer.MouseHit.point.x +  Direction*MyPlayer.MouseHit.normal.x * NormalScale);
				int y = Mathf.RoundToInt (MyPlayer.MouseHit.point.y +  Direction*MyPlayer.MouseHit.normal.y * NormalScale);
				int z = Mathf.RoundToInt (MyPlayer.MouseHit.point.z +  Direction*MyPlayer.MouseHit.normal.z * NormalScale);
				if (MyItemType == ItemType.BlockStructure) {
					//x += Mathf.RoundToInt(MySize.x/2f);
					y += Mathf.RoundToInt(MySize.y/2f);
					//z += Mathf.RoundToInt(MySize.z/2f);
				}
				MyPlayer.SelectedBlockModel.transform.position = new Vector3 (x, y, z);
			}
			//MyPlayer.SelectedBlockModel.transform.localScale = MyPlayer.blockStructure.MyBlocks.Size;
			//SelectedBlockModel.transform.position = HitBlocksTransform.position + PlaceBlock;
		}
	}

	public void UpdateEffectsGui() {
		if (MyPlayer.MyStats.HasEffectsChanged) {
			List<GameObject> MyEffectsGui = new List<GameObject>();
			for (int i = 0; i < gameObject.transform.childCount; i++) {
				if (gameObject.transform.GetChild(i).name.Contains("Effect")) {
					MyEffectsGui.Add (gameObject.transform.GetChild(i).gameObject);
				}
			}
			// should update the gui for the amount of icons - or have a ... sign at the end if list too long?
			for (int i = 0; i < MyEffectsGui.Count; i++) {
				// for all thee effects the player has - udpate the icons
				if (i < MyPlayer.MyStats.EffectsList.Count){
					MyEffectsGui [i].gameObject.SetActive(true);
					MyEffectsGui [i].GetComponent<RawImage>().texture = MyPlayer.MyStats.EffectsList [i].MyTexture;
				} else {
					MyEffectsGui [i].gameObject.SetActive(false);
				}
			}
			//Debug.LogError ("Updated player gui: " + MyPlayer.name);
			//Debug.LogError ("With: " + MyGui.MyEffects.Count + " gui effects and " + MyPlayer.MyStats.EffectsList.Count + " player effects.");
			MyPlayer.MyStats.HasEffectsChanged = false;
		}
	}
	// gui stuff - should only update when data changes!
	void UpdateTexts(BaseCharacter MyPlayer) {
		//MyGui.MyGoldText.text = "Gold: " + MyPlayer.Gold.ToString ();
		Item SelectedItemObject = null;
		//if (MyPlayer.MyInventory.GetIcon(MyPlayer.SelectedItem).MyIconType == IconType.Item)
		//	SelectedItemObject = MyPlayer.MyInventory.GetItem (MyPlayer.SelectedItem);
		//else
		//	SelectedItemObject = null;

		if (SelectedItemObject != null) {
			if (SelectedItemObject.MyItemType == ItemType.Block || SelectedItemObject.MyItemType == ItemType.Tool || SelectedItemObject.MyItemType == ItemType.BlockStructure)
				SetBlockModelValues (true, SelectedItemObject.MyItemType);
		} 
		else 
			SetBlockModelValues (false,0);
		
		Spell SelectedSpellObject = null;
		//if (MyPlayer.MyInventory.GetIcon(MyPlayer.SelectedItem).MyIconType == IconType.Spell)
		//	SelectedSpellObject = MyPlayer.MyInventory.GetSpell (MyPlayer.SelectedItem);
		//else
		//	SelectedSpellObject = null;
		
		if (SelectedItemObject != null) {
			//MyGui.DebugItemNameType.text = "Item: " + SelectedItemObject.Name + " && Type: " + MyData.IconsList [MyPlayer.SelectedItem].Index;
			//MyGui.DebugItemQuantity.text = "   Quantity: " + SelectedItemObject.Quantity;
			//MyGui.DebugItemBlockIndex.text = "   BlockIndex: " + SelectedItemObject.BlockIndex;
			//MyGui.DebugItemModelId.text = "   ModelId: " + SelectedItemObject.ModelId;
		} 
		if (SelectedSpellObject != null) {
			//MyGui.DebugItemNameType.text = "Spell: " + SelectedSpellObject.Name + " && Type: " + MyData.IconsList [MyPlayer.SelectedItem].Index;
			//MyGui.DebugItemQuantity.text = "   Damage: " + SelectedSpellObject.Damage;
			//MyGui.DebugItemBlockIndex.text = "   " + SelectedSpellObject.StatType + ": " + SelectedSpellObject.ManaCost;
			//MyGui.DebugItemModelId.text = "   TravelSpeed: " + SelectedSpellObject.TravelSpeed;
			//MyGui.DebugRow5.text = "   DamageType: " + DamageTypeToString(SelectedSpellObject.MyDamageType);
			//MyGui.DebugRow6.text = "   Animation: " + SelectedSpellObject.AnimationType;
			//MyGui.DebugRow6.text = "   Animation: " + SelectedSpellObject.AnimationType;
		}
		if (MyPlayer.IsHitBlocks) {
			Block SelectedBlock =  MyPlayer.SelectedBlock; //(Block) Terrain.GetBlock (MyPlayer.MouseHit, false);
			//Block SelectedBlock = (Block)MyBlockBase;
			//if (MyPlayer.SelectedBlock != null) 
			if (SelectedBlock != null) {
				//BlockData SelectedBlockData = MyData.GetBlockData (SelectedBlock.GetBlockIndex ());
				// set debug selected block data variables
				//MyGui.DebugBlockType.text = "Block Type: " + SelectedBlock.GetBlockIndex () + " : " + SelectedBlockData.Name;
				//if (MyGui.DebugBlockType != null && MyPlayer.MouseHit)
				//MyGui.DebugBlockType.text = "Block Position: " + MyPlayer.MouseHit.transform.position.x + " : " + 
				//	MyPlayer.MouseHit.transform.position.y + " : " + MyPlayer.MouseHit.transform.position.z;
				//MyGui.DebugBlockLighting.text = "BlockColor: Red: " + SelectedBlock.MyColor.r + " Green: "  + SelectedBlock.MyColor.g  + " Blue: "  + SelectedBlock.MyColor.b   + " Alpha: "  + SelectedBlock.MyColor.a;
				//MyGui.DebugBlockActivated.text = "Block Activated: " + SelectedBlock.IsActivated;
				//MyGui.DebugBlockModelId.text = "Block Model: " + SelectedBlockData.ModelId;

			} else {
				NoBlockSelectedText();
			}
		} else {
			NoBlockSelectedText();
		}
		/*if (MyPlayer.SelectedPlayer != null) {
			BaseCharacter enemy = (BaseCharacter)(MyPlayer.SelectedPlayer.GetComponent ("BaseCharacter"));
			BotMovement enemyMovement = (BotMovement)(MyPlayer.SelectedPlayer.GetComponent ("BotMovement"));
			if (enemy != null && enemyMovement != null) {
				ToggleEnemyStats(true);
				//MyGui.DebugEnemyBehaviourText.text = BehaviourToString(enemyMovement.MyBehaviour);
				//MyGui.DebugEnemyBehaviourText.text = AnimationStateToString(enemy.MyAnimationState);
				//MyGui.EnemyNameText.text = 
				MyGui.EnemyLevelText.text = enemy.name;
				//MyGui.EnemyNameText.text = " Lvl: " + Mathf.Round (enemy.MyStats.MyLevel.Level);
				MyGui.EnemyText.text = " Health: " + Mathf.Round (enemy.MyStats.GetStat ("Health")); 
				MyGui.EnemeyHealthBar.sizeDelta = new Vector2 (Mathf.Round (enemy.MyStats.GetPercentage ("Health") * 100f), 20);
				MyGui.EnemyManaText.text = " Mana: " + Mathf.Round (enemy.MyStats.GetStat ("Mana")); 
				MyGui.EnemeyManaBar.sizeDelta = new Vector2 (Mathf.Round (enemy.MyStats.GetPercentage ("Mana") * 100f), 20);
				MyGui.EnemyEnergyText.text = " Energy: " + Mathf.Round (enemy.MyStats.GetStat ("Energy")); 
				MyGui.EnemeyEnergyBar.sizeDelta = new Vector2 (Mathf.Round (enemy.MyStats.GetPercentage ("Energy") * 100f), 20);
			} else {
				ToggleEnemyStats (false);
			}
		} else {
			//MyGui.EnemyText.text = MyPlayer.MouseOverZone.name;
			ToggleEnemyStats (false);
		}*/
		if (gameObject.transform.FindChild ("HealthBackground")) {
			Text HealthText = gameObject.transform.FindChild ("HealthBackground").FindChild ("HealthText").gameObject.GetComponent<Text> ();
			RectTransform HealthBar = gameObject.transform.FindChild ("HealthBackground").FindChild ("HealthBar").gameObject.GetComponent<RectTransform> ();
			HealthText.text = "HP [" + Mathf.Round (MyPlayer.MyStats.GetStat ("Health")) + "]";
			HealthBar.sizeDelta = new Vector2 (Mathf.Round (MyPlayer.MyStats.GetPercentage ("Health") * 100f), 20); 
		}
		
		if (gameObject.transform.FindChild ("ManaBackground")) {
			Text ManaText = gameObject.transform.FindChild ("ManaBackground").FindChild ("ManaText").gameObject.GetComponent<Text> ();
			RectTransform ManaBar = gameObject.transform.FindChild ("ManaBackground").FindChild ("ManaBar").gameObject.GetComponent<RectTransform> ();
			ManaText.text = "M [" + Mathf.Round (MyPlayer.MyStats.GetStat ("Mana")) + "]";
			ManaBar.sizeDelta = new Vector2(  Mathf.Round (MyPlayer.MyStats.GetPercentage ("Mana") * 100f), 20); 
		}
		if (gameObject.transform.FindChild ("EnergyBackground")) {
			Text EnergyText = gameObject.transform.FindChild ("EnergyBackground").FindChild ("EnergyText").gameObject.GetComponent<Text> ();
			RectTransform EnergyBar = gameObject.transform.FindChild ("EnergyBackground").FindChild ("EnergyBar").gameObject.GetComponent<RectTransform> ();
			EnergyText.text = "E [" + Mathf.Round (MyPlayer.MyStats.GetStat ("Energy")) + "]";
			EnergyBar.sizeDelta = new Vector2 (Mathf.Round (MyPlayer.MyStats.GetPercentage ("Energy") * 100f), 20); 
		}
		Text ExperienceText = gameObject.transform.FindChild ("ExperienceBarBackground").FindChild ("ExperienceText").gameObject.GetComponent<Text> ();
		RectTransform ExperienceBar = gameObject.transform.FindChild ("ExperienceBarBackground").FindChild ("ExperienceBar").gameObject.GetComponent<RectTransform> ();
		Text LevelText = gameObject.transform.FindChild ("LevelBackground").FindChild ("LevelText").gameObject.GetComponent<Text> ();

		LevelText.text = " LVL [" + Mathf.Round (MyPlayer.MyStats.MyLevel.Level) + "]";
		ExperienceText.text = "XP [" + Mathf.Round (MyPlayer.MyStats.MyLevel.ExperiencePoints) + " / " + Mathf.Round (MyPlayer.MyStats.MyLevel.ExperienceRequired) + "]";
		ExperienceBar.sizeDelta = 
			new Vector2( Mathf.Round (MyPlayer.MyStats.MyLevel.GetPercentage ()* ExperienceBar.parent.gameObject.GetComponent<RectTransform>().sizeDelta.x), 
			            ExperienceBar.sizeDelta.y); 

	}

	void NoBlockSelectedText() {
		//MyGui.DebugBlockType.text = "No Block Selected.";
		//MyGui.DebugBlockLighting.text = "";
		//MyGui.DebugBlockActivated.text = "";
		//MyGui.DebugBlockModelId.text = "";
		if (MyPlayer.SelectedBlockModel != null)
			MyPlayer.SelectedBlockModel.gameObject.SetActive (false);
		else
			Debug.Log ("No SelectedBlockModel, prez fixes.");
	}

	// put this in the 'characterGui' class - will be attached to any characters where you want to display their gui
	// :: Health Bars Above Heads ::
	// here i wanted health bars that billboard the camera... still working on it

	// called if the character dies
	// need a fader effect on them
	/*public void DeActivateHealthBars() {
		if (MyHealthBar != null)
			MyHealthBar.SetActive (false);
		if (MyHealthBarBackground != null)
			MyHealthBarBackground.SetActive (false);
	}*/
	// Need to get them to rotate properly at player
	/*public void UpdateHealthBarAboveHead() {
		if (MyHealthBar != null) {//*MyHealthBar.transform.localScale.x
			//if (MyHealthBarBackground != null)
			//	MyHealthBar.transform.localPosition = MyHealthBarBackground.transform.localPosition;
			// x needs to be from 0 to 0.5f
			float xSize = 0.5f;
			float xScale = (xSize * MyPlayer.MyStats.GetPercentage ("Health"));
			float xLocalPos = (xSize / 2f) - (xSize / 2f) *  MyPlayer.MyStats.GetPercentage ("Health");
			Vector3 MyAngle = new Vector3 (xLocalPos, 0, 0);
			MyAngle.x = MyAngle.x * transform.localEulerAngles.x;
			xLocalPos += MyAngle.x;
			
			MyHealthBar.transform.localScale = new Vector3 (xScale, MyHealthBar.transform.localScale.y, MyHealthBar.transform.localScale.z);
			MyHealthBar.transform.localPosition = new Vector3 (xLocalPos, MyHealthBar.transform.localPosition.y, MyHealthBar.transform.localPosition.z);
			
			MyHealthBar.transform.LookAt (Camera.main.transform.position);
			MyHealthBar.transform.localEulerAngles += new Vector3(90,0,0);
			if (MyHealthBarBackground != null) {
				MyHealthBarBackground.transform.LookAt (Camera.main.transform.position);
				MyHealthBarBackground.transform.localEulerAngles += new Vector3(90,0,0);
				//MyHealthBarBackground.transform.localPosition = 
				//	new Vector3(MyHealthBar.transform.localPosition.x+xLocalPos/2, MyHealthBarBackground.transform.localPosition.y,MyHealthBarBackground.transform.localPosition.z);
			}
		}
	}*/
}
}
