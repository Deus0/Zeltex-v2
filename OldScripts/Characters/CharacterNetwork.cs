/*using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class CharacterNetwork : MonoBehaviour {
	private NetworkView MyNetworkView;
	//public float speed = 5f;
	//public float gravity = 5f;
	// used to disable character movement for other players
	private CharacterController MyCharacterController;
	// used to add the camera to the player upon loading
	private Camera MyCamera;
	public GameObject CameraPrefab;
	// 
	//public GameObject Bullet;
	private Vector3 ShootPosition;
	private Vector3 ShootDirection;
	public bool IsOffline = true;

	bool HasInitiated = false;

	public bool IsPlayer = true;
	// Use this for initialization
	void Start () {
		MyNetworkView = (NetworkView)GetComponent ("NetworkView");
		if (IsPlayer) {
			MyCharacterController = (CharacterController)GetComponent ("CharacterController");
			if (MyNetworkView.isMine) {
				if (!HasInitiated) {
					Vector3 NewCameraSpawnPosition = transform.position;
					GameObject MyCameraObject = (GameObject)Instantiate (CameraPrefab, NewCameraSpawnPosition, Quaternion.identity);
					MyCamera = (Camera)MyCameraObject.GetComponent ("Camera");
					MyCamera.gameObject.transform.parent = gameObject.transform;
					MyCamera.gameObject.transform.localPosition = new Vector3 (MyCamera.gameObject.transform.localPosition.x, 
			                                                          MyCamera.gameObject.transform.localPosition.y + 1.25f,
			                                                          MyCamera.gameObject.transform.localPosition.z);
					//FirstPersonController MyFps = (FirstPersonController)GetComponent ("FirstPersonController");
					HasInitiated = true; 
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!IsOffline) {
			if (MyNetworkView.isMine) {
				if (IsPlayer) {
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					ShootDirection = ray.direction;
					ShootPosition = ray.origin;
					//ShootDirection = Camera.main.gameObject.transform.forward;	// transform.forward; // 
					//ShootPosition = Camera.main.gameObject.transform.position;	//transform.position + new Vector3(0,0.25f,0); // 
				} else {
					//ShootDirection = gameObject.GetComponent<BaseCharacter>().ShootForwardVector;
					ShootDirection = transform.forward;
					//ShootPosition = gameObject.GetComponent<BaseCharacter>().ShootPosition;
					ShootPosition = transform.position;
				}
				//Screen.lockCursor = true;
				//Cursor.visible = false;
				//MyCharacterController.Move (new Vector3 (Input.GetAxis ("Horizontal") * speed * Time.deltaTime, -gravity * Time.deltaTime,
				//                                    Input.GetAxis ("Vertical") * speed * Time.deltaTime));
				//if (Input.GetKeyDown (KeyCode.Space)) {
				//	MyCharacterController.Move (new Vector3 (0, 100f*gravity * Time.deltaTime,    0));
				//}
				//if (Input.GetMouseButtonDown(0)) {
				//SpawnBullet ();
				//}
			} else {
				if (IsPlayer) {
					//GetComponentInChildren<AudioListener>().enabled = false; // Disables AudioListener of non-owned Player - prevents multiple AudioListeners from being present in scene.
					GetComponentInChildren<CharacterController> ().enabled = false; // Disables AudioListener of non-owned Player - prevents multiple AudioListeners from being present in scene.
					GetComponentInChildren<FirstPersonController> ().enabled = false; // Disables AudioListener of non-owned Player - prevents multiple AudioListeners from being present in scene.
				}
				//enabled = false;
			}
		}
	}
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		float ShootX = 0;
		float ShootY = 0;
		float ShootZ = 0;
		float ShootPosX = 0;
		float ShootPosY = 0;
		float ShootPosZ = 0;
		if (stream.isWriting) {
			// Sending
			//if (MyNetworkView.isMine) {
				ShootX = ShootDirection.x;
				ShootY = ShootDirection.y;
				ShootZ = ShootDirection.z;
				stream.Serialize (ref ShootX);
				stream.Serialize (ref ShootY);
			stream.Serialize (ref ShootZ);
			ShootPosX = ShootPosition.x;
			ShootPosY = ShootPosition.y;
			ShootPosZ = ShootPosition.z;
			stream.Serialize (ref ShootPosY);
			stream.Serialize (ref ShootPosY);
			stream.Serialize (ref ShootPosZ);
			//}
		} else {
			// revieving
			//if (!MyNetworkView.isMine) {
				stream.Serialize (ref ShootX);
				stream.Serialize (ref ShootY);
				stream.Serialize (ref ShootZ);
				ShootDirection = new Vector3(ShootX, ShootY, ShootZ);
				stream.Serialize (ref ShootPosY);
				stream.Serialize (ref ShootPosY);
				stream.Serialize (ref ShootPosZ);
				ShootPosition = new Vector3(ShootPosY, ShootPosY, ShootPosZ);
			//}
		}
	}
	//void OnNetworkInstantiate() {

	//}
	//void SpawnBullet() {
		// replace all spawning with this line if a client or server
		//	GameObject MyBullet = (GameObject) Network.Instantiate (Bullet, ShootPosition+ShootDirection*(0.5f), Camera.main.transform.rotation, 0);
		//Quaternion ShootDirectionQuat = Quaternion.Euler (ShootDirection.x,ShootDirection.y, ShootDirection.z);
		//AddForceToBullet(MyBullet);
		//Debug.Log ("Spawning bullet: " + ShootDirection.ToString ());
		// assuming this script does not run on the server - only on client :D allas i solved it


	//	Debug.Log ("Giving force to bullet: " + transform.forward.ToString ());
	//}

}
*/