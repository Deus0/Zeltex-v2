using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook;
using Facebook.Unity;
using Facebook.MiniJSON;

/*public class CustomFacebookLogin : MonoBehaviour {
	//string AppID = 975824305821070;
	public string UserName = "";
	public string Password;
	void OnGUI()
	{
		if (!FB.IsLoggedIn)
		{
			if (GUI.Button(new Rect(10, 10, 100, 20), "Login to Facebook")) {
				//FB.LogIN
				//FB.Login("email", LoginCallback);
				FB.LogInWithReadPermissions (
				new List<string>(){"email",},
				LoginCallback
					);
			}
		}
		GUI.Label(new Rect(200, 50, 100, 20), PhotonNetwork.connectionStateDetailed.ToString());
	}
	void Awake()
	{
		FB.Init(SetInit, OnHideUnity);
	}
	
	private void SetInit()
	{
		enabled = true;
		if (FB.IsLoggedIn)
		{
			Debug.Log("SetInit()");
			OnLoggedIn();
		}
	}
	
	private void OnHideUnity(bool isGameShown)
	{
		Debug.Log("OnHideUnity()");
	}
	
	void LoginCallback()
	{
		if (FB.IsLoggedIn)
		{
			OnLoggedIn();
		}
	}
	void OnLoggedIn()
	{
		PhotonNetwork.AuthValues = new AuthenticationValues();
		PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Facebook;
		PhotonNetwork.AuthValues.UserId = FB.AppId; // alternatively set by server
		PhotonNetwork.AuthValues.AddAuthParameter(UserName, FB.AppId);
		//PhotonNetwork.AuthValues.AddAuthParameter("token", FB.AccessToken);
		PhotonNetwork.ConnectUsingSettings("1.0");
	}
}*/
