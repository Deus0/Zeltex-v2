using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MyBoneMesh : MyMesh {
	public Transform MyTransform;
	public List<BoneWeight> Weights = new List<BoneWeight>(); // one weight per each vertex - links up to 4 bones?
	public List<Transform> Bones = new List<Transform>();

	//The bind poses. The bind pose at each index refers to the bone with the same index.
	//The bind pose is the inverse of inverse transformation matrix of the bone, when the bone is in the bind pose.
	public List<Matrix4x4> BindPoses = new List<Matrix4x4>();

	// used to colour bones	- each weighting of vertex will correspond to the colours of the bones
	public List<Color32> BoneColors = new List<Color32> ();
	//public List<Transform> Bones = new List<Transform>();

	// these variables are spefically for mesh creation
	public int NumberOfQuads = 4;
	public int HighestSubdivisionLevel = 3;
	public int WeightsGenerationType = 0;
	public bool IsThickness = true;
	// creation options
	public bool IsCreateBones = true;
	public bool IsCreateSpline = true;
	// references
	public SkinnedMeshRenderer MySkinnedMesh;
	public MeshCollider MyMeshCollider;
	
	public MyBoneMesh() 
	{

	}
	public MyBoneMesh(Transform MyTransform_) 
	{
		MyTransform = MyTransform_;
	}

	public void GetSkinnedDataFromMesh() {
		base.GetDataFromMesh (MySkinnedMesh.sharedMesh);
		if (MySkinnedMesh) {
			Bones.Clear ();
			for (int i = 0; i < MySkinnedMesh.bones.Length; i++)
				Bones.Add (MySkinnedMesh.bones [i]);
		}
	}
	public void UpdateBoneMesh() 
	{
		if (Bones.Count > 0) {
			MySkinnedMesh.sharedMesh = new Mesh ();
			base.UpdateMeshWithData (MySkinnedMesh.sharedMesh, MyMeshCollider, true);
			if (WeightsGenerationType == 0)
				UpdateWeightsToClosestBones ();
			else
				UpdateWeightsToClosestBones2 ();	// smoothed weighting

			TurnBonesToColors ();

			// set the bone weights
			MySkinnedMesh.sharedMesh.boneWeights = Weights.ToArray ();
			// set bind poses
			SetBindPoses ();
			MySkinnedMesh.sharedMesh.bindposes = BindPoses.ToArray ();

			// set bones
			MySkinnedMesh.bones = Bones.ToArray ();
		}
		
		if (Verticies.Count > 0) {
			if (Bones.Count == 0) {
				MySkinnedMesh.sharedMesh = new Mesh ();
				base.UpdateMeshWithData (MySkinnedMesh.sharedMesh, MyMeshCollider, true);
			}
			if (MyMeshCollider != null) {
				MyMeshCollider.sharedMesh = null;	// to refresh it
				MyMeshCollider.sharedMesh = MySkinnedMesh.sharedMesh;
			}
			// calculate normals again
			MySkinnedMesh.sharedMesh.RecalculateNormals ();
		}
	}
	
	// sets each vertex weight to the closest bone
	public void UpdateWeightsToClosestBones2() {
		Weights.Clear ();
		for (int i = 0; i < Verticies.Count; i++) {
			Vector3 VertexWorldPosition = Verticies[i] + MyTransform.position;
			BoneWeight Weight1 = new BoneWeight ();
			Weight1.boneIndex0 = FindClosestBone(VertexWorldPosition);
			Weight1.boneIndex1 = FindClosestBone2(VertexWorldPosition);
			float DistanceToBone1 = Vector3.Distance(VertexWorldPosition,Bones[Weight1.boneIndex0].position);
			float DistanceToBone2 = Vector3.Distance(VertexWorldPosition,Bones[Weight1.boneIndex1].position);
			float TotalDistance = DistanceToBone1+DistanceToBone2;
			Weight1.weight0 = DistanceToBone1/TotalDistance;
			Weight1.weight1 = DistanceToBone2/TotalDistance;
			Weights.Add (Weight1);
		}
		//TurnBonesToColors ();
	}

	// sets each vertex weight to the closest bone
	// this doesn't work if scale changes
	public void UpdateWeightsToClosestBones() {
		Weights.Clear ();
		for (int i = 0; i < Verticies.Count; i++) {
			BoneWeight Weight1 = new BoneWeight ();
			Weight1.boneIndex0 = FindClosestBone(Verticies[i] + MyTransform.position);
			Weight1.weight0 = 1;
			Weights.Add (Weight1);
		}
		//TurnBonesToColors ();
	}
	public int FindClosestBone(Vector3 Position) 
	{
		int BoneIndex = 0;
		if (Bones.Count > 0) {
			float ClosestDistance = Vector3.Distance (Bones [BoneIndex].position, Position);
			for (int i = 1; i < Bones.Count; i++) {
				float PossibleClosestDistance = Vector3.Distance (Bones [i].position, Position);
				if (PossibleClosestDistance < ClosestDistance) {
					ClosestDistance = PossibleClosestDistance;
					BoneIndex = i;
				}
			}
		}
		return BoneIndex;
	}
	
	public int FindClosestBone2(Vector3 Position) {
		int BoneIndex = 0;
		int BoneIndex2 = 0;
		if (Bones.Count > 0) {
			float ClosestDistance = Vector3.Distance (Bones [BoneIndex].position, Position);
			for (int i = 1; i < Bones.Count; i++) {
				float PossibleClosestDistance = Vector3.Distance (Bones [i].position, Position);
				if (PossibleClosestDistance < ClosestDistance) {
					ClosestDistance = PossibleClosestDistance;
					BoneIndex2 = BoneIndex;
					BoneIndex = i;
				}
			}
		}
		return BoneIndex2;
	}

	public void SetBindPoses() 
	{
		BindPoses.Clear ();
		for (int i = 0; i < Bones.Count; i++) {
			BindPoses.Add (Bones[i].worldToLocalMatrix * MyTransform.localToWorldMatrix);
		}
		Debug.Log ("Added " + BindPoses.Count + " BindPoses");
	}
	public void SetBoneColorsDefault() 
	{
		BoneColors.Clear ();
		for (int i = BoneColors.Count; i < Bones.Count; i++) {
			BoneColors.Add (new Color32 ((byte)Random.Range (0,255), 
			                             (byte)Random.Range (0,255), 
			                             (byte)Random.Range (0,255), 
			                             255));
		}
	}
	public Color32 MultiplyColorByFloat(Color32 InputColor, float Multiple) 
	{
		InputColor.r = (byte)(Mathf.RoundToInt(InputColor.r*Multiple));
		InputColor.g = (byte)(Mathf.RoundToInt(InputColor.g*Multiple));
		InputColor.b =  (byte)(Mathf.RoundToInt(InputColor.b*Multiple));
		InputColor.a = (byte)(Mathf.RoundToInt(InputColor.a*Multiple));
		return InputColor;
	}
	public Color32 AddColors(Color32 InputColor1, Color32 InputColor2) 
	{
		InputColor1.r += InputColor2.r;
		InputColor1.g += InputColor2.g;
		InputColor1.b += InputColor2.b;
		InputColor1.a += InputColor2.a;
		return InputColor1;
	}
	public void TurnBonesToColors() 
	{
		if (Bones.Count > 0) {
			Colors.Clear();
			SetBoneColorsDefault ();
			Debug.Log ("Changing bone weightings to colours on mesh");
			for (int i = 0; i < Weights.Count; i++) {
				Color32 NewColor = AddColors (
				AddColors (MultiplyColorByFloat (BoneColors [Weights [i].boneIndex0], Weights [i].weight0),
					MultiplyColorByFloat (BoneColors [Weights [i].boneIndex1], Weights [i].weight1)),
			    AddColors (MultiplyColorByFloat (BoneColors [Weights [i].boneIndex2], Weights [i].weight2),
					MultiplyColorByFloat (BoneColors [Weights [i].boneIndex3], Weights [i].weight3))
				);
				Colors.Add (NewColor);
			}
		}
	}

	public void CreateSpline() 
	{
		if (IsCreateSpline)
			CreateSplineMesh ();
		if (IsCreateBones)
			CreateSplineBones ();
		if (!IsCreateSpline) {
			GetSkinnedDataFromMesh();
		}
	}

	public void CreateSplineBones() 
	{
		for (int i = 0; i < NumberOfQuads+1; i++) {
			Transform Bone2 = new GameObject ("Bone " + i).transform;
			if (i >=1 ) {
				Bone2.parent = Bones[Bones.Count-1];
				Bone2.localPosition = new Vector3 (0, MyTransform.lossyScale.y, 0);
				Bone2.localRotation = Quaternion.identity;
			} else {// root bone
				Bone2.parent = MyTransform;
				Bone2.localPosition = new Vector3 (0, 0, 0);
				Bone2.localRotation = Quaternion.identity;
			}
			//if (i == 0 || i >= NumberOfQuads-1) 
			Bones.Add (Bone2);
		}
	}
	public void AddBone(Vector3 BoneBeginPosition, Vector3 BoneEndPosition) {
		Transform NewBone = (new GameObject ("Bone " + Bones.Count)).transform;
		NewBone.parent = MyTransform;
		NewBone.position = BoneBeginPosition;
		NewBone.localRotation = Quaternion.identity;
		Bones.Add (NewBone);

		Transform NewBone2 = (new GameObject ("Bone " + Bones.Count)).transform;
		NewBone2.parent = NewBone;
		NewBone2.position = BoneEndPosition;
		NewBone2.localRotation = Quaternion.identity;
		Bones.Add (NewBone2);
		UpdateBoneMesh ();

		//UpdateWeightsToClosestBones2 ();
		//TurnBonesToColors ();
	}

	// connects up 2 bones, which morphs 4 transforms into 3
	public void ConnectBones(int BoneIndex1, int BoneIndex2) {

	}

	public void CreateSplineMesh() 
	{
		Debug.Log ("Creating Basic Quad Mesh");
		// i should create the splines first
		// then go over and subdivide the whole thing
		// i'de just need to record the faces
		int PreviousIndex1 = 0;
		int PreviousIndex2 = 1;

		// front
		for (int i = 0; i < NumberOfQuads+1; i++) 
		{
			Verticies.Add (new Vector3 (-1, i, 1)); 
			Verticies.Add (new Vector3 (1, i, 1));
			if (i == 0) {
				PreviousIndex1 = Verticies.Count-2;
				PreviousIndex2 =  Verticies.Count-1;
			} else {
				int CurrentIndex1 = Verticies.Count-2;
				int CurrentIndex2 = Verticies.Count-1;
				// this adds alot of vertexes, so i need to remember the previous 2 points in the quad chain
				SubDivideLastQuad(CurrentIndex1, CurrentIndex2, PreviousIndex2, PreviousIndex1);
				PreviousIndex1 = CurrentIndex1;
				PreviousIndex2 = CurrentIndex2;
			}
		}

		if (IsThickness) 
		{
			// bottom
			Verticies.Add (new Vector3 (-1, 0, 1));
			Verticies.Add (new Vector3 (1, 0, 1));
			Verticies.Add (new Vector3 (1, 0, -1));
			Verticies.Add (new Vector3 (-1, 0, -1));
			SubDivideLastQuad ();
			// top
			Verticies.Add (new Vector3 (-1, NumberOfQuads, -1));
			Verticies.Add (new Vector3 (1, NumberOfQuads, -1));
			Verticies.Add (new Vector3 (1, NumberOfQuads, 1));
			Verticies.Add (new Vector3 (-1, NumberOfQuads, 1));
			SubDivideLastQuad ();

			// back
			for (int i = 0; i < NumberOfQuads+1; i++) {
				Verticies.Add (new Vector3 (1, i, -1));
				Verticies.Add (new Vector3 (-1, i, -1)); 
				if (i == 0) {
					PreviousIndex1 = Verticies.Count-2;
					PreviousIndex2 =  Verticies.Count-1;
				} else {
					int CurrentIndex1 = Verticies.Count - 2;
					int CurrentIndex2 = Verticies.Count - 1;
					// this adds alot of vertexes, so i need to remember the previous 2 points in the quad chain
					SubDivideLastQuad (CurrentIndex1, CurrentIndex2, PreviousIndex2, PreviousIndex1);
					PreviousIndex1 = CurrentIndex1;
					PreviousIndex2 = CurrentIndex2;
				}
			}
			// left side
			for (int i = 0; i < NumberOfQuads+1; i++) {
				Verticies.Add (new Vector3 (1, i, 1));
				Verticies.Add (new Vector3 (1, i, -1)); 
				if (i == 0) {
					PreviousIndex1 = Verticies.Count-2;
					PreviousIndex2 =  Verticies.Count-1;
				} else {
					int CurrentIndex1 = Verticies.Count - 2;
					int CurrentIndex2 = Verticies.Count - 1;
					// this adds alot of vertexes, so i need to remember the previous 2 points in the quad chain
					SubDivideLastQuad (CurrentIndex1, CurrentIndex2, PreviousIndex2, PreviousIndex1);
					PreviousIndex1 = CurrentIndex1;
					PreviousIndex2 = CurrentIndex2;
				}
			}
			// right side
			for (int i = 0; i < NumberOfQuads+1; i++) {
				Verticies.Add (new Vector3 (-1, i, -1)); 
				Verticies.Add (new Vector3 (-1, i, 1));
				if (i == 0) {
					PreviousIndex1 = Verticies.Count-2;
					PreviousIndex2 =  Verticies.Count-1;
				} else {
					int CurrentIndex1 = Verticies.Count - 2;
					int CurrentIndex2 = Verticies.Count - 1;
					// this adds alot of vertexes, so i need to remember the previous 2 points in the quad chain
					SubDivideLastQuad (CurrentIndex1, CurrentIndex2, PreviousIndex2, PreviousIndex1);
					PreviousIndex1 = CurrentIndex1;
					PreviousIndex2 = CurrentIndex2;
				}
			}
		}
		Debug.Log ("Added " + Weights.Count + " Bone Weights");
		Debug.Log ("Creating Bones");
		Debug.Log ("Added " + Bones.Count + " Bones");
		
		Debug.Log ("Finished Custom Bones Test.");
	}
	// input is a bunch of faces
	// output is a subdivision of them
	public void SubDivideFaces() 
	{

	}

	public void SubDivideQuads(int Subdivisions) 
	{
		for (int i = 0; i < Subdivisions; i++) 
		{

		}
	}
	public Vector3 GetMidPoint(Vector3 Point1, Vector3 Point2) 
	{
		Vector3 Difference = Point2 - Point1;
		return Point1 += (Difference / 2f);
	}
	public void SubDivideLastQuad() 
	{
		//SubDivideLastQuad (Verticies.Count - 2, Verticies.Count - 1, Verticies.Count - 3, Verticies.Count - 4);
		SubDivideLastQuad (Verticies.Count - 4, Verticies.Count - 3, Verticies.Count - 2, Verticies.Count - 1);
	}
	public void SubDivideLastQuad(int TopLeftCornerIndex,int TopRightCornerIndex, 	int BottomRightCornerIndex, int BottomLeftCornerIndex) 
	{
		// first remove indicies of Quad
		//for (int i = 0; i < 6; i++)
		//	Indicies.RemoveAt (Indicies.Count - 1);
		
		// Get vertex positions of the quad corners
		Vector3 TopLeftCorner = Verticies [TopLeftCornerIndex];
		Vector3 TopRightCorner = Verticies [TopRightCornerIndex];
		Vector3 BottomRightCorner = Verticies [BottomRightCornerIndex];
		Vector3 BottomLeftCorner = Verticies [BottomLeftCornerIndex];
		
		//for (int i = 0; i < 4; i++)
		//	Verticies.RemoveAt (Verticies.Count - 1);

		// 0,1,2,3
		//BottomLeftCornerIndex = Verticies.Count - 4;
		//BottomRightCornerIndex = Verticies.Count - 3;
		//TopLeftCornerIndex = Verticies.Count - 2;
		//TopRightCornerIndex = Verticies.Count - 1;
		SubDivideQuad (HighestSubdivisionLevel, 	TopLeftCorner,  	TopRightCorner, 	BottomRightCorner, 		BottomLeftCorner,
		               		TopLeftCornerIndex,	TopRightCornerIndex,BottomRightCornerIndex,	BottomLeftCornerIndex);
	}

	// Takes a bunch of faces and turns them into cubes based on their normals (or perhaps all one direction?)
	public void Extrude() 
	{

	}
	// Grabs 4 verticies
	public void SubDivideQuad(int SubDivisionsLeft, Vector3 TopLeftCorner, 		Vector3 TopRightCorner, 	Vector3 BottomRightCorner, 	Vector3 BottomLeftCorner,
	                          						int TopLeftCornerIndex,		int TopRightCornerIndex, 	int BottomRightCornerIndex, int BottomLeftCornerIndex) {
		if (SubDivisionsLeft >= 1) {
			SubDivisionsLeft--;

			// need to add in 4 mid points
			Vector3 MidTop = GetMidPoint (TopLeftCorner, TopRightCorner);
			Vector3 MidRight = GetMidPoint (TopRightCorner, BottomRightCorner);
			Vector3 MidBottom = GetMidPoint (BottomRightCorner, BottomLeftCorner);
			Vector3 MidLeft = GetMidPoint (BottomLeftCorner, TopLeftCorner);
			Vector3 MiddlePosition = GetMidPoint (MidTop, MidBottom);

			// Add verticies
			Verticies.Add (MiddlePosition);	int MiddlePositionIndex = Verticies.Count - 1;
			Verticies.Add (MidTop);			int MidTopIndex = Verticies.Count - 1;
			Verticies.Add (MidRight);		int MidRightIndex = Verticies.Count - 1;
			Verticies.Add (MidBottom);		int MidBottomIndex = Verticies.Count - 1;
			Verticies.Add (MidLeft);		int MidLeftIndex = Verticies.Count - 1;
			
			if (SubDivisionsLeft >= 1) {
				//SubDivideQuad(SubDivisionsLeft, 
				//              TopLeftCorner,		MidLeft, 		MiddlePosition, 		TopLeftCorner, 
				//              TopLeftCornerIndex,	MidLeftIndex,	MiddlePositionIndex, 	TopLeftCornerIndex);
				SubDivideQuad(SubDivisionsLeft, 	
								TopLeftCorner, 		MidTop, 		MiddlePosition, 		MidLeft,
				              TopLeftCornerIndex, MidTopIndex, 	MiddlePositionIndex, 	MidLeftIndex);
				SubDivideQuad(SubDivisionsLeft, MidTop, TopRightCorner, MidRight, MiddlePosition,
				              MidTopIndex, TopRightCornerIndex, MidRightIndex, MiddlePositionIndex);
				SubDivideQuad(SubDivisionsLeft, MiddlePosition, MidRight, BottomRightCorner, MidBottom,
				              MiddlePositionIndex, MidRightIndex, BottomRightCornerIndex, MidBottomIndex);
				SubDivideQuad(SubDivisionsLeft, MidLeft, MiddlePosition, MidBottom, BottomLeftCorner,
				              MidLeftIndex, MiddlePositionIndex, MidBottomIndex, BottomLeftCornerIndex);
			}
			else if (SubDivisionsLeft == 0) {
				AddQuadIndicies (TopLeftCornerIndex, 	MidTopIndex, 			MiddlePositionIndex, 	MidLeftIndex);			// top left quad
				AddQuadIndicies (MidTopIndex, 			TopRightCornerIndex, 	MidRightIndex, 			MiddlePositionIndex); 	// Top right quad
				AddQuadIndicies (MiddlePositionIndex, 	MidRightIndex, 			BottomRightCornerIndex, MidBottomIndex);		// BottomRight Quad
				AddQuadIndicies (MidLeftIndex, 			MiddlePositionIndex, 	MidBottomIndex, 		BottomLeftCornerIndex);	// Bottom Left Quad
			}
		}

		// no add in indicies for all the points - A total of 4 + 5 verticies - 6 indicies per quad - 24 indicies

		// top right is bottom left?
		// mid top is mid bottom
		//AddQuadIndicies (MidLeftIndex, MidTopIndex, MidRightIndex, MidLeftIndex);	// test diamond shape
		//AddQuadIndicies (TopLeftCornerIndex, TopRightCornerIndex, BottomRightCornerIndex, BottomLeftCornerIndex); // test quad
		
		//AddQuadIndicies (MidLeftIndex, MidRightIndex, BottomRightCornerIndex, BottomLeftCornerIndex); // BottomHalf
		//AddQuadIndicies (TopLeftCornerIndex, TopRightCornerIndex, MidRightIndex, MidLeftIndex); // Top Half
		//AddQuadIndicies (TopLeftCornerIndex, MidBottomIndex, BottomRightCornerIndex, BottomLeftCornerIndex); // Left Half
	}
	public void AddQuadIndicies(int Index1, int Index2, int Index3, int Index4) {
		Indicies.Add (Index1);
		Indicies.Add (Index3);
		Indicies.Add (Index2);
		Indicies.Add (Index1);
		Indicies.Add (Index4);
		Indicies.Add (Index3);
	}
	// return index of point in the middle of p1 and p2
	private int GetMiddlePoint(int p1, int p2)	{
		return -1;
	}
}

