using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/*
 * This class is the manager for the main menu of the game
 */
public class PUNMenuManager : MonoBehaviour {

	#region Public References

	public Text TitleText;							// The title text element
	public GlowingText PlayText;					// The glowing "tap-to-play" text element
	public FadingShade Shade;						// The black image used for fading in and out of scenes
	public GameObject MultiPlayMenu, LobbyMenu;		// The parent objects for the menu segments

	#endregion


	#region Private Variables

	private bool localMenuOpen;						// True if a local-play menu is open, false if a net-play menu is open
	private bool leavingMenu;						// True if a game is about to begin and the menu scene must be left
	private bool pastOpenMenu;						// True if the opening menu is no longer showing

	private string nextSceneName;					// The string name of the scene the menu should transition to next

	// Safe reference to the source of all game audio
	private AudioManager audioManager;
	private AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}

	// Safe reference to the match maker for net games
	private PUNQuickPlay matchMaker;
	private PUNQuickPlay MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = GetComponent<PUNQuickPlay>();
			}
			return matchMaker;
		}
	}

	// Safe reference to the manager for the lobby UI
	private PUNLobbyManager lobby;
	private PUNLobbyManager Lobby {
		get {
			if (!lobby) {
				lobby = GetComponent<PUNLobbyManager>();
			}
			return lobby;
		}
	}

	#endregion


	#region Behaviour API

	// Use this for initialization
	void Start () {

		// Initialize some UI elements
		Shade.Initialize();
		PlayText.enabled = false;
	}

	// Update is called once per frame
	void Update () {

		// Handle interactions before opening menu is past
		if (!pastOpenMenu) {
			// Enable glowing "tap-to-play" text when shade is gone
			if (Shade.IsHidden && !PlayText.enabled) {
				PlayText.Initialize();
			}

			// Check for input that exits opening menu
			CheckForIntroSkip();
		}
		else {
			// Check if the scene is being exited
			CheckForMenuExit();
		}
	}

	#endregion


	#region Public API

	// Used to gain a reliable reference to this manager
	public static PUNMenuManager Get() {
		return FindObjectOfType<PUNMenuManager>();
	}

	// Manage UI to show a network lobby
	public void ShowNetworkLobby(bool asHost) {
		PlayText.enabled = false;
		ToggleNetworkLobby(asHost);
	}

	// Manage UI to leave a local lobby
	public void ExitLocalLobby() {
		ToggleLobbyMenu();
		TogglePlayMenu();
	}

	// Manage leaving a network lobby
	public void ExitNetworkLobby() {
		UpdateConnectionStatus(PUNQuickPlay.NetGameStatus.NOTCONNECTED);

		// Disconnect from the network, leaving any game in progress
		PhotonNetwork.Disconnect();
		ToggleLobbyMenu();
		TogglePlayMenu();
	}

	// Begins the animations that lead to desired scene change
	public void LeaveMenu() {
		nextSceneName = localMenuOpen ? "LocalDuel" : "NetworkDuel";
		leavingMenu = true;
		Shade.Toggle();
	}

	#endregion


	#region Private API

	public void UpdateConnectionStatus(PUNQuickPlay.NetGameStatus connStatus) {
		switch (connStatus) {
		case PUNQuickPlay.NetGameStatus.CONNECTING:
			PlayText.text = "Connecting";
			PlayText.enabled = true;
			break;
		case PUNQuickPlay.NetGameStatus.FINDING:
			PlayText.text = "Finding a game";
			break;
		case PUNQuickPlay.NetGameStatus.ENTERING:
			PlayText.text = "Entering game";
			break;
		default:
			PlayText.enabled = false;
			break;
		}
	}

	// Handle animating UI elements when leaving menu scene
	private void CheckForMenuExit() {
		// After shade is black, leave menu
		if (!Shade.IsHidden && leavingMenu) {
			leavingMenu = false;

			// Clear event listeners
			EventManager.Nullify();

			// Change scene
			if (localMenuOpen) {
				SceneManager.LoadScene(nextSceneName);
			}
			else {
				BushidoNetManager.Get().LaunchNetworkDuel();
			}
		}
	}

	// Check for tap input that skips opening menu
	private void CheckForIntroSkip() {
		if (ReceivedInput()) {
			// Input will either skip title slide or open main menu
			if (Shade.IsHidden) {
				AudioManager.Get().PlayMenuSound();

				// Hide play text and show play menu
				PlayText.enabled = false;
				MultiPlayMenu.SetActive(true);

				// Prevent further non-button input
				pastOpenMenu = true;
			}
		}
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	// Toggle visibility of multiplayer menu
	private void TogglePlayMenu() {
		MultiPlayMenu.SetActive(!MultiPlayMenu.activeSelf);
	}

	// Toggle visibility of lobby menu
	private void ToggleLobbyMenu() {
		TitleText.gameObject.SetActive(LobbyMenu.activeSelf);
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	// Prepare network lobby and toggle menu
	private void ToggleNetworkLobby(bool asHost) {
		// Prepare network lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			Lobby.PrepareNetworkLobby(asHost);
		}
		ToggleLobbyMenu();
	}

	// Prepare local lobby and toggle menu
	private void ToggleLocalLobby() {
		// Prepare local lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			Lobby.PrepareLocalLobby();
		}
		ToggleLobbyMenu();
	}

	#endregion


	#region ButtonEvents

	// Open the local game lobby
	public void OnLocalGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = true;

		// Hide multiplay menu and show local lobby
		TogglePlayMenu();
		ToggleLocalLobby();
	}

	// Open the network game menu
	public void OnNetworkGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = false;

		// Hide play menu and begin connecting for network game
		TogglePlayMenu();
		MatchMaker.Connect();
	}

	#endregion
}
