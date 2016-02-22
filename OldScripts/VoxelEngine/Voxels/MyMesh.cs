using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MyMeshSaver {

}

// I use this to store all my mesh data
// want it to be
[System.Serializable]
public class MyMesh {
	public List<Vector3> Verticies = new List<Vector3>();
	public List<int> Indicies = new List<int>();
	public List<Color32> Colors = new List<Color32>();
	public List<Vector2> TextureCoordinates = new List<Vector2>();
	public int FaceCount;
	public Color32 CubeColor = new Color32(255,255,255,255);
	public float ColorDecreaseValue = 0f;
	public bool IsColorOverride = true;
	//public bool IsCentred = false;
	public Vector3 DifferenceBounds;	// used to centre a mesh
	public MyMesh() {

	}
	public MyMesh(Mesh NewMesh) {
		GetDataFromMesh (NewMesh);

	}

	public void ClearMesh() {
		FaceCount = 0;
		Verticies.Clear ();
		Indicies.Clear ();
		TextureCoordinates.Clear ();
		Colors.Clear ();
	}
	public void CentreMesh() {
		Debug.Log ("Centring Mesh : " + Verticies.Count + " : " + Time.time);
		// first find outer/lower bounds
		Vector3 LowerBounds = GetLowerBounds ();
		Vector3 HigherBounds = GetHigherBounds ();
		// then decrease all positions by half of that
		// eg lower bounds 3, higher bounds 5 - all points - -3+5 = 2, add add 1 to everything and the LB, HB = -4, +4
		DifferenceBounds = -(LowerBounds + HigherBounds) / 2f;
		for (int i = 0; i < Verticies.Count; i++) {
			Verticies[i] += DifferenceBounds;
		}
	}
	public Vector3 GetLowerBounds() {
		if (Verticies.Count > 0) {
			Vector3 LowerBounds = Verticies [0];
			for (int i = 1; i < Verticies.Count; i++) {
				if (LowerBounds.x > Verticies [i].x)
					LowerBounds.x = Verticies [i].x;
				if (LowerBounds.y > Verticies [i].y)
					LowerBounds.y = Verticies [i].y;
				if (LowerBounds.z > Verticies [i].z)
					LowerBounds.z = Verticies [i].z;
			}
			return LowerBounds;
		} else {
			return new Vector3 (0, 0, 0);
		}
	}
	public Vector3 GetHigherBounds() {
		if (Verticies.Count > 0) {
			Vector3 HigherBounds = Verticies [0];
			for (int i = 1; i < Verticies.Count; i++) {
				if (HigherBounds.x < Verticies [i].x)
					HigherBounds.x = Verticies [i].x;
				if (HigherBounds.y < Verticies [i].y)
					HigherBounds.y = Verticies [i].y;
				if (HigherBounds.z < Verticies [i].z)
					HigherBounds.z = Verticies [i].z;
			}
			return HigherBounds;
		} else {
			return new Vector3 (0, 0, 0);
		}
	}
	public void GetDataFromMesh(Mesh MyMesh_) {
		if (MyMesh_) {
			Verticies.Clear ();
			Indicies.Clear ();
			TextureCoordinates.Clear ();
			Colors.Clear ();
			for (int i = 0; i < MyMesh_.vertices.Length; i++)
				Verticies.Add (MyMesh_.vertices [i]);
			for (int i = 0; i < MyMesh_.triangles.Length; i++)
				Indicies.Add (MyMesh_.triangles [i]);
			for (int i = 0; i < MyMesh_.uv.Length; i++)
				TextureCoordinates.Add (MyMesh_.uv [i]);
			for (int i = 0; i < MyMesh_.colors32.Length; i++)
				Colors.Add (MyMesh_.colors32 [i]);
		}
	}
	public void UpdateMeshWithData(Mesh MyMesh_, MeshCollider MyMeshCollider, bool IsCentred) {
		if (MyMesh_ == null)
			MyMesh_ = new Mesh ();
		//if (MyMeshCollider == null)
		//	MyMeshCollider = new MeshCollider ();
		if (MyMesh_ != null) {
			if (IsCentred)
			{
				CentreMesh();
			}
			MyMesh_.Clear (); 
			MyMesh_.vertices = Verticies.ToArray ();
			MyMesh_.triangles = Indicies.ToArray ();
			if (TextureCoordinates.Count ==  Verticies.Count)
				MyMesh_.uv = TextureCoordinates.ToArray (); // add this line to the code here 
			if (Colors.Count ==  Verticies.Count)
				MyMesh_.colors32 = Colors.ToArray ();

			MyMesh_.RecalculateBounds ();
			MyMesh_.Optimize ();
			MyMesh_.RecalculateNormals ();

			if (MyMeshCollider != null) {
				MyMeshCollider.sharedMesh = null;	// to refresh it
				MyMeshCollider.sharedMesh = MyMesh_;
			} else {
				//Debug.LogError ("WTH mayne there is no collider :/ " + Time.time);
			}
		} else {
			Debug.LogError ("MyMesh_ is == null>....<");
		}
	}

