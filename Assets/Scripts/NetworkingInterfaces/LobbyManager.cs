using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour 
{
	private KeyCode MyExitKey = KeyCode.Escape;
	public GUISkin MySkin;
	private bool spawn = false;
	private int maxPlayer = 1;
	private Room[] game;
	private string roomName = "DEFAULT ROOM NAME";
	bool connecting = false;
	List<string> chatMessages;
	int maxChatMessages = 5;
	private string maxPlayerString = "2";
	public string Version = "Version 1";
	private Vector3 up;
	private Vector2 scrollPosition;
	
	void Start (){
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "My Player name");
		chatMessages = new List<string>();
	}
	
	void OnDestroy(){
		PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
	}
	
	public void AddChatMessage(string m){
		GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
	}
	
	[PunRPC]
	void AddChatMessage_RPC(string m)
	{
		while(chatMessages.Count >= maxChatMessages){
			chatMessages.RemoveAt(0);
		}
		chatMessages.Add(m);
	}
	
	void Connect()
	{
		PhotonNetwork.autoJoinLobby = true;
		PhotonNetwork.ConnectUsingSettings(Version);
	}
	void SelectMapGui() {
		LevelSelectGui.MapSelectionGui ();
		LevelSelectGui.MapSelectionGui2 ();
	}
	void BeginThing() {
		
		GUI.Box (new Rect (0, -Screen.height / 4f, 400, 140), "");
		GUI.color = Color.white;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
	}
	void EndThing() {
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}
	void UserConnectGui() {
		GUILayout.Label ("Username: ");
		PhotonNetwork.player.name = GUILayout.TextField (PhotonNetwork.player.name);
		
		if (GUILayout.Button ("Connect")) {
			connecting = true;
			Connect ();
		}
	}
	void LobbyGui() 
	{
		GUI.Box (new Rect (Screen.width / 2.5f, Screen.height / 3f, 400, 550), "");
		GUI.color = Color.white;
		GUILayout.BeginArea (new Rect (Screen.width / 2.5f, Screen.height / 3, 400, 500));
		GUI.color = Color.cyan;
		GUILayout.Box ("Lobby");
		GUI.color = Color.white;
		GUILayout.Label ("Session Name:");
		roomName = GUILayout.TextField (roomName);
		//GUILayout.Label ("Max amount of players 1 - 20:");
		//maxPlayerString = GUILayout.TextField (maxPlayerString,2);
		maxPlayer = 10;
		
		if (maxPlayerString != "") {
			
			maxPlayer = int.Parse (maxPlayerString);
			
			if (maxPlayer > 20)
				maxPlayer = 20;
			if (maxPlayer == 0)
				maxPlayer = 1;
		} else {
			maxPlayer = 1;
		}
		
		if (GUILayout.Button ("Create Room ")) {
			if (roomName != "" && maxPlayer > 0) {
				PhotonNetwork.CreateRoom (roomName);	//,true,true,maxPlayer);
			}
		}
		
		GUILayout.Space (20);
		GUI.color = Color.yellow;
		GUILayout.Box ("Sessions Open");
		GUI.color = Color.red;
		GUILayout.Space (20);
		
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, true, GUILayout.Width (400), GUILayout.Height (300));
		
		
		foreach (RoomInfo game in PhotonNetwork.GetRoomList ()) 
		{
			GUI.color = Color.green;
			GUILayout.Box (game.name + " " + game.playerCount + "/" + game.maxPlayers + " " + game.visible);
			if (GUILayout.Button ("Join Session")) {
				PhotonNetwork.JoinRoom (game.name);
			}
		}
		
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	void OnGUI()
	{
		GUI.skin = MySkin;
		BeginThing ();
		//GUI.color = Color.grey;
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = GUILayout.Toggle (PhotonNetwork.offlineMode, "Offline Mode");
			SelectMapGui();
		} 
		else 
		{
			if (!PhotonNetwork.connected && !connecting)
			{
				PhotonNetwork.offlineMode = GUILayout.Toggle (PhotonNetwork.offlineMode, "Offline Mode");
				UserConnectGui();
			}
			else if (PhotonNetwork.insideLobby == true && connecting)
			{
				LobbyGui();
			}
			else if (PhotonNetwork.connected && !connecting)
			{
				if (!GetComponent<PlayerSpawner>().HasSpawnedPlayer())
				{
					/*if (GUILayout.Button("Back", GUILayout.Width(300))) 
					{

					}
					if (GUILayout.Button("Play", GUILayout.Width(300))) 
					{
						GetComponent<NetworkSpawner>().SpawnPlayer();
					}*/
					SelectMapGui();
				} 
				else 
				{
					if (IsPaused) {
						if (GUILayout.Button("Exit", GUILayout.Width(300))) {
							
							GetComponent<PlayerSpawner>().Clear();
							PhotonNetwork.LeaveRoom();
							PhotonNetwork.JoinLobby();
							//GameObject.FindObjectOfType<VoxelEngine.VoxelSaver>().Clear();
							connecting = true;
							
							IsPaused = false;
							Time.timeScale = 1;
						}
					}
					if (Input.GetKeyDown(MyExitKey))
					{
						if (IsPaused) {
							IsPaused = false;
							Time.timeScale = 1;
						} else {
							IsPaused = true;
							Time.timeScale = 0;
						}
					}
				}
			
			}
		}
		EndThing ();
	}
	public bool IsPaused = false;
	
	void OnJoinedLobby()
	{
		Debug.Log("OnJoinedLobby");
	}
	
	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom( null );
	}
	
	void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom: " + PhotonNetwork.room.name);
		connecting = false;
	}
}
