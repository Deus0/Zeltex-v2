using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 		Movement:
//				- Move to Point
//				- A list of target movement points
//				- May contain a list of actions, like move to point, wait 5 seconds, move to other point etc
//				- Body to face direction of movement
//				- Uses Starfinder class to generate path and avoid obstacles
//				- Uses Map, with building locations, to generate paths around a building structure, or between buildings

// Maybe this should be split into 2 classes, one that simply moves to a position
//	and on that creates the positions and sets the new positions

// The Main AI class
// still lots of work to be done - optimizations
// it becomes laggy with too many bots
// need a simpler one and advanced one, so things like bosses will have the more processor heavy ones

public class NodePath {

};
public class NodeNetwork {

};


public enum Behaviour{
	Wait,
	Patrol,
	Wander,
	Attack,
	Flock,
	Flee,
	Follow,
	Mine
};

// Action performed when follow target character
public enum FollowAction {
	Behind,		// Simply follows the target
	NextTo,		// Follows the target, but on their left or right side
	InFrontOf,	// Tries to get in front of target
	Attack		// bot attempts to use a spell on the target
};

public enum AttackType {
	Heal,		// Attempts to heal the target
	Sheild,		// Attempts to cast defensive buffs on the target
	Damage		// Attempts to injure the target
};

// how each individual bot will react to a confrontation with another clan (outside a city)	- inside a city the gaurds will attack if fights break out
public enum AttackBehaviour {
	Coward,				// runs away from any battle
	HyperAggressive,	// attacks everything - seeks battle (wanders for battle)
	Defensive,			// attacks if attacked first, runs away if critical condition
	PassiveAggressive,	// if attacked, will fight to the death but won't attack first
	Aggressive			// Attacks if they are in range, but mainly just patrols their homes
};

public class MovementBehaviour {

};

public class BotMovement : MonoBehaviour {
	private GameManager MyGameManager;
	public bool IsDebugLines = false;
	// in perception
	public float MinimumDistanceToTarget = 3;	// since target points are meant to be without obstacle, i can basically walk on them

	// movement states
	public Behaviour MyBehaviour;
	
	public Vector3 TargetPosition;
	public GameObject FollowTarget;		// should be outside the class
	public float MovementSpeed = 10f;	// Target movement speed
	public float SeekSlowDownDistance = 5f;
	// gravity etc
	public bool IsLockZRotation = true;
	public bool CanFly = true;
	public Vector3 GravityForce = new Vector3(0,-100f,0);
	public Vector3 StickToGroundForce = new Vector3(0,-0.1f,0);

	// To go in Pathed Movement
	private float NewPositionCoolDown = 1.0f;	// the check for new position
	public List<TargetPoint> TargetPositions = new List<TargetPoint>();
	public float PatrolPathRange = 15;	// used to generate path points

	public Vector3 TargetLookAtPosition;

	private float LastTime= 0.0f;	// distance to point 
	public int TargetPointsIndex = 0;
	public Vector3 MaximumSpeed = new Vector3(1.5f,1.5f,1.5f);
	public Vector3 MaximumTurn = new Vector3(1.5f,1.5f,1.5f);
	public Vector3 MaximumForce = new Vector3(0.25f,0.25f,0.25f);
	// the wander force variable
	public float wandertheta;
	public Vector3 BorderMaximum;


	//www.opengameart.org
	// copied stuff
	
	// AI

	public bool IsBorders = false;
	public bool IsMoveToPosition = true;
	public bool IsLimitSpeed = true;

	public float TurnSpeed = 1;
	public bool IsJump;
	public float JumpForce = 15;
	public bool HasReachedPosition = false;
	public bool IsOnGround;
	public Vector3 GroundPosition;
	// used to judge how far away from objects it is
	public Vector3 Bounds;

	// wandering
	public Vector3 WanderBoundsMaximum;
	public Vector3 WanderBoundsMinimum;
	public bool IsWanderLimited = false;

	// Use this for initialization
	void Start () {
		LoadTargetDataFromMap ();
		FindNewPosition ();
	}
	void Awake() {
		GameManager[] GameManagers = FindObjectsOfType(typeof(GameManager)) as GameManager[];
		foreach (GameManager game in GameManagers) {
			MyGameManager = game;
		}
	}
	// Update is called once per frame
	void Update () {
		BaseCharacter MyBaseCharacter = (BaseCharacter)gameObject.GetComponent ("BaseCharacter");
		if (MyBaseCharacter != null)
			if (MyBaseCharacter.IsAlive ()) 
		{

			if (IsDebugLines) {
				DrawDebugMovement();
			}
		}
		UpdateMovement();
		//Jump();
		RenderTargetPoints();
	}

