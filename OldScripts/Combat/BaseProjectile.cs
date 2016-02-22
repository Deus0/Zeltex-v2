using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// so far this has various functinos:

// pulls physics objects towards it / away from it
// applies effects to other characters
// applies stat changes to other players - either on hit or in an area
// 

// needs:

// locking/homing onto characters and gravitating towards them
// breaking into a number of smaller ammunition
// laser ability - ray tracing - charges at first and then it fires accross a ray until it reaches the target, then it shrinks from that end
// wave damage - a wave of particles that grows in a cone and does damage when it hits
// upon hit creates a magic ruin that summons something above it after a period of time
// upon hit places a trap, when a user walks over it
// damages blocks

// another idea: spells are based on player stats like intell/wisdom - items like bombs depend on engineering stats and have a limited amount
// to restore mana you will need to eat more food
namespace OldCode {
public class BaseProjectile : MonoBehaviour {
	// base character that fired it
	private int PlayerIndex = 0;
	private int ClanIndex = 0;
	private GameObject HittingPlayer;
	
	// General projectile properties
	private bool DoesDamageBlocks = false;
	private bool IsCollide = true;
	private bool DoesGravitateTowardsPlanet = false;	// used for things like bombs to make them go downwards
	private bool HasDoneDamage = false;	// Bullets only do damage once
	private bool IsInitialized = false;

	// Lifespan
	private float LifeTime = 10f;
	private float TimeCreated = 0;
	private float TimeToLive = 30;
	private bool IsBeingDestroyed = false;

	// Various types of effects
	public GameObject ExplosionEffect;
	public AudioClip DeathSoundEffect;
	// pierce damage
	private float Damage;
	public bool DoesReviveOnHit = false;
	private DamageType MyDamageType = DamageType.Peirce;
	// apply effects
	public Effect OnHitEffect = null;
	public bool IsOnHitEffect = false;

	// Explosion
	public bool DoesDestroyOnHit = true;
	public bool IsAoe = false;
	private bool IsExplosionDamage = true;
	private float DamageRadius = 5f;
	private float AOEDamage = 10f;

	// Seeker Missile
	public bool IsSeekerMissile = false;
	private GameObject MyTarget;
	private float TimeTargettedEnemy = 0f;
	// gravitational pull
	private bool IsGravityPull = false;
	private Vector3 PullForce;
	private int ThingsInArea;
	private float GravitationalRadius = 5f;
	private float GravitationalForce = 25f;

	public void ApplyOnHitEffects(BaseCharacter HitCharacter) {
		if (IsOnHitEffect && OnHitEffect != null) {
			HitCharacter.MyStats.AddEffect (OnHitEffect);
		}
		if (DoesReviveOnHit) {
			HitCharacter.Ressurect (transform.position);	// if no respawn point - should check here with game manager if theres a respawn zone
		}
	}
	public GameObject GetCharacterThatSpawned() {
		return HittingPlayer;
	}
	
	public int GetPlayerIndex() {return PlayerIndex;}
	public int GetClanIndex() {return ClanIndex;}
	public float GetDamage() {
		return Damage;
	}
	void Start () {
		//Destroy (gameObject,LifeTime);
		//rigidBody.AddForce (speed*transform.forward);
		TimeCreated = Time.time;
		Debug.Log("= Spawning BaseProjectile at: " + Time.time);
	}
	// Update is called once per frame
	void Update () {
		if (IsInitialized) {
			if (Time.time - TimeCreated >= TimeToLive) {
				DestroyIt ();
			}
			if (IsGravityPull) {
				PullThings (transform.position, GravitationalRadius);
			}
		}
		if (IsSeekerMissile) {
			SeekTarget();
		}
	}
	
	public void DestroyItInTime(float TimeToLive_) {
		TimeCreated = Time.time;
		TimeToLive = TimeToLive_;
	}
	public void CreateDeathSound() {
		AudioSource MyAudioSource;
		GameObject NewSoundEffect = new GameObject ();
		NewSoundEffect.name = gameObject.name + " DeathSound";
		MyAudioSource = NewSoundEffect.AddComponent<AudioSource> ();
		if (DeathSoundEffect != null)
			MyAudioSource.PlayOneShot (DeathSoundEffect, 1f);
		Destroy (NewSoundEffect, 3f);
	}

