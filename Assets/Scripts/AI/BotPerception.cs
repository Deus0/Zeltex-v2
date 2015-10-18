using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//		Perception 	- A list of nearby characters, ally or foe
//				   	- Aggro metre, if hit by any of them, it increases percentage - resets when out of combat
//					- Contains look at code for head

// this should probaly be split into 2 classes
//		BotPerception	- includes where surrounding obstacles are, and enemies
//		BotCombat		- includes combat info

/*public enum AttackType {
	Melee,		// attacks up close, normally attacks the strongest
	Ranged,		// attacks from a distance
	Support	// targets allies - the lowest hp one, tries to heal them
};*/

public class BotPerception : MonoBehaviour {
	public Behaviour MyBehaviour;

	public List<BaseCharacter> NearByCharacters = new List<BaseCharacter>();
	public List<BaseCharacter> EnemyList = new List<BaseCharacter>();
	public List<BaseCharacter> AlliesList = new List<BaseCharacter>();

	//public bool IsAttacking = false;
	public bool IsCheckForEnemies = true;

	public bool IsInCombat = false;
	public bool IsConeVision = true;
	public float VisionRadius = 45;
	public bool IsCriticalCondition;	// less then 10% health	- (3 average damage hits left)
	public float AverageDamageDoneTo;	// average damage the bot is attacked with - if super high it will leave faster - records the last hits in the latest battle and averages them
	
	public float InCombatCoolDown = 10f;
	public float EnemyCheckRange = 12f;	
	public float ClosestDistance = 30f;

	
	public float AllyCheckLastTime= 0.0f;	// distance to point 
	public float AllyCheckCoolDown = 0f;

	public BaseCharacter AttackTarget;
	public BaseCharacter LastHitPlayer;	// the player who last hit this one
	public Player MyHittingPlayer;
	
	public float WallCheckRange = 10f;
	public float WallMovementForce = 3f;
	public Vector3 WallLeftLocation;
	public float WallLeftDistance;
	public Vector3 WallRightLocation;
	public float WallRightDistance;
	public Vector3 WallForwardLocation;
	public float WallForwardDistance;

	public float MinimumDistanceToTarget = 3;	// since target points are meant to be without obstacle, i can basically walk on them
	public float MinimumDistanceToEnemy = 3;	// this should take into account the bounds, and also the attack range
	public float MinimumDistanceToFollow = 3;	// this should be minimum the bounds of both this gameObject, and the follow target
	public float MinimumDistanceToAttackEnemy = 3;
	
	public float LastHitTime = 0;	// the last time the player was hit
	
	public bool IsAvoidWalls = true;
	public bool IsAggressive = true;
	public bool IsFlee = true;

	private bool IsFreeForAll = false;

	// Use this for initialization
	void Start () {
		if (GetManager.GetGameManager() != null)
			IsFreeForAll = GetManager.GetGameManager().FreeForAllMode;	// if no game manager?
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (IsAvoidWalls) 
		{
			//CheckForWalls ();
		}

		if (IsCheckForEnemies) 
		{
			PercieveCharacters(EnemyCheckRange);
		}

		CheckIfTargetIsUnavailable ();

		// if no longer a target
		if (AttackTarget == null && IsAggressive)
			SelectedTargetFromEnemyList ();
		CheckIfAttackTargetIsGone();
		if (AttackTarget)
			gameObject.GetComponent<BaseCharacter>().ShootTransform.LookAt(AttackTarget.transform.position);
		if (MyBehaviour == Behaviour.Attack) 
		{
			if (AttackTarget != null) 
			{
				BaseCharacter EnemyCharacter = AttackTarget.GetComponent<BaseCharacter>();
				if (EnemyCharacter.IsAlive()) {
					BotMovement MyMovement = gameObject.GetComponent<BotMovement>();

					MyMovement.TargetPosition = AttackTarget.transform.position;
					MyMovement.TargetLookAtPosition = AttackTarget.transform.position;
					Vector3 Difference = transform.position - MyMovement.TargetPosition;
					Difference.Normalize ();
					MyMovement.TargetPosition += Difference*MinimumDistanceToEnemy;
				}
			}
			AttackTargetWithInventory ();
		}
		else if (MyBehaviour == Behaviour.Mine) 
		{
			MineWithInventory();
		}
	}
	public bool ShouldJump(float DistanceToJump) {
		return (WallForwardDistance < DistanceToJump);
	}
	public Vector3 GetWallAvoidanceForce() {
		Vector3 WallAvoidanceForce = new Vector3 (0, 0, 0);
		if (WallLeftDistance < WallCheckRange)
			WallAvoidanceForce -= WallMovementForce*(WallCheckRange-WallLeftDistance)*(WallLeftLocation- transform.position);
		if (WallRightDistance < WallCheckRange)
			WallAvoidanceForce -=  WallMovementForce*(WallCheckRange-WallRightDistance)*(WallRightLocation- transform.position);
		if (WallForwardDistance < WallCheckRange)
			WallAvoidanceForce -=  WallMovementForce*(WallCheckRange-WallForwardDistance)*(WallForwardLocation- transform.position);
		return WallAvoidanceForce;
	}

