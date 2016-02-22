using UnityEngine;
using System.Collections;

namespace CombatSystem {
	[System.Serializable]
	public class BulletSettings {
		public GameObject SpawnerObject;
		public string Name = "Bullet";
		[Header("Base Stats")]
		public string StatUseName = "Mana";
		public float StatCost = 4;
		public string StatName = "Health";
		public float StatChange = -8f;
		public float LifeTime = 8f;
		[Header("Projectile")]
		public float FireRate = 0.7f;
		public float Randomness = 0.05f;
		// initial force given to the projectile
		public float BulletForce = 50;
		public bool DoesDestroyOnHitCharacters = true;
		public bool DoesDestroyOnHitTerrain = false;
		public bool IsDestroyTerrainOnHit = false;
		[Header("Explosion")]
		public bool IsExplosion = true;
		public float ExplosionSize = 0.1f;

		public BulletSettings() {}
	}

	public class Bullet : MonoBehaviour 
	{
		private PhotonView MyPhoton;
		[SerializeField] private BulletSettings Data = new BulletSettings();
		AudioSource MySource;
		bool IsUsed = false;	// has hit character
		bool HasHitTerrain = false;
		[Header("Options")]
		public float DissapearingTime = 5f;
		private float TimeDied;
		private static float MaxDissapearTime = 30f;
		private GameObject StuckToObject;
		private Vector3 StuckToObjectDifference;
		[Header("Special Effects")]
		public GameObject ExplosionEffectPrefab;
		//public float ExplosionSize = 1f;
		[Header("Audio")]
		public AudioClip OnHitTerrain;
		public AudioClip OnHitCharacterSound;
		public AudioClip OnSpawn;

		public GameObject GetSpawner() {
			return Data.SpawnerObject;
		}
		// Use this for initialization
		void Start ()
		{
			MySource = gameObject.AddComponent<AudioSource> ();
			MySource.PlayOneShot (OnSpawn);
			MyPhoton = gameObject.GetComponent<PhotonView> ();
		}

		public void UpdateData(BulletSettings MyData,  Vector3 BulletDirection) 
		{
			Data = MyData;
			DestroyInTime (Data.LifeTime);
			Rigidbody MyRigid = GetComponent<Rigidbody> ();
			MyRigid.AddForce ((new Vector3 (BulletDirection.x + Random.Range (-Data.Randomness, Data.Randomness),
			                                BulletDirection.y + Random.Range (-Data.Randomness, Data.Randomness),
			                                BulletDirection.z + Random.Range (-Data.Randomness, Data.Randomness)
			                                ) * Data.BulletForce));
		}

		public void DestroyInTime(float InTime) 
		{
			StartCoroutine(DestroyInTime2 (InTime));
		}

		IEnumerator DestroyInTime2(float LifeTime) 
		{
			//Debug.LogError(Time.time + " Before Yeild in destroying bullet.");
			yield return new WaitForSeconds(LifeTime);
			//Debug.LogError(Time.time + " Destroying bullet after: " + LifeTime + " seconds.");
			PhotonNetwork.Destroy (gameObject);
			//Destroy (BulletSpawn);
		}

		void Update() {
			if (StuckToObject && IsUsed) 
			{
				transform.position = StuckToObject.transform.position + StuckToObjectDifference;
				if (Time.time-TimeDied >= DissapearingTime) 
				{
					//hide it
					if (gameObject.GetComponent<MeshRenderer>()) 
					{
						gameObject.GetComponent<MeshRenderer>().enabled = false;
					}
					if (gameObject.GetComponent<SphereCollider>()) 
					{
						gameObject.GetComponent<SphereCollider>().enabled = false;
					}	
				}
			}
		}

		void OnCollisionEnter(Collision collision) 
		{
			if (Data.SpawnerObject != null)
				if (collision.gameObject == Data.SpawnerObject)
					return;
			if (!IsUsed) 
			{
				if (collision.gameObject.GetComponent<CharacterSystem.CharacterStats> ()) 
					if (collision.gameObject != Data.SpawnerObject)
				{
					MyPhoton.RPC("OnHitCharacter", PhotonTargets.All,
				             collision.gameObject.name,
				             collision.gameObject.transform.position);
					
					return;
				}
			}
			if (!HasHitTerrain)
			{
				HasHitTerrain = true;
				VoxelEngine.Chunk MyChunk = collision.gameObject.GetComponent<VoxelEngine.Chunk> ();
				if (MyChunk) {
					MySource.PlayOneShot (OnHitTerrain, 0.5f);
					Vector3 VelNormal = gameObject.GetComponent<Rigidbody>().velocity.normalized*MyChunk.transform.lossyScale.x/10f;
					gameObject.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0,50,0), collision.contacts[0].point);
					// destroy part of the chunk
					//if (Data.DoesDestroyOnHitTerrain)
					{
						CreateExplosionEffect(MyChunk.GetWorld().gameObject);
						//Destroy (gameObject,DissapearingTime);
						DestroyInTime(DissapearingTime);
						gameObject.GetComponent<Rigidbody>().isKinematic = true;
					}
					return;
				}
			}
		}
		
		private void CreateExplosionEffect() 
		{
			CreateExplosionEffect (null);
		}

		private void CreateExplosionEffect(GameObject MyWorld) 
		{
			if (ExplosionEffectPrefab) 
			{
				GameObject MyExplosion = (GameObject)Instantiate (ExplosionEffectPrefab, transform.position, transform.rotation);
				MyExplosion.GetComponent<AnimateSize>().MaxSize = new Vector3(Data.ExplosionSize*2f,
				                                                              Data.ExplosionSize*2f,Data.
				                                                              ExplosionSize*2f);
				if (Data.IsDestroyTerrainOnHit)
					if (MyWorld)
						MyExplosion.GetComponent<AnimateSize>().SetWorld(MyWorld);
			}
		}
		[PunRPC]
		public void OnHitCharacter(string CharacterName, Vector3 MyHitPosition) 
		{
			if (!IsUsed) {
				IsUsed = true;
				GameObject HitObject = GameObject.Find (CharacterName);
				if (HitObject) 
				{
					CharacterSystem.CharacterStats MyStats = HitObject.GetComponent<CharacterSystem.CharacterStats> ();
					if (MyStats) 
					{
						if (MyStats.Alive ()) 
						{
							if (OnHitCharacterSound)
								MySource.PlayOneShot (OnHitCharacterSound);
							
							MyStats.AddStat (Data.StatName, Data.StatChange);
							// if has killed player
							AnimationUtilities.StatPopUp.CreateTextPopup (transform.position, Data.StatChange);
							if (Data.DoesDestroyOnHitCharacters) {
							TimeDied = Time.time;
							if (Data.IsExplosion) {
								CreateExplosionEffect();
							}
								//Destroy (gameObject, MaxDissapearTime);
								DestroyInTime(MaxDissapearTime);
								gameObject.GetComponent<Rigidbody> ().isKinematic = true;
								StuckToObject = HitObject;
								StuckToObjectDifference = transform.position - MyHitPosition;
							}
							
							if (!MyStats.Alive ())
							{
								//HitObject.GetComponent<PhotonView>().owner.AddScore(-1);
								Data.SpawnerObject.GetComponent<PhotonView>().owner.AddScore(1);
							}
							return;
						}
						else 
						{
							MySource.PlayOneShot (OnHitTerrain);
						}
					}
				}
			}
		}
	}
}
