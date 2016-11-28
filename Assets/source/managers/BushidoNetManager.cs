using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class BushidoNetManager : NetworkLobbyManager {

	#region Public Accessors

	public int matchLimit;

	#endregion


	#region Overriden Callbacks

	public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
		var lobbyPlayer = Instantiate(lobbyPlayerPrefab) as LobbyPlayer;
		var lobbyUtility = LobbyUtility.Get();
		lobbyUtility.AddPlayerToLobby(lobbyPlayer);

		Debug.Log("LobbyPlayer created");
		return lobbyPlayer.gameObject;
	}
		
	public override void OnLobbyClientEnter() {
		Debug.Log("Client entered");
		//NetworkUtility.Get().CmdUpdateLobbyUI();
	}

	#endregion


	#region Public API

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	#endregion

}