public class CustomBones : MonoBehaviour {
	public MyBoneMesh MySkinnedMesh = new MyBoneMesh();
	public Material MyMaterial;
	public bool IsBoneColours;
	public Shader MyBoneShader;
	public Shader NormalShader;
	public AnimationCurve curve;
	public AnimationClip clip;
	private SkinnedMeshRenderer MySkinnedMeshRenderer;
	private Animation MyAnimation;
	public bool IsAnimating = false;
	// dynamic animation
	public float BoneAmplitude = 0.01f;
	public float WaveDifference =  0.07f;
	public float WaveSpeed = 3;
	public float RotationSpeed = 1;
	public float RotationAmplitude = 0.01f;
	public float SwaySpeed = 0.15f;
	public bool IsCreateSpline = false;
	MeshCollider MyMeshCollider;

	void Start() {
		MySkinnedMesh.MyTransform = gameObject.transform;
		MySkinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer> ();
		if (MySkinnedMeshRenderer == null)
			MySkinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer> (); 
		MySkinnedMesh.MySkinnedMesh = MySkinnedMeshRenderer;
		
		MyMeshCollider = gameObject.GetComponent<MeshCollider> ();
		if (MyMeshCollider == null)
			MyMeshCollider = gameObject.AddComponent<MeshCollider> ();
		
		MySkinnedMesh.MyMeshCollider = MyMeshCollider;
		MyMeshCollider.sharedMesh = MySkinnedMeshRenderer.sharedMesh;

		//MyAnimation = gameObject.AddComponent<Animation>();
		
		if (MyMaterial != null)
			MySkinnedMeshRenderer.material = MyMaterial;
		else
			MySkinnedMeshRenderer.material = new Material(Shader.Find("Diffuse"));

		if (IsCreateSpline) {
			MySkinnedMesh.CreateSpline ();
			//MySkinnedMesh.UpdateBoneMesh (MySkinnedMeshRenderer, gameObject.GetComponent<MeshCollider>());
			MySkinnedMesh.UpdateBoneMesh ();
		}
		//CreateAnimation ();
	}
	