	public void UpdateMovement() {
		BaseCharacter MyBaseCharacter = (BaseCharacter)gameObject.GetComponent ("BaseCharacter");
		if (MyBaseCharacter != null)
		if (MyBaseCharacter.IsAlive ()) {
			if (IsMoveToPosition) {
				float DistanceToTarget = Vector3.Distance(transform.position, TargetPosition);
				DistanceToTarget -= Bounds.magnitude;
				//Debug.LogError("Offsetting distance by magnitude: " + Bounds.magnitude);
				// if its attacking, it will not need to find new target more then the cooldown, however for the other movements the position changes to a new part of the path nodes
				if ((Time.time - LastTime > NewPositionCoolDown && MinimumDistanceToTarget > DistanceToTarget)) {	// if within the distance or the cooldown has passed, move on to next position
					LastTime = Time.time;
					if (!HasReachedPosition)  {
						if (MinimumDistanceToTarget > DistanceToTarget)
							HasReachedPosition = true;
					}
					FindNewPosition ();
				}
				MoveToPosition (TargetPosition);
				if (IsLimitSpeed) {
					//LimitMovementSpeed ();
				}
			}
			ApplyGravity(CanFly);
		} else {
			ApplyGravity(false);
		}
	}
	
	void ApplyGravity (bool CanFly2) {
		Rigidbody MyRigidBody = gameObject.GetComponent<Rigidbody>();
		if (MyRigidBody) {
			if (!CanFly2) {
				if (!IsGrounded ())
					MyRigidBody.AddForce (GravityForce);
			}
		}
		//else if (CanFly2 && IsGrounded ())
		//	MyRigidBody.AddForce (-GravityForce * MyRigidBody.mass);
	}

	public bool IsGrounded () {
		float Threshold = -0.01f;
		IsOnGround = false;
		RaycastHit hit;
		float GroundCheckRange = Bounds.y + Threshold;
		if (CanFly)
			GroundCheckRange *= 2;
		GroundPosition = transform.position + -transform.up * GroundCheckRange;

		if (Physics.Raycast (transform.position, -transform.up, out hit, GroundCheckRange)) {
			GroundPosition = hit.point;
			IsOnGround = true;
			return true;
		}
		return false;
	}

