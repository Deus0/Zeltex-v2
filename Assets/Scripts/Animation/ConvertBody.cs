using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityStandardAssets.Characters.FirstPerson;
// merge with explode script
// so i can blow up parts of the body itself

// add in function for removing just parts of the body
// an example would be // remove body parts that intersect with a sphere at x location - or that intersect with a raycast

// another one would be, convert only the bottom parts of the body, ie feet, then calves etc, until it is just a head

public class ConvertBody : MonoBehaviour {
	public bool IsKeepAttached = true;
	private List<Mesh> MyBodyMeshes = new List<Mesh>();
	private List<Material> MyBodyMaterials = new List<Material>();
	private List<Vector3> MyBodyMeshPositions = new List<Vector3>();
	private List<Vector3> MyBodyMeshScales = new List<Vector3>();
	private List<Quaternion> MyBodyMeshRotations = new List<Quaternion>();
	private List<int> MyParentIndexes = new List<int>();
	private List<GameObject> MySpawnedBodyParts = new List<GameObject> ();
	public float MinSizeVariation = 0.5f;
	public float MaxSizeVariation = 1.8f;
	public bool IsRainbowColours = false;
	public bool IsDarkColours = true;
	float TimeSpawned = 0f;
	GameObject RootSpawn;
	public float TimeFrozen = 3f;

