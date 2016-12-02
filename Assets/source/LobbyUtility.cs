using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyUtility : NetworkBehaviour {

	/*
	#region Public Accessors

	public bool HostReady {
		get {
			return HostReady;
		}
	}

	public bool ClientReady {
		get {
			return clientReady;
		}
	}

	public bool HostInLobby {
		get {
			return hostInLobby;
		}
	}

	public bool ClientInLobby {
		get {
			return clientInLobby;
		}
	}

	#endregion


	#region Private Variables

	[SyncVar(hook = "OnHostReadyStatusChanged")]
	private bool hostReady;

	[SyncVar(hook = "OnClientReadyStatusChanged")]
	private bool clientReady;

	[SyncVar(hook = "OnHostEnteredLobby")]
	private bool hostInLobby;

	[SyncVar(hook = "OnClientEnteredLobby")]
	private bool clientInLobby;

	#endregion


	#region Public API

	public static LobbyUtility Get() {
		return FindObjectOfType<LobbyUtility>();
	}

	// Returns whether the player was host or client
	public bool OnPlayerEnteredLobby() {
		// Update lobby status
		if (!hostInLobby) {
			hostInLobby = true;
			return true;
		}
		else {
			clientInLobby = true;
			return false;
		}
	}

	public void OnPlayerReady(bool hostSamurai) {
		// Update lobby ready status
		if (hostSamurai) {
			hostReady = true;
		}
		else {
			clientReady = true;
		}
	}

	#endregion


	#region SyncVar Hooks

	private void OnHostEnteredLobby(bool newHostInLobby) {
		hostInLobby = newHostInLobby;
		MenuManager.Get().GetLobby(false).UpdateLobbySamurai();
	}

	private void OnClientEnteredLobby(bool newClientInLobby) {
		clientInLobby = newClientInLobby;
		MenuManager.Get().GetLobby(false).UpdateLobbySamurai();
	}

	private void OnHostReadyStatusChanged(bool newHostReady) {
		hostReady = newHostReady;
		MenuManager.Get().GetLobby(false).UpdateLobbyReadyBoxes();
	}

	private void OnClientReadyStatusChanged(bool newClientReady) {
		clientReady = newClientReady;
		MenuManager.Get().GetLobby(false).UpdateLobbyReadyBoxes();
	}

	#endregion */
}