	public void DrawDebugMovement() {
		Rigidbody MyRigidBody = gameObject.GetComponent<Rigidbody>();
		Vector3 NormalizedVelocity = MyRigidBody.velocity;
		NormalizedVelocity.Normalize ();

		Color32 MovingTowardsColor = new Color32 (0, 0, 200, 255);
		Color32 DesiredPositionColor = new Color32 (0, 200, 200, 255);


		if (IsOnGround)
			Debug.DrawLine (transform.position, GroundPosition, Color.red);
		else
			Debug.DrawLine (transform.position, GroundPosition, Color.white);

		
		// Direction needed to go - target direction
		Debug.DrawLine (transform.position, TargetPosition, Color.green);
		
		// Target Position
		Debug.DrawLine (TargetPosition, TargetPosition + new Vector3 (0, 5, 0), Color.white);
		
		// Direction Traveling
		Debug.DrawLine (transform.position, transform.position + transform.forward * 5f, Color.blue);
		Vector3 DebugSize = new Vector3 (2, 2, 2);
		// Raytrace closes ground point and show that
		//Vector3 GroundPosition = new Vector3 (transform.position.x, 0, transform.position.z);
		//Debug.DrawRay(transform.position, 	GroundPosition, Color.blue);
		//Gizmos.DrawWireCube(TargetPosition-transform.position, new Vector3(15, 15, 15));

		// Direction Velocity Traveling to - White
		Debug.DrawLine (transform.position, transform.position + NormalizedVelocity * 10f, 					MovingTowardsColor);
		DebugShapes.DrawSquare (transform.position + NormalizedVelocity * 10f - DebugSize / 2f, DebugSize, 	MovingTowardsColor);

		// Desired target position
		DebugShapes.DrawSquare (TargetLookAtPosition + DebugSize / 2f, DebugSize, DesiredPositionColor);							// desired LookAtTarget position - green
		//DebugShapes.DrawSquare (TargetPosition, DebugSize*1.2f, new Color(255,0,0));
		DebugShapes.DrawSquare (TargetPosition - DebugSize / 2f, DebugSize, DesiredPositionColor);				// desired target position - RED

		Vector3 DebugCharacterPosition = transform.position - DebugSize / 2f + new Vector3 (0, Bounds.y, 0);
		 {
			if (MyBehaviour == Behaviour.Wander)
				DebugShapes.DrawSquare (DebugCharacterPosition, DebugSize, new Color (0, 255, 0));
			else if (MyBehaviour == Behaviour.Patrol)
				DebugShapes.DrawSquare (DebugCharacterPosition, DebugSize, new Color (0, 0, 255));
			else if (MyBehaviour == Behaviour.Wait)
				DebugShapes.DrawSquare (DebugCharacterPosition, DebugSize, new Color (55, 55, 55));
			else
				DebugShapes.DrawSquare (DebugCharacterPosition, DebugSize, new Color (255, 255, 255));
		}
	}
	/*public void Jump() {
		if (!CanFly)
			CheckForJump ();
		if (IsJump && IsOnGround && !CanFly) {
			gameObject.GetComponent<Rigidbody>().AddForce (-JumpForce*100.0f * GravityForce.normalized * gameObject.GetComponent<Rigidbody>().mass);
			IsJump = false;
		}
	}*/
	public void LimitMovementSpeed() {
		Rigidbody MyRigidBody = gameObject.GetComponent<Rigidbody>();
		MyRigidBody.velocity = Vector3.ClampMagnitude (MyRigidBody.velocity, MaximumSpeed.magnitude);
		// Limit the speed
		/*if (MyRigidBody.velocity.x > MaximumSpeed.x)
			MyRigidBody.velocity = new Vector3 (MaximumSpeed.x, MyRigidBody.velocity.y, MyRigidBody.velocity.z);
		if (MyRigidBody.velocity.y > MaximumSpeed.y)
			MyRigidBody.velocity = new Vector3 (MyRigidBody.velocity.x, MaximumSpeed.y, MyRigidBody.velocity.z);
		if (MyRigidBody.velocity.z > MaximumSpeed.z)
			MyRigidBody.velocity = new Vector3 (MyRigidBody.velocity.x, MyRigidBody.velocity.y, MaximumSpeed.z);
		
		if (MyRigidBody.velocity.x < -MaximumSpeed.x)
			MyRigidBody.velocity = new Vector3 (-MaximumSpeed.x, MyRigidBody.velocity.y, MyRigidBody.velocity.z);
		if (MyRigidBody.velocity.y < -MaximumSpeed.y)
			MyRigidBody.velocity = new Vector3 (MyRigidBody.velocity.x, -MaximumSpeed.y, MyRigidBody.velocity.z);
		if (MyRigidBody.velocity.z < -MaximumSpeed.z)
			MyRigidBody.velocity = new Vector3 (MyRigidBody.velocity.x, MyRigidBody.velocity.y, -MaximumSpeed.z);*/
	}
		// Patrol Paths
	public void CreateTargetData() {
		/*// set up our initial target positions
		Vector3 newPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
		TargetPositions.Add (newPosition);
		newPosition = new Vector3 (transform.position.x + 2*PatrolPathRange, transform.position.y, transform.position.z);
		TargetPositions.Add (newPosition);
		newPosition = new Vector3 (transform.position.x + 2*PatrolPathRange, transform.position.y, transform.position.z + 2*PatrolPathRange);
		TargetPositions.Add (newPosition);
		newPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z + 2*PatrolPathRange);
		TargetPositions.Add (newPosition);
		TargetPosition = transform.position;*/
	}

	// when i've already placed target points on a map
	// can also load player specific target points
	public void LoadTargetDataFromMap() {
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
		TargetPoint[] TargetPoints = FindObjectsOfType (typeof(TargetPoint)) as TargetPoint[];
		for (int i = 0; i < TargetPoints.Length; i++) {
			if (TargetPoints[i].tag == "TargetPoint")
			for (int j = 0; j < TargetPoints.Length; j++) {
				TargetPoint MyTarget = (TargetPoint) TargetPoints[j].GetComponent ("TargetPoint");
				if ((MyTarget.IsPlayerSpecific && MyTarget.PlayerIndex == MyCharacter.PlayerIndex) || !MyTarget.IsPlayerSpecific){
					if (TargetPositions.Count == MyTarget.TargetPointIndex) {
						TargetPositions.Add (MyTarget);
						j = TargetPoints.Length;
					}
				}
			}
		}
	}