	// Use this for initialization
	void Start () {
	
	}
	void Awake() {
		ChangeBodyColours();
		float UniformScaler = Random.Range (MinSizeVariation, MaxSizeVariation);
		Transform MyTransform = transform.GetChild (0);
		MyTransform.localScale = new Vector3 (MyTransform.localScale.x * UniformScaler,
		                                      MyTransform.localScale.y * UniformScaler,
		                                      MyTransform.localScale.z * UniformScaler);
		//gameObject.transform.GetChild (0).localPosition = new Vector3 (gameObject.transform.GetChild (0).localPosition.x, 
		//                                                               gameObject.transform.GetChild (0).localPosition.y + (1-UniformScaler), 
		//                                                               gameObject.transform.GetChild (0).localPosition.z);
		if (gameObject.GetComponent<CharacterController> ()) {
			gameObject.GetComponent<CharacterController> ().center *= UniformScaler;
			gameObject.GetComponent<CharacterController> ().height *= UniformScaler;
			gameObject.GetComponent<CharacterController> ().radius *= UniformScaler;
		}
	}
	// Update is called once per frame
	void Update () {
		if (TimeSpawned != 0f) {
			if (Time.time-TimeSpawned >= TimeFrozen) {
				for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
					MySpawnedBodyParts[i].GetComponent<Rigidbody>().isKinematic = false;
				}
				TimeSpawned = 0f;
			}
		}
	}
	public void ChangeBodyColours() {
		ClearData ();
		GatherBodyParts ();
		int Max = 255;
		if (IsDarkColours)
			Max = 125;
		byte red = (byte)Random.Range(0,Max);
		byte green = (byte)Random.Range(0,Max);
		byte blue = (byte)Random.Range(0,Max);

		for (int i = 0; i < MyBodyMaterials.Count; i++) {
			if (IsRainbowColours) {
				red = (byte)Random.Range(0,255);
				green = (byte)Random.Range(0,255);
				blue = (byte)Random.Range(0,255);
			}
			MyBodyMaterials[i].color = new Color32(red, green, blue,255);
		}
		ClearData ();
	}
	// gets any child body with a mesh
	// keeps its mesh and position/rotation
	// creates a new body in that position with that, and a rigidbody
	void ClearData() {
		MyBodyMeshes.Clear ();
		MyBodyMaterials.Clear ();
		MyBodyMeshPositions.Clear ();
		MyBodyMeshScales.Clear ();
		MyBodyMeshRotations.Clear ();
	}

	public void ConvertBodyParts(bool IsExplode, float TimeToGetDestroyed) {
		ClearData ();
		GatherBodyParts ();

		RootSpawn = new GameObject ();
		RootSpawn.transform.position = gameObject.transform.position;
		RootSpawn.name = gameObject.name + "'s Lifeless Corpse";
		for (int i = 0; i < MyBodyMeshes.Count; i++) {
			CreateNewBodyPart(MyBodyMeshes[i], MyBodyMeshPositions[i], MyBodyMeshRotations[i], MyBodyMeshScales[i], MyBodyMaterials[i], MyParentIndexes[i]);
		}
		if (!IsExplode)
			ConnectUpBodyParts ();

		DestroyImmediate(gameObject.transform.GetChild(0).gameObject);	// destroy the animation
		Destroy(RootSpawn, TimeToGetDestroyed);	// now destroy the corpse eventually

		if (gameObject.GetComponent<Player> ()) {
			gameObject.transform.SetParent(RootSpawn.transform);
			Transform HeadTransform = RootSpawn.transform.FindChild(" Instance").transform;
			Transform CameraTransform = gameObject.transform.GetChild(0);
			gameObject.GetComponent<PlayerMovement>().ToggleDeathPostProcessing();
			CameraTransform.SetParent(HeadTransform, false);	// = MySpawnedBodyParts[0].transform.position;
			CameraTransform.position = HeadTransform.position;
			CameraTransform.rotation = HeadTransform.rotation;
			CameraTransform.eulerAngles = CameraTransform.eulerAngles + new Vector3(-90,0,0);
			gameObject.GetComponent<PlayerMovement>().DisableMovement();
			gameObject.GetComponent<PlayerMovement>().EnablePlayerInput();
			gameObject.GetComponent<Player>().enabled = false;
			GetManager.GetCameraManager().EnableMouseGui();
			//Destroy(gameObject.GetComponent<CustomController>());
			//Destroy(gameObject.GetComponent<CharacterController>());
		}

		ClearData ();
		TimeSpawned = Time.time;
	}

	public void ConnectUpBodyParts() {
		for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
			if (MyParentIndexes[i] != -1) 
			{
				GameObject MyParent = MySpawnedBodyParts[MyParentIndexes[i]];
				if (MyParent != MySpawnedBodyParts[i]) {
					FixedJoint MyJoint = MySpawnedBodyParts[i].AddComponent<FixedJoint> ();
					MyJoint.connectedBody = MyParent.GetComponent<Rigidbody>();
				}
			}
		}
	}

	void CreateNewBodyPart(Mesh NewMesh, Vector3 NewPosition, Quaternion NewRotation, Vector3 NewScale, Material NewMaterial, int ParentIndex) {
		GameObject NewBodyPart = new GameObject ();
		MySpawnedBodyParts.Add (NewBodyPart);
		NewBodyPart.name = NewMesh.name;
		NewBodyPart.transform.localScale = gameObject.transform.GetChild(0).localScale;
		//NewBodyPart.transform.localScale = gameObject.transform.localScale;
		NewBodyPart.transform.SetParent (RootSpawn.transform);
		if (ParentIndex >= 0 && ParentIndex < MySpawnedBodyParts.Count) {
			//NewBodyPart.transform.SetParent (MySpawnedBodyParts [ParentIndex].transform);
		}
		NewBodyPart.transform.position = NewPosition;
		NewBodyPart.transform.rotation = NewRotation;
		MeshRenderer MyMeshRenderer = NewBodyPart.AddComponent<MeshRenderer> ();
		MyMeshRenderer.material = NewMaterial;
		MeshFilter MyMeshFilter = NewBodyPart.AddComponent<MeshFilter> ();
		MyMeshFilter.mesh = NewMesh;
		Rigidbody MyRigid = NewBodyPart.AddComponent<Rigidbody> ();
		MeshCollider MyMeshCollider = NewBodyPart.AddComponent<MeshCollider> ();
		MyMeshCollider.sharedMesh = NewMesh;
		MyMeshCollider.convex = true;
		MyRigid.isKinematic = true;
	}
	void GatherBodyParts() {
		CheckChildMeshes (transform.GetChild (0).gameObject, -1);
	}
	void CheckChildMeshes(GameObject Child, int ParentIndex) {
		for (int i = 0; i < Child.transform.childCount; i++) {
			GameObject ChildObject = Child.transform.GetChild(i).gameObject;
			if (ChildObject.transform.childCount > 0) {
				ChildObject = ChildObject.transform.GetChild(0).gameObject;

				if (ChildObject.GetComponent<MeshFilter>()) 
				{
					MyBodyMeshes.Add (ChildObject.GetComponent<MeshFilter>().mesh);
					MyBodyMaterials.Add (ChildObject.GetComponent<MeshRenderer>().material);
					MyBodyMeshPositions.Add (ChildObject.transform.position);
					MyBodyMeshScales.Add (ChildObject.transform.localScale);
					MyBodyMeshRotations.Add (ChildObject.transform.rotation);
					if (ParentIndex < 0)
						MyParentIndexes.Add (0);
					else
						MyParentIndexes.Add (ParentIndex);

					CheckChildMeshes (Child.transform.GetChild (i).gameObject, MyBodyMeshes.Count - 1);
				}
				else
				{
					CheckChildMeshes (Child.transform.GetChild (i).gameObject, ParentIndex);
				}
			}
		}
	}
}
