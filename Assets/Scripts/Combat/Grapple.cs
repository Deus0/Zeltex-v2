using UnityEngine;
using System.Collections;

public class Grapple : MonoBehaviour {
	public GameObject GrappleBeamPrefab;
	public Vector3 HandPosition;
	private bool IsGrappling = false;
	public float MovementForce = 10;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.H))
			CreateGrappleBeam ();
		if (Input.GetKeyDown (KeyCode.Space))
			ReleaseGrapple ();
		if (Input.GetKeyDown (KeyCode.W))
			gameObject.GetComponent<Rigidbody> ().AddForce (MovementForce*transform.forward);
		if (Input.GetKeyDown (KeyCode.S))
			gameObject.GetComponent<Rigidbody> ().AddForce (-MovementForce*transform.forward);
	}
	// idea is when user hits the key
	// a grapple starts being created inside the users hand
	// When it reaches the end of his hand, it creates another link in the chain
	// it creates as much links needed between it and the wall, so stop when the chain reaches the wall
	// it only applies physics to the 'hook' part of the chain
	// the rest are linked
	public bool IsHoldingGrapple = false;	// if true the player is pulled alongwith the hook shot
	public void CreateGrappleBeam() {
		if (!IsGrappling) {
			HandPosition = transform.position;	// default for now
			RaycastHit hit;
			Player MyPlayer = gameObject.GetComponent<Player> ();
			if (Physics.Raycast (transform.position, MyPlayer.ShootForwardVector, out hit, 100)) {
				HandPosition = hit.point;
				Quaternion NewRotation = new Quaternion ();
				//NewRotation.eulerAngles = (hit.point - gameObject.transform.position);
				NewRotation.eulerAngles =  MyPlayer.ShootForwardVector;
				NewRotation = Quaternion.identity;
				//Debug.LogError ("Angle of Grapple: " + NewRotation.eulerAngles.ToString());
				GameObject MyChain = (GameObject)Instantiate (GrappleBeamPrefab, HandPosition, NewRotation);
				//NewRotation = MyChain.transform.rotation;
				MyChain.transform.LookAt(hit.point);
				//Debug.LogError ("Angle of Grapple: " + MyChain.transform.rotation.eulerAngles.ToString());
				if (IsHoldingGrapple) {
					//MyChain.transform.rotation = NewRotation;
					transform.position = MyChain.transform.GetChild (7).transform.position;
					Vector3 CharacterBodyOffset = new Vector3(0,-1.5f,0);
					transform.position = transform.position+CharacterBodyOffset;
					gameObject.AddComponent<FixedJoint> ();
					gameObject.GetComponent<FixedJoint> ().connectedBody = MyChain.transform.GetChild (7).GetComponent<Rigidbody> ();
					gameObject.GetComponent<CharacterController> ().enabled = false;
					IsGrappling = true;
					gameObject.GetComponent<Rigidbody>().isKinematic = false;
				}
			}
		}
	}
	public void ReleaseGrapple() {
		if (IsGrappling) {
			IsGrappling = false;
			if (IsHoldingGrapple) {
				Destroy (gameObject.GetComponent<FixedJoint> ());
				gameObject.GetComponent<CharacterController> ().enabled = true;
				Quaternion MyQuat = gameObject.transform.rotation;
				MyQuat.eulerAngles = new Vector3(0,gameObject.transform.rotation.eulerAngles.y,0);
				gameObject.transform.rotation = MyQuat;
				gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
	}
}