	public void CheckIfFollowTargetIsGone() {
		// If target has been killed
		if (FollowTarget == null) {
			EnterNewMode (Behaviour.Wait);
		} else {
			BaseCharacter EnemyCharacter = (BaseCharacter) FollowTarget.GetComponent ("BaseCharacter");
			if (!EnemyCharacter.IsAlive ()) {
				EnterNewMode (Behaviour.Wait);
			}
		}
	}
	
	public void EnterNewMode(Behaviour NewBehaviour) {
		//if (MyBehaviour != NewBehaviour) 
		{
			MyBehaviour = NewBehaviour;
			HasReachedPosition = true;	// which resets any patrol points
			TargetPointsIndex = 0;
			FindNewPosition ();
		}
	}
	// will loop through the positions in the target array
	public void FindNewPosition() {
		if (Behaviour.Attack == MyBehaviour) {

		} else if (Behaviour.Mine == MyBehaviour) {
			FindNewPatrolPosition ();
			// now offset by distance of bounds

		} else if (Behaviour.Follow == MyBehaviour) {
			CheckIfFollowTargetIsGone ();	
			if (FollowTarget) {
				BaseCharacter EnemyCharacter = (BaseCharacter)FollowTarget.GetComponent ("BaseCharacter");
				if (EnemyCharacter.IsAlive ()) {
					TargetPosition = FollowTarget.transform.position;
					TargetLookAtPosition = FollowTarget.transform.position;
					Vector3 Difference = transform.position - TargetPosition;
					Difference.Normalize ();
					TargetPosition += Difference * MinimumDistanceToTarget;
				}
			}
		} else if (Behaviour.Flee == MyBehaviour) {
			/*
				BaseCharacter EnemyCharacter = (BaseCharacter) AttackTarget.GetComponent ("BaseCharacter");
				TargetPosition = AttackTarget.transform.position;
				Vector3 Direction = (AttackTarget.transform.position-transform.position).normalized;
				float DistanceFromEnemy = Vector3.Distance(AttackTarget.transform.position,transform.position);
				TargetLookAtPosition = transform.position + Direction*DistanceFromEnemy*2;// look in opposite direction of enemy
				*/
		} else if (Behaviour.Wander == MyBehaviour) {
			// this behaviour finds target position everyframe
			//TargetPosition = transform.position + GetRandomPosition ();
			//TargetPosition.y = transform.position.y;
			//TargetLookAtPosition = TargetPosition;
		} else if (Behaviour.Patrol == MyBehaviour) {	// go to different points in array
			FindNewPatrolPosition ();
		} else if (Behaviour.Wait == MyBehaviour) {
			TargetPosition = transform.position;
			TargetLookAtPosition = TargetPosition;
		} else {
			TargetPosition = transform.position;
		}
	}

	public void FindNewPatrolPosition() {
		if (HasReachedPosition) {
			TargetPointsIndex++;
			HasReachedPosition = false;
		}
		if (TargetPointsIndex >= TargetPositions.Count)
			TargetPointsIndex = 0;
		if (TargetPositions.Count > 0 && TargetPointsIndex >= 0 && TargetPointsIndex < TargetPositions.Count) 
		{
			if (TargetPositions [TargetPointsIndex])
				TargetPosition = TargetPositions [TargetPointsIndex].transform.position;
			else
				TargetPositions.RemoveAt (TargetPointsIndex);
		}
	}


		// check for jump
	/*public void CheckForJump() {
		RaycastHit hit;
		Vector3 JumpForwardLocation = transform.position + new Vector3 (0, -Bounds.y+0.1f, 0);//+ transform.forward * WallCheckRange;
		if (Physics.Raycast (JumpForwardLocation, transform.forward, out hit, Bounds.magnitude+0.1f)) {
			//WallForwardLocation = hit.point;
			// also && Target position is in front of block
			if (hit.collider.gameObject.tag == "Terrain")
				IsJump = true;
			else 
				IsJump = false;
		} else {
			IsJump = false;
		}
	}*/
// AI MOVEMENT
	
	public void SeekTarget(Vector3 Target) {
		//Debug.LogError (gameObject.name + " is seeking target");
		if (gameObject.GetComponent<BotGroundMove> ()) {
			gameObject.GetComponent<BotGroundMove> ().MoveToTarget(Target, TargetLookAtPosition, Behaviour.Attack == MyBehaviour);
		} else {
			Rigidbody rigidBody = (Rigidbody)(gameObject.GetComponent ("Rigidbody"));
			rigidBody.AddForce (Seek (Target));
		}
	}
	
