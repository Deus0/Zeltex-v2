using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {
	int MyClientID = -1;
	List<int> MyClientIDs = new List<int>();
	private List<NetworkedPlayer> MyPlayers = new List<NetworkedPlayer>();

	public static int MaxConnections = 32;
	string GameName = "Zeltex";
	bool IsRefreshing = false;
	public HostData[] MyHostData;
	int DefaultPort = 25001;
	float ButtonWidth;
	float ButtonHeight;
	//public GameObject PlayerPrefab;
	//public GameObject SpawnPosition;
	public InputField MyPortInput;
	public InputField MyIPAddressInput;
	public InputField MyGameName;
	public GameObject NetworkGui;
	public bool IsOfflineMode = true;
	public bool IsInitialSpawn = false;
	//public List<GameObject> Players = new List<GameObject>();
	//public Text DebugForce1;
	//public Text DebugForce2;
	bool HasPublicAddress;
	public bool IsLobby = false;
	public bool IsDebugging = false;
	
	// Use this for initialization
	void Start () {
		ButtonWidth = Screen.width * 0.15f;
		ButtonHeight = Screen.height * 0.075f;
	}

	public void StartServer() {
		IsRefreshing = false;
		int Port = DefaultPort;
		if (MyPortInput.text != "")
			Port = int.Parse(MyPortInput.text);
		HasPublicAddress = Network.HavePublicAddress ();
		HasPublicAddress = true;
		Network.InitializeServer (MaxConnections, Port, HasPublicAddress);
		if (MyGameName.text != "")
			GameName = MyGameName.text;
		MasterServer.RegisterHost (GameName, "Zeltex", "This is Zeltex");
	}
	// from servers
	void OnServerInitialized() {
		Debug.Log ("Server Initialized at " + Time.time);
		if (IsLobby) {	// if they all start the game at the same time
			GetManager.GetGameManager ().PlayGameDeathMatch ();
		}
		MyClientIDs.Add (MyClientIDs.Count);
		MyClientID = 0;
		GetManager.GetChatBox ().EnterText ("Now Hosting Game - ClientID: " + MyClientID);

		GameObject MyLocalPlayer = GetManager.GetCharacterManager ().GetLocalPlayer ().gameObject;
		Vector3 MyPosition = MyLocalPlayer.transform.position;
		Quaternion MyRotation = MyLocalPlayer.transform.rotation;
		Destroy (MyLocalPlayer);
		GameObject NewSpawn = GetManager.GetCharacterManager ().SpawnPlayerOnline ();
		NewSpawn.transform.position = MyPosition;
		NewSpawn.transform.rotation = MyRotation;
		//GameObject MyOnlinePlayer = (GameObject)Network.Instantiate (MyLocalPlayer, MyLocalPlayer.transform.position, MyLocalPlayer.transform.rotation, MyClientID);
		//GetManager.GetCharacterManager ().UpdateLocalPlayer (MyOnlinePlayer);
		// carry on as usual! nothing to see here :D
	}

	// Called on clients that connect	// should set up the chunks to stream from this one
	void OnConnectedToServer() {
		Debug.LogError ("A client has connected " + Time.time);
		if (MyClientID == -1) {
			//MyClientID = Random.Range (1, 10000);
			MyClientID = MyClientIDs.Count+1;
			GetManager.GetGameManager ().PlayAdventureGame ();
		} else if (MyClientID == 0) {
			// load servers - loadedcharacters - first
			for (int i = 0; i < GetManager.GetCharacterManager().LoadedCharacters.Count; i++) {
				//GameObject NewCharacter = Instantiate(GetManager.GetCharacterManager().LoadedCharacters[i]);
			}
		}
		GameObject.Find ("ChatScreen").GetComponent<ChatBox>().EnterText ("Welcome new player, your networkID is: " + MyClientID);
		// need to spawn all the other character objects in the scene
		// need to stream world from the right position!
	}
	void OnPlayerConnected(NetworkPlayer ConnectedPlayer) {
		MyClientIDs.Add (MyClientIDs.Count);
		GameObject.Find ("ChatScreen").GetComponent<ChatBox>().EnterText ("A new client has connected! : " + (MyClientIDs.Count-1));
		foreach(NetworkedPlayer MyPlayer in MyPlayers)
			MyPlayer.SendNickNameTo(ConnectedPlayer);
	}
	void OnPlayerDisconnected(NetworkPlayer ConnectedPlayer) {
		GameObject.Find ("ChatScreen").GetComponent<ChatBox>().EnterText ("Client " + (ConnectedPlayer.ipAddress) + " has disconnected.");
		/*MyClientIDs.Add (MyClientIDs.Count);
		GameObject.Find ("ChatScreen").GetComponent<ChatBox>().EnterText ("A new client has connected! : " + (MyClientIDs.Count-1));
		foreach(NetworkedPlayer MyPlayer in MyPlayers)
			MyPlayer.SendNickNameTo(ConnectedPlayer);*/
	}

	void OnMasterServerEvent(MasterServerEvent MSE) {
		if (MSE == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log ("Registration Success");
		} else if (MSE == MasterServerEvent.RegistrationFailedGameName) {
			Debug.Log ("Registration Failed, no game name");
		}
	}
	public EventsManager MyEventsManager;
	IEnumerator RefreshHosts() {
		MasterServer.RequestHostList (GameName);
		IsRefreshing = true;
		while (IsRefreshing) {
			yield return new WaitForSeconds(1.5f);
			MyHostData = (HostData[]) MasterServer.PollHostList ();
			if (MyEventsManager) {
				MyEventsManager.ClearAll();
				for (int i = 0; i < MyHostData.Length; i++) {
					MyEventsManager.NewEvent(MyHostData [i].gameName + " : " + MyHostData [i].ip [0]);
				}
				MyEventsManager.ResetGui();
			}
			if (MyHostData.Length > 0)
				IsRefreshing = false;
		}
	}
	public void ConnectToSelectedHost() {
		Network.Connect (MyHostData [MyEventsManager.SelectedIndex]);
	}
	public bool IsConnected() {
		return (!(!Network.isClient && !Network.isServer));
	}
	public void ConnectToHost() {
		StartCoroutine (RefreshHosts ());
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		if (stream.isWriting) {
			// Sending
			for (int i = 0; i < MyClientIDs.Count; i++) {
				int NewID =  MyClientIDs[i];
				stream.Serialize (ref NewID);
			}
		} else {
			// revieving
			MyClientIDs.Clear();
			for (int i = 0; i < MyClientIDs.Count; i++) {
				int NewID = 0;
				stream.Serialize (ref NewID);
				MyClientIDs.Add (NewID);
			}
		}
	}
	// put this in the gui manager later
	void OnGUI() {
		if (IsDebugging) {
			GUI.Label (new Rect (new Vector2 (Screen.width / 4f, 0), new Vector2 (100, 40)), "ClientID: " + MyClientID);
			if (Network.isClient)
				GUI.Label (new Rect (new Vector2 (Screen.width / 2f, 0), new Vector2 (100, 40)), "Server");
			else if (Network.isServer)
				GUI.Label (new Rect (new Vector2 (Screen.width / 2f, 0), new Vector2 (100, 40)), "Client");
			else
				GUI.Label (new Rect (new Vector2 (Screen.width / 2f, 0), new Vector2 (100, 40)), "Offline");
		}
	}
	/*void OnGUI() {
		if (!IsOfflineMode) {
			if (!Network.isClient && !Network.isServer) {
				if (MyHostData != null)
					for (int i = 0; i < MyHostData.Length; i++) {
					if (GUI.Button (new Rect (Screen.width/2f-ButtonWidth/2f + ButtonWidth * 1.2f, 
					                          Screen.height*0.25f-ButtonHeight/2f+ (ButtonHeight * i * 1.2f), 
					                          ButtonWidth * 3, ButtonHeight), 
				                				MyHostData [i].gameName + " : " + MyHostData [i].ip [0])) {
							Debug.Log ("Connecting to Host");
							if (MyIPAddressInput.text != "") {
								MyHostData [i].ip [0] = MyIPAddressInput.text;
							}
							Network.Connect (MyHostData [i]);
						}
					}
			} else {
				NetworkGui.SetActive (false);
			}
		}
	}*/
}