	public void Add(MyMesh AddMesh) {
		if (AddMesh != null) {
			int VertexBuffer = Verticies.Count;
			for (int i = 0; i < AddMesh.Verticies.Count; i++)
				Verticies.Add (AddMesh.Verticies [i]);

			for (int i = 0; i < AddMesh.Indicies.Count; i++) {
				Indicies.Add (VertexBuffer+AddMesh.Indicies [i]);
			}
			for (int i = 0; i < AddMesh.TextureCoordinates.Count; i++)
				TextureCoordinates.Add (AddMesh.TextureCoordinates [i]);
			for (int i = 0; i < AddMesh.TextureCoordinates.Count; i++)
				Colors.Add (AddMesh.Colors [i]);
		}
	}
	
	public void Refresh(List<Vector3> Verticies_, List<int> Indicies_, List<Vector2> TextureCoordinates_) {
		//ClearMesh ();
		Verticies = Verticies_;
		Indicies = Indicies_;
	}

	public static void CreateCubeMesh(Mesh MyMesh) {
		MyMesh NewMesh = new global::MyMesh();
		NewMesh.CreateCube (new Vector3 (), new Vector3 (1, 1, 1), true, true, true, true, true, true, new Vector2(1,1), new Color32 (255, 255, 255, 255));

		//MyMesh = GetComponent<MeshFilter> ().sharedMesh;
		MyMesh.Clear (); 
		//mesh = new Mesh();
		//mesh.verticies.RemoveAt (0);
			MyMesh.vertices = NewMesh.Verticies.ToArray ();
			MyMesh.triangles = NewMesh.Indicies.ToArray ();
		MyMesh.uv = NewMesh.TextureCoordinates.ToArray (); // add this line to the code here 
		MyMesh.colors32 = NewMesh.Colors.ToArray();
		MyMesh.RecalculateBounds();
		MyMesh.Optimize ();
		MyMesh.RecalculateNormals ();
	}
	public void AddQuadTextureFace(MyMesh MyCustomMesh, Vector2 TextureUnit) {
		float TextureSize = 1f / 8f;
		TextureUnit.x = TextureUnit.x / 8f;
		TextureUnit.y = TextureUnit.y / 8f;
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x + TextureSize, TextureUnit.y));
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x + TextureSize, TextureUnit.y+TextureSize));
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x, TextureUnit.y));
		
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x+TextureSize, TextureUnit.y+TextureSize));
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x, TextureUnit.y+TextureSize));
		MyCustomMesh.TextureCoordinates.Add (new Vector2 (TextureUnit.x, TextureUnit.y));
	}

	public void CreateCube(Vector3 Position, Vector3 Size, bool IsFrontFace, bool IsBackFace, bool IsLeftFace, bool IsRightFace, bool IsTopFace, bool IsBottomFace, Vector2 TextureUnit, Color32 MyColor) {
		if (!IsColorOverride) 
			CubeColor = MyColor;
		CreateCube (Position, Size, IsFrontFace, IsBackFace, IsLeftFace, IsRightFace, IsTopFace, IsBottomFace,  TextureUnit);
	}

	// adds 6 vertexes, 6 indicies, 6 colors per side of the cube
	public void CreateCube(Vector3 Position, Vector3 Size, bool IsFrontFace, bool IsBackFace, bool IsLeftFace, bool IsRightFace, bool IsTopFace, bool IsBottomFace, Vector2 TextureUnit) {
		//float[4] NewColor = {CubeColor.r, CubeColor.g, CubeColor.b, CubeColor.a};
		//CubeColor = new Color (CubeColor.r - ColorDecreaseValue, CubeColor.g - ColorDecreaseValue, CubeColor.b - ColorDecreaseValue, CubeColor.a);
		//CubeColor = new Color (255, 0, 0, 120);
		//CubeColor.r -= ColorDecreaseValue; CubeColor.g -= ColorDecreaseValue; CubeColor.b -= ColorDecreaseValue; CubeColor.a -= ColorDecreaseValue;
		MyMesh MyCustomMesh = this;
		//Debug.Log ("Adding cube at position: " + Position.x + " : " + Position.y + " : " + Position.z);
		if (IsBackFace || IsFrontFace || IsLeftFace || IsRightFace || IsTopFace || IsBottomFace) {
			List<Vector3> CubeVerticies = new List<Vector3>();
			// Remember Y is top
			float x = Position.x;
			float y = Position.y;
			float z = Position.z;
			// The Verticies of a cube
			//if (!IsCentred) {
				CubeVerticies.Add (new Vector3 (0, 1, 0));
				CubeVerticies.Add (new Vector3 (1, 1, 0));
				CubeVerticies.Add (new Vector3 (1, 0, 0));
				CubeVerticies.Add (new Vector3 (0, 0, 0));
				CubeVerticies.Add (new Vector3 (0, 1, 1));
				CubeVerticies.Add (new Vector3 (1, 1, 1));
				CubeVerticies.Add (new Vector3 (1, 0, 1));
				CubeVerticies.Add (new Vector3 (0, 0, 1));
			//} else {
			/*CubeVerticies.Add (new Vector3 (-0.5f, 0.5f, -0.5f));
				CubeVerticies.Add (new Vector3 (0.5f, 0.5f, -0.5f));
				CubeVerticies.Add (new Vector3 (0.5f, -0.5f, -0.5f));
				CubeVerticies.Add (new Vector3 (-0.5f, -0.5f, -0.5f));
				CubeVerticies.Add (new Vector3 (-0.5f, 0.5f, 0.5f));
				CubeVerticies.Add (new Vector3 (0.5f, 0.5f, 0.5f));
				CubeVerticies.Add (new Vector3 (0.5f, -0.5f, 0.5f));
				CubeVerticies.Add (new Vector3 (-0.5f, -0.5f, 0.5f));*/
			//}

			for (int i = 0; i < CubeVerticies.Count; i++) {
				CubeVerticies[i] = new Vector3(CubeVerticies[i].x*Size.x, CubeVerticies[i].y*Size.y, CubeVerticies[i].z*Size.z);
				CubeVerticies[i] += Position;
			}
			Vector2 TexturePosition = new Vector2(0,0);	// goes to 8,8
			Debug.Log("loading backface: " + CubeVerticies.Count);
			// problem with front/back faces
			if (IsFrontFace ) {
				MyCustomMesh.FaceCount++;
				MyCustomMesh.Verticies.Add (CubeVerticies[4]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				MyCustomMesh.Verticies.Add (CubeVerticies[5]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				MyCustomMesh.Verticies.Add (CubeVerticies[6]);
				MyCustomMesh.Verticies.Add (CubeVerticies[5]);
				// back face
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
			if (IsBackFace) {//aswdad
				MyCustomMesh.FaceCount++;
				/**/
				MyCustomMesh.Verticies.Add (CubeVerticies[0]);
				MyCustomMesh.Verticies.Add (CubeVerticies[1]);
				MyCustomMesh.Verticies.Add (CubeVerticies[3]);
				MyCustomMesh.Verticies.Add (CubeVerticies[1]);
				MyCustomMesh.Verticies.Add (CubeVerticies[2]);
				MyCustomMesh.Verticies.Add (CubeVerticies[3]);
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
			if (IsLeftFace) {//dis
				MyCustomMesh.FaceCount++;
				MyCustomMesh.Verticies.Add (CubeVerticies[4]);
				MyCustomMesh.Verticies.Add (CubeVerticies[0]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				MyCustomMesh.Verticies.Add (CubeVerticies[0]);
				MyCustomMesh.Verticies.Add (CubeVerticies[3]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
			if (IsRightFace) {
				MyCustomMesh.FaceCount++;
				//Left face
				MyCustomMesh.Verticies.Add (CubeVerticies[5]);
				MyCustomMesh.Verticies.Add (CubeVerticies[6]);
				MyCustomMesh.Verticies.Add (CubeVerticies[1]);
				MyCustomMesh.Verticies.Add (CubeVerticies[6]);
				MyCustomMesh.Verticies.Add (CubeVerticies[2]);
				MyCustomMesh.Verticies.Add (CubeVerticies[1]);
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
			if (IsTopFace) {
				MyCustomMesh.FaceCount++;
				//Left face
				MyCustomMesh.Verticies.Add (CubeVerticies[4]);
				MyCustomMesh.Verticies.Add (CubeVerticies[5]);
				MyCustomMesh.Verticies.Add (CubeVerticies[0]);
				MyCustomMesh.Verticies.Add (CubeVerticies[5]);
				MyCustomMesh.Verticies.Add (CubeVerticies[1]);
				MyCustomMesh.Verticies.Add (CubeVerticies[0]);
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
			if (IsBottomFace) {
				MyCustomMesh.FaceCount++;
				//Left face
				MyCustomMesh.Verticies.Add (CubeVerticies[6]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				MyCustomMesh.Verticies.Add (CubeVerticies[2]);
				MyCustomMesh.Verticies.Add (CubeVerticies[7]);
				MyCustomMesh.Verticies.Add (CubeVerticies[3]);
				MyCustomMesh.Verticies.Add (CubeVerticies[2]);
				for (int i = 5; i > -1; i--) {
					MyCustomMesh.Indicies.Add (MyCustomMesh.FaceCount*6-i-1);
				}
				AddQuadTextureFace(MyCustomMesh, TextureUnit);
				for (int i = 0; i < 6; i++) {
					MyCustomMesh.Colors.Add (MyCustomMesh.CubeColor);
				}
			}
		}
	}
};