using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// just a utility script that grabs the mesh from datamanager as soon as its loaded into the game

public class GetVoxelModel : MonoBehaviour {
	public BlockStructure MyBlockStructure;
	public string MyBlockStructureName = "Sphere";
	public string MyVoxelModelName = "Mesh 4";
	public BlockMesher MyBlockMesher;
	public MyMesh MyMesh;

	public int Type = 0;
	public float NoiseValue = 0.1f;
	public float SphereSize = 0.75f;
	public float MinimumVariation = 0.4f;
	public float MaximumVariation = 1.3f;

	public bool HasLoaded = false;
	// this should generate its own model instead
	public bool IsGetFromDatabase = true;
	public bool IsVaryScale = false;
	public bool IsCentred = true;

	// explosion
	float TimeSinceExploded = 0f;
	List<GameObject> MySpawnedBlocks = new List<GameObject> ();
	public Mesh MyCubeMesh;			// cube to spawn in explosion!
	public Material CubeMaterial;
	// force to add in explosion
	public float MinimumOomph = 1f;
	public float Oomph = 2f;
	public Shader MyShader;

	// Use this for initialization
	void Start () {
		if (IsGetFromDatabase) {
			LoadModelFromDataBase ();
		} else {	// generate new mesh
			MyBlockMesher = new BlockMesher();
			//	MyBlockStructure = GetManager.GetDataManager ().GetBlockStructure(MyBlockStructureName);
			MyBlockStructure.Reset ();
			if (IsVaryScale) {
				float CurrentSize = MyBlockStructure.MyBlocks.Scale.x;
				float NewSize = CurrentSize * (Random.Range (MinimumVariation, MaximumVariation));
				MyBlockStructure.MyBlocks.Scale += new Vector3 (NewSize, NewSize, NewSize);
				//MyBlockStructure.MyBlockTypes[0] = Random.Range(1,12);
			}
			//MyBlockStructure.UpdateBlockStructureWithType();
			if (Type == 0) {
				MyBlockStructure.Sphere (Random.Range (1, 12), SphereSize);	//	7 for cores
				MyBlockStructure.AddNoise (NoiseValue);
			} else {
				MyBlockStructure.MyBlocks.Size.y = 8;
				MyBlockStructure.Reset ();
				MyBlockStructure.Cylinder (2, SphereSize);
			}
			MyMesh = new MyMesh ();
			MyBlockMesher.UpdateMeshesOnThread (MyMesh, MyBlockStructure.MyBlocks);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (IsGetFromDatabase) { 
			LoadModelFromDataBase ();
		} else {
			if (MyBlockMesher != null)
			if (MyBlockMesher.CanUpdateMesh) {
				//Debug.LogError ("BlockMesher: " + i + " : Has finished creating a mesh." + Time.time);
				Mesh NewMesh = MyBlockMesher.CreateMesh (MyMesh);

				gameObject.GetComponent<MeshFilter> ().mesh = NewMesh;
				if (gameObject.GetComponent<MeshCollider> () != null) {
					gameObject.GetComponent<MeshCollider> ().sharedMesh = null;
					gameObject.GetComponent<MeshCollider> ().sharedMesh = gameObject.GetComponent<MeshFilter> ().mesh;
				}
				MyBlockMesher.CanUpdateMesh = false;
				HasLoaded = true;
				MyBlockMesher = null;	// finished with it!
			}
		}
		AddForceToExplosion ();
	}
	// once the models finished generating itll be loaded
	public void LoadModelFromDataBase() {
		if (!HasLoaded && GetManager.GetDataManager ().IsModel (MyVoxelModelName)) {
			MyBlockStructure = GetManager.GetDataManager ().GetBlockStructure(MyBlockStructureName);
			gameObject.GetComponent<MeshFilter> ().mesh = GetManager.GetDataManager ().GetModel (MyVoxelModelName);
			if (gameObject.GetComponent<MeshCollider> () != null) {
				gameObject.GetComponent<MeshCollider> ().sharedMesh = null;
				gameObject.GetComponent<MeshCollider> ().sharedMesh = gameObject.GetComponent<MeshFilter> ().mesh;
			}
			HasLoaded = true;
		}
	}

	void AddForceToExplosion() 
	{
		if (TimeSinceExploded != 0f) 
		{
			if (Time.time - TimeSinceExploded >= 0.1f) 
			{
				for (int i = 0; i < MySpawnedBlocks.Count; i++) 
				{
					if (MySpawnedBlocks [i]) 
					{
						MySpawnedBlocks [i].GetComponent<Rigidbody> ().isKinematic = false;
						float NewForceX = Random.Range (-Oomph, Oomph);
						if (NewForceX > 0)
							NewForceX = Mathf.Clamp (NewForceX, MinimumOomph, NewForceX);
						else
							NewForceX = Mathf.Clamp (NewForceX, -MinimumOomph, NewForceX);
						float NewForceY = Random.Range (-Oomph, Oomph); 
						if (NewForceY > 0)
							NewForceY = Mathf.Clamp (NewForceY, MinimumOomph, NewForceY);
						else
							NewForceY = Mathf.Clamp (NewForceY, -MinimumOomph, NewForceY);
						float NewForceZ = Random.Range (-Oomph, Oomph); 
						if (NewForceZ > 0)
							NewForceZ = Mathf.Clamp (NewForceZ, MinimumOomph, NewForceZ);
						else
							NewForceZ = Mathf.Clamp (NewForceZ, -MinimumOomph, NewForceZ);
						
						Vector3 NewForce = new Vector3 (NewForceX, NewForceY, NewForceZ);
						MySpawnedBlocks [i].GetComponent<Rigidbody> ().AddForce (NewForce);
					}
				}
				TimeSinceExploded = 0f;
			}
		}
	}
	public void Explode() 
	{
		GameObject NewObject = new GameObject ();
		NewObject.transform.position = transform.position;
		NewObject.name = gameObject.name + "'s Exploded corpse";
		if (MyCubeMesh)
		for (int i = 0; i < MyBlockStructure.MyBlocks.Size.x; i += 1)
				for (int j = 0; j < MyBlockStructure.MyBlocks.Size.y; j += 1)
					for (int k = 0; k < MyBlockStructure.MyBlocks.Size.z; k += 1)
			{
				int MyBlockType = MyBlockStructure.MyBlocks.GetBlockType(new Vector3(i,j,k));
					if (MyBlockType != 0) {
					GameObject NewBlock = new GameObject();
						NewBlock.transform.rotation = transform.rotation;
						NewBlock.transform.localScale = gameObject.transform.localScale;
						
						NewBlock.transform.position = NewBlock.transform.TransformPoint((MyMesh.DifferenceBounds.x/2f)*NewBlock.transform.localScale.x,
						                                                                (MyMesh.DifferenceBounds.y/2f)*NewBlock.transform.localScale.y,
						                                                                (MyMesh.DifferenceBounds.z/2f)*NewBlock.transform.localScale.z);

						NewBlock.transform.localScale = new Vector3(NewBlock.transform.localScale.x *  MyBlockStructure.MyBlocks.Scale.x, 
						                                            NewBlock.transform.localScale.y *  MyBlockStructure.MyBlocks.Scale.y,
						                                            NewBlock.transform.localScale.z *  MyBlockStructure.MyBlocks.Scale.z);
						NewBlock.transform.position += 	NewBlock.transform.TransformPoint(new Vector3(i+0.5f,j+0.5f,k+0.5f));

						NewBlock.AddComponent<MeshFilter>().mesh = MyCubeMesh;
						Material MyMaterial = new Material(MyShader);
						if (MyBlockType >= 0 && MyBlockType < GetManager.GetTextureManager().BlockTextures.Count)
							MyMaterial.mainTexture = GetManager.GetTextureManager().BlockTextures[MyBlockType];
						NewBlock.AddComponent<MeshRenderer>().material = MyMaterial;
					NewBlock.AddComponent<BoxCollider>();
					NewBlock.AddComponent<Rigidbody>().isKinematic = true;
					
					NewBlock.transform.SetParent(NewObject.transform, false);
					MySpawnedBlocks.Add (NewBlock);
				}
			}
		Destroy (NewObject,15f);
		// now hide mesh
		if (gameObject.GetComponent<MeshRenderer> ())
			gameObject.GetComponent<MeshRenderer> ().enabled = false;
		if (gameObject.GetComponent<MeshCollider> ())
			gameObject.GetComponent<MeshCollider> ().enabled = false;
		if (gameObject.GetComponent<Rigidbody> ())
			gameObject.GetComponent<Rigidbody> ().isKinematic = true;

		TimeSinceExploded = Time.time;
	}
}
