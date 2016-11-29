using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

	[SyncVar]
	public bool isHost;

	[SyncVar]
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

	public static LobbyPlayer[] GetAll() {
		return FindObjectsOfType<LobbyPlayer>();
	}
		
	public override void OnStartLocalPlayer() {
		Debug.Log("Local Client");
		CmdAddPlayerToLobby();
	}

	[Command]
	public void CmdAddPlayerToLobby() {
		isHost = LobbyUtility.Get().OnPlayerEnteredLobby();
	}
}
