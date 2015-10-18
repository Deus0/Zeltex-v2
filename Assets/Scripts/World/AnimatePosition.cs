using UnityEngine;
using System.Collections;

public class AnimatePosition : MonoBehaviour {
	Vector3 BeginPosition;
	Vector3 EndPosition;
	public Vector3 IncreasePosition;
	bool IsAnimating = true;
	float Direction = 1;
	float LastTime = 0;
	public float Speed = 20;
	public float WaitTime = 10;
	float LastWait = 0;
	bool IsPaused;

	// Use this for initialization
	void Start () {
		BeginPosition = transform.position;
		EndPosition = BeginPosition + IncreasePosition;
	}
	void Awake() {
		LastTime = Time.time;
	}
	// Update is called once per frame
	void Update () {
		if (IsAnimating) {
			if (IsPaused) {
				if (Time.time - LastTime > WaitTime) {
					UnPauseAnimation();
				}
			} else {
				Vector3 NewPosition = transform.position;
				if (Direction == 1)
					NewPosition = EndPosition;
				else if (Direction == -1)
					NewPosition = BeginPosition;
				transform.position = Vector3.Lerp (transform.position, NewPosition, (Time.time - LastTime) * Speed);
				if (transform.position == EndPosition && Direction == 1) {
					Direction = -1;
					PauseAnimation();
				}
				if (transform.position == BeginPosition && Direction == -1) {
					Direction = 1;
					PauseAnimation();
				}
			}
		}
	}
	public void PauseAnimation() {
		LastWait = Time.time;
		IsPaused = true;
	}
	public void UnPauseAnimation() {
		LastTime = Time.time;
		IsPaused = false;
	}
}
