using UnityEngine;
using System.Collections;

// need to open the door differently depending on what side im on

// Rotation point should be a setting - Centre, Left, Right - and should get the actual value by calculating the objects boundary box

public class Door : MonoBehaviour {
	[Header("Events")]
	public UnityEngine.Events.UnityEvent OnOpenDoor = null;
	public UnityEngine.Events.UnityEvent OnClickLockedDoor = null;
	[Header("Options")]
	[SerializeField] private bool IsLocked = false;
	[SerializeField] private float TimeToAnimate = 1.5f;
	[Tooltip("The amount of euler angles to rotate")]
	public Vector3 RotateAngle;
	public Vector3 RotationPoint = new Vector3(0.5f,0,0);
	public bool IsForeverRotate = false;
	[Header("Audio")]
	public float SoundVolume = 1f;
	public AudioSource SoundSource;	// source of where the sound is emmited from - getting hit, casting spell, dying etc
	public AudioClip OpeningSound;
	public AudioClip ClosingSound;

	// states
	private bool IsOpeningDoor = false;
	private bool IsClosingDoor = false;
	private int LastState = 1;					// used to toggle door
	private Vector3 BeginAngle = new Vector3(0,0,0);
	private Vector3 EndAngle = new Vector3(0,90,0);
	private float Direction = -1;
	private float BeginDirection = -1;
	private float EndDirection = 1;
	private Vector3 Pivot;
	private Vector3 BeginPosition;
	private Vector3 EndPosition;

	public bool IsAnimatePosition = false;
	public Vector3 NewPosition;

	void Start() {
		SoundSource = GetComponent<AudioSource>();
		BeginAngle = transform.rotation.eulerAngles;
		BeginPosition = transform.position;
		Pivot = (RotationPoint) + BeginPosition;
		RotationPoint = transform.InverseTransformDirection (RotationPoint);
		if (EndAngle.x > 0 || RotateAngle.y > 0 || RotateAngle.z > 0) {
			BeginDirection = -1; EndDirection = 1;
		} else {
			BeginDirection = 1; EndDirection = -1;
		}
		transform.RotateAround (Pivot, 
		                        Vector3.up*BeginDirection,
		                        RotateAngle.x);
		EndPosition = transform.position;
		EndAngle = transform.rotation.eulerAngles;
		RestoreBeginState ();

		if (IsAnimatePosition) 
		{
			EndPosition = BeginPosition + NewPosition;
			EndAngle = BeginAngle;
		}
	}
	private void RestoreBeginState() 
	{
		transform.position = BeginPosition;
		transform.eulerAngles = BeginAngle;
	}
	private void RestoreEndState() {
		transform.position = EndPosition;
		transform.eulerAngles = EndAngle;
	}
	// Update is called once per frame
	void Update () {
		if ((IsOpeningDoor || IsClosingDoor)) {		
			Animate();
		}
	}

	private void Animate() 
	{
		float TimeSinceBegun = Time.time - TimeBegun;
		if (TimeSinceBegun >= TimeToAnimate) {
			StopAnimation();
		} else {
			float AnimationLerpTime = (Time.deltaTime/TimeToAnimate);
			if (!IsAnimatePosition) {
				float AngleAdditionX = (RotateAngle.x)*AnimationLerpTime;
				transform.RotateAround (Pivot, 
				                        Vector3.up*Direction,
				                        AngleAdditionX);
			} else {
				if (IsOpeningDoor)
					transform.position = Vector3.Lerp(EndPosition, BeginPosition, TimeSinceBegun/TimeToAnimate);
					else if (IsClosingDoor)
						transform.position = Vector3.Lerp(BeginPosition, EndPosition, TimeSinceBegun/TimeToAnimate);
			}
		}
	}

	private void StopAnimation() {
		if (IsOpeningDoor || IsClosingDoor) {
			//set angle to perfectness
			// which way do i have to go to rotate the angle perfectly to our target
			if (IsOpeningDoor) {
				RestoreBeginState();
			} else if (IsClosingDoor) {
				RestoreEndState();
			}
			IsOpeningDoor = false;
			IsClosingDoor = false;
		}
	}

	public void ToggleDoor() {
		if (!IsLocked) {
			Debug.Log ("Toggling Door");
			if (IsOpeningDoor || LastState == 1) {
				CloseDoor ();
			} else if (IsClosingDoor || LastState == 2) {
				OpenDoor ();
				if (OnOpenDoor != null)
					OnOpenDoor.Invoke();
			}
		} else {
			//CloseDoor ();
			//if (OnClickLockedDoor != null)
			//	OnClickLockedDoor.Invoke();
		}
	}
	public void Lock() {
		IsLocked = true;
	}
	public void Unlock() {
		IsLocked = false;
	}
	private float TimeBegun;
	private bool IsAnimating() {
		if (IsClosingDoor || IsOpeningDoor)
			return true;
		else
			return false;
	}
	public void OpenDoor() 
	{
		if (!IsAnimating ()) {
			Debug.Log ("Opening Door");
			TimeBegun = Time.time;
			IsClosingDoor = false;
			IsOpeningDoor = true;
			LastState = 1;
			//TargetAngle = EndAngle;
			//OldAngle = BeginAngle;
			Direction = EndDirection;
			if (SoundSource != null && OpeningSound != null)
				SoundSource.PlayOneShot (OpeningSound, SoundVolume);
		}
	}

	public void CloseDoor() {
		if (!IsAnimating ()) {
			Debug.Log ("Closing Door");
			TimeBegun = Time.time;
			IsClosingDoor = true;
			IsOpeningDoor = false;
			LastState = 2;
			//TargetAngle = BeginAngle;
			//OldAngle = TargetAngle;
			Direction = BeginDirection;
			
			if (SoundSource  != null && ClosingSound != null)
				SoundSource.PlayOneShot (ClosingSound, SoundVolume);
		}
	}
}