	public void CheckForWalls() {
		WallLeftLocation = transform.position + -transform.right * WallCheckRange;
		WallRightLocation = transform.position + transform.right * WallCheckRange;
		WallForwardLocation = transform.position + transform.forward * WallCheckRange;
		
		RaycastHit hit;
		//if (Physics.Raycast (ray, out hit, WallCheckRange)) {
		if (Physics.Raycast (transform.position, -transform.right, out hit, WallCheckRange)) {
			WallLeftLocation = hit.point;
		}
		WallLeftDistance = Vector3.Distance (WallLeftLocation, transform.position);
		if (Physics.Raycast (transform.position, transform.right, out hit, WallCheckRange)) {
			WallRightLocation = hit.point;
		}
		WallRightDistance = Vector3.Distance (WallRightLocation, transform.position);

		if (Physics.Raycast (transform.position, transform.forward, out hit, WallCheckRange)) {
			WallForwardLocation = hit.point;
		}
		WallForwardDistance = Vector3.Distance (WallForwardLocation, transform.position);
	}

	public void DebugPerception() {
		Vector3 DebugSize = new Vector3 (2, 2, 2);
		//Vector3 DebugCharacterPosition = transform.position - DebugSize / 2f + new Vector3 (0, Bounds.y, 0);
		Color32 ConeVisionColor = new Color32 (105, 0, 0, 255);
		// Cone Vision
		// if (ConeVision) {
		Vector3 RotationConeLeftBounds = (transform.forward - new Vector3 (VisionRadius / 2f, 0, 0).normalized).normalized;
		Vector3 RotationConeRightBounds = (transform.forward + new Vector3 (VisionRadius / 2f, 0, 0).normalized).normalized;
		RotationConeLeftBounds.y = 0;
		RotationConeRightBounds.y = 0;
		//Debug.LogError ("transform forward: " + transform.forward.ToString ());
		Debug.DrawLine (transform.position, transform.position + RotationConeLeftBounds * EnemyCheckRange, ConeVisionColor);
		Debug.DrawLine (transform.position, transform.position + RotationConeRightBounds * EnemyCheckRange, ConeVisionColor);
		Debug.DrawLine (transform.position + RotationConeLeftBounds * EnemyCheckRange, transform.position + RotationConeRightBounds * EnemyCheckRange, ConeVisionColor);
		
		Debug.DrawLine (transform.position, WallLeftLocation, Color.white);
		Debug.DrawLine (transform.position, WallRightLocation, Color.white);

		if (IsInCombat) {
			//DebugShapes.DrawSquare (DebugCharacterPosition, DebugSize, new Color (255, 0, 0));
		}
	}

	public bool CheckIfAttackTargetIsGone() {
		// If target has been killed
		if (AttackTarget == null) {
			//EnterNewMode (MyPreCombatBehaviour);
			return false;
		} else {
			BaseCharacter EnemyCharacter = (BaseCharacter) AttackTarget.GetComponent ("BaseCharacter");
			if (!EnemyCharacter.IsAlive ()) {
				//EnterNewMode (MyPreCombatBehaviour);
				return false;
			}
		}
		return true;
	}

