using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyUtility : NetworkBehaviour {

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

	public List<LobbyPlayer> Lobby {
		get {
			return lobby;
		}
	}

	#endregion


	#region Private Variables

	private List<LobbyPlayer> lobby = new List<LobbyPlayer>();

	[SyncVar]
	private bool hostReady, clientReady;

	[SyncVar]
	private bool hostInLobby, clientInLobby;

	private int countDown = 3;

	#endregion


	#region Public API

	public static LobbyUtility Get() {
		return FindObjectOfType<LobbyUtility>();
	}

	public void AddPlayerToLobby(LobbyPlayer player) {
		// Set player as host or client and update lobby status
		if (lobby.Count == 0) {
			hostInLobby = true;
			player.RpcSetAsHost();
		}
		else {
			clientInLobby = true;
		}

		// Add player to lobby slots
		lobby.Add(player);

		// Update lobby UI
		//RpcUpdateLobbyUI();
	}

	public IEnumerator CountDown() {
		yield return new WaitForSeconds(countDown);

		Debug.Log("Change Scene Now");
	}

	#endregion


	#region Server Commands

	[Command]
	public void CmdSignalPlayerReady(bool hostSamurai) {

		// Update lobby ready status
		if (hostSamurai) {
			hostReady = true;
		}
		else {
			clientReady = true;
		}

		// Update lobby UI
		RpcUpdateLobbyUI();

		// Check if both players are ready
		if (hostReady && clientReady) {
			StartCoroutine(CountDown());
		}
	}

	#endregion


	#region Client RPC's

	[ClientRpc]
	public void RpcUpdateLobbyUI() {
		// Update lobby UI on both clients
		MenuManager.Get().UpdateLobbyDialog();
	}

	#endregion
}