	// the prizes algorithm
	// this is what makes the movement paths
	public Vector3 Seek(Vector3 Target) {
		float DistanceToTarget = Vector3.Distance (transform.position, TargetPosition);

		Rigidbody MyRigidBody = (Rigidbody)(gameObject.GetComponent("Rigidbody"));
		float SeekMovementSpeed = MovementSpeed * MyRigidBody.mass;
		Vector3 DesiredSpeed = (Target - transform.position);
		DesiredSpeed.Normalize(); 
		
		if (DistanceToTarget < SeekSlowDownDistance) { //set the magnitude according to how close we are.
			float m = SeekMovementSpeed * (DistanceToTarget / SeekSlowDownDistance);
			DesiredSpeed *= (m);
		} else {
			DesiredSpeed *= (SeekMovementSpeed);	// MovementSpeed
		}
		// for fleeing make it run away in the same way
		if (MyBehaviour == Behaviour.Flee)
			DesiredSpeed *= -1f;
		
		// Now adjust seek force by a wall avoidance force
		// make a new function - WallAvoidanceForce
		if (gameObject.GetComponent<BotPerception> ()) {
			//DesiredSpeed += gameObject.GetComponent<BotPerception> ().GetWallAvoidanceForce();
		}
		
		Vector3 Velocity;
		Velocity = MyRigidBody.velocity;
		
		Vector3 SteerForce = DesiredSpeed - Velocity;
		
		// Limit the movement force
		Vector3 TemporaryMaximumForce = new Vector3 (MaximumForce.x * MyRigidBody.mass, MaximumForce.y * MyRigidBody.mass, MaximumForce.z * MyRigidBody.mass);
		SteerForce = LimitForce (SteerForce, TemporaryMaximumForce);
		if (!CanFly) {
			SteerForce.y = -MaximumForce.y;
		}
		return SteerForce;
	}
	public Vector3 LimitForce(Vector3 MovementForce, Vector3 MaximumForce) {
		if (MovementForce.x > MaximumForce.x)
			MovementForce.x = MaximumForce.x;
		if (MovementForce.x < -MaximumForce.x)
			MovementForce.x = -MaximumForce.x;
		if (MovementForce.y > MaximumForce.y)
			MovementForce.y = MaximumForce.y;
		if (MovementForce.z < -MaximumForce.z)
			MovementForce.z = -MaximumForce.z;
		return MovementForce;
	}
	public void LookAtTarget(Vector3 Target) {
		if (gameObject.GetComponent<BotGroundMove> () == null) {
			//CurrentRotation = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
			Quaternion CurrentQuat = transform.rotation;
			Quaternion TargetQuat = transform.rotation;
			TargetQuat.SetLookRotation (Target);
			transform.rotation = Quaternion.Slerp (CurrentQuat, TargetQuat, Time.deltaTime * TurnSpeed);
		}
	}
	// Main Movement Function
	public void MoveToPosition(Vector3 Target) {
		if (!CanFly)
			Target.y = transform.position.y;	// should be ground position lel, where ever that is

		if (Behaviour.Wander == MyBehaviour) {
			TargetPosition = Wander (Target);
			if (IsWanderLimited) {
				TargetPosition.x = Mathf.Clamp (TargetPosition.x, WanderBoundsMinimum.x, WanderBoundsMaximum.x);
				TargetPosition.y = Mathf.Clamp (TargetPosition.y, WanderBoundsMinimum.y, WanderBoundsMaximum.y);
				TargetPosition.z = Mathf.Clamp (TargetPosition.z, WanderBoundsMinimum.z, WanderBoundsMaximum.z);
			}
			TargetLookAtPosition = TargetPosition;
			//Seek (TargetPosition);
			SeekTarget (Target);
		} else if (Behaviour.Wait == MyBehaviour) {
			TargetLookAtPosition = transform.forward*5f;
			SeekTarget (TargetPosition);
			// if a bot gets moved they will move back to position
		} else if (Behaviour.Flock == MyBehaviour) {
			Flock ();
		} else if (Behaviour.Attack == MyBehaviour) {
			//if (AttackTarget) {
			//	TargetLookAtPosition = FollowTarget.transform.position;
			//} else {	// else it is dead, or moved out of position
				
			//}
			SeekTarget (Target);
		} else if (Behaviour.Follow == MyBehaviour) {
			if (FollowTarget) {
				TargetLookAtPosition = FollowTarget.transform.position;
			} else {	// else it is dead, or moved out of position
				
			}
			SeekTarget (Target);
		} else {
			Rigidbody MyRigidBody = gameObject.GetComponent <Rigidbody>();
			if (MyRigidBody) {
				Vector3 NormalizedVelocity = MyRigidBody.velocity;
				NormalizedVelocity.Normalize ();
				TargetLookAtPosition = transform.position+NormalizedVelocity * 10f;
			} else {
				TargetLookAtPosition = TargetPosition;
			}
			SeekTarget (Target);
		}
		if (IsBorders)
			Borders (BorderMaximum);
		if (MyBehaviour != Behaviour.Attack)
			LookAtTarget(TargetLookAtPosition);
	}
	public void Borders(Vector3 Maximum) {
		bool IsWrap = false;
		float Radius = 1f;
		if (IsWrap) {
			if (transform.position.x < -Radius) 
				transform.position = new Vector3 (Maximum.x + Radius, transform.position.y, transform.position.z);
			if (transform.position.x > Maximum.x + Radius) 
				transform.position = new Vector3 (-Radius, transform.position.y, transform.position.z);
			if (transform.position.y < -Radius) 
				transform.position = new Vector3 (transform.position.x, Maximum.y + Radius, transform.position.z);
			if (transform.position.y > Maximum.y + Radius) 
				transform.position = new Vector3 (transform.position.x, -Radius, transform.position.z);
			if (transform.position.z < -Radius) 
				transform.position = new Vector3 (transform.position.x, transform.position.y, Maximum.z + Radius);
			if (transform.position.z > Maximum.z + Radius) 
				transform.position = new Vector3 (transform.position.x, transform.position.y, -Radius);
		} else {
			if (transform.position.x < -Radius) 
				transform.position = new Vector3 (Radius, transform.position.y, transform.position.z);
			if (transform.position.x > Maximum.x + Radius) 
				transform.position = new Vector3 (Maximum.x -Radius, transform.position.y, transform.position.z);
			if (transform.position.y < -Radius) 
				transform.position = new Vector3 (transform.position.x, Radius, transform.position.z);
			if (transform.position.y > Maximum.y + Radius) 
				transform.position = new Vector3 (transform.position.x, Maximum.y + -Radius, transform.position.z);
			if (transform.position.z < -Radius) 
				transform.position = new Vector3 (transform.position.x, transform.position.y, Radius);
			if (transform.position.z > Maximum.z + Radius) 
				transform.position = new Vector3 (transform.position.x, transform.position.y, Maximum.z + -Radius);
		}
		if (TargetPosition.x < -Radius)
			TargetPosition.x = Radius;
		if (TargetPosition.x > Maximum.x + Radius)
			TargetPosition.x = Maximum.x - Radius;
		if (TargetPosition.y < -Radius)
			TargetPosition.y = Radius;
		if (TargetPosition.y > Maximum.y + Radius)
			TargetPosition.y = Maximum.y - Radius;
		if (TargetPosition.z < -Radius)
			TargetPosition.z = Radius;
		if (TargetPosition.z > Maximum.z + Radius)
			TargetPosition.z = Maximum.z - Radius;
	}


