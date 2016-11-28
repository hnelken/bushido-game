using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

	private bool isHost;
	private bool ready;

	public bool IsHost {
		get {
			return isHost;
		}
	}

	public bool Ready {
		get {
			return ready;
		}
	}

	public static LobbyPlayer GetLocalPlayer() {
		foreach (LobbyPlayer player in GetAll()) {
			if (player.isLocalPlayer) {
				return player;
			}
		}
		Debug.LogError("No Local Player");
		return null;
	}

	public static LobbyPlayer[] GetAll() {
		return FindObjectsOfType<LobbyPlayer>();
	}

	[ClientRpc]
	public void RpcSignalReady() {
		ready = true;
	}

	[ClientRpc]
	public void RpcSetAsHost() {
		isHost = true;
	}
}
