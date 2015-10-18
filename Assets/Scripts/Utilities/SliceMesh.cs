using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// check out the assets stores packages on destroying meshes
public class SliceMesh : MonoBehaviour {
	public float PlaneDistance = 1f;
	public Vector3 MyPlanePosition = new Vector3 (0,0,0);	// the normal for a plane
	public Vector3 MyPlaneNormal = new Vector3 (0,1f,0);	// the normal for a plane
	//public  MyPlaneRotation = new Rotator (0, 0, 0);
	public bool IsSlice = true;
	public CustomMesh MyMesh;
	public bool TestSliceMesh = false;
	public CustomMesh SlicedCustomMesh1;
	public CustomMesh SlicedCustomMesh2;
	public Plane MySlicePlane;
	public List<int> SlicedIndicies1;
	public List<Vector3> SlicedPoints1;
	public List<int> SlicedIndicies2;
	public List<Vector3> SlicedPoints2;
	// The indicies that are made from the lines intersecting with planes
	public List<int> SlicedIndicies3;
	public List<Vector3> SlicedPoints3;
	// The indicies that are remade from SlicedIndicies3
	public List<int> SlicedIndicies4;
	public List<Vector3> SlicedPoints4;
	// Use this for initialization
	void Start () {
		MyMesh = (CustomMesh)gameObject.GetComponent ("CustomMesh");
		//SliceMyMesh ();
	}

	// Debug stuff
	// Debug lines for the slicing
	// an option to create new meshes from slices
	// make the original custom mesh debug the mesh points

