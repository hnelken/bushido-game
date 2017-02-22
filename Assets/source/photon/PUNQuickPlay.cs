using Photon;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PUNQuickPlay : Photon.PunBehaviour {

	public enum NetGameStatus {
		NOTCONNECTED, CONNECTING, FINDING, ENTERING
	}

	#region Private Variables

	private PUNNetworkPlayer thisPlayer;		// Reference to local player object
	private bool playerIsHost;					// True if the local player created the room they are joining

	private PUNLobbyManager lobby;
	private PUNLobbyManager Lobby {
		get {
			if (!lobby) {
				lobby = GetComponent<PUNLobbyManager>();
			}
			return lobby;
		}
	}

	private PUNMenuManager menu;
	private PUNMenuManager Menu {
		get {
			if (!menu) {
				menu = GetComponent<PUNMenuManager>();
			}
			return menu;
		}
	}

	#endregion


	#region Behaviour API
	// Use this for initialization
	void Start() {
	}

	#endregion


	#region Public API

	public static PUNQuickPlay Get() {
		return GameObject.FindObjectOfType<PUNQuickPlay>();
	}
	
	public void Connect() {
		Menu.UpdateConnectionStatus(NetGameStatus.CONNECTING);

		// Connect to photon network
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	/*
	public void QuickPlay() {
		// Try quickly joining a random room as client
		this.playerIsHost = false;
		PhotonNetwork.JoinRandomRoom();
	}*/
		
	public bool LocalPlayerIsHost() {
		return playerIsHost;
	}

	#endregion


	#region PunBehaviour API

	public override void OnJoinedLobby() {
		Menu.UpdateConnectionStatus(NetGameStatus.FINDING);

		// Try quickly joining a random room as client
		this.playerIsHost = false;
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {

		// Must create a room, player will be the host
		this.playerIsHost = true;
		PhotonNetwork.CreateRoom(null);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer player) {
		if (PhotonNetwork.isMasterClient) {
			PUNLobbyManager.Get().SyncLobbySettings();
		}
	}

	public override void OnJoinedRoom() {
		Menu.UpdateConnectionStatus(NetGameStatus.ENTERING);

		// Instantiate the player input prefab and enable it
		GameObject playerObject = PhotonNetwork.Instantiate("PunPlayer", Vector3.zero, Quaternion.identity, 0);
		thisPlayer = playerObject.GetComponent<PUNNetworkPlayer>();
		thisPlayer.enabled = true;

		// Set the player as host or client
		if (this.playerIsHost) {
			thisPlayer.SetAsHost();
		}

		// Signal to the lobby that a player has entered
		StartCoroutine(WaitAndShowLobby());
	}

	public IEnumerator WaitAndShowLobby() {

		yield return new WaitForSeconds(1);

		// Update the lobby on all clients
		thisPlayer.EnterLobby();

		// Show the lobby for the local player
		PUNMenuManager.Get().ShowNetworkLobby(thisPlayer.IsHost);
	}

	[PunRPC]
	void SyncSettings() {

	}

	#endregion
}