	public void AddToCharacterList(BaseCharacter NewEnemy, List<BaseCharacter> CharacterList) {
		// should check if already in list first
		bool IsInList = false;
		for (int i = 0; i < CharacterList.Count; i++) {
			if (NewEnemy == CharacterList[i])
			{
				IsInList = true;
				i = CharacterList.Count;
			}
		}
		if (!IsInList)
			CharacterList.Add (NewEnemy);
	}
	// Vision
	public static List<BaseCharacter> GetNearByCharacters(GameObject MyObject, float CheckRange, bool IsConeVision, float VisionRadius) {
		List<BaseCharacter> NearByList = new List<BaseCharacter> ();
		if (!IsConeVision) {
			Collider[] hitColliders = Physics.OverlapSphere (MyObject.transform.position, CheckRange);
			for (int i = 0; i < hitColliders.Length; i++) {
				BaseCharacter NearByCharacter = hitColliders [i].gameObject.GetComponent<BaseCharacter>();
				if (NearByCharacter != null && hitColliders [i].gameObject != MyObject) {
					NearByList.Add (NearByCharacter);
				}
			}
		} else {	// for cone vision
			RaycastHit[] Targets = Physics.SphereCastAll(MyObject.transform.position, CheckRange, MyObject.transform.forward, CheckRange);
			for (int i = 0; i < Targets.Length; i++) {
				if (Vector3.Angle (MyObject.transform.forward, Targets[i].transform.position-MyObject.transform.position) <= VisionRadius) 
				if (Physics.Raycast(MyObject.transform.position, Targets[i].transform.position-MyObject.transform.position)){
					BaseCharacter NearByCharacter = Targets[i].collider.gameObject.GetComponent<BaseCharacter>();
					if (NearByCharacter != null && Targets[i].collider.gameObject != MyObject)
						NearByList.Add (NearByCharacter);
				}
			}
		}
		return NearByList;
	}
	public void PercieveCharacters(float CheckRange) {
		EnemyList.Clear ();
		AlliesList.Clear ();
		NearByCharacters.Clear ();
		NearByCharacters = BotPerception.GetNearByCharacters(gameObject, CheckRange, IsConeVision, VisionRadius);
		
		// check if they enemies or allies
		for (int i = 0; i < NearByCharacters.Count; i++) {
			if (IsEnemy(NearByCharacters[i]))
				AddToCharacterList(NearByCharacters[i], EnemyList);
			 else
			    AddToCharacterList(NearByCharacters[i], AlliesList);
		}
	}
	public bool IsEnemy(BaseCharacter PossibleEnemy) {
		return BotPerception.IsEnemy (gameObject.GetComponent <BaseCharacter> (), PossibleEnemy);
	}

	public static bool IsEnemy(BaseCharacter MyCharacter, BaseCharacter PossibleEnemy) {
		bool IsFriendlyFire = GetManager.GetGameManager().IsFriendlyFire();	// is everyon an enemy
		if (PossibleEnemy != null)
			if (PossibleEnemy.PlayerIndex != MyCharacter.PlayerIndex) 
				if (!PossibleEnemy.MyStats.IsDead)
					if (IsFriendlyFire || (!IsFriendlyFire && PossibleEnemy.ClanIndex != MyCharacter.ClanIndex))
				{
					return true;
				}
		return false;
	}

	public void SelectedTargetFromEnemyList() {
		if (EnemyList.Count == 0) {
			// if its out of range, set it to null
			AttackTarget = null;
		} else {
			BaseCharacter MyClosestEnemy = GetClosestEnemyFromList(gameObject, EnemyList);
			if (MyClosestEnemy != null) {
				AttackTarget = MyClosestEnemy;
			}
			if (gameObject && gameObject.GetComponent<BotMovement>())
				gameObject.GetComponent<BotMovement>().EnterNewMode(Behaviour.Attack);
		}
	}
	public void CheckIfTargetIsUnavailable() {
		if (AttackTarget != null) {
			BaseCharacter MyTarget = (BaseCharacter)AttackTarget.GetComponent ("BaseCharacter");
			if (MyTarget.MyStats.IsDead == true)
				AttackTarget = null;
			else if (Vector3.Distance (AttackTarget.transform.position,gameObject.transform.position) > EnemyCheckRange)
				AttackTarget = null;
		}
	}
	public void CheckEnemyListForAllies() {
		float ClosestRange = EnemyCheckRange;
		BaseCharacter ThisBaseCharacter = (BaseCharacter) gameObject.GetComponent ("BaseCharacter");
		for (int i = 0; i < EnemyList.Count; i++) {
			BaseCharacter EnemyBaseCharacter = (BaseCharacter) EnemyList[i].GetComponent ("BaseCharacter");
			if (ThisBaseCharacter.ClanIndex == EnemyBaseCharacter.ClanIndex) {
				EnemyList.RemoveAt(i);
			}
		}
	}
	public static BaseCharacter GetClosestEnemyFromList(GameObject MyObject, List<BaseCharacter> EnemyList) {
		int ClosestEnemyIndex = -1;
		float ClosestRange = 1000f;
		for (int i = EnemyList.Count-1; i >= 0; i--) {
			if (EnemyList[i] != null && !EnemyList[i].MyStats.IsDead) {
				float DistanceToEnemy = Vector3.Distance (MyObject.transform.position, EnemyList[i].transform.position);
				if (DistanceToEnemy < ClosestRange) {
					ClosestRange = DistanceToEnemy;
					ClosestEnemyIndex = i;
				}
			}
		}
		if (ClosestEnemyIndex != -1)
			return EnemyList [ClosestEnemyIndex];
		else
			return null;
	}

