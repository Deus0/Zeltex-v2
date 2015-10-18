using UnityEngine;
using System.Collections;

// need to open the door differently depending on what side im on

public class Door : MonoBehaviour {
	public bool IsLocked = false;
	public bool IsOpeningDoor = false;
	public bool IsClosingDoor = false;
	public float Speed = 75;
	public Vector3 RotateAngle;
	private float Direction;
	//private float NewAngleY;
	private Vector3 TargetAngle;
	private Vector3 OldAngle = new Vector3(0,0,0);
	public Vector3 RotationPoint = new Vector3(0.5f,0,0);
	public Vector3 RotationAxis = new Vector3(0,1,0);
	private int LastState = 1;
	public Vector3 BeginAngle = new Vector3(0,0,0);
	public Vector3 EndAngle= new Vector3(0,90,0);
	public AudioSource SoundSource;	// source of where the sound is emmited from - getting hit, casting spell, dying etc
	public float SoundVolume = 1f;
	public AudioClip OpeningSound;
	public AudioClip ClosingSound;
	public float BeginDirection = -1;
	public float EndDirection = 1;
	private Vector3 Pivot;
	private Vector3 OriginalPosition;
	public bool IsForeverRotate = false;

	void Start() {
		SoundSource = GetComponent<AudioSource>();
		BeginAngle = transform.rotation.eulerAngles;
		OriginalPosition = transform.position;
		Pivot = RotationPoint + OriginalPosition;
	}

	// Update is called once per frame
	void Update () {
		if ((IsOpeningDoor || IsClosingDoor)) {		
			AnimateRotation();
		}
	}
	public void AnimateRotation() {
		Pivot = RotationPoint + OriginalPosition;
		if (IsClosingDoor) {
			TargetAngle = BeginAngle;
			OldAngle = TargetAngle;
			Direction = BeginDirection;
		} else if (IsOpeningDoor) {
			TargetAngle = EndAngle;
			OldAngle = BeginAngle;
			Direction = EndDirection;
		}

		// Rotate towards where we want to be - depending on time passed and direction going
		transform.RotateAround (Pivot, RotationAxis, Time.deltaTime * Speed*Direction);
		// our new angle we want to keep
		Vector3 CurrentAngle = transform.rotation.eulerAngles;
		// keep only the angles we are using
		// set limits for angle
		if (!(CurrentAngle.x  >= BeginAngle.x && CurrentAngle.x <= EndAngle.x 
		      && CurrentAngle.y  >= BeginAngle.y && CurrentAngle.y <= EndAngle.y
		      && CurrentAngle.z  >= BeginAngle.z && CurrentAngle.z <= EndAngle.z
		      )) {
			if (!IsForeverRotate) {
			// Once its passed the target angle, rotate it back the other way
				transform.RotateAround (Pivot, RotationAxis, TargetAngle.x - CurrentAngle.x);
				transform.RotateAround (Pivot, RotationAxis, TargetAngle.y - CurrentAngle.y);
				transform.RotateAround (Pivot, RotationAxis, TargetAngle.z - CurrentAngle.z);
				IsOpeningDoor = false;
				IsClosingDoor = false;
			}
		}
		//transform.position += MyPosition;
	}
	public void ToggleDoor() {
		if (!IsLocked) {
			Debug.Log ("Toggling Door");
			if (IsOpeningDoor || LastState == 1) {
				CloseDoor ();
			} else if (IsClosingDoor || LastState == 2) {
				OpenDoor ();
			}
		} else {
			CloseDoor ();
		}
	}

	public void OpenDoor() {
		if (SoundSource  != null && OpeningSound != null)
			SoundSource.PlayOneShot (OpeningSound, SoundVolume);
		Debug.Log ("Opening Door");
		//IsAnimating = true;
		IsClosingDoor = false;
		IsOpeningDoor = true;
		LastState = 1;
	}
	public void CloseDoor() {
		if (SoundSource  != null && ClosingSound != null)
			SoundSource.PlayOneShot (ClosingSound, SoundVolume);
		Debug.Log ("Closing Door");
		//IsAnimating = true;
		IsClosingDoor = true;
		IsOpeningDoor = false;
		LastState = 2;
	}
}
