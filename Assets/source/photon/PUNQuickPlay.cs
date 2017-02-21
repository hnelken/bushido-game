using Photon;
using UnityEngine;
using System.Collections;

public class PUNQuickPlay : Photon.PunBehaviour {

	#region Private Variables

	private bool readyToPlay;		// True if the player has successfully joined a lobby and can search for a room 
	private bool playerIsHost;		// True if the local player created the room they are joining

	#endregion


	#region Behaviour API
	// Use this for initialization
	void Start() {
		// Connect to photon network
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	/*
	void OnGUI() {
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}
	*/

	#endregion


	#region Public API

	public static PUNQuickPlay Get() {
		return GameObject.FindObjectOfType<PUNQuickPlay>();
	}

	public void QuickPlay() {
		// Try quickly joining a random room as client
		this.playerIsHost = false;
		PhotonNetwork.JoinRandomRoom();
	}
		
	public bool LocalPlayerIsHost() {
		return playerIsHost;
	}

	public bool ReadyToPlay() {
		return readyToPlay;
	}

	#endregion


	#region PunBehaviour API

	public override void OnJoinedLobby() {
		this.readyToPlay = true;
	}

	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
		Debug.Log("Failed to join random room");

		// Must create a room, player will be the host
		this.playerIsHost = true;
		PhotonNetwork.CreateRoom(null);
	}

	public override void OnJoinedRoom() {
		// Instantiate the player input prefab and enable it
		GameObject playerObject = PhotonNetwork.Instantiate("PunPlayer", Vector3.zero, Quaternion.identity, 0);
		PUNNetworkPlayer player = playerObject.GetComponent<PUNNetworkPlayer>();
		player.enabled = true;

		// Set the player as host or client
		if (this.playerIsHost) {
			player.SetAsHost();
		}
		else {
			Debug.Log("client entered room: " + this.playerIsHost);

		}

		// Signal to the lobby that a player has entered
		//OnPlayerEnteredLobby();
		StartCoroutine(WaitAndEnterLobby(player));
	}

	public IEnumerator WaitAndEnterLobby(PUNNetworkPlayer player) {

		yield return new WaitForSeconds(2);
		Debug.Log("Entering Lobby");
		player.EnterLobby();
		// Show the lobby for the local player
		PUNMenuManager.Get().ShowNetworkLobby();
	}

	#endregion
}
