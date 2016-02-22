using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
//using UnityStandardAssets.Characters.FirstPerson;

// all input has been moved to playermovement
// this just contains raycasting stuff, will soon be moved to basecharacter

// Stats and Inventory to be moved to their own components
namespace OldCode {
	public class Player : BaseCharacter {
		private CharacterController MyController;
		// used for raycasting
		public LayerMask BuildingZoneLayer;
		public LayerMask CharactersLayer;
		public LayerMask ChunksLayer;
		public bool IsFlyMode = false;

		// Use this for initialization
		void Start () { 
			SelectedBlockModel = (GameObject) Instantiate (SelectedBlockModelPrefab, new Vector3 (), Quaternion.identity);
		}

		void OnDestroy() {
			if (SelectedBlockModel)
				Destroy (SelectedBlockModel.gameObject);
		}

		void Update() {
			//if (gameObject.GetComponent<PlayerMovement> ()) 
			//GameObject MyCam = gameObject.GetComponent<PlayerMovement> ().MyCamera;
			if (gameObject.transform.FindChild ("Camera(Clone)")) 
			{
				GameObject MyCamera = gameObject.transform.FindChild ("Camera(Clone)").gameObject;//GetComponent<PlayerMovement> ().MyCamera;
				if (MyCamera != null) {
					ShootTransform = MyCamera.transform;
					//ShootPosition = MyCamera.transform.position;
					//ShootForwardVector = MyCamera.transform.forward;
					//SpawnRotationQuaternion = MyCamera.transform.rotation;
				
					if (Cursor.lockState != CursorLockMode.Locked) {
						ShootPosition = Camera.main.ScreenPointToRay (Input.mousePosition).origin;
					}
				}
			}

			UpdateStats ();
			base.ShootProjectiles ();
			{
				RayCastAll ();
			}
		}
		
		// Raycast for terrain
		public void RayCastAll() {
			IsHitBlocks = false;
			IsMouseHit = false;
			// select only zones 1 to 8
			LayerMask IgnoreZonesLayer = (1 << 8);
			IgnoreZonesLayer = ~IgnoreZonesLayer;

			RaycastHit HitGameObject = GetMouseHitWithLayer (IgnoreZonesLayer);
			SelectedBlock = null;
			if (HitGameObject.transform != null)
				if (HitGameObject.transform.gameObject != null) 
			{
				GameObject HitObject = HitGameObject.transform.gameObject;
				MouseHit = HitGameObject;
				//IsHitBlocks = true;
				IsMouseHit = true;
				if (HitObject != null)
				if (HitObject.tag == ("Terrain")) {	// if what is hit is a terrain object
					IsHitBlocks = true;
					// this gets invalid cast exceptions alot
					try {
						SelectedBlock =  (Block) Terrain.GetBlock (MouseHit, false);
					} catch {
						
					}
				}
				else if (HitObject.tag == ("Character") || HitObject.tag == ("Turret")) 
				{
					BaseCharacter SelectedChar = HitObject.GetComponent <BaseCharacter> ();
					if (SelectedChar)
					{
						// if not dead and not self, select
						if (SelectedChar.PlayerIndex != PlayerIndex && !SelectedChar.MyStats.IsDead)
							MouseOverPlayer = HitObject;
					}
				}
			}
			if (GetManager.GetZoneManager ().IsShowZones) {
				RaycastHit HitZone = GetMouseHitWithLayer (BuildingZoneLayer);
				// ray cast for our zones
				if (HitZone.transform != null)
				if (HitZone.transform.gameObject != null) {
					MouseOverZone = HitZone.collider.gameObject;
					Zone MyZone = MouseOverZone.GetComponent<Zone> ();
					MyZone.MouseOver ();
				}
			}
		}

		public RaycastHit GetMouseHitWithLayer(LayerMask MyLayerMask) {
			RaycastHit hit;
			Transform CamTransform;
			if (Camera.main != null) {
				CamTransform = Camera.main.transform;
			} else {
				CamTransform = gameObject.transform;
			}
			Vector3 StartPosition = CamTransform.transform.position;
			Vector3 Direction = CamTransform.transform.forward;
			Ray ray; 
			//if (MyGameManager.MyCameraMode == CameraMode.FirstPerson) 
			if (Camera.main != null) {
				//ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (Cursor.lockState == CursorLockMode.Locked) {
					ray = Camera.main.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));	// maybe for spray add extra to this
					//ray.origin = StartPosition;
					//ray.direction = Direction;
				} else
					ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			} else {
				ray = new Ray ();
			}
			if (Physics.Raycast (ray.origin, ray.direction, out hit, RayCastRange, MyLayerMask)) {
				Debug.DrawLine (ray.origin, hit.point, Color.green);	// Debug line for a ray that hits the layer
			} else {
				Debug.DrawLine (ray.origin, ray.origin + ray.direction*RayCastRange, Color.blue);		// debug for missing ray
			}
			return hit;
		}

		public Vector3 MouseToWorldPoint() {
			Vector3 Position;
			Position = Input.mousePosition;

			return Position;
		}
	}
}
