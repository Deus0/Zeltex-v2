using UnityEngine;
using System.Collections;

public class NetworkStatus : MonoBehaviour 
{
	public GUISkin Skin;
	public float width = 200;
	public float height = 120;
	
	void OnGUI()
    {
        if( Skin != null )
        {
            GUI.skin = Skin;
        }

		Rect centeredRect = new Rect(  Screen.width - (width ), Screen.height-( height ), width, height );

        GUILayout.BeginArea( centeredRect, GUI.skin.box );
        {
            GUILayout.Label( "Connecting" + GetConnectingDots(), GUI.skin.customStyles[ 0 ] );
			//string MyStatus = "";
			//if (PhotonNetwork.connectionStateDetailed == PeerState.
            GUILayout.Label( "Status: " + PhotonNetwork.connectionState );
        }
        GUILayout.EndArea();

        if( PhotonNetwork.connectionStateDetailed == PeerState.Joined )
        {
            enabled = false;
        }
    }

    string GetConnectingDots()
    {
        string str = "";
        int numberOfDots = Mathf.FloorToInt( Time.timeSinceLevelLoad * 3f % 4 );

        for( int i = 0; i < numberOfDots; ++i )
        {
            str += " .";
        }

        return str;
    }
}
