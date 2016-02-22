using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using ItemSystem;

// Need to decouple this script
// to do:
//		record animation curves of the body falling
//			stop recording curve when the body stops moving
//		reverse the curve to make it get back up when its ressurected

// merge with explode script - that use for voxels
// so i can blow up parts of the body itself

// add in function for removing just parts of the body
// an example would be // remove body parts that intersect with a sphere at x location - or that intersect with a raycast

// Ideally on death, activate a post processing effect script
//		and activate particle system
//		activate sounds of dying


// another one would be, convert only the bottom parts of the body, ie feet, then calves etc, until it is just a head
namespace AnimationUtilities {
	[System.Serializable]
	public class MyEvent : UnityEvent<GameObject> {}

	public class Ragdoll : SkeletonModifier {
		public bool IsDebugMode = false;
		public KeyCode MyExplodeKey;
		public KeyCode RagdollKey;
		public KeyCode MyReviveKey;

		public bool IsAddPartColliders = true;
		public bool IsAttachMainCamera = false;
		[Tooltip("Used for character interaction")]
		public List<MyEvent> OnInteract;
		[Tooltip("When the body is converted into a ragdoll")]
		public UnityEvent OnRagdoll;
		[Tooltip("When the body is converted into a ragdoll")]
		public UnityEvent OnRevive;
		[Tooltip("If a Camera is attached, reattach it to the ragdoll")]
		public UnityEvent OnReattachCamera;
		[Tooltip("How long is the paused after death before falling")]
		public float TimeFrozen = 1f;
		[Tooltip("Artificial Gravity after death")]
		public Vector3 GravityForce = new Vector3 (0, -2, 0);
		public float ReverseTimeDilation = 0.2f;
		//public bool IsTesting = false;
		private float TimeSpawned = 0f;

		GameObject RootSpawn;	// spawned new body main
		protected List<GameObject> MySpawnedBodyParts = new List<GameObject> ();
		public Vector3 ForceOnExplosion = new Vector3 ();
		public bool IsForce = false;
		public float ForceStrength = 10f;

		public bool CanTimeBeReversed = false;


		void Start() 
		{
			GatherBodyParts ();
			for (int i = 0; i < MyBodyParts.Count; i++) {
				if (IsAddPartColliders && MyBodyParts [i].GetComponent<MeshCollider> () == null) {
					MeshCollider MyBoxCollider = MyBodyParts [i].AddComponent<MeshCollider> ();
					//MyBoxCollider.isTrigger = true;
					MyBoxCollider.convex = true;
				}
				BodyPart MyBodyPart = MyBodyParts [i].GetComponent<BodyPart> ();
				if (MyBodyPart == null)
					MyBodyPart = MyBodyParts [i].AddComponent<BodyPart> ();
				MyBodyPart.MyParent = gameObject;
				MyBodyPart.RagdollBrain = this;
			}
			RootSpawn = gameObject;
		}

		// Update is called once per frame
		void Update () 
		{
			AwakenBody ();
			if (IsDebugMode) {
				if (Input.GetKeyDown(MyReviveKey)) {
					ReverseDeath();
				}
				else if (Input.GetKeyDown(MyExplodeKey)) {
					Explode();
				} else if (Input.GetKeyDown(RagdollKey)) {
					RagDoll();
				}
			}
		}

		public void Explode2() {
			ConvertBodyParts (2, 300f);	// only detatch some
		}

		// Toggles the attachtness of a body part
		public void ActivateBodyPart(GameObject MyBodyPart) {
			BodyPart MyData = MyBodyPart.GetComponent<BodyPart> ();
			if (MyData.IsRigidBody) {
				ReattachBodyPart (MyBodyPart);
			} else {
				DetatchBodyPart(MyBodyPart);
			}
		}

