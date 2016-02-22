using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class MyAnimationData {
	public string ObjectPath;
	public string PropertyName;
	public GameObject MyBindedObject;
	public AnimationCurve MyCurve;

	public MyAnimationData(string NewPath, string NewPropertyName, GameObject NewObject, AnimationCurve NewCurve) {
		ObjectPath = NewPath;
		PropertyName = NewPropertyName;
		MyBindedObject = NewObject;
		MyCurve = NewCurve;
	}
	public string GetLabel() {
		string Label = "";
		Label += MyBindedObject.name + "-";
		Label += PropertyName + "-";
		//Label += MyCurve.ToString () + "-";
		return Label;
	}
}

[ExecuteInEditMode]
public class CustomAnimator : MonoBehaviour {
	[Header("Debug")]
	public bool IsDebugMode = false;
	public KeyCode BeginAnimationKey = KeyCode.O;
	public KeyCode PauseAnimationKey = KeyCode.P;
	private bool IsFixedUpdate = false;
	[Header("References")]
	public AnimationClip MyAnimation;
	public GameObject RootBone;
	[SerializeField] private List<MyAnimationData> MyBones = new List<MyAnimationData>();
	[SerializeField] private List<string> BoneNames = new List<string>();
	[SerializeField] private List<GameObject> BoneReferences = new List<GameObject>();
	[Header("Options")]
	public List<string> MyMasking;
	public bool IsImport = false;
	public float AnimationTimeBegin;
	public bool IsAnimating = true;
	Vector3 InitialLocalPosition;
	//public AnimationCurve MyCurve;
	//public List<GameObject> MyBones;
	
