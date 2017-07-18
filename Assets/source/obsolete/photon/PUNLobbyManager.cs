using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PUNLobbyManager : MonoBehaviour {

	#region Editor References

	public Image LeftCheckbox, RightCheckbox;			// The left and right checkbox image elements
	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements

	public Button LeftReady, RightReady, NetReady;		// The left, right, and center ready button elements
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Text LeftText, RightText;					// The text element displaying the status of each player's presence in lobby
	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	private PhotonView photonView;
	private List<PUNNetworkPlayer> players = new List<PUNNetworkPlayer>();	// The list of players in the lobby

	// Lobby type and info
	private bool localLobby;							// True if the lobby is for a local game, false if for a network game

	// Lobby status variables
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

	// The menu manager governing this lobby
	private PUNMenuManager menu;
	public PUNMenuManager Menu {
		get {
			if (!menu) {
				menu = PUNMenuManager.Get();
			}
			return menu;
		}
	}

	private PUNLobbyManager lobby;
	public PUNLobbyManager Lobby {
		get {
			if (!lobby) {
				lobby = GetComponent<PUNLobbyManager>();
			}
			return lobby;
		}
	}

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

	#endregion


	#region Public API

	public void PrintLobbyStatus() {
		Debug.Log(hostInLobby + ":" + clientInLobby + " - " + hostReady + ":" + clientReady);
	}

	public static PUNLobbyManager Get() {
		return FindObjectOfType<PUNLobbyManager>();
	}

	// Synchronize lobby settings for players just entering a network game
	public void SyncLobbySettings() {
		photonView.RPC("SyncLobby", PhotonTargets.All, hostReady, clientReady, bestOfIndex);
	}

	// Handles a player joining a room
	public void OnPlayerEnteredLobby(PUNNetworkPlayer player) {
		this.players.Add(player);
		if (hostInLobby && !clientInLobby) {
			OnLobbyFull();
		}
		UpdateLobbyUI();
	}

	public void OnPlayerSignalReady() {
		UpdateLobbyUI();
	}

	// Prepare the lobby menu for a local lobby
	public void PrepareLocalLobby() {
		// Both players are immediately present in a local game
		hostInLobby = true;
		clientInLobby = true;

		// Initialize lobby UI
		PrepareLobbyUI(true, true);
		UpdateLobbySamurai();
	}

	// Prepare the lobby menu for a network lobby
	public void PrepareNetworkLobby(bool asHost) {
		PrepareLobbyUI(false, asHost);
	}

	// Use the network players in the scene to update the lobby UI
	public void UpdateLobbyUI() {
		// Get lobby info from player objects
		foreach (PUNNetworkPlayer player in players) {
			if (Globals.IsNetPlayerHost(player)) {//player.IsHost) {
				hostInLobby = true;
				hostReady = player.IsReadyForMatchStart;
			}
			else {
				clientInLobby = true;
				clientReady = player.IsReadyForMatchStart;
			}
		}

		// Refresh UI elements
		UpdateLobbySamurai();
		UpdateLobbyReadyStatus();
		UpdateBestOfText();
	}

	#endregion


	#region Private API

	private void OnLobbyFull() {
		Menu.PlayText.enabled = false;
		NetReady.gameObject.SetActive(true);
		LeftArrow.gameObject.SetActive(true);
		RightArrow.gameObject.SetActive(true);
	}

	// Increment or decrement the "best-of" index and update the UI
	private void ChangeBestOfIndexOnAllClients(bool minus) {
		photonView.RPC("SyncChangeBestOfIndex", PhotonTargets.All, minus);
	}

	// Change the current win limit selection
	private void ChangeBestOfIndex(bool minus) {
		Audio.PlayMenuSound();

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

	// Initialize the lobby UI for a local or network game
	private void PrepareLobbyUI(bool isLocalLobby, bool asHost) {
		// Set lobby type
		localLobby = isLocalLobby;

		// Initialize lobby settings if the host, client syncs automatically
		if (asHost) {
			bestOfIndex = 1;
			UpdateBestOfText();
			ClearReadyStatus();

			if (!isLocalLobby) {
				clientInLobby = false;
				UpdateLobbySamurai();
				LeftArrow.gameObject.SetActive(false);
				RightArrow.gameObject.SetActive(false);
			}
		}

		// Hide or show ready buttons depending on lobby type
		NetReady.gameObject.SetActive(false);
		LeftReady.gameObject.SetActive(localLobby);
		RightReady.gameObject.SetActive(localLobby);
	}

	// Reset ready status of both players and refresh UI
	private void ClearReadyStatus() {
		// Both players are no longer ready
		hostReady = false;
		clientReady = false;

		foreach (PUNNetworkPlayer player in this.players) {
			player.ClearMatchReadyStatus();
		}

		// Best-of selector arrows become available again
		LeftArrow.gameObject.SetActive(true);
		RightArrow.gameObject.SetActive(true);

		// Update UI
		UpdateLobbyReadyStatus();
	}

	// Updates the samurai image elements based on presence of players
	private void UpdateLobbySamurai() {
		// Enable or disable the samurai images
		LeftSamurai.enabled = hostInLobby;
		RightSamurai.enabled = clientInLobby;

		// Set the lobby text to show presence of each player
		LeftText.enabled = hostInLobby;
		RightText.enabled = clientInLobby;

		if (clientInLobby) {
			Menu.PlayText.enabled = false;
		}
		else {
			Menu.PlayText.text = "Waiting for another player";
			Menu.PlayText.enabled = true;
		}
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (hostReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (clientReady) ? CheckedBox : UncheckedBox;

		if (localLobby) {
			// Hide/show local lobby ready buttons
			LeftReady.gameObject.SetActive(!hostReady);
			RightReady.gameObject.SetActive(!clientReady);
		}
		else {
			// Hide/show network lobby UI
			UpdateNetworkReadyStatus();
		}

		// Check if both players are ready
		CheckReadyStatus();
	}

	// Update UI to reflect ready status of both network players
	private void UpdateNetworkReadyStatus() {
		// Check if local player is host or client
		if (PUNQuickPlay.Get().LocalPlayerIsHost()) {
			// Hide or show all lobby buttons depending on ready status of host
			NetReady.gameObject.SetActive(!hostReady);
			LeftArrow.gameObject.SetActive(!hostReady);
			RightArrow.gameObject.SetActive(!hostReady);
		}
		else {
			// Hide or show all lobby buttons depending on ready status of client
			NetReady.gameObject.SetActive(!clientReady);
			LeftArrow.gameObject.SetActive(!clientReady);
			RightArrow.gameObject.SetActive(!clientReady);
		}
	}

	// Check for and handle both players being ready to leave the lobby
	private void CheckReadyStatus() {
		// Check if both players are ready
		if (hostReady && clientReady) {
			// Begin countdown to round begin
			OnBothPlayersReady();
		}
	}

	// Triggered when both lobby players are ready to begin a match
	private void OnBothPlayersReady() {
		// Turn off arrow buttons in local lobby
		if (localLobby) {
			LeftArrow.gameObject.SetActive(false);
			RightArrow.gameObject.SetActive(false);
		}

		// Set the win limit in the network manager
		//BushidoNetManager.Get().SetMatchLimit(BestOfNumText.text);

		// Begin countdown to game start
		countDown = 5;					// Set countdown to 5
		countingDown = true;			// Set countdown as active
		UpdateLobbyText(countDown);		// Update countdown UI
		StartCoroutine(CountDown());	// Begin counting
	}

	// Take a step in the countdown towards leaving the lobby
	private IEnumerator CountDown() {

		yield return new WaitForSeconds(1);

		// Check if the countdown was cancelled
		if (countingDown) {
			countDown -= 1;				// Decrement the countdown
			UpdateLobbyText(countDown);	// Update the countdown UI
			CheckCountDownStatus();		// Check if the countdown finished
		}
	}

	// Determines the status of the countdown after the counter changes
	private void CheckCountDownStatus() {
		// Check if countdown has expired
		if (countDown == 0) {
			Menu.LeaveMenu();
		}
		else {
			// Not finished, continue count down
			StartCoroutine(CountDown());
		}
	}

	// Update the big text element that displays the countdown when leaving the lobby
	private void UpdateLobbyText(int countdown) {
		if (!LobbyText.enabled) {
			LobbyText.enabled = true;
		}
		// Show countdown
		LobbyText.text = "Game starting in  " + countdown;
	}

	// Updates "best of" match number text from options array
	private void UpdateBestOfText() {
		BestOfNumText.text = bestOfOptions[bestOfIndex];
	}

	#endregion


	#region Button Events 

	// Handle a network player signalling ready
	public void OnNetworkReady() {
		Audio.PlayMenuSound();

		// Hide "best of" selector arrows for local player
		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);

		PUNNetworkPlayer.GetLocalPlayer().SetAsReadyForMatchStart();
		OnPlayerSignalReady();
	}

	// Handle the local left-side player signalling ready
	public void OnLocalLobbyReadyLeft() {
		Audio.PlayMenuSound();

		// Set host as ready
		hostReady = true;

		// Update UI
		LeftReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
	}

	// Handle the local right-side player signalling ready
	public void OnLocalLobbyReadyRight() {
		Audio.PlayMenuSound();

		// Set client as ready
		clientReady = true;

		// Update UI
		RightReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
	}

	// Handle the left arrow button being pressed
	public void OnLeftPressed() {

		// Decrement the "best-of" index
		if (PhotonNetwork.connected) {
			ChangeBestOfIndexOnAllClients(true);
		}
		else {
			ChangeBestOfIndex(true);
		}
	}

	// Handle the right arrow button being pressed
	public void OnRightPressed() {

		// Increment the "best-of" index
		if (PhotonNetwork.connected) {
			ChangeBestOfIndexOnAllClients(false);
		}
		else {
			ChangeBestOfIndex(false);
		}
	}

	// Handle the lobby exit button being pressed
	public void OnLobbyExit() {
		Audio.PlayMenuSound();

		// Stop count down if it was in progress
		LobbyText.enabled = false;
		countingDown = false;

		// Exit the local or network lobby accordingly
		if (localLobby) {
			Menu.ExitLocalLobby();
		}
		else {
			Menu.ExitNetworkLobby();
		}
	}

	#endregion
}