	// To go to combat class
	public bool IsRespondToAttacks = true;
	public void TakeHit(float TimeHit, BaseCharacter HittingPlayer) {
		LastHitTime = Time.time;
		AddToCharacterList (HittingPlayer, EnemyList);
		LastHitPlayer = HittingPlayer; 
		EnterCombat(true, true);			// this will make it attack or flee
		if (IsRespondToAttacks) {
			AttackTarget = HittingPlayer;
			if (gameObject.GetComponent<BotMovement> ())
				gameObject.GetComponent<BotMovement> ().EnterNewMode (Behaviour.Attack);
		}
	}

	// Sets the 
	public void EnterCombat(bool NewCombatState, bool IsRenew) {
		if (IsRenew) {
			LastHitTime = Time.time;
		}
		if (IsInCombat != NewCombatState) {
			IsInCombat = NewCombatState;
			LastHitTime = Time.time;
			if (IsInCombat) {
				//GetComponent<MeshRenderer>().material.color = GetManager.GetCharacterManager().InCombatColor;
			} else {
				//GetComponent<MeshRenderer>().material.color = new Color32(255,255,255,255);
			}
		}
	}
	
	public void UpdateCombatStatus() {
		// if havnt been hit for the cooldown, come out of combat
		if (IsInCombat) {
			if (InCombatCoolDown < Time.time - LastHitTime) {	// if time is up
				EnterCombat (false, false);
			}
		}
	}
	
	
	public void StartMining() {
		BaseCharacter BaseChar = gameObject.GetComponent <BaseCharacter>();
		BaseChar.MyInventory.IsShoot = true;
		BaseChar.MyInventory.SelectItem (2);
	}
	