	#if UNITY_EDITOR
	public UnityEditor.EditorCurveBinding[] MyCurveBindings;
	#endif
	Quaternion DebugRotation;
	void OnGUI() {
		if (IsDebugMode)
		{
			GUILayout.Label("IsAnimating " + IsAnimating.ToString() + " - Time: " + AnimationTime.ToString() );
			GUILayout.Label("DebugRotation: " + DebugRotation.ToString());

			GUILayout.Label("Animation Clip: " + MyAnimation.name + " [" + MyAnimation.length + "]");
			GUILayout.Label("Bones To animate: " + BoneReferences.Count + " with " + MyBones.Count + " instructions");
			GUILayout.Label("Bounds: " + MyAnimation.localBounds.ToString());
			for (int i = 0; i < BoneReferences.Count; i++) 
			{
				if (BoneReferences[i])
					GUILayout.Label (i + ":" + BoneReferences[i].name);
				else
					GUILayout.Label (i + ": Is Null..");
			}
			if (Input.GetKey(KeyCode.X)) {
				for (int i = 0; i < MyBones.Count; i += 4) {
					if (i+3 < MyBones.Count)
						GUILayout.Label ("[" + i + "]:[" + MyBones[i].PropertyName+"]"
						                 +"\t[" + (i+1) + "]:[" + MyBones[i+1].PropertyName+"]"
						                 +"\t[" + (i+2) + "]:[" + MyBones[i+2].PropertyName+"]"
						                 +"\t[" + (i+3) + "]:[" + MyBones[i+3].PropertyName+"]");
				}
			} 
			else if (Input.GetKey(KeyCode.Z)) {
				for (int i = 0; i < MyBones.Count; i += 4) {
					if (i+3 < MyBones.Count)
						GUILayout.Label ("[" + i + "]:[" + MyBones[i].MyBindedObject.name+"]"
						                 +"\t[" + (i+1) + "]:[" + MyBones[i+1].MyBindedObject.name+"]"
						                 +"\t[" + (i+2) + "]:[" + MyBones[i+2].MyBindedObject.name+"]"
						                 +"\t[" + (i+3) + "]:[" + MyBones[i+3].MyBindedObject.name+"]");
				}
			}else {
			GUILayout.Label ("Curves");
			for (int i = 0; i < MyBones.Count; i+= 4) 
			{
				if (i+3 < MyBones.Count)
				//GUILayout.Label (MyBones[i].GetLabel());
					GUILayout.Label (	RoundTo2Dec(MyBones[i].MyCurve.Evaluate(AnimationTime)).ToString()
					                 +"\t"+RoundTo2Dec(MyBones[i+1].MyCurve.Evaluate(AnimationTime)).ToString()
					                 +"\t"+RoundTo2Dec(MyBones[i+2].MyCurve.Evaluate(AnimationTime)).ToString()
					                 +"\t"+RoundTo2Dec(MyBones[i+3].MyCurve.Evaluate(AnimationTime)).ToString()
					                 );
			}
			}
			/*for (int i = 0; i < MyAnimation.events.Length; i++) 
			{
				GUILayout.Label(MyAnimation.events[i].functionName);
			}*/
			
			/*for (int i = 0; i < MyCurveBindings.Length; i++)  {
				GUILayout.Label("CurveBinding: " + MyCurveBindings[i].propertyName +
				                ":" + MyCurveBindings[i].path);
				               // + ":" + MyCurveBindings[i].isPPtrCurve);
			}*/
		}
	}
	public float RoundTo2Dec(float Input) {
		return Mathf.RoundToInt (100*Input)/100f;
	}
	private void ConvertAnimationToData() {
#if UNITY_EDITOR
		MyBones.Clear();
		BoneNames.Clear();
		BoneReferences.Clear();
		//MyCurves = UnityEditor.AnimationUtility.GetAllCurves (MyAnimation);
		MyCurveBindings = UnityEditor.AnimationUtility.GetCurveBindings (MyAnimation);
		for (int i = 0; i < MyCurveBindings.Length; i++) {
			//MyCurves.Add();
			string BoneName = MyCurveBindings[i].path;
			while (true) {
				BoneName = BoneName.Substring(BoneName.IndexOf("/")+1);
				if (!BoneName.Contains("/"))
					break;
			}
			GameObject MyBone = FindChildBone(gameObject, BoneName);
			AddToBoneNames(BoneName);
			MyBones.Add (new MyAnimationData(BoneName,
			                                 MyCurveBindings[i].propertyName,
			                                 MyBone,
			                                 UnityEditor.AnimationUtility.GetEditorCurve(MyAnimation, MyCurveBindings[i])));
			if (MyAnimation.isLooping) 
			{
				MyBones [i].MyCurve.postWrapMode = WrapMode.Loop;
			}
		}
		for (int i = 0; i < BoneNames.Count; i++) {
			BoneReferences.Add (FindChildBone(gameObject, BoneNames[i]));
		}
		#endif
	}
	void Start() 
	{
		if (RootBone == null)
			RootBone = gameObject;
		//AddPositions ();
		ResetAnimation();
	}
	void Update() 
	{
		// actions
		if (IsImport) 
		{ 
			IsImport = false;
			Debug.LogError("Importing Animation!");
			ConvertAnimationToData();
		}
		if (Input.GetKeyDown (BeginAnimationKey)) 
		{
			ResetAnimation();
		}
		if (Input.GetKeyDown (PauseAnimationKey)) 
		{
			IsAnimating = !IsAnimating;
			if (IsAnimating)
				ResetAnimation();
		}
		/*if (Input.GetKeyDown (KeyCode.I)) 
		{
			AddPositions();
		}*/
		FrameCount++;
		if (!IsFixedUpdate && FrameCount % 4 == 0) {
			UpdateAnimation ();
		}
	}
	int FrameCount = 0;
	void FixedUpdate() 
	{
		if (IsFixedUpdate)
			UpdateAnimation ();
	}

	void AddToBoneNames(string NewName) 
	{
		for (int i = 0; i < BoneNames.Count; i++) 
		{
			if (BoneNames[i] == NewName)
				return;
		}
		BoneNames.Add (NewName);
	}
	public GameObject FindChildBone(GameObject ParentBone, string BoneName) 
	{
		if (BoneName == "")
			return RootBone;
		for (int i = 0; i < ParentBone.transform.childCount; i++) {
			GameObject ChildBone = ParentBone.transform.GetChild(i).gameObject;
			if (ChildBone.name == BoneName)
				return ChildBone;
			GameObject ChildBone2 = FindChildBone(ChildBone, BoneName);
			if (ChildBone2 != null)
				return ChildBone2;
		}
		return null;
	}
	public void ResetAnimation() {
		InitialLocalPosition = transform.localPosition;
		AnimationTimeBegin = Time.time;
		IsAnimating = true;
	}
	GameObject GetBone(string BoneName) {
		return gameObject;
	}
	void AddPositions() {
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.x", gameObject, new AnimationCurve()));
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.y", gameObject, new AnimationCurve()));
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.z", gameObject, new AnimationCurve()));
	}
	float AnimationTime;

