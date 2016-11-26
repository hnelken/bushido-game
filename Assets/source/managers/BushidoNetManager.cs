using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class BushidoNetManager : NetworkLobbyManager {

	[HideInInspector]
	public int matchLimit;

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
		var lobbyPlayer = Instantiate(lobbyPlayerPrefab);
		Debug.Log(playerControllerId);
		if (lobbySlots[0]) {
			lobbySlots[1] = lobbyPlayer;
		}
		else {
			lobbySlots[0] = lobbyPlayer;
		}
		MenuManager.Get().UpdateLobbyDialog(lobbySlots);
		return lobbyPlayer.gameObject;
	}

	public override void OnServerConnect(NetworkConnection conn) {
		//Debug.Log(lobbySlots.Length);
	}

	void OnClientEnterLobby() {
		//this.lobbySlots
		//this.matches
	}

	void OnClientExitLobby() {

	}

	void OnClientReady(bool readyState) {

	}
}
