using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace CombatSystem 
{
	public class Shooter : MonoBehaviour
	{
		private PhotonView m_PhotonView;
		private GameObject BulletPrefab;
		private string PrefabName = "Bullet";
		//private Vector3 BulletOffset = new Vector3 (0.2f, 0, 0);	// calculate by the bounds of the character
		private Vector3 BulletDirection = new Vector3 (0, 0, 0);
		private float LastFired = 0f;
		//public string StatName = "Mana";
		//public float StatCost = 1;
		public Transform HotSpotTransform;
		[Header("Options")]
		public bool IsChargeMode = false;	// hold down left click to charge up a bullet
		public BulletSettings MyBulletSettings = new BulletSettings();
		public bool IsShootFacingDirection = false;
		public bool IsReverseDirection = false;
		//public float BulletLifeTime = 1f;
		//float BulletSize = 1f;
		//public float MinBulletSize = 2f;
		//public float MaxBulletSize = 4f;
		public bool IsShootFacingDirection2 = false;

		//private Transform MyParentTransform;
		
		//RectTransform MyRect;
		// Use this for initialization
		void Start ()
		{
			m_PhotonView = GetComponent<PhotonView>();
			MyBulletSettings.SpawnerObject = gameObject;
			LastFired = Time.time;
			if (HotSpotTransform == null)
				HotSpotTransform = transform;
			BulletPrefab = Resources.Load ("Bullet") as GameObject;
		}

		public void Shoot() 
		{
			if (Time.time - LastFired >= MyBulletSettings.FireRate)
			{
				Debug.Log ("Firing bullet :3");
				if (gameObject.GetComponent<CharacterSystem.CharacterStats> ().HasStat (MyBulletSettings.StatUseName, MyBulletSettings.StatCost))
				{
					gameObject.GetComponent<CharacterSystem.CharacterStats> ().AddStat (MyBulletSettings.StatUseName, -MyBulletSettings.StatCost);
					FireBullet ();
				}
			}
		}

		private void FireBullet() 
		{
			//BulletSize = Random.Range (MinBulletSize, MaxBulletSize);
			LastFired = Time.time;
			if (IsShootFacingDirection) 
			{
				/*BulletDirection.y = 0;
						if (transform.localScale.x < 0)
							BulletDirection.x = -1;
						else
							BulletDirection.x = 1;
						*/
				BulletDirection = HotSpotTransform.forward;
			} 
			else if (IsReverseDirection) 
			{
				BulletDirection.y = 0;
				if (transform.localScale.x < 0)
					BulletDirection.x = 1;
				else
					BulletDirection.x = -1;
			}
			/*Vector3 TransformedBulletOffset = new Vector3(transform.lossyScale.x*BulletOffset.x, 
			                                              transform.lossyScale.y*BulletOffset.y,
			                                              transform.lossyScale.z*BulletOffset.z);*/	//MyRect.lossyScale.x * BulletOffset2
			//TransformedBulletOffset = HotSpotTransform.TransformDirection(TransformedBulletOffset);
			CreateBullet (BulletPrefab);
		}

		private void CreateBullet(GameObject Prefab)
		{
			//Debug.LogError ("Creating new bullet at: " + Time.time);
			/*if (IsShootFacingDirection2) {
				BulletDirection = HotSpotTransform.forward*BulletDirection.z - HotSpotTransform.right*BulletDirection.x + HotSpotTransform.up*BulletDirection.y;
				BulletDirection.Normalize();
			}*/
			Vector3 BulletDirection = HotSpotTransform.rotation*Vector3.forward;
			Vector3 BulletSpawnPosition = HotSpotTransform.position + BulletDirection*0.2f;
			
			//Debug.LogError ("Spawning bullet in direction of: " + BulletDirection.ToString ());
			if (PhotonNetwork.connected) 
			{
				GameObject MyBullet = (GameObject)PhotonNetwork.Instantiate (BulletPrefab.name, BulletSpawnPosition, Quaternion.LookRotation (BulletDirection), 0);
				m_PhotonView.RPC ("UpdateBullet",
				                  PhotonTargets.All,
				                  BulletSpawnPosition,
				                  BulletDirection);
			} 
			else 
			{
				GameObject MyBullet = (GameObject)Instantiate (BulletPrefab, BulletSpawnPosition, Quaternion.LookRotation (BulletDirection));
				UpdateBullet(BulletSpawnPosition, BulletDirection);
			}

		}

		//GameObject MyBullet;
		[PunRPC]
		private void UpdateBullet(Vector3 BulletSpawnPosition, Vector3 BulletDirection) 
		{
			GameObject MyBullet = GameObject.Find ("Bullet(Clone)");
			if (MyBullet) {
				MyBullet.name = "Bullet" + Random.Range (1, 100000);
				MyBullet.GetComponent<CombatSystem.Bullet> ().UpdateData (MyBulletSettings, BulletDirection);
			}
		}
	}
}

// old code

/*// escape thrusters
if (Time.time - LastBoost >= BoostRate) {
	LastBoost = Time.time;
	if (CrossPlatformInputManager.GetButton ("Fire2")) {
		Vector2 BulletDirection = new Vector2 (0, 0);
		if (transform.localScale.x < 0)
			BulletDirection.x = 1;
		else
			BulletDirection.x = -1;
		Vector3 BulletOffset = new Vector3 (-MyRect.lossyScale.x / 3f, 0, 0);
		BulletOffset = transform.parent.TransformDirection(BulletOffset);
		CreateBullet (BulletDirection, BulletOffset, BulletForce, Size, BoostLifeTime, BoostPrefab);
	}
	// Jetpack physics
	if (CrossPlatformInputManager.GetButton ("Fire3")) {
		Vector3 BulletOffset = new Vector3 (0, -Mathf.Abs (MyRect.lossyScale.x) / 3f, 0);
		BulletOffset = transform.parent.TransformDirection(BulletOffset);
		Vector2 BulletDirection = new Vector2 (0, -1);
		Vector3 GravityDirection = gameObject.GetComponent<ArtificialGravity>().GravityForce.normalized;
		BulletDirection = new Vector2(GravityDirection.x, 
		                              GravityDirection.y);
		BulletOffset = Mathf.Abs (MyRect.lossyScale.x/3f)*( new Vector3(GravityDirection.x/2f, GravityDirection.y,0));
		CreateBullet (BulletDirection, BulletOffset, BulletForce, Size, BoostLifeTime, RocketBoostPrefab);
	}
}*/
