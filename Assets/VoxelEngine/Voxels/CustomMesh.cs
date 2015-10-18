using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[ExecuteInEditMode]
public class CustomMesh : MonoBehaviour {
	public bool IsRefresh = false;
	public Vector2 TextureUnit;
	public bool IsSingleCube = true;
	public bool IsFrontFace = true;
	public bool IsBackFace = false;
	public bool IsLeftFace = true;
	public bool IsRightFace = true;
	public bool IsTopFace = true;
	public bool IsBottomFace = true;
	public Vector3 Size = new Vector3(1,1,1);
	//public int FaceCount = 0;
	public MyMesh MyCustomMesh;
	public bool IsRenderMesh = true;
	// This first list contains every vertex of the mesh that we are going to render
	// make these non serializable

	public Mesh mesh;
	
	// Use this for initialization
	void Start () {
		if (IsSingleCube) {
			Debug.Log ("Creating Single Cube.");
			MyCustomMesh.ClearMesh();
			MyCustomMesh.CreateCube (new Vector3(0,0,0), Size,IsFrontFace, IsBackFace, IsBackFace, IsLeftFace, IsTopFace, IsBottomFace, TextureUnit);
			UpdateMesh (MyCustomMesh);
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (IsRefresh) {
			MyCustomMesh.ClearMesh();
			Debug.Log ("Creating Single Cube.");
			CreateCube (new Vector3(0,0,0), Size);
			UpdateMesh (MyCustomMesh);
			IsRefresh = false;
		}
		//Debug.Log ("Running Custom Mesh");
		//Vector3 NewAngle = transform.rotation.eulerAngles;
		//NewAngle.x += 0.0005f;
		//transform.Rotate (NewAngle);
	}

	public void CreateCube(Vector3 Position, Vector3 Size, bool IsFrontFace_, bool IsBackFace_, bool IsLeftFace_, bool IsRightFace_, bool IsTopFace_, bool IsBottomFace_) {
		IsFrontFace = IsFrontFace_;
		IsBackFace = IsBackFace_;
		IsLeftFace = IsLeftFace_;
		IsRightFace = IsRightFace_;
		IsTopFace = IsTopFace_;
		IsBottomFace = IsBottomFace_;
		CreateCube (Position, Size);
	}

	public void CreateCube(Vector3 Position, Vector3 Size) {
		MyCustomMesh.CreateCube (Position, Size,IsFrontFace, IsBackFace, IsBackFace, IsLeftFace, IsTopFace, IsBottomFace, TextureUnit);
	}
	
	public void UpdateMesh(MyMesh NewMesh){
		MeshFilter NewMeshFilter = gameObject.GetComponent<MeshFilter>();
		if (NewMeshFilter != null) {
			NewMesh.UpdateMeshWithData(NewMeshFilter.mesh, gameObject.GetComponent<MeshCollider>(), true);
		}
	}
}