	public void Flock() {
		/*if (AllyCheckCoolDown != 0) {
			if (Time.time - AllyCheckLastTime > AllyCheckCoolDown) {	// if within the distance or the cooldown has passed, move on to next position
				AllyCheckLastTime = Time.time;
				AlliesList = GetAllies ();
			}
			Flock (AlliesList);
		}*/
	}

	// We accumulate a new acceleration each time based on three rules
	void Flock(List<GameObject> boids) {
		Vector3 SeparateBoids = Separate(boids);   // Separation
		Vector3 AlignBoids = Align(boids);      // Alignment
		Vector3 CohesionBoids = Cohesion(boids);   // Cohesion
		// Arbitrarily weight these forces
		SeparateBoids *= (1.5f);
		AlignBoids *= (1.0f);
		CohesionBoids *= (1.0f);
		// Add the force vectors to acceleration
		Rigidbody rigidBody = (Rigidbody)(gameObject.GetComponent("Rigidbody"));
		rigidBody.AddForce (SeparateBoids);
		rigidBody.AddForce (AlignBoids);
		rigidBody.AddForce (CohesionBoids);
	}
	// Separation
	// Method checks for nearby boids and steers away
	Vector3 Separate (List<GameObject> boids) {
		float DesiredSeparation = 25.0f;
		Vector3 steer = new Vector3(0,0,0);
		int count = 0;
		// For every boid in the system, check if it's too close
		for (int i = 0; i < boids.Count; i++) {
			GameObject other = boids[i];
			float d = Vector3.Distance(transform.position,other.transform.position);
			// If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
			if ((d > 0) && (d < DesiredSeparation)) {
				// Calculate vector pointing away from neighbor
				Vector3 diff = transform.position - other.transform.position;
				diff.Normalize();
				diff /= d;        // Weight by distance
				steer += diff;
				count++;            // Keep track of how many
			}
		}
		// Average -- divide by how many
		if (count > 0) {
			steer /= count;
		}
		
		// As long as the vector is greater than 0
		if (steer.magnitude > 0) {
			// Implement Reynolds: Steering = Desired - Velocity
			steer.Normalize();
			Rigidbody MyRigidBody = (Rigidbody) gameObject.GetComponent ("Rigidbody");
			steer *= (MaximumSpeed.magnitude);
			steer -= (MyRigidBody.velocity);
			//steer.limit(maxforce);
			if (steer.x > MaximumForce.x) 
				steer.x = MaximumForce.x;
			if (steer.x < -MaximumForce.x) 
				steer.x = -MaximumForce.x;
			
			if (steer.y > MaximumForce.y) 
				steer.y = MaximumForce.y;
			if (steer.y < -MaximumForce.y) 
				steer.y = -MaximumForce.y;
			
			if (steer.z > MaximumForce.z) 
				steer.z = MaximumForce.z;
			if (steer.z < -MaximumForce.z) 
				steer.z = -MaximumForce.z;
		}
		return steer;
	}
	// Alignment
	// For every nearby boid in the system, calculate the average velocity
	Vector3 Align (List<GameObject> boids) {
		float neighbordist = 50;
		Vector3 sum = new Vector3(0,0,0);
		int count = 0;
		for (int i = 0; i < boids.Count; i++) {
			GameObject other = boids[i];
			float d = Vector3.Distance(transform.position,other.transform.position);
			if ((d > 0) && (d < neighbordist)) {
				Rigidbody OtherRigidBody = (Rigidbody) other.GetComponent ("Rigidbody");
				sum += (OtherRigidBody.velocity);
				count++;
			}
		}
		if (count > 0) {
			sum /= ((float)count);
			sum.Normalize();
			sum *= (MaximumSpeed.magnitude);
			Rigidbody MyRigidBody = (Rigidbody) gameObject.GetComponent ("Rigidbody");
			Vector3 steer = (sum - MyRigidBody.velocity);
			//steer.limit(maxforce);
			if (steer.x > MaximumForce.x) 
				steer.x = MaximumForce.x;
			if (steer.x < -MaximumForce.x) 
				steer.x = -MaximumForce.x;

			if (steer.y > MaximumForce.y) 
				steer.y = MaximumForce.y;
			if (steer.y < -MaximumForce.y) 
				steer.y = -MaximumForce.y;
			
			if (steer.z > MaximumForce.z) 
				steer.z = MaximumForce.z;
			if (steer.z < -MaximumForce.z) 
				steer.z = -MaximumForce.z;
			return steer;
		} else {
			return new Vector3(0,0,0);
		}
	}
	
