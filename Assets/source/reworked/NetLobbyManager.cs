using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetLobbyManager : BaseLobbyManager {

	#region Editor References

	public Button Exit, NetReady;						// The exit button and the ready button elements
	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements
	public Text LeftText, RightText;					// The text elements displaying the player numbers

	#endregion


	#region Private Variables

	// Component references
	private PopupManager popup;							// The popup menu for leaving a lobby and requeuing for a game
	private PhotonView photonView;						// A reference to the photon view component used to make RPC calls

	// Lobby status variables
	private List<PUNNetworkPlayer> players;				// The mutable list of players in the lobby
	private bool hostInLobby, clientInLobby;			// True if the host/client is present in the lobby, false if not

	#endregion


	#region Life Cycle API

	void Start() {
		// Set component references
		this.photonView = GetComponent<PhotonView>();
		this.popup = GetComponent<PopupManager>();
		this.countdown = GetComponent<CountdownManager>();

		// Setup PUN event listener
		Globals.MatchMaker.InitializeForNewScene();
		PUNQuickPlay.Disconnect += OnOpponentLeftLobby;

		// Initialize the mutable list of players in the lobby
		this.players = new List<PUNNetworkPlayer>();
	}

	#endregion


	#region Photon RPC's

	[PunRPC]
	void SyncSetBestOfIndex(int index) {
		// Set best of index directly and update UI
		bestOfIndex = index;
		UpdateBestOfText();
	}

	[PunRPC]
	void SyncChangeBestOfIndex(bool minus) {
		// Increment/decrement the index and update UI
		ChangeBestOfIndex(minus);
	}

	[PunRPC]
	void SyncClearReadyStatus() {
		// Reset the countdown and ready status
		countdown.ClearReadyStatus();
	}

	// Directly set the best of index on all clients and update the UI
	private void SetBestOfIndexOnAllClients(int index) {
		photonView.RPC("SyncSetBestOfIndex", PhotonTargets.AllBuffered, index);
	}

	// Increment or decrement the "best-of" index on all clients and update the UI
	private void ChangeBestOfIndexOnAllClients(bool minus) {
		photonView.RPC("SyncChangeBestOfIndex", PhotonTargets.All, minus);
	}

	// Reset the countdown and ready status for both players
	private void ClearReadyStatusOnAllClients() {
		photonView.RPC("SyncClearReadyStatus", PhotonTargets.All);
	}

	#endregion


	#region Delegate Event Blocks

	/**** COUNTDOWN EVENTS ****/

	// Called when both players are ready
	protected override void OnAllPlayersReady () {
		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
	}

	// Called when the ready status is reset
	protected override void OnClearReadyStatus() {
		if (!Globals.Menu.PlayText.enabled) {
			// Re-enable interactive UI
			SetInteractiveUIVisible(true);
		}
	}

	// Called when the countdown reaches zero
	protected override void OnCountdownComplete() {
		if (PUNNetworkPlayer.GetAllPlayers().Length == 2) {
			// Leave the lobby for the duel scene
			Globals.Menu.LeaveForDuelScene();
		}
	}


	/**** POPUP EVENTS ****/

	// Called when a player OK's their opponent leaving the game
	public void OnPlayerRequeueForGame() {
		// Reenable interactive UI
		SetUIInteractable(true);

		// Exit lobby and requeue for another game
		LeaveLobby(true);
	}

	// Called when a player acknowledges leaving the game
	public void OnPlayerExitLobby() {
		// Reenable interactive UI
		SetUIInteractable(true);

		// Exit back to main menu
		LeaveLobby(false);
	}

	// Called when a player cancels leaving the game
	public void CancelExitLobby() {
		// Reenable interactive UI
		SetUIInteractable(true);
	}

	#endregion


	#region Public API
		
	// Prepare the menu to display a network game lobby
	public void PrepareLobby(bool asHost) {
		// Initialize countdown component
		InitializeCountdown(true);

		// Initialize lobby settings if the host, client syncs automatically
		if (asHost) {
			// Initialize "best of" selector for all clients
			SetBestOfIndexOnAllClients(1);

			// Set client as not present
			clientInLobby = false;
			UpdateLobbySamurai(true);

			// Add UI to show client is not present yet
			Globals.Menu.PlayText.text = "Waiting for another player";
			Globals.Menu.PlayText.enabled = true;

			// Hide host UI until client joins
			SetInteractiveUIVisible(false);
		}
	}

	// Handles a player joining a room
	public void OnPlayerEnteredLobby(PUNNetworkPlayer player) {
		// Add the player to the list
		this.players.Add(player);

		// Check for UI changes due to player joining
		CheckForFullLobby();

		// Refresh all UI elements
		UpdateLobbySamurai(false);
	}

	// Handles a player leaving a room
	public void OnOpponentLeftLobby() {
		// Update list of players and refresh UI
		players = new List<PUNNetworkPlayer>(PUNNetworkPlayer.GetAllPlayers());
		UpdateLobbySamurai(false);

		// Prevent players from joining while we look at the popup menu
		PhotonNetwork.room.IsVisible = false;
		PhotonNetwork.room.IsOpen = false;

		// Trigger popup menu
		ShowPopup(false);
	}

	#endregion 


	#region Private API

	// Change visibility of the selector arrows and the ready button
	protected override void SetInteractiveUIVisible(bool visible) {
		LeftArrow.gameObject.SetActive(visible);
		RightArrow.gameObject.SetActive(visible);
		NetReady.gameObject.SetActive(visible);
	}

	// Change interact-ability of the selector arrows and the ready button
	private void SetUIInteractable(bool interactable) {
		LeftArrow.interactable = interactable;
		RightArrow.interactable = interactable;
		NetReady.interactable = interactable;
		Exit.interactable = interactable;
	}

	// Initialize and display the popup based on cause
	private void ShowPopup(bool manualExit) {
		// Disable lobby interactive UI
		SetUIInteractable(false);

		if (manualExit) {
			// Reset the ready status and stop countdown if in progress
			ClearReadyStatusOnAllClients();

			// Popup the notification asking if you want to leave the game
			popup.Initialize(OnPlayerExitLobby, CancelExitLobby, "You  are  leaving\nthe  game", true);
		}
		else if (!countdown.IsFinished()) {
			Debug.Log("finished: " + countdown.IsFinished());
			// Popup the notification that a player has left
			popup.Initialize(OnPlayerRequeueForGame, null, "Your  opponent\nhas  left", false);
		}
		else {
			Debug.Log("finished: " + countdown.IsFinished());
		}
	}

	// Updates the samurai image elements based on presence of players
	private void UpdateLobbySamurai(bool force) {
		if (!force) {
			hostInLobby = false;
			clientInLobby = false;
			// Get lobby info from player objects
			foreach (PUNNetworkPlayer player in players) {
				if (player.IsHost) {
					hostInLobby = true;
				}
				else {
					clientInLobby = true;
				}
			}
		}

		// Enable or disable the samurai images
		LeftSamurai.enabled = hostInLobby;
		RightSamurai.enabled = clientInLobby;

		// Set the lobby text to show presence of each player
		LeftText.enabled = hostInLobby;
		RightText.enabled = clientInLobby;
	}

	// Check for UI changes due to players entering the lobby
	private void CheckForFullLobby() {
		// Change UI depending on player being host or client
		if (PhotonNetwork.isMasterClient) {
			// Change UI for host now that a client has entered
			if (Globals.Menu.PlayText.enabled && Globals.Menu.PlayText.text == "Waiting for another player") {
				
				// Hide "waiting for player" text
				Globals.Menu.PlayText.enabled = false;
				SetInteractiveUIVisible(true);
			}
		}
		else {	// Show UI for client
			countdown.ClearReadyStatus();
		}
	}

	// Leave the lobby for the main menu or to find a different game
	private void LeaveLobby(bool playerLeft) {
		// Stop count down if it was in progress
		LobbyText.enabled = false;
		countdown.HaltCountdownOnAllClients();

		// Check the reason for leaving
		if (playerLeft) {
			// Close the lobby and look for another game
			Globals.Menu.RequeueForGame();
		}
		else {
			// Exit the lobby and go back to the main menu
			Globals.Menu.ExitNetworkLobby();
		}
	}

	#endregion


	#region Button Events 

	// Handle a network player signalling ready
	public void OnNetworkReady() {
		Globals.Audio.PlayMenuSound();

		// Hide "best of" selector arrows for local player
		SetInteractiveUIVisible(false);

		// Signal local player ready
		PUNNetworkPlayer.GetLocalPlayer().SetAsReady();
		countdown.SignalPlayerReady(PhotonNetwork.isMasterClient);
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

		// Ask player if they want to leave via popup
		ShowPopup(true);
	}

	#endregion
}