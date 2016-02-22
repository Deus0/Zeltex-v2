using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerSpawner : MonoBehaviour 
{
	private KeyCode MyScoreKey = KeyCode.Tab;
	public List<string> PlayerNames;
	public bool IsDebugMode = false;
	private GameObject MyPlayerSpawn;

	[Header("Options")]
	[Tooltip("The name of the player prefab, located in [Assets/Resources/] folder.")]
	public string PrefabName;
	[Tooltip("Link to main camera")]
	public Camera MyCamera;
	[Tooltip("The offset of the camera from the player.")]
	public Vector3 CameraOffset = new Vector3(0, 0.47f, 0.06f);
	[Tooltip("How long it takes the player to respawn.")]
	public float RespawnTime = 10f;
	[Header("Events")]
	public UnityEvent OnSpawnPlayer;
	[Header("Audio")]
	private AudioSource MyAudio;
	public AudioClip OnSpawnSound;
	public AudioClip OnTickSound;
	public UnityEngine.UI.Text MyText;
	public float MaxVolume = 5f;
	public GUISkin MySkin;
	private string DefaultPlayerName = "Player(Clone)";	// used to rename players when a new player connects

	void Start() 
	{
		MyAudio = gameObject.GetComponent<AudioSource> ();
		if (MyAudio == null)
			MyAudio = gameObject.AddComponent<AudioSource> ();
		if (MyText)
			MyText.gameObject.SetActive (false);
	}

	void OnGUI() 
	{
		if (IsDebugMode) 
		{
			GUILayout.Label("");
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
			GUILayout.Label("HasSpawnedPlayer " + HasSpawnedPlayer());
			if (MyPlayerSpawn) 
				GUILayout.Label("MyPlayerSpawn " + MyPlayerSpawn.name + " : " + MyPlayerSpawn.transform.position.ToString());
			
			if (Input.GetKey(MyScoreKey)) 
			{
				GameObject PlayerPrefab = Resources.Load(PrefabName + ".prefab", typeof(GameObject)) as GameObject;
				GUILayout.Label(PrefabName + " Loaded: " + (PlayerPrefab == null));
			}
		} 
		// show all players and their scores
		if (PhotonNetwork.connected && !PhotonNetwork.insideLobby && Input.GetKey(KeyCode.Tab))
		{
			GUI.skin = MySkin;
			//GUILayout.Label("Players [" + PhotonNetwork.playerList.Length + "]");
			
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++) {
				if (PhotonNetwork.playerList[i].isLocal)
					DebugPlayer(PhotonNetwork.playerList[i]);
			}
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++) {
				if (!PhotonNetwork.playerList[i].isLocal)
					DebugPlayer(PhotonNetwork.playerList[i]);
			}
		}
	}

	public void Clear() {
		ToggleDeathCam (true);
		Destroy (MyPlayerSpawn);
	}
	// for debugging
	void Update()
	{
		if (PhotonNetwork.connectionStateDetailed == PeerState.Joined 
		    && !HasSpawnedPlayer()) {
			//CreatePlayerObject (true);
		}
		else // destroy player if not connected lol
		{

		}
	}

	/*void Update() 
	{
		if (Input.GetKeyDown (KeyCode.B)) {
			CreatePlayerObject ();
		}
	}*/
	// when client joins a room
	void OnJoinedRoom()
	{
		if (enabled) 
		{
			Debug.LogError ("Joined the room " + PhotonNetwork.room.name+ ": " + PhotonNetwork.playerName + ":" + PhotonNetwork.room.masterClientId);
			StartCoroutine (UpdateAllCharacters ());
			//CreatePlayerObject (true);
			//OnSpawnPlayer.Invoke ();
		}
	}
	void OnCreatedRoom()
	{
		Debug.LogError ("Joined the room: " + PhotonNetwork.playerName + ":" + PhotonNetwork.room.name);
		//OnJoinedRoom ();
	}

	private void DebugPlayer(PhotonPlayer MyPlayer)
	{
		GUILayout.Label("[" + MyPlayer.ID + "]:[" 
		                + MyPlayer.name 
		                + "]\tScore [" + MyPlayer.GetScore()+"]");
	}
	public void RevivePlayer() {
		StartCoroutine (RespawnCharacter2("Character", RespawnTime));
	}
	public void RevivePlayer2(float RespawnTime) {
		StartCoroutine (RespawnCharacter2("Character", RespawnTime));
	}
	public void RevivePlayer3(string MyCharacter, float RespawnTime) 
	{
		StartCoroutine (RespawnCharacter2(MyCharacter, RespawnTime));
	}
	private IEnumerator RespawnCharacter2(string MyCharacter, float RespawnTime) 
	{
		//UnityEngine.UI.Text MyText = transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
		if (MyText)
			MyText.gameObject.SetActive (true);
		for (float i = RespawnTime; i >= 0; i--) 
		{
			if (OnTickSound)
				MyAudio.PlayOneShot(OnTickSound, MaxVolume-MaxVolume*(i/RespawnTime));
			if (MyText)
				MyText.text = "[" + (i) + "]";
			yield return new WaitForSeconds(1);
		}
		MyAudio.Stop ();
		if (MyText)
			MyText.gameObject.SetActive (false);
		SpawnPlayer ();
	}

	void OnPhotonPlayerConnected(PhotonPlayer newPlayer) 
	{
		Debug.LogError ("Player has connected - " + newPlayer.name);
		if (!newPlayer.isLocal) 
		{
			//DoTheThing(MyPlayerSpawn, MyPlayerSpawn.transform.GetChild(0).name);	// updates locally but need it to update the new connected player
		}
	}

	// PhotonNetwork.AllocateViewID 
	// gameObject.GetComponent<PhotonView>().ownerId

	private void PlayerSpawnSound() 
	{
		if (MyAudio) 
		{
			if (OnSpawnSound) 
			{
				MyAudio.PlayOneShot(OnSpawnSound);
			}
		}
	}
	
	public bool HasSpawnedPlayer() 
	{
		return (MyPlayerSpawn != null);
	}

	// converts a generic character into a player - using various input classes
	public void UpdateSpawnedPlayer(Transform NewPlayer) 
	{
		if (NewPlayer == null)
			return;
		MyPlayerSpawn = NewPlayer.gameObject;
		CharacterSystem.CharacterPrefabLoader.ConvertToPlayer (MyPlayerSpawn);
		//CharacterSystem.CharacterPrefabLoader.AddGuis (MyPlayerSpawn, true);
		
		SetPlayerSettings ();
		ToggleDeathCam (false);
		PlayerSpawnSound ();
		
		// Networking, player name stuff
		gameObject.GetComponent<PhotonView>().RPC("UpdateCharacterNameNetwork",
		                                          PhotonTargets.All,
		                                          PhotonNetwork.player.name 
		                                          );
		UpdateCharacterName(MyPlayerSpawn.transform, PhotonNetwork.player.name);
	}

	public void SpawnPlayer()
	{
		Vector3 SpawnPosition = transform.position+ new Vector3 (	Random.Range (-transform.localScale.x/2f, transform.localScale.x/2f),
		                                     	Random.Range (-transform.localScale.y/2f, transform.localScale.y/2f),
		                                     	Random.Range (-transform.localScale.z/2f, transform.localScale.z/2f));
		MyPlayerSpawn = PhotonNetwork.Instantiate(PrefabName, 
		                                          SpawnPosition, 
		                                          transform.rotation,
		                                          0);

		if (MyPlayerSpawn == null)
			return;

		UpdateSpawnedPlayer (MyPlayerSpawn.transform);
	}

	[PunRPC]
	public void UpdateCharacterNameNetwork(string NewName) 
	{
		//Debug.LogError ("Trying to update: " + SpawnerName + " with " + NewName);

		// this is probaly a bad way to do it but whatevers.. lol
		// find the character, on each players game, with a default name, and 'initiate' it for use!
		GameObject MyCharacter = GameObject.Find (DefaultPlayerName);
		if (MyCharacter)	// if found
			UpdateCharacterName (MyCharacter.transform, NewName);
	}

	private void UpdateCharacterName(Transform MyCharacter,string NewName) 
	{
		if (MyCharacter)
			if (MyCharacter.name == DefaultPlayerName)
		{
			MyCharacter.name = NewName;
			if (MyCharacter.GetComponent<GuiSystem.GuiManager> ())
			{
				MyCharacter.GetComponent<GuiSystem.GuiManager> ().UpdateLabel ();
			}
			if (GetComponent<CharacterSystem.CharacterManager>())
			{
				GetComponent<CharacterSystem.CharacterManager> ().Add(MyCharacter);
			}
		} else {
			//Debug.LogError (SpawnerName + " not found!");
		}
	}

	// when level has been loaded i need to reload all the changed stuff
	IEnumerator UpdateAllCharacters() 
	{
		yield return new WaitForSeconds(0.1f);	// wait for the spawned map stuff to be instantiated
		CharacterSystem.Character[] MyCharacters = FindObjectsOfType (typeof(CharacterSystem.Character)) as CharacterSystem.Character[];
		//var MyCharacters = FindObjectsOfType(typeof(CharacterSystem.Character));
		//Debug.LogError (MyCharacters.Length + " Previous characters in scene.");
		for (int i = 0; i < MyCharacters.Length; i++) 
		{
			Transform MyCharacter = MyCharacters[i].transform;
			if (MyCharacter.gameObject.activeSelf) {
				if (MyCharacter.name == DefaultPlayerName) 
				{
					UpdateCharacterName (MyCharacter, 
					                     MyCharacter.GetComponent<PhotonView> ().owner.name);
				} 
				
			}
		}
		for (int i = MyCharacters.Length-1; i >= 0; i--) 
		{
			CharacterSystem.Character MyCharacter = MyCharacters [i] as CharacterSystem.Character;
			if (MyCharacter.gameObject.activeSelf) 
			{
				//GameObject MyCharacterSpawner = MyCharacter.transform.parent.gameObject;
				if (MyCharacter.name == "Minion") 
				{// if bot
					{
						//Debug.LogError("Renaming bot: " + MyCharacterSpawner.name);
						gameObject.GetComponent<WorldUtilities.ZoneSpawner>().UpdateMinionName(MyCharacter.gameObject);
					}
				}
			}
		}
	}
	
	public void ToggleDeathCam(bool IsEnabled) 
	{
		Transform MainCamera = Camera.main.transform;
		NewMovement.MouseLook MyMouseLook = MainCamera.GetComponent<NewMovement.MouseLook>();
		MainCamera.GetComponent<CameraMovement> ().enabled = IsEnabled;
		if (MyMouseLook)
			MyMouseLook.enabled = !IsEnabled;
		if (IsEnabled)	// is free roam cam
		{
			Camera.main.GetComponent<CharacterSystem.MouseLocker>().SetController(null);
			MyCamera.transform.SetParent (null, false);
		} 
		else 
		{
			Transform MyCameraBone = MyPlayerSpawn.transform.FindChild ("CameraBone");
			MyCamera.transform.position = MyPlayerSpawn.transform.position + MyPlayerSpawn.transform.TransformDirection (CameraOffset);
			MyCamera.transform.rotation = MyPlayerSpawn.transform.rotation;
			MyCamera.transform.SetParent (MyCameraBone);
			Camera.main.GetComponent<CharacterSystem.MouseLocker>().SetController(MyPlayerSpawn.transform);
			MyPlayerSpawn.GetComponent<TestMovement>().enabled = true;
			if (MyMouseLook)
			{
				MyMouseLook.UpdateBody(MyPlayerSpawn.transform);
				MyMouseLook.UpdateCamera(MyCameraBone);
			}
		}
	}
	// client side, player specific things for input
	private void SetPlayerSettings() 
	{
		MyPlayerSpawn.SetActive (true);
		
		Transform MyCameraBone = MyPlayerSpawn.transform.FindChild ("CameraBone");
		Transform MyLabelTransform = MyPlayerSpawn.transform.FindChild("Label");
		GuiSystem.GuiManager MyGuiManager = MyPlayerSpawn.GetComponent<GuiSystem.GuiManager> ();
		CharacterSystem.CharacterStats MyStats = MyPlayerSpawn.GetComponent<CharacterSystem.CharacterStats> ();
		CharacterSystem.MyController PlayerController = MyPlayerSpawn.GetComponent<CharacterSystem.MyController> ();
		if (PlayerController) {
			PlayerController.enabled = true;
			PlayerController.UpdateCamera (MyCameraBone);
		}
		//NewMovement.MouseLook MyMouseLook = MyPlayerSpawn.GetComponent<>();

		if (MyStats)
		{
			//MyPlayerSpawn.transform.GetChild (0).GetComponent<GuiSystem.GuiManager> ().UpdateLabel ();
			MyStats.OnDeath.AddListener(delegate{
				RevivePlayer();
			});
		}

		// player label positioning is different then the norm
		if (MyLabelTransform) 
		{
			MyLabelTransform.gameObject.GetComponent<GUI3D.Orbitor> ().enabled = false;
			MyLabelTransform.GetComponent<GUI3D.Follower> ().enabled = false;
			MyLabelTransform.GetComponent<GUI3D.Billboard> ().enabled = false;
			MyLabelTransform.GetComponent<GUI3D.Orbitor> ().enabled = false;
			MyLabelTransform.SetParent(MyCameraBone, false);
			MyLabelTransform.transform.localScale = new Vector3 (0.0003f, 0.0003f, 0.0003f);
			MyLabelTransform.localPosition = new Vector3(0, 0.185f, 0.2f);
			MyLabelTransform.localRotation = Quaternion.identity;
		}
		
		for (int i = 1; i < MyPlayerSpawn.transform.childCount; i++) 
		{
			Transform ChildTransform = MyPlayerSpawn.transform.GetChild(i);
			if (ChildTransform.GetComponent<GUI3D.Orbitor> ())
				ChildTransform.GetComponent<GUI3D.Orbitor> ().Target = MyCameraBone.gameObject;
			if (ChildTransform.GetComponent<GUI3D.Billboard> ())
			{
				if (ChildTransform.name != "Label") 
				{
					ChildTransform.GetComponent<GUI3D.Billboard> ().TargetCharacter = MyCameraBone.gameObject;
				}
			}
		}
	}

}
