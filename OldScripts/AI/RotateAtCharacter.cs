using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
public class RotateAtCharacter : MonoBehaviour {
	public GameObject MyGunMesh;	// if set to null, it will be whatever gameobject the script is attached to
	private Transform MyGunMeshTransform;
	public bool CanShootThroughWalls = false;
	public bool IsPivotOffset = true;
	public float TurnSpeed = 10f;
	private float CheckCoolDown = 0.5f;
	public float AttackDistance = 8f;
	private BaseCharacter MyTargetCharacter;
	private float PivotY;
	private Vector3 OriginalPosition;
	private Vector3 RotationOffset = new Vector3(90,0,0);
	private float LastChecked;
	private bool HasLoaded = false;

	void Start () {
		if (MyGunMesh == null)
			MyGunMeshTransform = gameObject.transform;
		else
			MyGunMeshTransform = MyGunMesh.transform;
		LastChecked = Time.time + Random.Range (0f,1.5f);
		CheckCoolDown +=  + Random.Range (0f,1.5f);
		OriginalPosition = MyGunMeshTransform.position;
		MyGunMeshTransform.eulerAngles = MyGunMeshTransform.eulerAngles + RotationOffset;
		UpdatePivot ();
	}

	// Update is called once per frame
	void Update () {
		if (!HasLoaded)
		if (gameObject.GetComponent<GetVoxelModel> ()) {
			if (gameObject.GetComponent<GetVoxelModel> ().HasLoaded) {
				UpdatePivot ();
				HasLoaded = true;
			}
		}
		if ( Time.time - LastChecked > CheckCoolDown) {
			FindClosestCharacter ();
			LastChecked = Time.time;
		}
		TargetCharacter ();
	}
	public void UpdatePivot() {
		if (IsPivotOffset) {
			if (MyGunMeshTransform != transform) {
				MeshRenderer MyMeshRenderer = MyGunMeshTransform.gameObject.GetComponent<MeshRenderer> ();
				if (MyMeshRenderer != null)
					PivotY = MyMeshRenderer.bounds.size.y;
			} else {
				BoxCollider MyCollider = MyGunMeshTransform.gameObject.GetComponent<BoxCollider> ();
				if (MyCollider != null)
					PivotY = MyCollider.bounds.size.y / 2f;
				MeshCollider MyMeshCollider = MyGunMeshTransform.gameObject.GetComponent<MeshCollider> ();
				if (MyMeshCollider != null)
					PivotY = MyMeshCollider.bounds.size.y;
			}
		}
	}
	public bool IsShootAtTarget = true;
	// uses pivot offset to rotate the cannon towards the target
	public void TargetCharacter() 
	{
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
		MyCharacter.IsMouseHit = false;
		Quaternion TargetAngle = MyGunMeshTransform.rotation;
		if (MyTargetCharacter != null) {
			Quaternion CurrentAngle = MyGunMeshTransform.rotation;
			// the angle facing the player
			TargetAngle = Quaternion.LookRotation (MyTargetCharacter.transform.position - OriginalPosition);
			TargetAngle.eulerAngles += RotationOffset;

			MyCharacter.ShootTransform = MyGunMeshTransform;
			MyCharacter.ShootPositionOffset = -MyGunMeshTransform.up * PivotY;
			MyCharacter.ShootPosition = MyCharacter.ShootTransform.transform.position + MyCharacter.ShootPositionOffset;
			MyCharacter.ShootForwardVector = MyGunMeshTransform.up;
			MyCharacter.IsShootUpwards = true;
			MyCharacter.ShootTransformRotationOffset = -RotationOffset;
			if (IsShootAtTarget) {
				RaycastHit MyHit;
				if (!CanShootThroughWalls) {
					Debug.LogError ("Cannot shoot through walls");
					if (Physics.Raycast (MyCharacter.ShootPosition, MyCharacter.ShootForwardVector, out MyHit, AttackDistance)) {
						Debug.DrawLine (MyCharacter.ShootPosition, MyCharacter.ShootPosition + MyCharacter.ShootForwardVector * AttackDistance, Color.blue);
						if (MyHit.collider.gameObject == MyTargetCharacter.gameObject) {
							MyCharacter.MouseHit = MyHit;
							FireTheLasers ();
						}
					} else {
						Debug.DrawLine (MyCharacter.ShootPosition, MyCharacter.ShootPosition + MyCharacter.ShootForwardVector * AttackDistance, Color.red);
					}
				} else if (CanShootThroughWalls) { // i should do another raycast, ignoring the chunks layer here
					Debug.DrawLine (MyCharacter.ShootPosition, MyTargetCharacter.transform.position, Color.white);
					FireTheLasers ();
				}
				//Debug.DrawRay (MyCharacter.ShootPosition, MyCharacter.ShootForwardVector, Color.red);
			} else {
				MyCharacter.MyInventory.IsShoot = false;
			}
		} else {
			MyCharacter.MyInventory.IsShoot = false;
		}

		float TurnSpeed2 = TurnSpeed * Time.deltaTime;
		MyGunMeshTransform.rotation = Quaternion.Lerp (MyGunMeshTransform.rotation, TargetAngle, TurnSpeed);
		MyGunMeshTransform.position = OriginalPosition + MyGunMeshTransform.up * PivotY;
	}
	public void FireTheLasers() {
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
		MyCharacter.MyInventory.IsShoot = true;
		MyCharacter.IsMouseHit = true;
	}

	public void FindClosestCharacter() {
		if (MyTargetCharacter != null) {	// i should get the spells range for attack distance
			if (Vector3.Distance(transform.position, MyTargetCharacter.transform.position) >= AttackDistance || MyTargetCharacter.MyStats.IsDead) {
				MyTargetCharacter = null;
			}
		}

		if (MyTargetCharacter == null) {
			BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
			List<BaseCharacter> NearbyCharacters = BotPerception.GetNearByCharacters (gameObject, AttackDistance, false, 0);
			for (int i = NearbyCharacters.Count-1; i >= 0; i--) {
				if (!BotPerception.IsEnemy (MyCharacter, NearbyCharacters [i])) {
					NearbyCharacters.RemoveAt (i);
				}
			}
			BaseCharacter ClosestCharacter = BotPerception.GetClosestEnemyFromList (gameObject, NearbyCharacters);
			MyTargetCharacter = ClosestCharacter;
		}
	}
}
}