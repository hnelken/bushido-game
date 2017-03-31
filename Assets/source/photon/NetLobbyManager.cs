using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetLobbyManager : MonoBehaviour {

	#region Editor References

	public Button NetReady;								// The center ready button element
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements
	public Image LeftCheckbox, RightCheckbox;			// The left and right checkbox image elements

	public Text LeftText, RightText;					// The text element displaying the status of each player's presence in lobby
	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	private PhotonView photonView;
	private List<PUNNetworkPlayer> players = new List<PUNNetworkPlayer>();	// The list of players in the lobby

	// Lobby status variables
	private bool clientFound;							// True if a client has entered a host's game, false if the host is waiting
	private bool hostReady, clientReady;				// True if the host/client is ready to leave the lobby, false if not
	private bool hostInLobby, clientInLobby;			// True if the host/client is present in the lobby, false if not

	// Countdown variables
	private bool countingDown;							// True if the countdown to match start is active, false if not
	private int countDown;								// The remaining number of seconds in the countdown

	// "Best of" variables
	private int bestOfIndex = 1;						// The index in the array of options for the win limit
	private string[] bestOfOptions =  {					// The array of options for the "best of" text
		"3", "5", "7"
	};

	// The sprite for the checked box
	private Sprite checkedBox;
	private Sprite CheckedBox {
		get {
			if (!checkedBox) {
				checkedBox = Resources.Load<Sprite>("sprites/checkbox-checked");
			}
			return checkedBox;
		}
	}

	// The sprite for the unchecked box
	private Sprite uncheckedBox;
	private Sprite UncheckedBox {
		get {
			if (!uncheckedBox) {
				uncheckedBox = Resources.Load<Sprite>("sprites/checkbox-unchecked");
			}
			return uncheckedBox;
		}
	}

	#endregion


	#region Photon Behaviour API

	// Use this for initialization
	void Start() {
		this.photonView = GetComponent<PhotonView>();

		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;
	}

	#endregion


	#region Photon RPC's

	[PunRPC]
	void SyncLobby(bool _hostReady, bool _clientReady, int _bestOfIndex) {
		this.hostReady = _hostReady;
		this.clientReady = _clientReady;
		this.bestOfIndex = _bestOfIndex;

		UpdateLobbyUI();
	}

	[PunRPC]
	void SyncChangeBestOfIndex(bool minus) {
		ChangeBestOfIndex(minus);
	}

	// Synchronize lobby settings for players just entering a network game
	public void SyncLobbySettings() {
		photonView.RPC("SyncLobby", PhotonTargets.All, hostReady, clientReady, bestOfIndex);
	}

	// Increment or decrement the "best-of" index and update the UI
	private void ChangeBestOfIndexOnAllClients(bool minus) {
		photonView.RPC("SyncChangeBestOfIndex", PhotonTargets.All, minus);
	}

	#endregion


	#region Public API

	public static PUNLobbyManager Get() {
		return FindObjectOfType<PUNLobbyManager>();
	}
		
	// Prepare the lobby menu for a network lobby
	public void PrepareNetworkLobby(bool asHost) {
		// Initialize lobby settings if the host, client syncs automatically
		if (asHost) {
			// Initialize "best of" selector
			bestOfIndex = 1;
			UpdateBestOfText();

			// Set both players as not ready
			ClearReadyStatus();

			// Set client as not present
			clientInLobby = false;
			UpdateLobbySamurai();

			// Add UI to show client is not present yet
			Globals.Menu.PlayText.text = "Waiting for another player";
			Globals.Menu.PlayText.enabled = true;

			// Hide host UI until client joins
			LeftArrow.gameObject.SetActive(false);
			RightArrow.gameObject.SetActive(false);
			NetReady.gameObject.SetActive(false);
		}
	}
	
	// Use the network players in the scene to update the lobby UI
	public void UpdateLobbyUI() {
		// Get lobby info from player objects
		foreach (PUNNetworkPlayer player in players) {
			if (player.IsHost) {
				hostInLobby = true;
				hostReady = player.IsReady;
			}
			else {
				clientInLobby = true;
				clientReady = player.IsReady;
			}
		}

		// Refresh UI elements
		UpdateLobbySamurai();
		UpdateLobbyReadyStatus();
		UpdateBestOfText();
	}

	// Handles a player joining a room
	public void OnPlayerEnteredLobby(PUNNetworkPlayer player) {
		this.players.Add(player);

		// Refresh all UI elements
		UpdateLobbyUI();
	}

	// Updates lobby UI when a player is newly ready
	public void OnPlayerSignalReady() {
		UpdateLobbyUI();
	}

	#endregion


	#region Private API

	// Change the current win limit selection
	private void ChangeBestOfIndex(bool minus) {
		// Changing match parameters resets ready status
		ClearReadyStatus();

		// Increment or decrement the index with wraparound
		if (minus) {
			bestOfIndex = (bestOfIndex > 0) ? bestOfIndex - 1 : bestOfOptions.Length - 1;
		}
		else {
			bestOfIndex = (bestOfIndex < bestOfOptions.Length - 1) ? bestOfIndex + 1 : 0;
		}

		// Update UI
		UpdateBestOfText();
	}

	// Updates "best of" text from index in options array
	private void UpdateBestOfText() {
		BestOfNumText.text = bestOfOptions[bestOfIndex];
	}

	// Updates the samurai image elements based on presence of players
	private void UpdateLobbySamurai() {
		// Enable or disable the samurai images
		LeftSamurai.enabled = hostInLobby;
		RightSamurai.enabled = clientInLobby;

		// Set the lobby text to show presence of each player
		LeftText.enabled = hostInLobby;
		RightText.enabled = clientInLobby;
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (hostReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (clientReady) ? CheckedBox : UncheckedBox;

		// Hide/show network lobby UI
		UpdateReadyUI();

		// Check if both players are ready
		CheckReadyStatus();
	}

	// Update UI to reflect ready status of both network players
	private void UpdateReadyUI() {
		// Check if local player is host or client
		if (PUNQuickPlay.Get().LocalPlayerIsHost()) {
			// Change UI in succession to avoid overlapping elements
			if (Globals.Menu.PlayText.enabled && Globals.Menu.PlayText.text == "Waiting for another player") {
				// Hide "waiting for player" text
				Globals.Menu.PlayText.enabled = false;
			}
			else {
				// Hide or show all lobby buttons depending on ready status of host
				NetReady.gameObject.SetActive(!hostReady);
				LeftArrow.gameObject.SetActive(!hostReady);
				RightArrow.gameObject.SetActive(!hostReady);
			}
		}
		else {
			// Hide or show all lobby buttons depending on ready status of client
			NetReady.gameObject.SetActive(!clientReady);
			LeftArrow.gameObject.SetActive(!clientReady);
			RightArrow.gameObject.SetActive(!clientReady);
		}
	}

	// Reset ready status of both players and refresh UI
	private void ClearReadyStatus() {
		// Both players are no longer ready
		hostReady = false;
		clientReady = false;

		// Clear ready status on player objects
		foreach (PUNNetworkPlayer player in this.players) {
			player.ClearReadyStatus();
		}

		// Update UI
		UpdateLobbyReadyStatus();
	}

	// Check for and handle both players being ready to leave the lobby
	private void CheckReadyStatus() {
		// Check if both players are ready
		if (hostReady && clientReady) {
			// Set the win limit in the network manager
			//BushidoNetManager.Get().SetMatchLimit(BestOfNumText.text);

			// Begin countdown to game start
			countDown = 5;						// Set countdown to 5
			countingDown = true;				// Set countdown as active
			SetCountDownText(countDown);		// Update countdown UI
			StartCoroutine(CountDown());		// Begin counting interval
		}
	}

	// Take a step in the countdown towards leaving the lobby
	private IEnumerator CountDown() {
		yield return new WaitForSeconds(1);

		// Check if the countdown was cancelled
		if (countingDown) {
			countDown -= 1;						// Decrement the countdown
			SetCountDownText(countDown);		// Update the countdown UI
			CheckCountDownStatus();				// Check if the countdown finished
		}
	}

	// Determines the status of the countdown each interval
	private void CheckCountDownStatus() {
		// Check if countdown has expired
		if (countDown == 0) {
			Globals.Menu.LeaveMenu();
		}
		else {
			// Not finished, continue count down
			StartCoroutine(CountDown());
		}
	}

	// Sets the text element to display the countdown
	private void SetCountDownText(int countdown) {
		if (!LobbyText.enabled) {
			LobbyText.enabled = true;
		}
		// Show countdown
		LobbyText.text = "Game starting in  " + countdown;
	}

	#endregion


	#region Button Events 

	// Handle a network player signalling ready
	public void OnNetworkReady() {
		Globals.Audio.PlayMenuSound();

		// Hide "best of" selector arrows for local player
		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);

		// Signal local player ready
		PUNNetworkPlayer.GetLocalPlayer().SetAsReady();
		OnPlayerSignalReady();
	}

	// Handle the left arrow button being pressed
	public void OnLeftPressed() {
		Globals.Audio.PlayMenuSound();

		// Decrement the "best-of" index
		ChangeBestOfIndexOnAllClients(true);
	}

	// Handle the right arrow button being pressed
	public void OnRightPressed() {
		Globals.Audio.PlayMenuSound();

		// Increment the "best-of" index
		ChangeBestOfIndexOnAllClients(false);
	}

	// Handle the lobby exit button being pressed
	public void OnLobbyExit() {
		Globals.Audio.PlayMenuSound();

		// Stop count down if it was in progress
		LobbyText.enabled = false;
		countingDown = false;

		// Exit the local or network lobby accordingly
		Globals.Menu.ExitNetworkLobby();
	}

	#endregion
}

