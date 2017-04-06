using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

	public static string LocalDuelScene = "LocalDuel-Fix";
	public static string NetDuelScene = "NetworkDuel";

	// Safe reference to the PUN match maker
	private static PUNQuickPlay matchMaker;
	public static PUNQuickPlay MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = Get().GetComponent<PUNQuickPlay>();
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

	// Private reference to an instance of this component
	private static Globals Get() {
		return FindObjectOfType<Globals>();
	}
}
