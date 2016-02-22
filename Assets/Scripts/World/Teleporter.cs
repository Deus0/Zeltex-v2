using UnityEngine;
using System.Collections;
using ItemSystem;
using CharacterSystem;

namespace WorldUtilities {
	/*
	Teleports a character when they walk on it
	 * */
	public class Teleporter : MonoBehaviour {
		public int EmmissionRate = 80;
		public float TeleportCooldown = 5f;
		float LastTeleported = -15f;
		public GameObject TeleportLocation;
		[Tooltip("The Particles created when item is picked up")]
		public GameObject ParticlesPrefab;
		[Tooltip("Played when item is picked up")]
		public AudioClip MyPickupSound;
		private bool IsUseDifference = false;
		private ParticleSystem MyParticles;
		private void CopyParticles(ParticleSystem ParticlesA, ParticleSystem ParticlesB) 
		{
			ParticlesA.startColor = ParticlesB.startColor;
			ParticlesA.startDelay = ParticlesB.startDelay;
			ParticlesA.startLifetime = ParticlesB.startLifetime;
			ParticlesA.startRotation = ParticlesB.startRotation;
			ParticlesA.startSize = ParticlesB.startSize;
			ParticlesA.startSpeed = ParticlesB.startSpeed;
			ParticlesA.gravityModifier = ParticlesB.gravityModifier;
		}
		void Start() {
			MyParticles = gameObject.GetComponent<ParticleSystem>();
			if (ParticlesPrefab) 
			{
				ParticleSystem ParticlesB = ParticlesPrefab.GetComponent<ParticleSystem> ();
				CopyParticles(ParticlesB, MyParticles);
			}
		}
		void Update() {
			if (Time.time - LastTeleported < TeleportCooldown) {
				float TimePercent = (Time.time-LastTeleported)/TeleportCooldown;
				if (Time.time-LastTeleported < TeleportCooldown-1f)
					TimePercent = 0f;
				//else
				//	TimePercent = (Time.time-1f)/TeleportCooldown;

				if (MyParticles) {
					MyParticles.emissionRate = EmmissionRate*TimePercent;
				}
			}
		}
		private void Use() {
			LastTeleported = Time.time;
		}
		public void TeleportToLocation(GameObject CollideWithItem, GameObject MyCharacter) {
			if (MyCharacter.GetComponent<CharacterSystem.Character>())
			if (Time.time - LastTeleported > TeleportCooldown) {
				Use();
				Teleporter MyTeleporter2 = TeleportLocation.GetComponent<Teleporter>();
				if (CollideWithItem.GetComponent<ItemSystem.ItemObject>()) {
					Vector3 Difference = (TeleportLocation.transform.position-transform.position);	// difference between teleport locations
					if (IsUseDifference)
						MyCharacter.transform.position += Difference;
					else {
						MyCharacter.transform.position = new Vector3(TeleportLocation.transform.position.x,
						                                             MyCharacter.transform.position.y+Difference.y,
						                                             TeleportLocation.transform.position.z);
					}
					if (MyTeleporter2 != null) {
						MyTeleporter2.Use();
					}
					
					if (ParticlesPrefab) {
						GameObject ItemLeftOver = (GameObject)Instantiate (ParticlesPrefab, TeleportLocation.transform.position, ParticlesPrefab.transform.rotation);
						//ItemLeftOver.transform.localScale = transform.localScale;
						if (MyPickupSound) {
							AudioSource MySource = ItemLeftOver.AddComponent<AudioSource> ();
							if (MyPickupSound != null)
								MySource.PlayOneShot (MyPickupSound);
						}
						ItemLeftOver.AddComponent<ParticlesEmmisionOverLifetime> ();
						Destroy (ItemLeftOver, 5f);
					}
				}
			}
		}
	}
}