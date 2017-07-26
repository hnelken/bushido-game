using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetDuelManager : BaseDuelManager {

	private PopupManager popup;								// The popup manager component for this scene
	private PhotonView photonView;							// The photon view used to make rpc's

	private bool leftPlayerReady, rightPlayerReady;
	private bool paused;									// True if the game is paused, false if not


	#region Photon RPC's

	[PunRPC]
	public void SyncTriggerGameStart() {
		TriggerGameStart();
	}

	[PunRPC]
	public void SyncPopFlag() {
		PopFlag();
	}

	[PunRPC]
	private void SyncSetRandomWaitTime(float waitTime) {
		SetWaitTime(waitTime);
	}

	private void TriggerGameStartOnAllClients() {
		photonView.RPC("SyncTriggerGameStart", PhotonTargets.All);
	}

	private void PopFlagOnAllClients() {
		photonView.RPC("SyncPopFlag", PhotonTargets.All);
	}

	private void SetRandomWaitTimeOnAllClients(float waitTime) {
		photonView.RPC("SyncSetRandomWaitTime", PhotonTargets.All, waitTime);
	}

	#endregion


	#region Public API

	public void SetWaitTime(float waitTime) {
		randomWait = waitTime;
		Get().StartCoroutine(WaitAndPopFlag());
	}

	public void SetStartTime(float time) {
		startTime = time;
	}

	public void SetCurrentTime(int time) {
		currTime = time;
	}

	public override void TriggerGameStart() {
		// Begin to allow input from both players
		PUNNetworkPlayer.ResetBothPlayersForNewRound();
		GUI.ToggleShadeForRoundStart();
		AudioManager.Get().StartMusic();
	}

	public override void PopFlag() {
		// No strike, record time of flag pop and start timer
		startTime = Time.realtimeSinceStartup;

		GUI.ToggleTimer();

		// "Pop" the flag 
		GUI.ToggleFlag();
		flagPopped = true;

		AudioManager.Get().PlayPopSound();
	}

	// Enables input and begins the randomly timed wait before popping the flag
	public override void BeginRound() {

		// Set local player as ready to begin the round
		PUNNetworkPlayer localPlayer = PUNNetworkPlayer.GetLocalPlayer();
		if (!localPlayer.IsReadyForRoundStart) {
			localPlayer.SetAsReadyForRoundStart();
		}

		if (ReadyToStartRound(PUNNetworkPlayer.GetAllPlayers())) {
			// Delayed negation of strike status to avoid UI issues
			playerStrike = false;

			// Allow input and begin delayed flag display
			waitingForInput = true;

			if (PhotonNetwork.isMasterClient) {
				SetRandomWaitTimeOnAllClients(Random.Range(4, 7));
			}
		}
		else {
			Get().StartCoroutine(WaitAndStartRound());
		}
	}

	public void OnPlayerLeftGameOK() {
		// Leave the duel
		LeaveForPostGame();
	}
		
	public bool IsGamePaused() {
		return paused;
	}

	#endregion


	#region Private API

	protected override void SetupMatch () {
		this.photonView = GetComponent<PhotonView>();
		this.popup = GetComponent<PopupManager>();

		// Setup PUN event listener
		Globals.MatchMaker.InitializeForNewScene();
		PUNQuickPlay.Disconnect += PauseAndShowPopup;

		// Get match limit from match info
		winLimit = BushidoMatchInfo.Get().MatchLimit;
	}

	protected override void SetupRound() {
		// Check that both players are still present at start of round
		if (PUNNetworkPlayer.GetAllPlayers().Length != 2) {
			Debug.Log("Missing players");

			// Players are missing, pause the game if not already paused
			if (!paused) {
				// Pause and show popup
				PauseAndShowPopup();
			}
		}
		else if (PhotonNetwork.isMasterClient) {
			// Synchronize round start
			TriggerGameStartOnAllClients();
		}
	}

	private bool ReadyToStartRound(PUNNetworkPlayer[] players) {
		foreach (PUNNetworkPlayer player in players) {
			if (!player.IsReadyForRoundStart) {
				return false;
			}
		}
		return true;
	}

	private void PauseAndShowPopup() {
		paused = true;
		Time.timeScale = 0;

		// Prevent players from joining while we look at the popup menu
		PhotonNetwork.room.IsVisible = false;
		PhotonNetwork.room.IsOpen = false;

		// Show popup
		popup.Initialize(OnPlayerLeftGameOK, "Your  opponent\nhas  left");
	}

	#endregion


	#region Delayed Routines

	// Displays the flag after a randomized wait time
	public override IEnumerator WaitAndPopFlag() {

		yield return new WaitForSeconds(randomWait);

		// Only pop flag if the player has not struck early
		if (!playerStrike) {
			if (PhotonNetwork.isMasterClient) {
				PopFlagOnAllClients();
			}
		}
	}

	public IEnumerator WaitAndStartRound() {

		yield return new WaitForSeconds(1);

		BeginRound();
	}

	#endregion
}
