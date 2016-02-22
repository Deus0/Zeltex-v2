using UnityEngine;
using System.Collections;

namespace CombatSystem 
{
	public class Sheild : MonoBehaviour 
	{
		private CharacterSystem.CharacterStats MyStats;
		public bool IsActivated = false;
		public float StatCost = 2;
		public string StatName = "Mana";
		float CoolDown = 1f;
		private float LastTime;
		private float RepelForce = 40;
		// animating options
		private Vector3 OriginalScale;
		private float AnimationSpeed = 4;
		private float Variation = 0.025f;

		void Start() 
		{
			OriginalScale = transform.localScale;
			if (MyStats == null)
				MyStats = transform.parent.gameObject.GetComponent <CharacterSystem.CharacterStats> ();
			Activate (IsActivated);
		}

		public void Activate(bool NewState) 
		{
			LastTime = Time.time;
			IsActivated = NewState;
			gameObject.GetComponent<MeshRenderer> ().enabled = NewState;
			gameObject.GetComponent<SphereCollider> ().enabled = NewState;
			gameObject.GetComponent<ParticleSystem> ().enableEmission = NewState;
		}

		void Update() 
		{
			if (IsActivated)
			{
				// consume stat per second
				if (Time.time - LastTime >= CoolDown) 
				{
					LastTime = Time.time;
					MyStats.AddStat (StatName, -StatCost);
					if (MyStats.GetStatValue (StatName) <= 0)
						Activate (false);
				}
				// every on sec decrease mana by 1 for activated sheild
				transform.localScale = OriginalScale + OriginalScale * (Variation * Mathf.Sin (Time.time * AnimationSpeed));
			} 
			else 
			{
				transform.localScale = Vector3.Lerp(transform.localScale, OriginalScale, Time.deltaTime);
			}
		}

		void OnTriggerEnter(Collider other) 
		{
			UseTheForce (other.gameObject);
		}

		void OnTriggerStay(Collider other)
		{ 
			UseTheForce (other.gameObject);
		}

		void UseTheForce(GameObject MyColliderObject) 
		{
			if (IsActivated) 
			{
				if (MyColliderObject.tag != null)
				if (MyColliderObject.tag == "Character" || MyColliderObject.tag == "Bullet") 
				{
					if (MyColliderObject.tag == "Bullet") {
						CombatSystem.Bullet MyBullet = MyColliderObject.GetComponent<CombatSystem.Bullet> ();
						if (MyBullet)
						{
							if (MyBullet.GetSpawner () == transform.parent.gameObject)
								return;	// if bullet belongs to user of sheild dont use the force
						}
					}
					//Debug.LogError ("Sheild using the force on" + MyColliderObject.name);
					Rigidbody MyRigid = MyColliderObject.GetComponent<Rigidbody> ();
					if (MyRigid) 
					{
						float Force = MyRigid.velocity.magnitude;
						if (Force < RepelForce)
							Force = RepelForce;
						MyRigid.AddForceAtPosition ((MyColliderObject.transform.position - transform.position).normalized * Force, transform.position);

					}
				}
			}
		}

		public void BulletHitsSheild(GameObject MyBullet)
		{
			Destroy(MyBullet);
		}
	}
}
