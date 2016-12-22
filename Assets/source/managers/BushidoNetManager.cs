using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class BushidoNetManager : NetworkLobbyManager {

	#region Public Accessors

	public int matchLimit;

	#endregion


	#region Public API

	public override void OnLobbyClientExit ()
	{
		base.OnLobbyClientExit ();
		Debug.Log("Client left lobby");
	}

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	public override void OnLobbyServerPlayersReady() {
		Debug.Log("Both players ready");
	}

	public void OnBothPlayersReady() {
		ServerChangeScene("NetworkDuel");
	}

	#endregion

}
