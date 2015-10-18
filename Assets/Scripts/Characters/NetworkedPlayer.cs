using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Characters.FirstPerson;

// Character Net Handler - should be called
// simply shares any relevant data over the network

public class NetworkedPlayer : MonoBehaviour {
	NetworkView MyNetworkView;
	public string OnlineName = "";

	void Awake()
	{
		MyNetworkView = gameObject.GetComponent<NetworkView> ();
		OnlineName = gameObject.name;
		// maybe reload the last used name from PlayerPrefs or generate a random name
		BroadcastNickName();
	}
	public void BroadcastNickName()
	{
		if (MyNetworkView && (Network.isServer || Network.isClient))
			MyNetworkView.RPC("UpdateNickName", RPCMode.All, OnlineName);
	}
	public void SendNickNameTo(NetworkPlayer aPlayer)
	{
		if (MyNetworkView && (Network.isServer || Network.isClient))
			MyNetworkView.RPC("UpdateNickName", aPlayer, OnlineName);
	}
	public void ChangeNick(string aNewNickName)
	{
		if (!MyNetworkView.isMine)
			return; // We can only change the nick when we are the owner of this player object
		OnlineName = aNewNickName;
		BroadcastNickName();
	}
	[RPC]
	void UpdateNickName(string aNewNickName)
	{
		if (MyNetworkView.isMine)
			return; // We don't want others to change our nickname
		OnlineName = aNewNickName;

	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		float ShootX = 0;
		float ShootY = 0;
		float ShootZ = 0;
		float ShootPosX = 0;
		float ShootPosY = 0;
		float ShootPosZ = 0;

		float ShootRotQuatX = 0;
		float ShootRotQuatY = 0;
		float ShootRotQuatZ = 0;
		float ShootRotQuatW = 0;
		BaseCharacter MyCharacter = gameObject.GetComponent<BaseCharacter> ();
		if (stream.isWriting) {
			// Sending
			ShootX = MyCharacter.ShootForwardVector.x;
			ShootY = MyCharacter.ShootForwardVector.y;
			ShootZ = MyCharacter.ShootForwardVector.z;
			stream.Serialize (ref ShootX);
			stream.Serialize (ref ShootY);
			stream.Serialize (ref ShootZ);
			ShootPosX = MyCharacter.ShootPosition.x;
			ShootPosY = MyCharacter.ShootPosition.y;
			ShootPosZ = MyCharacter.ShootPosition.z;
			stream.Serialize (ref ShootPosX);
			stream.Serialize (ref ShootPosY);
			stream.Serialize (ref ShootPosZ);
			
			ShootRotQuatX = MyCharacter.SpawnRotationQuaternion.x;
			ShootRotQuatY = MyCharacter.SpawnRotationQuaternion.y;
			ShootRotQuatZ = MyCharacter.SpawnRotationQuaternion.z;
			ShootRotQuatW = MyCharacter.SpawnRotationQuaternion.w;
			stream.Serialize (ref ShootRotQuatX);
			stream.Serialize (ref ShootRotQuatY);
			stream.Serialize (ref ShootRotQuatZ);
			stream.Serialize (ref ShootRotQuatW);
		} else {
			// revieving
			stream.Serialize (ref ShootX);
			stream.Serialize (ref ShootY);
			stream.Serialize (ref ShootZ);
			MyCharacter.ShootForwardVector = new Vector3(ShootX, ShootY, ShootZ);
			stream.Serialize (ref ShootPosX);
			stream.Serialize (ref ShootPosY);
			stream.Serialize (ref ShootPosZ);
			MyCharacter.ShootPosition = new Vector3(ShootPosX, ShootPosY, ShootPosZ);
			stream.Serialize (ref ShootRotQuatX);
			stream.Serialize (ref ShootRotQuatY);
			stream.Serialize (ref ShootRotQuatZ);
			stream.Serialize (ref ShootRotQuatW);
			MyCharacter.SpawnRotationQuaternion = new Quaternion(ShootRotQuatX, ShootRotQuatY, ShootRotQuatZ, ShootRotQuatW);
		}
	}
}
