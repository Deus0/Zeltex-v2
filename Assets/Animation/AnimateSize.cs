using UnityEngine;
using System.Collections;

// first expand, then decrease

// seperate into explosion and animate size
// use events for the animate part
// and animation curves to control size

public class AnimateSize : MonoBehaviour {
	public bool IsDestroyOnFinish = true;
	public bool IsDecrease = true;
	public bool IsIncreasing = true;
	public Vector3 BeginSize = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 MaxSize = new Vector3 (1, 1, 1);
	Vector3 NothingSize = new Vector3 (0, 0, 0);

	public float TimeStartedIncreasing;
	public float TimeStartedDecreasing;

	public float TimeToIncrease = 0.7f;
	public float TimeToDecrease = 0.45f;
	public float PauseTime = 0.1f;

	public VoxelEngine.World DestroyingWorld;

	// Use this for initialization
	void Start () 
	{
		TimeStartedIncreasing = Time.time;
		TimeStartedDecreasing = Time.time+TimeToIncrease+PauseTime;
	}

	public void SetWorld(GameObject MyWorld) 
	{
		DestroyingWorld = MyWorld.GetComponent<VoxelEngine.World> ();
	}

	public void OnMaxSize()
	{
		if (DestroyingWorld) 
		{
			//	Debug.LogError("Hit terrain with: " + collision.contacts[0].normal.ToString() + ":vel:" + gameObject.GetComponent<Rigidbody>().velocity.ToString()
			//               + ":velnormal:" + VelNormal.ToString());
		
			DestroyingWorld.UpdateBlockTypeSize (DestroyingWorld.transform.InverseTransformPoint (transform.position),
		                                       // +VelNormal),
		                                       //-collision.contacts[0].normal*MyChunk.transform.lossyScale.x/10f), 
		                                       0, 
			                                     (MaxSize.x / 2f)/DestroyingWorld.transform.localScale.x);	// world point
		}
	}
		// Update is called once per frame
	void Update ()
	{
		if (Time.time - TimeStartedIncreasing < TimeToIncrease+PauseTime)
		{
			if (Time.time-TimeStartedIncreasing <= TimeToIncrease) {
				transform.localScale = Vector3.Lerp (BeginSize, MaxSize, (Time.time - TimeStartedIncreasing)/TimeToIncrease);
			}
			else {
				if (IsIncreasing) {
					IsIncreasing = false;
					OnMaxSize();
				} 
				// else just wait til pause is done
			}
		} else {
			if (!IsDecrease) {
				if (IsDestroyOnFinish)
					Destroy (gameObject);
				else
					this.enabled = false;
			} else {
				transform.localScale = Vector3.Lerp (MaxSize, NothingSize, (Time.time - TimeStartedDecreasing)/TimeToDecrease);
				if (Time.time - TimeStartedDecreasing >= TimeToDecrease) {
					if (IsDestroyOnFinish)
						Destroy (gameObject);
					else
						this.enabled = false;
				}
			}
		}
		//Debug.DrawLine (Vector3.zero, new Vector3 (1, 0, 0), Color.red);
	}
	
	// Will be called after all regular rendering is done
	//public void OnRenderObject ()
	//{
	//	DebugShapes.DrawCube (transform.position, new Vector3 (0.5f, 0.5f, 0.5f), Color.black);
	//}
}
