using UnityEngine;
using System.Collections;

// first expand, then decrease

public class AnimateSize : MonoBehaviour {
	public bool IsDestroyOnFinish = true;
	public bool IsDecrease = true;
	public Vector3 BeginSize = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 MaxSize = new Vector3 (1, 1, 1);
	Vector3 NothingSize = new Vector3 (0, 0, 0);

	public float TimeStartedIncreasing;
	public float TimeStartedDecreasing;

	public float TimeToIncrease = 0.7f;
	public float TimeToDecrease = 0.45f;
	public float PauseTime = 0.1f;

	// Use this for initialization
	void Start () {
		TimeStartedIncreasing = Time.time;
		TimeStartedDecreasing = Time.time+TimeToIncrease+PauseTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (!(Time.time - TimeStartedIncreasing >= TimeToIncrease+PauseTime)) {
			transform.localScale = Vector3.Lerp (BeginSize, MaxSize, (Time.time - TimeStartedIncreasing)/TimeToIncrease);
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