	public void WasHit() {
		if (DoesDestroyOnHit) {
			CreateDeathSound();
			Destroy (gameObject, 0);	// destroys the bullet
		}
	}
	// this will get called on all computers
	[RPC]
	public void UpdateDataWithCharacter(int PlayerIndex_, int ClanIndex_, float Damage_, float TravelSpeed_, float LifeTime_) {
		//float Damage_ = 5;
		//float TravelSpeed_ = 150f;
		PlayerIndex = PlayerIndex_;
		ClanIndex = ClanIndex_;
		//this.ClanIndex = MyCharacter.ClanIndex;
		//this.HittingPlayer = MyCharacter.gameObject;//(BaseCharacter) gameObject.GetComponent ("BaseCharacter");
		this.Damage = Damage_;
		this.AOEDamage = Damage_;
		
		TimeCreated = Time.time;
		TimeToLive = LifeTime_;
		//DestroyItInTime(LifeTime_);
		this.DamageRadius = 2f;
		//TravelSpeed = TravelSpeed_;

		Rigidbody MyRigid = gameObject.GetComponent<Rigidbody> ();
		MyRigid.AddForce (transform.forward*(TravelSpeed_));

		GameManager MyGame = GetManager.GetGameManager ();
		if (MyGame != null) {
			HittingPlayer = GetManager.GetCharacterManager().GetCharacter(PlayerIndex);
		}

		IsInitialized = true;
	}

	// seeks the closest target
	public void SeekTarget() {
		if (!MyTarget) {
			Collider[] hitColliders = Physics.OverlapSphere (transform.position, 5f);
			float ClosestDistance = 10f;
			for (int i = 0; i < hitColliders.Length; i++) {
				BaseCharacter enemy = (BaseCharacter)hitColliders [i].gameObject.GetComponent ("BaseCharacter");
				if (enemy) 
				if (!enemy.MyStats.IsDead && enemy.PlayerIndex != PlayerIndex && enemy.ClanIndex != ClanIndex) {	// can target lol
					float MyDistance = Vector3.Distance (transform.position, enemy.transform.position);
					if (MyDistance < ClosestDistance) {
						MyTarget = enemy.gameObject;
						ClosestDistance = MyDistance;
						TimeTargettedEnemy = Time.time;
					}
				}
			}
			Debug.Log ("Explosion Colliding with " + ThingsInArea + " objects");
		} else {
			transform.position = Vector3.Lerp(transform.position, MyTarget.transform.position, (Time.time-TimeTargettedEnemy)/3f);
			//Rigidbody MyRigid = gameObject.GetComponent<Rigidbody> ();
			//MyRigid.AddForce ((MyTarget.transform.position-transform.position).normalized*5f);
		}
	}
	public void HasHit() {
		HasDoneDamage = true;
	}
	public bool DoneDamage() {
		return HasDoneDamage;
	}
	public void PullThings(Vector3 center, float radius) {
			Collider[] hitColliders = Physics.OverlapSphere (center, radius);
			//int i = 0;
			Debug.Log ("Colliding with " + hitColliders.Length + " objects");
			//int EnemyHitCount = 0;
			//ThingsInArea = hitColliders.Length;
			for (int i = 0; i < hitColliders.Length; i++) {
				BaseCharacter enemy = (BaseCharacter)hitColliders [i].gameObject.GetComponent ("BaseCharacter");
				if (enemy && enemy.PlayerIndex != PlayerIndex) {
					Rigidbody EnemyBody = (Rigidbody)enemy.gameObject.GetComponent ("Rigidbody");
					PullForce =  transform.position-enemy.gameObject.transform.position;
					PullForce.Normalize();
					if (EnemyBody != null)
					EnemyBody.AddForce (PullForce * GravitationalForce);
				}
			}
	}
	void OnDestroy() {
		//Debug.Log ("Destroying bomb!");
	}

	// this should be called inside the collision
	public void Explode() {
		//if (IsExplosionDamage) {
			ExplosionDamage (transform.position, DamageRadius);
		//}
	}
	void ExplosionDamage(Vector3 center, float radius) {
		Debug.Log ("Applying explosion damage");
		Collider[] hitColliders = Physics.OverlapSphere (center, radius);
		ThingsInArea = 0;
		for (int i = 0; i < hitColliders.Length; i++) {
			BaseCharacter enemy = (BaseCharacter)hitColliders [i].gameObject.GetComponent ("BaseCharacter");
			if (enemy) {
				Debug.Log ("Enemy: " + enemy.name + " Is taking some damage :O");
				enemy.TakeHitAOE (gameObject);
				ThingsInArea++;
			}
		}
		Debug.Log ("Explosion Colliding with " + ThingsInArea + " objects");

	}
	// still useful for finding close blocks with rayhit
	/*
	 * 
		LayerMask NewLayer = LayerMask.NameToLayer ("Chunks");
		int LayerBitmask = 1 << NewLayer.value;
		Vector3[] SphereDirections = GetSphereDirections (16);
		for (int j = 0; j < 1; j++) {	// check through the block spawns
			
			List<RaycastHit> MyBlockHits = new List<RaycastHit> ();
			List<int> BlockTypes = new List<int> ();
			for (int i = 0; i < SphereDirections.Length; i++) {
				Ray ray = new Ray ();
				ray.origin = transform.position;
				ray.direction = SphereDirections [i];	//new Vector3(i,j,0);
				//Debug.LogError ("RayCasting for Characters");
				RaycastHit hit;
				if (Physics.Raycast (ray.origin, ray.direction, out hit, radius,LayerBitmask)) { 
					Debug.DrawLine (ray.origin, hit.point, Color.green, 2.5f, true);	// draw a line from the start position to the hit position

					Chunk SelectedChunk = hit.transform.gameObject.GetComponent <Chunk> ();
					if (SelectedChunk) {
						if (!GetManager.GetTerrainManager ().IsBlockEmpty (hit)) {
							BlockTypes.Add (GetManager.GetTerrainManager ().RemoveBlockStage1 (hit));
							MyBlockHits.Add (hit);
						}
					}
				} else {
					Debug.DrawLine (ray.origin, ray.origin + ray.direction.normalized * radius, Color.white, 3f, true);	// draw a line from the start position to the hit positio
				}
			}
			for (int i = 0; i < MyBlockHits.Count; i++)
				GetManager.GetTerrainManager ().RemoveBlockStage2 (MyBlockHits[i], BlockTypes[i]);
		}*/

