using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour {
	public float LifeTime = 10f;
	public bool IsPickUp = true;	// is it able to be picked up

	// if is item pickup, then droptype != IconType.None
	public IconType DropType;
	public Item DropItem;
	//public int DropQuantity = 0;

	//public bool IsSpin = false;
	public bool IsStatIncrease = false;
	public float StatIncrement = 10f;
	public string StatName = "Health";
	public bool IsAddEffect = true;
	public Effect PickupEffect;

	public int EffectTicksMax = 4;
	public float EffectCoolDown = 3f;
	public float EffectIncrement = 1f;
	public string EffectName = "HealOverTime";
	public AudioClip PickUpSound;
	public float SoundVolume = 1f;
	public AudioSource SoundSource;	// source of where the sound is emmited from - getting hit, casting spell, dying etc

	public ParticleSystem MyParticles;

	void Start() {
		if (LifeTime > 0)
			Destroy (gameObject, LifeTime);
		SoundSource = GetComponent<AudioSource>();
		if (gameObject.transform.childCount > 0)
			MyParticles = gameObject.transform.GetChild (0).GetComponent<ParticleSystem> ();
	}
	// Update is called once per frame
	void Update () {
		//if (IsSpin) 
		//	transform.Rotate (new Vector3(15,30,45) * Time.deltaTime);
	}
	void OnTriggerEnter(Collider other) {
		BaseCharacter NewCharacter = other.gameObject.GetComponent<BaseCharacter> ();
		if (NewCharacter != null) {
			NewCharacter.PickUpItem(gameObject);
		}
	}
	public bool CanPickup() {
		return IsPickUp;
	}
	public bool PickUp() {
		if (IsPickUp) {
			Debug.Log ("Picking up Item: " + Time.time);
			SoundSource.PlayOneShot (PickUpSound, SoundVolume);
			Destroy (gameObject, 15);
			gameObject.GetComponent<MeshRenderer> ().enabled = false;	// need animation or something on this - shrinking maybe

			if (gameObject.GetComponent<MeshCollider> ())
				gameObject.GetComponent<MeshCollider> ().enabled = false;

			if (gameObject.GetComponent<Rotator> ())
				gameObject.GetComponent<Rotator> ().enabled = false;

			BoxCollider[] myColliders = gameObject.GetComponents<BoxCollider>();
			foreach(BoxCollider bc in myColliders) 
				bc.enabled = false;

			IsPickUp = false;	// so it can't be picked up anymore lol
			if (MyParticles != null) {
				MyParticles.enableEmission = false;
			}
			return true;
		}
		return false;
	}
	/*void OnCollisionStay(Collision collision) {
		BaseProjectile bullet = (BaseProjectile) collision.gameObject.GetComponent("BaseProjectile");
		if (bullet) {
			SliceMesh Slice = (SliceMesh) gameObject.GetComponent("SliceMesh");
			Slice.TestSliceMesh = true;
			bullet.gameObject.SetActive (false);
			Destroy (bullet.gameObject);
		}
	}*/
}