	// Cohesion
	// For the average location (i.e. center) of all nearby boids, calculate steering vector towards that location
	Vector3 Cohesion (List<GameObject> boids) {
		float neighbordist = 50;
		Vector3 sum = new Vector3(0,0,0);   // Start with empty vector to accumulate all locations
		int count = 0;
//		for (GameObject other : boids) {
		for (int i = 0; i < boids.Count; i++) {
			GameObject other = boids[i];
			float d = Vector3.Distance(transform.position,other.transform.position);
			//float d = Vector3.Distance(transform.position,other.location);
			if ((d > 0) && (d < neighbordist)) {
				sum += other.transform.position; // Add location
				count++;
			}
		}
		if (count > 0) {
			sum /= (count);
			//return sum;
			return Seek(sum);  // Steer towards the location
		} else {
			return new Vector3(0,0,0);
		}
	}
	public Vector3 Wander(Vector3 Target) {
		Rigidbody rigidBody = (Rigidbody)(gameObject.GetComponent("Rigidbody"));
		Vector3 Velocity = rigidBody.velocity;
		float wanderR = 0.5f;         // Radius for our "wander circle"
		float wanderD = 5;         // Distance for our "wander circle"
		float change = 0.001f*Time.deltaTime;
		wandertheta += Random.Range(-change,change);     // Randomly change wander theta
		
		// Now we have to calculate the new location to steer towards on the wander circle
		Vector3 CircleLocation = Velocity;    // Start with velocity
		CircleLocation.Normalize();            // Normalize to get heading
		CircleLocation *= (wanderD);          // Multiply by distance
		CircleLocation += (transform.position);               // Make it relative to boid's location
		
		float h =  Velocity.magnitude;        // We need to know the heading to offset wandertheta
		// this is our wander offset
		Vector3 CircleOffset = new Vector3(wanderR*Mathf.Cos(wandertheta+h),wanderR*Mathf.Sin(wandertheta+h),wanderR*Mathf.Atan(wandertheta+h));
		return CircleLocation + CircleOffset;
	}
// ----- Commands -----