	//Transform[] bones = new Transform[2];
	void Update() {
		if (IsAnimating) {
			//gameObject.GetComponent<MeshCollider> ().sharedMesh = MySkinnedMeshRenderer.BakeMesh();

			for (int i = 0; i < MySkinnedMesh.Bones.Count; i++) {
				float MyBoneAmplitude = BoneAmplitude * i;
				float MyRotationAmplitude = RotationAmplitude * i;
				float MyWaveDifference = WaveDifference * i;
				if (i < 2) {
					MySkinnedMesh.Bones [i].eulerAngles +=
						new Vector3 (0, 0, SwaySpeed * Mathf.Sin (MyWaveDifference + RotationSpeed * Time.time));
				}
				//if (i < 1)
				MySkinnedMesh.Bones [i].localPosition +=
					new Vector3 (0, MyBoneAmplitude * Mathf.Sin (MyWaveDifference + WaveSpeed * Time.time), 0);
				//Quaternion BoneRotation = 
				MySkinnedMesh.Bones [i].eulerAngles +=
					new Vector3 (0, MyRotationAmplitude * Mathf.Sin (MyWaveDifference + RotationSpeed * Time.time), 0);
				
			}
			//MySkinnedMesh.Bones [4].localPosition += new Vector3 (0,0.5f * Mathf.Sin (Time.time), 0);
		}
		//DebugRenderBoneLinks ();
		if (MyMaterial) {
			if (IsBoneColours) {
				MyMaterial.shader = MyBoneShader;
			} else {
				MyMaterial.shader = NormalShader;
			}
		}
	}
	public void CreateTestBoneMesh() {
		

	}
	public void CreateAnimation() {
		curve = new AnimationCurve();
		curve.keys = new Keyframe[] {
			new Keyframe(0, 0, 0, 0), 
			new Keyframe(1, 3, 0, 0), 
			new Keyframe(2, 0.0F, 0, 0)
		};
		
		clip = new AnimationClip();
		clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.z", curve);
		MyAnimation.AddClip(clip, "test");
		MyAnimation.Play("test");
	}
	public void ConvertBoneWeightsToColours() {

	}
	public void DebugRenderVerticies() {

	}

