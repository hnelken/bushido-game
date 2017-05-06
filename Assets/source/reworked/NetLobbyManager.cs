using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetLobbyManager : MonoBehaviour {

	#region Editor References

	public Button NetReady;								// The center ready button element
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements

	public Text LeftText, RightText;					// The text element displaying the status of each player's presence in lobby
	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	// Component references
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

		// Setup countdown reference
		this.countdown = GetComponent<CountdownManager>();
		this.countdown.Initialize(LobbyText);

		// Setup countdown event blocks
		CountdownManager.AllReady += SetMatchWinLimit;
		CountdownManager.ResetReady += ShowInteractiveUI;
		CountdownManager.CountdownComplete += LeaveLobby;

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

	// Directly set the best of index on all clients and update the UI
	private void SetBestOfIndexOnAllClients(int index) {
		photonView.RPC("SyncSetBestOfIndex", PhotonTargets.AllBuffered, index);
	}

	// Increment or decrement the "best-of" index on all clients and update the UI
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
	public void OnPlayerLeftLobby() {
		players = new List<PUNNetworkPlayer>(PUNNetworkPlayer.GetAllPlayers());
		UpdateLobbySamurai(false);
		ShowOkMenu();
	}

	#endregion 


	#region Event Blocks

	// Called when countdown completes 
	public void LeaveLobby() {
		// Leave the lobby for the duel scene
		Globals.Menu.LeaveMenu();
	}

	// Called when ready status is reset
	public void ShowInteractiveUI() {
		// Re-enable interactive UI
		SetInteractiveUIVisible(true);
	}

	// Called when both players are ready
	public void SetMatchWinLimit() {
		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
	}

	#endregion


	#region Private API

	private void ShowOkMenu() {
		/* 
		 * TODO: 
		 * Pop up ok menu
		 * Toggle shade to half alpha
		 * Disable lobby interactive UI
		 * Attach "OnPlayerRequeue" function to OK button
		 */
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

	private void LeaveLobby(bool playerLeft) {
		// Stop count down if it was in progress
		LobbyText.enabled = false;
		countdown.HaltCountdown();

		if (playerLeft) {
			Globals.Menu.RequeueForGame();
		}
		else {
			// Exit the local or network lobby accordingly
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

		// Exit back to main menu
		LeaveLobby(false);
	}

	public void OnPlayerRequeueForGame() {
		Globals.Audio.PlayMenuSound();

		// Exit lobby and requeue for another game
		LeaveLobby(true);
	}

	#endregion
}