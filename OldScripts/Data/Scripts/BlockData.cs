using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlockData {
	public string Name;
	public int ModelId;	// the type of model it is. 
	public int TextureId;	// which texture map it is
	// 0 for Blocks. 
	// 1 for smoothed terrain
	public IconType DropType;
	public int DropIndex;
	public int DropQuantity;
	
	public int FunctionId;	// the type of function performed when block is activated
	// 0 for nothing
	// 1 for disapearing
	// 2 for spawning monster
	// 3 for shooting a spell
	
	public int OnDestroyFunctionId;
	// 0 for nothing
	// 1 for destroy building - destroys all blocks in zone
	public bool DoesActivateOnClick;
	public bool DoesActivateOnStep;
	public Vector3 ShootingDirection;	// when shooting a spell, which direction will it face
	public Stats BlockStats;	// default stats - when hit or activated will spawn stats for the block - eg turretsq

	public bool CanBeRemoved = true;
	public float Health = 100f;
	public float Defence = 100f;
	
	public AudioClip RemovedSoundEffect;
	public AudioClip PlaceSoundEffect;

	public bool IsDeformed = false;

	public BlockData() {
		DropQuantity = 1;
		DropType = IconType.Item;
		Name = "Block X";
	}
	public BlockData(int NewBlockType) {
		DropIndex = NewBlockType;
		DropQuantity = 1;
		DropType = IconType.Item;
		Name = "Block" + NewBlockType; 
		TextureId = NewBlockType;
	}
	
};
