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

	public MenuManager Menu {
		get {
			if (!menu) {
				menu = MenuManager.Get();
			}
			return menu;
		}
	}

	private MenuManager menu;

	public static LobbyPlayer[] GetAll() {
		return FindObjectsOfType<LobbyPlayer>();
	}

	public static LobbyPlayer GetLocalPlayer() {
		// Find local player and give ready signal
		foreach (LobbyPlayer player in LobbyPlayer.GetAll()) {
			if (player.isLocalPlayer) {
				return player;
			}
		}
		Debug.Log("Local player not found");
		return null;
	}
		
	public override void OnStartLocalPlayer() {
		if (!Menu.LobbyMenu.activeSelf) {
			Menu.ShowNetworkLobby();
		}
		CmdAddPlayerToLobby();
	}

	[Command]
	public void CmdGiveReadySignal() {
		Menu.Lobby.OnPlayerReady(isHost);
		ready = true;
	}

	[Command]
	public void CmdAddPlayerToLobby() {
		isHost = Menu.OnNetworkPlayerEnteredLobby();
	}

	[Command]
	public void CmdChangeBestOfIndex(bool minus) {
		Menu.Lobby.ChangeBestOfIndex(minus);
	}
}
