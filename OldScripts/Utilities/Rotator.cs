using UnityEngine;
using System.Collections;

// this class is used to rotate things over time
// perfect for item drops and powerups

public class Rotator : MonoBehaviour {
	public bool IsAddTorque = false;
	public float thetaX = 0;
	public float thetaY = 0;
	public float thetaZ = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Rigidbody MyRigidBody = gameObject.GetComponent<Rigidbody> ();
		if (!IsAddTorque) {
			Quaternion OldRotation = transform.rotation;
			Quaternion NewRotation = OldRotation;
			NewRotation.eulerAngles += (new Vector3 (thetaX, thetaY, thetaZ));
			Quaternion AwesomeRotation = Quaternion.Slerp (OldRotation, NewRotation, Time.time);
			transform.rotation = (AwesomeRotation);
		} else {
			if (MyRigidBody == null)
				MyRigidBody.AddTorque(new Vector3(thetaX,thetaY, thetaZ));
		}
	}
}
