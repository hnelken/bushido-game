using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetLobbyManager : MonoBehaviour {

	#region Editor References

	public Button Exit, NetReady;						// The exit button and the ready button elements
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements

	public Text LeftText, RightText;					// The text element displaying the status of each player's presence in lobby
	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	// Component references
	private PopupManager popup;						// The popup menu for leaving a lobby and requeuing for a game
	private PhotonView photonView;						// A reference to the photon view component used to make RPC calls
	private CountdownManager countdown;					// A reference to the countdown manager component for beginning a match

	// Lobby status variables
	private List<PUNNetworkPlayer> players;				// The mutable list of players in the lobby
	private bool hostInLobby, clientInLobby;			// True if the host/client is present in the lobby, false if not

	// "Best of" variables
	private int bestOfIndex = 1;						// The index in the array of options for the win limit
	private string[] bestOfOptions =  {					// The array of options for the "best of" text
		"3", "5", "7"
	};

	#endregion


	#region Photon Behaviour API

	// Use this for initialization
	void Start() {
		// Set references
		this.photonView = GetComponent<PhotonView>();
		this.popup = GetComponent<PopupManager>();

		// Setup countdown reference
		this.countdown = GetComponent<CountdownManager>();
		this.countdown.Initialize(LobbyText);

		// Setup countdown event blocks
		CountdownManager.AllReady += SetMatchWinLimit;
		CountdownManager.ResetReady += ShowInteractiveUI;
		CountdownManager.CountdownComplete += LeaveForDuelScene;

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


	#region Public API

	public static PUNLobbyManager Get() {
		return FindObjectOfType<PUNLobbyManager>();
	}
		
	// Prepare the lobby menu for a network lobby
	public void PrepareNetworkLobby(bool asHost) {
		// Initialize lobby settings if the host, client syncs automatically
		if (asHost) {
			// Initialize "best of" selector for all clients
			SetBestOfIndexOnAllClients(1);

			// Set both players as not ready
			countdown.ClearReadyStatus();

			// Set client as not present
			clientInLobby = false;
			UpdateLobbySamurai(true);

			// Add UI to show client is not present yet
			Globals.Menu.PlayText.text = "Waiting for another player";
			Globals.Menu.PlayText.enabled = true;

			// Hide host UI until client joins
			HideInteractiveUI();
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


	#region Delegate Event Blocks

	// Called when countdown completes 
	public void LeaveForDuelScene() {
		if (PUNNetworkPlayer.GetAllPlayers().Length == 2) {
			// Leave the lobby for the duel scene
			Globals.Menu.LeaveForDuelScene();
		}
	}

	// Called when ready status is reset
	public void ShowInteractiveUI() {
		if (!Globals.Menu.PlayText.enabled) {
			// Re-enable interactive UI
			SetInteractiveUIVisible(true);
		}
	}

	// Called when both players are ready
	public void SetMatchWinLimit() {
		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
	}
		
	public void OnPlayerRequeueForGame() {
		ToggleUIInteractivity();

		// Exit lobby and requeue for another game
		LeaveLobby(true);
	}

	public void OnPlayerExitLobby() {
		// Close popup
		HidePopup();

		// Exit back to main menu
		LeaveLobby(false);
	}

	public void CancelExitLobby() {
		// Close popup
		HidePopup();
	}

	#endregion


	#region Private API

	private void HidePopup() {
		ToggleUIInteractivity();
	}

	private void ShowPopup(bool manualExit) {
		// Disable lobby interactive UI
		ToggleUIInteractivity();

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

	private void ToggleUIInteractivity() {
		LeftArrow.interactable = !LeftArrow.interactable;
		RightArrow.interactable = !RightArrow.interactable;
		NetReady.interactable = !NetReady.interactable;
		Exit.interactable = !Exit.interactable;
	}

	// Disable use of the interactive UI
	private void HideInteractiveUI() {
		SetInteractiveUIVisible(false);
	}

	// Change visibility of the "best-of" selector and the ready button
	private void SetInteractiveUIVisible(bool visible) {
		LeftArrow.gameObject.SetActive(visible);
		RightArrow.gameObject.SetActive(visible);
		NetReady.gameObject.SetActive(visible);
	}

	// Change the current win limit selection
	private void ChangeBestOfIndex(bool minus) {
		// Changing match parameters resets ready status
		countdown.ClearReadyStatus();

		// Increment or decrement the index with wraparound
		if (minus) {
			bestOfIndex = (bestOfIndex > 0) 
				? bestOfIndex - 1 
				: bestOfOptions.Length - 1;
		}
		else {
			bestOfIndex = (bestOfIndex < bestOfOptions.Length - 1) 
				? bestOfIndex + 1 
				: 0;
		}

		// Update UI
		UpdateBestOfText();
	}

	// Updates "best of" text from index in options array
	private void UpdateBestOfText() {
		BestOfNumText.text = bestOfOptions[bestOfIndex];
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
				ShowInteractiveUI();
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
		HideInteractiveUI();

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