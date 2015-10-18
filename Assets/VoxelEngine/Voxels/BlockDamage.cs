using UnityEngine;
using System.Collections;
/*
	Upon recieving damage, a blockDamage class will be created
	It will render an animation for the damaged block
	when the health reaches 0, the block will be destroyed and an item released
	
 */
[System.Serializable]
public class BlockDamage {
	public float Health = 100.0f;
	public float MaxHealth = 100.0f;
	public float Defence = 100.0f;
	public bool IsBlockDead = false;
	public void Initialize() {
		MaxHealth = Health;
	}
}

// need to make it like this
// need to only hold the int to this 
public class BlockDamageGui : MonoBehaviour {
	public float Health = 100.0f;
	public float MaxHealth = 100.0f;
	public float Defence = 100.0f;
	public bool IsBlockDead = false;

	float LastRegen = 0;
	public Chunk MyChunk;
	public int x, y, z;
	public Block MyBlock;
	public GameObject LookAtObject;
	BlockDamage MyBlockDamage;

	void Start() {
		LastRegen = Time.time;
	}
	bool HasInitiated = false;
	public void CheckForInitiation() {
		if (!HasInitiated) {
			MaxHealth = Health;
			//MyBlockDamage.Initialize();
			HasInitiated = true;
		}
	}
	// Update is called once per frame
	void Update () {
		CheckForInitiation ();
		if (Time.time - LastRegen >= 1) {
			LastRegen = Time.time;
			Health += 1;
			if (Health > MaxHealth)
				Health = MaxHealth;
			AdjustHealthBar();
			if (Health == MaxHealth) {
				MyChunk.world.SetBlock(x,y,z, MyBlock.GetBlockIndex());
				Destroy (gameObject);
			}
		}
		//LookAtObject = GetManager.GetMainCamera ().gameObject;
		if (LookAtObject) {
			transform.LookAt(LookAtObject.transform.position);
		}
	}
	public bool IsDead() {
		return IsBlockDead;
	}
	public void AdjustHealthBar() {
		// canvas/background size
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1,0.2f);
		// health bar size
		gameObject.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(GetHealthPercentage(),0.2f);
		gameObject.transform.GetChild (0).gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0.5f - (GetHealthPercentage ()) / 2f, 0);
	}
	// applied every .1 seconds or the rate that the pickaxe hits the block
	public float ApplyDamage(float MiningSkill) {
		CheckForInitiation ();
		if (!IsBlockDead) {
			float Damage = (MiningSkill * 100.0f) / Defence;
			Health -= Damage;
			AdjustHealthBar();
			if (Health <= 0)
			{
				IsBlockDead = true;
			}
			return Damage;
		} else {
			return 0;
		}
	}
	public float GetHealthPercentage() {
		return Health / MaxHealth;
	}
}