		public void DetatchBodyPart(GameObject MyBodyPart) {
			//MySpawnedBodyParts.Clear ();
			List<GameObject> DetatchedBodyParts = new List<GameObject> ();
			if (MyBodyPart.activeSelf) {
					//DetatchedBodyParts.Add (FirstPart);	// should be a 
				List<GameObject> MyList = FindChildren (MyBodyPart);
					//Debug.LogError ("But Body part has: " + MyBodyPart.transform.childCount + " Children!");
				for (int j = 0; j < MyList.Count; j++) {
					GameObject NewPart = CreateNewBodyPart (MyList [j]);
					if (NewPart != null) 
					{
						DetatchedBodyParts.Add (NewPart); 
					}
				}
				//Debug.LogError ("Detatched " + MyList.Count + " children!");
				TimeSpawned = Time.time;
				ConnectUpBodyParts (DetatchedBodyParts);
			}
		}
		
		public void ReverseDeath() 
		{
			for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
				ReattachBodyPart(MySpawnedBodyParts[i]);
			}
		}
		// need to check
		public void ReattachBodyPart(GameObject MyBodyPart) 
		{
			BodyPart BodyPartComponent = MyBodyPart.GetComponent<BodyPart>();
			if (CanTimeBeReversed) {
				ReverseMovement MyReverseMovement = MyBodyPart.GetComponent<ReverseMovement> ();
				//Debug.LogError("Reattaching BodyParts v1 ");
				if (MyReverseMovement) {
					MyReverseMovement.Reverse ();
					//Debug.LogError("Reattaching BodyParts");
					MyReverseMovement.OnEndReverse.AddListener (// this ain't working atm! debug more!
					delegate {
						//Debug.LogError("Ending Reverse Movement: " + BodyPartComponent.name);
						if (BodyPartComponent)
						if (BodyPartComponent.OriginalBodyPart)
							BodyPartComponent.OriginalBodyPart.SetActive (true);
						//if (MyBodyPart.transform.childCount > 0)
						//	Debug.LogError("Wut it contains child: " + MyBodyPart.transform.childCount);
						RemoveBodyPart (MyBodyPart);
						if (MySpawnedBodyParts.Count == 0) {
							OnRevive.Invoke ();
						}
					});
				}
			} else {	// use general lerping to get bone back on

			}
		}
		public void RemoveBodyPart(GameObject MyBodyPart) 
		{
			if (MyBodyPart != null)
			for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
				if (MySpawnedBodyParts[i] == MyBodyPart) 
				{
					if (IsAttachMainCamera) 
					{
						if (MyBodyPart.name.Contains("Head")) 
						{
							if (MyBodyPart.transform.childCount > 0) 
							{
								Debug.LogError("Setting up camera");
								Camera.main.gameObject.transform.SetParent(MyBodyPart.transform);
								//Camera.SetupCurrent(MyBodyPart.transform.GetChild(0).gameObject.GetComponent<Camera>());
								/*GameObject NewCameraObject = new GameObject();
								NewCameraObject.transform.SetParent(gameObject.transform.parent);
								NewCameraObject.transform.position = MyBodyPart.transform.GetChild(0).transform.position;
								NewCameraObject.transform.rotation = MyBodyPart.transform.GetChild(0).transform.rotation;
								//NewCameraObject.transform.localPosition =  new Vector3(0, 0.47f, 0.06f);
								Camera NewCamera = NewCameraObject.AddComponent<Camera>();
								Camera.SetupCurrent(NewCamera);
								UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController MyController = 
									transform.parent.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>();
								if (MyController) {
									MyController.cam = NewCamera;
								}
								NewCamera.nearClipPlane = 0.01f;
								NewCamera.fieldOfView = 90;
								NewCamera.tag = "MainCamera";
								NewCamera.name = NewCamera.tag;*/
							}
						}
					}
					MySpawnedBodyParts.RemoveAt(i);
					DestroyImmediate (MyBodyPart);
				} else {
					Destroy (MyBodyPart);
				}
			}
		}

		public void Explode() {
			ConvertBodyParts (0, 300f);
		}
		public void RagDoll() {
			ConvertBodyParts (1, 300f);
		}
		// this happens on all parts, need it to just happen on some!
		public void AwakenBody() 
		{
			for (int i = MySpawnedBodyParts.Count-1; i >= 0; i--) { 
				if (MySpawnedBodyParts[i] != null) {
				if (MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned != 0)
				if (Time.time - MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned >= TimeFrozen) {
					if (MySpawnedBodyParts [i] != null) 
					if (MySpawnedBodyParts [i].GetComponent<Rigidbody> ()) {
						MySpawnedBodyParts [i].GetComponent<Rigidbody> ().isKinematic = false;
						if (IsForce) {
							MySpawnedBodyParts [i].GetComponent<Rigidbody> ().velocity = ForceOnExplosion;	// reset velocity
							ForceOnExplosion = new Vector3 (Random.Range (-ForceStrength, ForceStrength),
						                               Random.Range (-ForceStrength, ForceStrength),
						                               Random.Range (-ForceStrength, ForceStrength));
							}
							if (CanTimeBeReversed) {
								ReverseMovement MyReverseMovement = MySpawnedBodyParts [i].GetComponent<ReverseMovement> ();
								if (MyReverseMovement == null) 
								{ 
										MyReverseMovement =MySpawnedBodyParts [i].AddComponent<ReverseMovement> ();
								}
								MyReverseMovement.ReverseTimeDilation = ReverseTimeDilation;
							}
					}
					MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned = 0;
				}
				} else {
					MySpawnedBodyParts.RemoveAt (i);
				}
			}
		}
		void AutoReverse(ReverseMovement MyReverseMovement) {
			if (CanTimeBeReversed) {
				float AutoReverseTime = -1;
				if (AutoReverseTime == -1) {
					AutoReverseTime = MyReverseMovement.MutateReverseTime ();
				} else {
					MyReverseMovement.SetAutoReverseTime (AutoReverseTime);
				}
			}
		}
		public bool HasMainCameraAttached() {
			for (int i = 0; i < transform.parent.childCount; i++) {
				if (transform.parent.GetChild(i).GetComponent<Camera>())
					return true;
			}
			return false;
		}
		// gets any child body with a mesh
		// keeps its mesh and position/rotation
		// creates a new body in that position with that, and a rigidbody
		public void ConvertBodyParts(int RagdollType, float TimeToGetDestroyed) {
			GatherBodyParts ();
			//RootSpawn = new GameObject ();
			//RootSpawn.name = gameObject.name + "'s Lifeless Corpse";
			//if (IsAttachMainCamera) 
			//{
			//	Camera.main.transform.parent = null;
			//}
			for (int i = 0; i < MyBodyParts.Count; i++) 
			{
				CreateNewBodyPart(MyBodyParts[i]);
			}
			if (RagdollType != 0)	// if not explode type
			ConnectUpBodyParts (MySpawnedBodyParts, RagdollType == 1);
			//ConnectUpBodyParts ();

			if (HasMainCameraAttached()) 
			{
				ReattachCamera ();
			}
			//Destroy(RootSpawn, TimeToGetDestroyed);	// now destroy the corpse eventually

			if (OnRagdoll != null)
			{
				OnRagdoll.Invoke();
			}
			//ClearData ();
			TimeSpawned = Time.time;
		}

		private Transform FindHead() {
			if (RootSpawn == null)
				return null;
			for (int i = 0; i < RootSpawn.transform.childCount; i++) {
				if (RootSpawn.transform.GetChild(i).name.Contains("Head")) {
					return RootSpawn.transform.GetChild(i);
				}
			}
			return null;
		}
		private void ReattachCamera() {
			// if has camera attached
			//if (gameObject.GetComponent<Player> ()) 
			if (Camera.main)
			{	// attach it to the head object
				Camera.main.transform.SetParent(null);
				Transform HeadTransform = FindHead();
				if (HeadTransform != null) {
					Transform CameraTransform = Camera.main.transform;	//gameObject.transform.GetChild (0);
					CameraTransform.SetParent (HeadTransform);	// = MySpawnedBodyParts[0].transform.position;
					//CameraTransform.position = HeadTransform.position;
					//CameraTransform.rotation = HeadTransform.rotation;
					//CameraTransform.eulerAngles = CameraTransform.eulerAngles + new Vector3 (90, 0, 0);	// offset for proper rotation!
					
					//gameObject.GetComponent<PlayerMovement> ().ToggleDeathPostProcessing ();	// maybe add death post processing script on?
					if (OnReattachCamera != null) 
					{
						OnReattachCamera.Invoke();
					}
				}
			}
		}
		
		public void ConnectUpBodyParts() {
			ConnectUpBodyParts (MySpawnedBodyParts, true);
		}
		public void ConnectUpBodyParts(List<GameObject> NewlyDetatchedParts) {
			ConnectUpBodyParts (NewlyDetatchedParts, true);
		}
		public void ConnectUpBodyParts(List<GameObject> NewlyDetatchedParts, bool IsConnectAll) {
			//Debug.LogError ("Connect parts with joints: " + NewlyDetatchedParts.Count);
			for (int i = 0; i < NewlyDetatchedParts.Count; i++) {
				int IsConnect = Random.Range (1,100);
				if (IsConnectAll || IsConnect < 66) 
				{
					if (MyParentIndexes[i] != -1) 
					{
						GameObject MyParent = GetParentBodyPart(i, NewlyDetatchedParts);
						if (MyParent != null) 
						{
							ConnectParts(MyParent, MySpawnedBodyParts[i]);
						}
					}
				}
			}
		}
		public void ConnectParts(GameObject ParentPart, GameObject ChildPart) {
			if (ParentPart != ChildPart) 
			{
				FixedJoint MyJoint = ChildPart.AddComponent<FixedJoint> ();
				MyJoint.connectedBody = ParentPart.GetComponent<Rigidbody> ();
			}
		}

		GameObject CreateNewBodyPart(GameObject OldBodyPart) {	//int ParentIndex, 
			if (OldBodyPart.activeSelf) {
				OldBodyPart.SetActive (false);
				// setups
				Material NewMaterial = OldBodyPart.GetComponent<MeshRenderer>().material;
				Mesh NewMesh = OldBodyPart.GetComponent<MeshFilter>().mesh;
				Vector3 NewPosition = OldBodyPart.transform.position;
				Vector3 NewScale = OldBodyPart.transform.lossyScale;
				Quaternion NewRotation = OldBodyPart.transform.rotation;
				// gameobject stuff
				GameObject NewBodyPart = new GameObject ();
				NewBodyPart.layer = gameObject.layer;
				MySpawnedBodyParts.Add (NewBodyPart);
				NewBodyPart.name = OldBodyPart.name + " Part";
				// transforms
				NewBodyPart.transform.localScale = NewScale;
				NewBodyPart.transform.position = NewPosition;
				NewBodyPart.transform.rotation = NewRotation;
				NewBodyPart.transform.SetParent (RootSpawn.transform);
				// render
				MeshRenderer MyMeshRenderer = NewBodyPart.AddComponent<MeshRenderer> ();
				MyMeshRenderer.material = NewMaterial;
				MeshFilter MyMeshFilter = NewBodyPart.AddComponent<MeshFilter> ();
				MyMeshFilter.mesh = NewMesh;
				// movement
				Rigidbody MyRigid = NewBodyPart.AddComponent<Rigidbody> ();
				MyRigid.drag = 0.1f;
				// collider
				MeshCollider MyMeshCollider = NewBodyPart.AddComponent<MeshCollider> ();
				MyMeshCollider.sharedMesh = NewMesh;
				MyMeshCollider.convex = true;
				MyRigid.isKinematic = true;
				MyRigid.useGravity = false;
				// gravity
				ArtificialGravity MyGrav = NewBodyPart.AddComponent<ArtificialGravity> ();
				MyGrav.GravityForce = GravityForce;
				// extra data
				BodyPart MyBodyPart = NewBodyPart.AddComponent<BodyPart>();
				MyBodyPart.MyParent = OldBodyPart.transform.parent.gameObject;
				MyBodyPart.RagdollBrain = this;
				MyBodyPart.OriginalBodyPart = OldBodyPart;
				MyBodyPart.TimeSpawned = Time.time;
				MyBodyPart.IsRigidBody = true;
				ItemSystem.ItemObject MyItem = NewBodyPart.AddComponent<ItemSystem.ItemObject>();
				MyItem.MyItem = new ItemSystem.Item();
				MyItem.MyItem.Name = OldBodyPart.name;
				MyItem.MyItem.SetQuantity(1);
				return NewBodyPart;
			}
			return null;
		}
	}
}
