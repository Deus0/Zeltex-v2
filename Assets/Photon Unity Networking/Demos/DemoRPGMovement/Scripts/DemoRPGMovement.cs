using UnityEngine;
using System.Collections;

public class DemoRPGMovement : MonoBehaviour 
{
    public RPGCamera Camera;

    void OnJoinedRoom()
    {
        CreatePlayerObject();
    }

    void CreatePlayerObject()
    {
		Vector3 position = transform.position;

        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Robot Kyle RPG", position, Quaternion.identity, 0 );

        Camera.Target = newPlayerObject.transform;
    }
}