	public void StopMining() {
		BaseCharacter BaseChar = gameObject.GetComponent <BaseCharacter>();
		BaseChar.MyInventory.IsShoot = false;
	}
	public void MineWithInventory() {
		//Debug.LogError ("Checking : " + gameObject.name + " For mining!");
		StopMining ();
		BaseCharacter MyChar = gameObject.GetComponent<BaseCharacter>();
		MyChar.IsHitBlocks = false;
		// select weapon
		// SelectedItem = 0;
		// get the weapons distance of attack - // or perhaps its aoe? use different ray traces per weapon type
		//if (MinimumDistanceToAttackEnemy > DistanceToTarget && 
		Vector3 TargetPosition = gameObject.GetComponent<BotMovement> ().TargetPosition;
		RaycastHit MouseHit;
		//Debug.DrawRay (transform.position, TargetPosition-transform.position, new Color32(255,0,0,255));
		if (Physics.Raycast (transform.position, TargetPosition-transform.position, out MouseHit, 15f)) {	// should be minimum mining distance
			GameObject HitTerrain = MouseHit.transform.gameObject;
			Vector3 HitBlockPosition = MouseHit.point - MouseHit.normal*0.5f;
			HitBlockPosition = new Vector3(Mathf.RoundToInt(HitBlockPosition.x),Mathf.RoundToInt(HitBlockPosition.y),Mathf.RoundToInt(HitBlockPosition.z));
			//Debug.LogError ("Maybe.. : " + gameObject.name + " For mining: " + HitBlockPosition.ToString() + " : " + TargetPosition.ToString());
			if (HitTerrain != null)
			if (HitTerrain.tag == ("Terrain")) 
			if (HitBlockPosition == TargetPosition) {	// if what is hit is a terrain object
					//Debug.Log ("Success : " + gameObject.name + " For mining!");
					StartMining();
					MyChar.IsHitBlocks = true;
					MyChar.MouseHit = MouseHit;
				try {
					MyChar.SelectedBlock =  (Block) Terrain.GetBlock (MouseHit, false);
				} catch {
					
				}
			}
			//Debug.LogError ("Hit: " + HitTerrain.name);
			Debug.DrawLine(transform.position, MouseHit.point,Color.black);	// draw a line from the start position to the hit position
		}
		else
			Debug.DrawLine(transform.position, transform.forward*15f,Color.white);	// draw a line from the start position to the hit position
	}
	// use something like ShootTransform, if shooting out of a gun!
	// Checks if conditions for attack a target are met
	public void AttackTargetWithInventory() {
		StopShooting();
		// select weapon
		// SelectedItem = 0;
		// get the weapons distance of attack - // or perhaps its aoe? use different ray traces per weapon type
		//if (MinimumDistanceToAttackEnemy > DistanceToTarget && 
		if (AttackTarget != null) {
			//Vector3 Direction = (transform.position-AttackTarget.transform.position).normalized;
			//gameObject.GetComponent<BotMovement>().TargetPosition = AttackTarget.transform.position+Direction*2f;
			//LookAtTarget(AttackTarget.transform.position);

			//gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles += (gameObject.GetComponent<BaseCharacter>().ShootTransformRotationOffset);

			//float AngleY = gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles.y;
			// need to limit heads rotation
			//gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles = new Vector3(gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles.x, 
			//                                                                                  AngleY, 
			//                                                                                  gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles.z);


			float AngleCheckRange = 0.2f;
			RaycastHit hit;
			float CheckDensity = 3f;
			for (float i = -AngleCheckRange; i <= AngleCheckRange; i += AngleCheckRange/CheckDensity)
				for (float j = -AngleCheckRange; j <= AngleCheckRange; j += AngleCheckRange/CheckDensity) 
				for (float k = -AngleCheckRange; k <= AngleCheckRange; k += AngleCheckRange/CheckDensity) {
					Vector3 Direction = gameObject.GetComponent<BaseCharacter>().ShootTransform.forward+new Vector3(i,j,k);
					if (Physics.Raycast (gameObject.GetComponent<BaseCharacter>().ShootTransform.position, Direction,out hit, MinimumDistanceToAttackEnemy)) {
					if (hit.collider.gameObject.GetComponent<BaseCharacter>() != null) {
						// if ray trace falls onto a character
						gameObject.GetComponent<BaseCharacter>().MouseHit = hit;
						StartShooting();
							i = AngleCheckRange; j = AngleCheckRange; k = AngleCheckRange;	// end search
					}
						Debug.DrawLine(gameObject.GetComponent<BaseCharacter>().ShootTransform.position, gameObject.GetComponent<BaseCharacter>().ShootTransform.position+MinimumDistanceToEnemy*Direction, Color.red);
				} else {
						Debug.DrawLine(gameObject.GetComponent<BaseCharacter>().ShootTransform.position, gameObject.GetComponent<BaseCharacter>().ShootTransform.position+MinimumDistanceToEnemy*Direction, Color.white);
				}
				}
			//gameObject.GetComponent<BaseCharacter>().ShootTransform.eulerAngles -= (gameObject.GetComponent<BaseCharacter>().ShootTransformRotationOffset);
		}
	}
	public void LookAtTarget(Vector3 Target) {
		//CurrentRotation = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
		Quaternion CurrentQuat = transform.rotation;
		Quaternion TargetQuat = transform.rotation;
		TargetQuat.SetLookRotation(Target);
		transform.rotation = Quaternion.Slerp (CurrentQuat, TargetQuat, Time.deltaTime * 5f);
		// limit turn - need to use time.deltaTime here
		//if (IsLockZRotation) {
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,transform.localEulerAngles.y,0);
			transform.localEulerAngles = new Vector3(0,transform.localEulerAngles.y,0);
		//}
	}
	
	public void StartShooting() {
		BaseCharacter BaseChar = gameObject.GetComponent <BaseCharacter>();
		BaseChar.MyInventory.IsShoot = true;
		BaseChar.MyInventory.SelectedItem = 0;
	}
	
	public void StopShooting() {
		BaseCharacter BaseChar = gameObject.GetComponent <BaseCharacter>();
		BaseChar.MyInventory.IsShoot = false;
	}
}