	// makes the character follow another
	public void Follow(GameObject FollowThisGuy) {
		FollowTarget = FollowThisGuy;
		EnterNewMode (Behaviour.Follow);
		BaseCharacter FollowThisCharacter = (BaseCharacter) FollowThisGuy.GetComponent ("BaseCharacter");
		BaseCharacter MyBotCharacter = (BaseCharacter) gameObject.GetComponent ("BaseCharacter");
		MyBotCharacter.ClanIndex = FollowThisCharacter.ClanIndex;
		Debug.Log ("Ordered bot to follow me.");
	}

	public void CommandAddTargetPoint(Vector3 NewTargetPoint) {
		SpawnTargetPoint (NewTargetPoint);
		EnterNewMode (Behaviour.Patrol);
	}
	public void CommandMineBlock(Vector3 BlockPosition) {
		ClearTargetPoints ();
		// theoritically the commanding character should of had the world class, and used the maths there to find the local position of the grid block
		SpawnTargetPoint (new Vector3(Mathf.RoundToInt(BlockPosition.x),Mathf.RoundToInt(BlockPosition.y), Mathf.RoundToInt(BlockPosition.z)));
		EnterNewMode (Behaviour.Mine);
	}
	public void CommandMoveToPosition(Vector3 NewTargetPoint) {
		ClearTargetPoints ();
		SpawnTargetPoint (NewTargetPoint);
		EnterNewMode (Behaviour.Patrol);
	}
	public void SpawnTargetPoint(Vector3 NewTargetPoint) {
		GameObject NewTargetPointObject = new GameObject();
		NewTargetPointObject.transform.position = NewTargetPoint;
		NewTargetPointObject.name = "TargetPoint: " + NewTargetPoint.ToString();
		TargetPoint MyTarget = NewTargetPointObject.AddComponent<TargetPoint> ();
		MyTarget.TargetPointIndex = TargetPositions.Count;
		TargetPositions.Add (MyTarget);
		LineRenderer MyLine = NewTargetPointObject.AddComponent<LineRenderer> ();
		if (TargetPositions.Count >= 2)
			MyLine.SetPosition(0, TargetPositions[TargetPositions.Count-2].transform.position);
		else
			MyLine.SetPosition(0, TargetPositions[TargetPositions.Count-1].transform.position);

		MyLine.SetPosition(1, TargetPositions[TargetPositions.Count-1].transform.position);
		MyLine.SetWidth (0.2f,0.15f);
		MyLine.material = GetManager.GetCharacterManager ().MyBotPathMaterial;
	}

	public void RenderTargetPoints() {
		BaseCharacter MyLocalCharacter = GetManager.GetCharacterManager ().GetLocalPlayer ();
		if (MyLocalCharacter && MyLocalCharacter.SelectedPlayer) {
			if (MyLocalCharacter.SelectedPlayer == gameObject) {
				for (int i = 0; i < TargetPositions.Count; i++) {
					TargetPositions [i].gameObject.SetActive (true);
					if (TargetPositions [i]) {
						if (MyBehaviour != Behaviour.Mine) {
							DebugDraw.DrawSphere (TargetPositions [i].transform.position, 0.2f, Color.blue);
						} else {
							//DebugDraw.DrawCube (TargetPositions [i].transform.position, Quaternion.identity, 0.55f, Color.white);
						}
					}
				}
			} else {
				HideTargetPositions();
			}
		} else {
			HideTargetPositions();
		}
	}
	public void HideTargetPositions() {
		for (int i = 0; i < TargetPositions.Count; i++) {
			TargetPositions[i].gameObject.SetActive(false);
		}
	}
	public void ClearTargetPoints() {
		for (int i = 0; i < TargetPositions.Count; i++) {
			if (TargetPositions[i])
				Destroy (TargetPositions[i].gameObject);
		}
		TargetPositions.Clear ();
	}
}