	public bool IsMask(string BoneName) {
		for (int i = 0; i < MyMasking.Count; i++) {
			if (BoneName == MyMasking[i])
				return true;
		}
		return false;
	}
	public void UpdateAnimation() 
	{
		if (IsAnimating) 
		{
			AnimationTime = Time.time-AnimationTimeBegin;
			for (int z = 0; z < BoneReferences.Count; z++)
			if (!IsMask(BoneReferences[z].name))
			{
				GameObject MyBone = BoneReferences[z];//GetBone (BoneNames [z]);
				Vector3 NewLocalPosition = MyBone.transform.localPosition - InitialLocalPosition;
				Vector3 NewScale = MyBone.transform.localScale;
				Quaternion NewRotation = MyBone.transform.rotation;
				//if (MyAnimation.isLooping)
				{
					//AnimationTime %= MyAnimation.length;
					//if (AnimationTime > MyAnimation.length)
				}
				for (int i = 0; i < MyBones.Count; i++)
				{
					if (BoneNames [z] == MyBones [i].ObjectPath) 
					{
						try {
							if (MyBones [i].PropertyName == "m_LocalPosition.x")
								NewLocalPosition.x = MyBones [i].MyCurve.Evaluate (AnimationTime);
							else if (MyBones [i].PropertyName == "m_LocalPosition.y")
								NewLocalPosition.y = MyBones [i].MyCurve.Evaluate (AnimationTime);
							else if (MyBones [i].PropertyName == "m_LocalPosition.z")
								NewLocalPosition.z = MyBones [i].MyCurve.Evaluate (AnimationTime);
							
							if (MyBones [i].PropertyName == "m_LocalScale.x")
								NewScale.x = MyBones [i].MyCurve.Evaluate (AnimationTime);
							else if (MyBones [i].PropertyName == "m_LocalScale.y")
								NewScale.y = MyBones [i].MyCurve.Evaluate (AnimationTime);
							else if (MyBones [i].PropertyName == "m_LocalScale.z")
								NewScale.z = MyBones [i].MyCurve.Evaluate (AnimationTime);
							
							if (MyBones [i].PropertyName == "m_LocalRotation.x") 
							{
								NewRotation.x = MyBones [i].MyCurve.Evaluate (AnimationTime);
							} 
							else if (MyBones [i].PropertyName == "m_LocalRotation.y") 
							{
								NewRotation.y = MyBones [i].MyCurve.Evaluate (AnimationTime);
							}
							else if (MyBones [i].PropertyName == "m_LocalRotation.z") 
							{
								NewRotation.z = MyBones [i].MyCurve.Evaluate (AnimationTime);
							} 
							else if (MyBones [i].PropertyName == "m_LocalRotation.w") 
							{
								NewRotation.w = MyBones [i].MyCurve.Evaluate (AnimationTime);
							}
						} catch (System.MissingMethodException e) {


						}
					}
				}
				if (NewLocalPosition != MyBone.transform.localPosition) 
				{
					//Debug.LogError(NewLocalPosition.ToString());
					MyBone.transform.localPosition = InitialLocalPosition + NewLocalPosition;
				}
				if (NewScale != MyBone.transform.localScale) 
				{
					//Debug.LogError(NewLocalPosition.ToString());
					MyBone.transform.localScale = NewScale;
				}
				if (NewRotation != MyBone.transform.rotation) 
				{
					//Debug.LogError (NewRotation.ToString ());
					MyBone.transform.localRotation = NewRotation;
					DebugRotation = NewRotation;
				}
			}
			AnimationTime = 0;
		}
	}
}
