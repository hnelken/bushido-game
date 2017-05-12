using Photon;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PUNQuickPlay : Photon.PunBehaviour {
	
	public delegate void PUNEvent();
	public static event PUNEvent Disconnect;	// Event for a player disconnecting

	// Triggers "game start" event
	public static void TriggerDisconnect() {
		if (Disconnect != null) {
			Disconnect();
		}
	}

	public enum NetGameStatus {
		NOTCONNECTED, CONNECTING, FINDING, ENTERING
	}

	#region Private Variables

	private PUNNetworkPlayer thisPlayer;		// Reference to local player object
	private bool playerIsHost;					// True if the local player created the room they are joining

	#endregion


	#region Behaviour API

	void Start() {
		PhotonNetwork.automaticallySyncScene = true;
	}

	#endregion


	#region Public API

	public static PUNQuickPlay Get() {
		return GameObject.FindObjectOfType<PUNQuickPlay>();
	}

	public void InitializeForNewScene() {
		Disconnect = null;
	}
	
	public void Connect() {
		Globals.Menu.UpdateConnectionStatus(NetGameStatus.CONNECTING);

		// Connect to photon network
		PhotonNetwork.ConnectUsingSettings("0.1");
	}
		
	public bool LocalPlayerIsHost() {
		return playerIsHost;
	}

	#endregion


	#region PunBehaviour API

	// Called when a player disconnects from the room
	public override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
		Debug.Log("Disconnect");

		TriggerDisconnect();
		//Globals.NetLobby.OnOpponentLeftLobby();
	}

	// Called when a player connects to Photon successfully
	public override void OnJoinedLobby() {
		QuickPlay();
	}

	// Called when there are no rooms to join immediately
	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
		// Must create a room, player will be the host
		this.playerIsHost = true;
		PhotonNetwork.CreateRoom(null);
	}

	// Called when a player joins a room as host OR client
	public override void OnJoinedRoom() {
		Globals.Menu.UpdateConnectionStatus(NetGameStatus.ENTERING);

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
		Globals.Menu.ShowNetworkLobby(thisPlayer.IsHost);
	}

	public void QuickPlay() {
		Globals.Menu.UpdateConnectionStatus(NetGameStatus.FINDING);

		// Try quickly joining a random room as client
		this.playerIsHost = false;
		PhotonNetwork.JoinRandomRoom();
	}

	#endregion
}