	// for sheilds i want them to be a block model
// and when it gets hit it will have a transparency wave at the point of hit
// that transparency will travel outwards in a wave as it decrases in value

	private Vector3[] GetSphereDirections(int  NumDirections) {
		Vector3[] Points = new Vector3[NumDirections];
		float Increment = Mathf.PI * (3 - Mathf.Sqrt (5));
		float Offset = 2f / NumDirections;
		for (int i = 0; i < NumDirections; i++) {
			float y = i*Offset - 1 + (Offset/2f);
			float radius = Mathf.Sqrt(1-y*y);
			float phi = i*Increment;
			float x = Mathf.Cos (phi)*radius;
			float z = Mathf.Sin (phi)*radius;
			Points[i] = new Vector3(x,y,z);
		}
		return Points;
	}
	// this could be used for a seperate collider on the object that is trigger? perhaps aoe effect like this lol - 
	// would be cleaner then creating the sphere every frame
	/*void OnTriggerEnter(Collider other) {
		Debug.LogError ("On Trigger Enter: " + name);
		if (DoesDestroyOnHit) { 
			Rigidbody MyRigid = (Rigidbody) gameObject.GetComponent ("Rigidbody");
			MyRigid.isKinematic = true;
			Destroy (gameObject, 3f);
		}
	}*/
	public void DestroyIt() {
		if (!IsBeingDestroyed) {
			IsBeingDestroyed = true;
			if (ExplosionEffect) {
				GameObject Explosion = (GameObject)Instantiate (ExplosionEffect, transform.position, Quaternion.identity);
				//Destroy (Explosion, 4f);
				//Explosion.transform.localScale = new Vector3 (DamageRadius, DamageRadius, DamageRadius);
			}
			if (IsAoe) {
				Explode ();
			
				// add effect
				if (DoesDamageBlocks) {
					//Debug.LogError ("Destroying bomb v2");
					//GetManager.GetTerrainManager ().RemoveBlock (GetManager.GetWorld (), transform.position, DamageRadius);
				
					List<GameObject> NewBlocks = GetManager.GetTerrainManager ().RemoveBlock (GetManager.GetWorld (), transform.position, DamageRadius);
					for (int i = 0; i < NewBlocks.Count; i++) {
						Rigidbody NewRigid = NewBlocks [i].GetComponent<Rigidbody> ();
						Rigidbody MyRigid = GetComponent<Rigidbody> ();
						if (MyRigid != null)
						if (NewRigid != null) {
							Vector3 ForceDirection = transform.forward.normalized;
							NewRigid.AddForce (ForceDirection * 1500f);
						}
					}
				}
			}
			Destroy (gameObject);
		}
	}
	void OnCollisionEnter(Collision other) {
		if (DoesDestroyOnHit) {
			DestroyIt ();
		}
		if (other.gameObject.tag == "PickUp") {
			//other.gameObject.SetActive (false);
		}
		if (other.gameObject.tag == "Player") {
			
		}
		if (other.gameObject.tag == "Terrain") {
			//Destroy (gameObject);
		}
		//rigidbody.velocity.Set (-rigidbody.velocity.x,-rigidbody.velocity.y,-rigidbody.velocity.z);
		//Destroy (gameObject,3);
	}
	
	/*void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		float ShootX = 0;
		float ShootY = 0;
		float ShootZ = 0;
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
		if (stream.isWriting) {
			// Sending
			ShootX = MyCharacter.ShootForwardVector.x;
			ShootY = MyCharacter.ShootForwardVector.y;
			ShootZ = MyCharacter.ShootForwardVector.z;
			stream.Serialize (ref ShootX);
			stream.Serialize (ref ShootY);
			stream.Serialize (ref ShootZ);
		} else {
			// revieving
			stream.Serialize (ref ShootX);
			stream.Serialize (ref ShootY);
			stream.Serialize (ref ShootZ);
			//MyCharacter.ShootForwardVector = new Vector3(ShootX, ShootY, ShootZ);
		}
	}*/
}
}
