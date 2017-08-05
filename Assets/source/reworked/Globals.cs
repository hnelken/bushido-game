﻿using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {
	
	public static string MainMenuScene = "Menu-Fix";
	public static string SoloDuelScene = "SoloDuel";
	public static string LocalDuelScene = "LocalDuel-Fix";
	public static string LocalPostScene = "LocalPostGame";
	public static string NetDuelScene = "NetworkDuel-Fix";
	public static string NetPostScene = "NetPostGame";
	public static bool LocalPlayerIsHost;

	public static bool IsNetPlayerHost(PUNNetworkPlayer player) {
		if (player.photonView.isMine) {
			return LocalPlayerIsHost;
		}
		else {
			return !LocalPlayerIsHost;
		}
	}

	// Safe reference to the PUN match maker
	private static PUNQuickPlay matchMaker;
	public static PUNQuickPlay MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = GameObject.FindObjectOfType<PUNQuickPlay>();
			}
			return matchMaker;
		}
	}

	// Safe reference to the main menu manager
	private static MainMenuManager menu;
	public static MainMenuManager Menu {
		get {
			if (!menu) {
				menu = Get().GetComponent<MainMenuManager>();
			}
			return menu;
		}
	}

	private static SoloLobbyManager soloLobby;
	public static SoloLobbyManager SoloLobby {
		get {
			if (!soloLobby) {
				soloLobby = Get().GetComponent<SoloLobbyManager>();
			}
			return soloLobby;
		}
	}

	// Safe reference to the local lobby manager
	private static LocalLobbyManager localLobby;
	public static LocalLobbyManager LocalLobby {
		get {
			if (!localLobby) {
				localLobby = Get().GetComponent<LocalLobbyManager>();
			}
			return localLobby;
		}
	}

	// Safe reference to the network lobby manager
	private static NetLobbyManager netLobby;
	public static NetLobbyManager NetLobby {
		get {
			if (!netLobby) {
				netLobby = Get().GetComponent<NetLobbyManager>();
			}
			return netLobby;
		}
	}

	// Safe reference to the source of all game audio
	private static AudioManager audioManager;
	public static AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}

	// The sprite for the checked box
	private static Sprite checkedBox;
	public static Sprite CheckedBox {
		get {
			if (!checkedBox) {
				checkedBox = Resources.Load<Sprite>("sprites/checkbox-checked");
			}
			return checkedBox;
		}
	}

	// The sprite for the unchecked box
	private static Sprite uncheckedBox;
	public static Sprite UncheckedBox {
		get {
			if (!uncheckedBox) {
				uncheckedBox = Resources.Load<Sprite>("sprites/checkbox-unchecked");
			}
			return uncheckedBox;
		}
	}

	// Private reference to an instance of this component
	private static Globals Get() {
		return FindObjectOfType<Globals>();
	}
}
