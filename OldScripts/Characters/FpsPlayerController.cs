using UnityEngine;
using System.Collections;
using UnityEngine.UI;
// unity 3d student .com

public class FpsPlayerController : MonoBehaviour {
	public float speed;
	public Text countText;
	public Text winText;
	private int count;
	public GameObject Bullet;
	public Vector3 BulletOffset;
	
	Vector3 previousMousePosition;

	public Vector3 MaxAcc;
	public Vector3 MaxVel;
	public Vector3 Acc;
	public Vector3 Vel;
	public Vector3 Loc;	

	// Use this for initialization
	void Start () {
		float TestReduction = 1;
		float friction = 0.8f;
		for (int i = 0; i < 20; i++) {
			Debug.Log ("Test " + i + " : " + TestReduction);
			TestReduction *= friction;
		}
		count = 0;
		UpdateCountText ();
		winText.text = "";
		GameObject go = new GameObject("GUIText " + 0);
		Loc = transform.localPosition;
	}
	// Update is called once per frame
	void Update () {
		//bool IsShoot = Input.GetKeyDown ("1");
		bool IsShoot = Input.GetMouseButtonDown(1);
		if (IsShoot) {
				Instantiate (Bullet, transform.position + BulletOffset, Quaternion.identity);
		}
	}
	void FixedUpdate() {
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		Vector3 mouse = Input.mousePosition;
		
		Vector3 movement = new Vector3 (moveHorizontal, 0, moveVertical);
		Vector3 newForce =(Time.deltaTime * speed * movement);
		if (moveHorizontal == 0) {
			Vel.x *= 0.75f;
		}
		if (moveVertical == 0) {
			Vel.z *= 0.75f;
		}
		Acc += newForce;
		if (Acc.x > MaxAcc.x)
			Acc.x = MaxAcc.x;
		if (Acc.x < -MaxAcc.x)
			Acc.x = -MaxAcc.x;
		if (Acc.y > MaxAcc.y)
			Acc.y = MaxAcc.y;
		if (Acc.y < -MaxAcc.y)
			Acc.y = -MaxAcc.y;
		if (Acc.z > MaxAcc.z)
			Acc.z = MaxAcc.z;
		if (Acc.z < -MaxAcc.z)
			Acc.z = -MaxAcc.z;


		Vel += Acc;
		if (Vel.x > MaxVel.x)
			Vel.x = MaxVel.x;
		if (Vel.x < -MaxVel.x)
			Vel.x = -MaxVel.x;
		if (Vel.y > MaxVel.y)
			Vel.y = MaxVel.y;
		if (Vel.y < -MaxVel.y)
			Vel.y = -MaxVel.y;
		if (Vel.z > MaxVel.z)
			Vel.z = MaxVel.z;
		if (Vel.z < -MaxVel.z)
			Vel.z = -MaxVel.z;
		float MinimumVel = 0.005f;
		if (Vel.x > -MinimumVel && Vel.x < MinimumVel)
			Vel.x = 0;
		if (Vel.y > -MinimumVel && Vel.y < MinimumVel)
			Vel.y = 0;
		if (Vel.z > -MinimumVel && Vel.z < MinimumVel)
			Vel.z = 0;
		Loc += Vel;	

		transform.position = Loc;

		//rigidbody.AddForce (newForce);	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive (false);
			count++;
			UpdateCountText ();
		}
	}
	void UpdateCountText() {
		countText.text = "Count: " + count.ToString ();
		if (count>= 12)
			winText.text = "U r Wiinnar";
	}
}
