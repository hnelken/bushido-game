﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

	[SyncVar]
	private bool isHost;

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
		
	public override void OnStartLocalPlayer() {
		Debug.Log("Local Client");
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
}