	// Update is called once per frame
	void Update () {
		if (TestSliceMesh) {
			TestSliceMesh = false;
			SliceMyMesh();
		}
		float DebugSize = 0.1f;
		// original triangle points
		for (int i = 0; i < SlicedPoints3.Count; i++) {
			Debug.DrawLine((gameObject.transform.position + gameObject.transform.rotation*SlicedPoints3[i]), 
			               (gameObject.transform.position + gameObject.transform.rotation*SlicedPoints3[i]) + new Vector3(0,0,DebugSize),
			               Color.red);
		}
		// sliced points
		for (int i = 0; i < SlicedPoints4.Count; i++) {
			Debug.DrawLine((gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[i]), 
			               (gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[i]) + new Vector3(0,0,DebugSize),
			                Color.green);
			if (i != SlicedPoints4.Count-1)
				Debug.DrawLine((gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[i]) + new Vector3(0,0,DebugSize), 
				               (gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[i+1]) + new Vector3(0,0,DebugSize),
				               Color.blue);
			else
				Debug.DrawLine((gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[i]) + new Vector3(0,0,DebugSize), 
				               (gameObject.transform.position + gameObject.transform.rotation*SlicedPoints4[0]) + new Vector3(0,0,DebugSize),
				               Color.blue);
		}
		/*for (int i = 0; i <  SlicedIndicies3.Count; i++) {
			Debug.DrawLine (gameObject.transform.position + SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]], 
			                gameObject.transform.position + SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] + new Vector3(0,0,0.5f),
			                Color.red);
		}*/
	}
	public void DebugVector3(Vector3 DebugVector) {
		Debug.Log ("     Debug Vector3: " + DebugVector.x + " :: " + DebugVector.y + " :: " + DebugVector.z);
	}
	public void SliceMyMesh() {
		if (MyMesh) {
			//GameObject SlicedMesh = (GameObject) Instantiate (gameObject, transform.position, Quaternion.identity);	// off set the positions after the meshes are created, by finding out the releative bounds difference
			SlicedCustomMesh1 = (CustomMesh) gameObject.GetComponent("CustomMesh");
			//SlicedCustomMesh2 = (CustomMesh) SlicedMesh.GetComponent("CustomMesh");

			//SlicedCustomMesh2.IsSingleCube = false;

			//MyPlaneNormal = MyPlaneRotation.transform.position;
			MySlicePlane = new Plane(MyPlaneNormal, PlaneDistance);
			MySlicePlane.SetNormalAndPosition(MyPlaneNormal, new Vector3(0,0.5f,0));
			 SlicedPoints1 = new List<Vector3>();
			SlicedIndicies1 = new List<int>();
			SlicedPoints2 = new List<Vector3>();
			SlicedIndicies2 = new List<int>();
			// Where the plane intersects
			SlicedIndicies3 = new List<int>();
			SlicedIndicies4 = new List<int>();
			SlicedPoints4 = new List<Vector3>();
			//SlicedMesh.MyCustomMesh

			// Add all the indicies that intersect with the plane
			for (int i = 0; i < SlicedCustomMesh1.MyCustomMesh.Indicies.Count; i += 3) {	// for all the meshes triangles
				Vector3 Vertex1 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i]];
				Vector3 Vertex2 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i+1]];
				Vector3 Vertex3 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i+2]];
				
				if (MySlicePlane.GetSide(Vertex1+MyPlanePosition) && MySlicePlane.GetSide(Vertex2+MyPlanePosition) && MySlicePlane.GetSide(Vertex3+MyPlanePosition)) {	// if all 3 verticies are on the same side

				} else if (!MySlicePlane.GetSide(Vertex1+MyPlanePosition) && !MySlicePlane.GetSide(Vertex2+MyPlanePosition) && !MySlicePlane.GetSide(Vertex3+MyPlanePosition)) { // if they are not on same side, they are intersected by the plane

				} else {
					SlicedIndicies3.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i]);
					SlicedIndicies3.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+1]);
					SlicedIndicies3.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+2]);
					SlicedPoints3.Add (Vertex1);
					SlicedPoints3.Add (Vertex2);
					SlicedPoints3.Add (Vertex3);
				}
			}

			// Now create slices in these triangles
			for (int i = 0; i < SlicedIndicies3.Count; i += 3) {	// for all the meshes triangles
				Vector3 Vertex1 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]];
				Vector3 Vertex2 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i+1]];
				Vector3 Vertex3 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i+2]];
				// from a slice through a triangle
				// we can get a maximum of 5 points
				// break down 5 points into 3 triangles
				// test this by chose random points along the triangle lines and adding them vertexes

				//RaycastHit hit1;
				Ray Ray1 = new Ray(Vertex1, Vertex1-Vertex2);
				float RayDistance1 = Vector3.Distance (Vertex1, Vertex2);
				if (MySlicePlane.Raycast(Ray1, out RayDistance1)) {
					Debug.Log(" Triangles are sliced 1" );
					DebugVector3 (SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]]);
				} else  {
					Debug.Log(i + " Triangles Not Sliced Vertex 1 - ");
					Debug.Log ("    Distance: " + RayDistance1);
					DebugVector3 (Vertex1);
					DebugVector3 (Vertex1 + Ray1.direction*RayDistance1);

					if (RayDistance1 != 0) {
						SlicedIndicies4.Add (i);
						SlicedPoints4.Add (Vertex1 + Ray1.direction*RayDistance1);
						//SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] = Vertex1 + Ray1.direction*RayDistance1;
					}
				}
				Ray Ray2 = new Ray(Vertex2, Vertex2-Vertex3);
				float RayDistance2 = Vector3.Distance (Vertex1, Vertex2);
				if (MySlicePlane.Raycast(Ray2, out RayDistance2)) {
					Debug.Log(" Triangles are sliced 2" );
					SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] = Vertex2 + Ray2.direction*RayDistance2;
				} else  {
					Debug.Log(i + " Triangles Not Sliced 2 - ");
					Debug.Log ("    Distance: " + RayDistance2);
					DebugVector3 (Vertex2);
					DebugVector3 (Vertex2 + Ray2.direction*RayDistance2);
					if (RayDistance2 != 0) {
						SlicedIndicies4.Add (i+1);
						SlicedPoints4.Add (Vertex2 + Ray2.direction*RayDistance2);
						//SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] = Vertex2 + Ray2.direction*RayDistance2;
					}
				}
				Ray Ray3 = new Ray(Vertex3, Vertex3-Vertex1);
				float RayDistance3 = Vector3.Distance (Vertex3, Vertex1);
				if (MySlicePlane.Raycast(Ray3, out RayDistance3)) {
					Debug.Log(" Triangles are sliced 3" );
					DebugVector3 (Vertex2);
					SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] = Vertex3 + Ray3.direction*RayDistance3;
				} else  {
					Debug.Log(i + " Triangles a Not Sliced 3 -");
					Debug.Log ("    Distance: " + RayDistance3);
					DebugVector3 (Vertex3);
					DebugVector3 (Vertex3 + Ray3.direction*RayDistance3);
					if (RayDistance3 != 0) {
						SlicedIndicies4.Add (i+2);
						SlicedPoints4.Add (Vertex3 + Ray3.direction*RayDistance3);
						//SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedIndicies3[i]] = Vertex3 + Ray3.direction*RayDistance3;
					}
				}
				//Mathf.Int
			}
			// test slice through the middle, where the plane is .5, .5 ,.5
			// Concieve a 2d plane that is the slice (sword action pewpew)
			// find all the intersecting points with the mesh
			for (int i = 0; i < SlicedCustomMesh1.MyCustomMesh.Indicies.Count; i += 3) {	// for all the meshes triangles
				//SlicedCustomMesh2.MyCustomMesh.Indicies[i] = 0;
				Vector3 Vertex1 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i]];
				Vector3 Vertex2 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i+1]];
				Vector3 Vertex3 = SlicedCustomMesh1.MyCustomMesh.Verticies[SlicedCustomMesh1.MyCustomMesh.Indicies[i+2]];
				// if our vertexes are on the positive side of the plane, add to mesh 1
				//if (Vertex1.y > 0.5f) {
				if (MySlicePlane.GetSide(Vertex1) && MySlicePlane.GetSide(Vertex2) && MySlicePlane.GetSide(Vertex3)) {	// if all 3 verticies are on the same side
					SlicedIndicies1.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i]);
					SlicedIndicies1.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+1]);
					SlicedIndicies1.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+2]);
				//} else {
				} else if (!MySlicePlane.GetSide(Vertex1) && !MySlicePlane.GetSide(Vertex2) && !MySlicePlane.GetSide(Vertex3)){ // if they are not on same side, they are intersected by the plane
					SlicedIndicies2.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i]);
					SlicedIndicies2.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+1]);
					SlicedIndicies2.Add(SlicedCustomMesh1.MyCustomMesh.Indicies[i+2]);
				}
			}

			//for (int i = 0; i < 0
			// Add all the points onto 2 new meshes

			// Add the rest of the meshes points onto the 2 created ones

			// Instantiate the new objects in the points it was cut

			// test this with non moving objects for positioning
			//SlicedCustomMesh1.MyCustomMesh.Verticies = SlicedPoints1;
			if (IsSlice) {
				SlicedCustomMesh1.MyCustomMesh.Indicies.Clear ();
				//SlicedCustomMesh1.MyCustomMesh.Indicies = SlicedIndicies1;
				SlicedCustomMesh1.MyCustomMesh.Indicies = SlicedIndicies3;
				//SlicedCustomMesh2.MyCustomMesh.Indicies.Clear ();
				//SlicedCustomMesh2.MyCustomMesh.Indicies = SlicedIndicies2;

			//SlicedCustomMesh1.MyCustomMesh.Refresh(SlicedPoints1, SlicedIndicies1, SlicedCustomMesh1.MyCustomMesh.TextureCoordinates);
			//SlicedCustomMesh2.MyCustomMesh.Refresh(SlicedPoints2, SlicedIndicies2, SlicedCustomMesh2.MyCustomMesh.TextureCoordinates);

				SlicedCustomMesh1.UpdateMesh(SlicedCustomMesh1.MyCustomMesh);
				//SlicedCustomMesh2.UpdateMesh(SlicedCustomMesh2.MyCustomMesh);
			}
		}
	}
}