	// should just be - from bone parent to child, render all the links from the root bone
	public void DebugRenderBoneLinks() {
		if (MySkinnedMesh.Bones.Count > 0) {
			DebugDraw.DrawSphere (MySkinnedMesh.Bones [0].transform.position, 0.2f, Color.red);
			for (int i = 0; i < MySkinnedMesh.Bones.Count-1; i++) {
				Color32 NewColor = Color.blue;
				if (i == MySkinnedMesh.Bones.Count-2)
					NewColor = Color.red;
				DebugRenderBone (MySkinnedMesh.Bones [i].transform, MySkinnedMesh.Bones [i + 1].transform, NewColor);
			}
		}
	}
	public void DebugRenderBone(Transform Joint1, Transform Joint2, Color32 NewColor) {
		DebugDraw.DrawSphere (Joint2.position, 0.2f, NewColor);
		//Transform NewTransform = transform; transform.LookAt
		//Quaternion NewQuaternion = new Quaternion();
		//NewQuaternion.eulerAngles = MySkinnedMesh.Bones[i].transform.position-MySkinnedMesh.Bones[i+1].transform.position;
		//if (i != MySkinnedMesh.Bones.Count-1)
			Debug.DrawLine (Joint1.position,
			                Joint2.position,
		                Color.blue);
		/*DebugDraw.DrawCubeStretched(MySkinnedMesh.Bones[i].transform.position, 
				                            NewQuaternion, 
				                            new Vector3(0.05f, 
				            Vector3.Distance(MySkinnedMesh.Bones[i].transform.position,MySkinnedMesh.Bones[i+1].transform.position), 
				                                	0.05f),
				                            Color.blue);*/
	}
		//for the normal of each vertex render a different colour per bone
	public void DebugRenderBoneWeights() {

	}
}