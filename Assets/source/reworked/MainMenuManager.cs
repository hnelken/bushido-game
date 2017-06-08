﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

	#region Public References

	public Text TitleText;							// The title text element
	public GlowingText PlayText;					// The glowing "tap-to-play" text element
	public FadingShade Shade;						// The black image used for fading in and out of scenes

	public GameObject MultiPlayMenu;				// The parent objects for the multiplayer menu
	public GameObject LocalLobby, NetLobby;			// The parent objects for the lobby menus

	#endregion


	#region Private Variables

	private bool localLobbyOpen;					// True if a local-play lobby is open, false if a net-play menu is open
	private bool leavingMenu;						// True if a game is about to begin and the menu scene must be left
	private bool pastIntroMenu;						// True if the opening menu is no longer showing

	private string nextSceneName;					// The string name of the scene the menu should transition to next

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
		if (!pastIntroMenu) {
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

	// Update menu text to display network game search status
	public void UpdateConnectionStatus(PUNQuickPlay.NetGameStatus connStatus) {
		switch (connStatus) {
		case PUNQuickPlay.NetGameStatus.CONNECTING:
			PlayText.text = "Connecting";
			PlayText.enabled = true;
			break;
		case PUNQuickPlay.NetGameStatus.FINDING:
			PlayText.text = "Finding a game";
			PlayText.enabled = true;
			break;
		case PUNQuickPlay.NetGameStatus.ENTERING:
			PlayText.text = "Entering game";
			break;
		default:
			PlayText.enabled = false;
			break;
		}
	}

	// Manage UI to show a network lobby
	public void ShowNetworkLobby(bool asHost) {
		Debug.Log("ShowLobby");
		PlayText.enabled = false;
		ToggleNetworkLobby(asHost);
	}

	// Manage UI to leave a local lobby
	public void ExitLocalLobby() {
		// Switch menu visibility
		ToggleLobbyMenu();
		TogglePlayMenu();
	}

	// Manage leaving a network lobby
	public void ExitNetworkLobby() {
		UpdateConnectionStatus(PUNQuickPlay.NetGameStatus.NOTCONNECTED);

		// Disconnect from the network, leaving any game in progress
		PhotonNetwork.Disconnect();

		// Switch menu visibility
		ToggleLobbyMenu();
		TogglePlayMenu();
	}

	// Handle searching for a new game after a player has left
	public void RequeueForGame() {
		// Exit current room to allow search for new opponent
		PhotonNetwork.LeaveRoom();
		ToggleLobbyMenu();
	}

	// Begins the animations that lead to desired scene change
	public void LeaveForDuelScene() {
		nextSceneName = localLobbyOpen ? Globals.LocalDuelScene : Globals.NetDuelScene;
		PUNNetworkPlayer.SignalBothPlayersLeaveLobby();
		leavingMenu = true;
		Shade.Toggle();
	}

	#endregion


	#region Private API

	// Handle animating UI elements when leaving menu scene
	private void CheckForMenuExit() {
		// After shade is black, leave menu
		if (!Shade.IsHidden && leavingMenu) {
			leavingMenu = false;

			// Change scene depending on local/net duel
			if (localLobbyOpen) {
				// Local lobby, load duel scene
				SceneManager.LoadScene(nextSceneName);
			}
			else if (PhotonNetwork.isMasterClient) {
				// Load network duel scene
				PhotonNetwork.LoadLevel(nextSceneName);
			}
		}
	}

	// Check for tap input that skips opening menu
	private void CheckForIntroSkip() {
		if (ReceivedInput()) {
			// Input will either skip title slide or open main menu
			if (Shade.IsHidden) {
				Globals.Audio.PlayMenuSound();

				// Hide play text and show play menu
				PlayText.enabled = false;
				MultiPlayMenu.SetActive(true);

				// Prevent further non-button input
				pastIntroMenu = true;
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
		GameObject lobbyMenu = (localLobbyOpen) ? LocalLobby : NetLobby;
		TitleText.gameObject.SetActive(lobbyMenu.activeSelf);
		lobbyMenu.SetActive(!lobbyMenu.activeSelf);
	}

	// Prepare network lobby and toggle menu
	private void ToggleNetworkLobby(bool asHost) {
		// Prepare network lobby if menu is not visible
		if (!NetLobby.activeSelf) {
			Globals.NetLobby.PrepareLobby(asHost);
		}
		ToggleLobbyMenu();
	}

	// Prepare local lobby and toggle menu
	private void ToggleLocalLobby() {
		// Prepare local lobby if menu is not visible
		if (!LocalLobby.activeSelf) {
			Globals.LocalLobby.PrepareLobby();
		}
		ToggleLobbyMenu();
	}

	#endregion


	#region ButtonEvents

	// Open the local game lobby
	public void OnLocalGameSelect() {
		Globals.Audio.PlayMenuSound();
		localLobbyOpen = true;

		// Hide multiplay menu and show local lobby
		TogglePlayMenu();
		ToggleLocalLobby();
	}

	// Open the network game menu
	public void OnNetworkGameSelect() {
		Globals.Audio.PlayMenuSound();
		localLobbyOpen = false;

		// Hide play menu and begin connecting for network game
		TogglePlayMenu();
		Globals.MatchMaker.Connect();
	}

	#endregion
}
