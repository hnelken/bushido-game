using Photon;
using UnityEngine;
using System.Collections;

public class PUNQuickPlay : Photon.PunBehaviour {

	private bool playerIsHost;		// True if the local player created the room they are joining

	// Use this for initialization
	void Start() {
		// Connect to photon network
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	void OnGUI() {
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}

	public override void OnJoinedLobby() {
		// Try quickly joining a random room
		PhotonNetwork.JoinRandomRoom();
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

		// Set the player as host if they made the room
		if (this.playerIsHost) {
			player.SetAsHost();
		}
	}
}
