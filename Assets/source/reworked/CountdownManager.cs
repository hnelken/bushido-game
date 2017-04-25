using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : MonoBehaviour {

	#region Delegate Events

	public delegate void CountdownEvent();
	public static event CountdownEvent AllReady;						// Event for both players being ready
	public static event CountdownEvent ResetReady;						// Event for the ready status being reset
	public static event CountdownEvent LeftReady, RightReady;			// Events for each player signalling ready

	// Triggers "game start" event
	public static void TriggerAllReady() {
		if (AllReady != null) {
			AllReady();
		}
	}
		
	// Triggers "game start" event
	public static void TriggerResetReady() {
		if (ResetReady != null) {
			ResetReady();
		}
	}

	// Triggers "game start" event
	public static void TriggerLeftReady() {
		if (LeftReady != null) {
			LeftReady();
		}
	}

	// Triggers "game start" event
	public static void TriggerRightReady() {
		if (RightReady != null) {
			RightReady();
		}
	}

	#endregion


	#region Editor References

	public Image LeftCheckbox, RightCheckbox;
	public Text CountdownText;

	#endregion


	#region Private Variables

	private PhotonView photonView;
	private bool leftReady, rightReady;

	// Countdown variables
	private bool countingDown;							// True if the countdown to match start is active, false if not
	private int countDown;								// The remaining number of seconds in the countdown

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


	#region Public API

	public void SignalPlayerReady(bool left) {
		if (left) {
			leftReady = true;
			TriggerLeftReady();
		}
		else {
			rightReady = true;
			TriggerRightReady();
		}

		SyncLobbySettings();
	}

	// Synchronize lobby settings for players just entering a network game
	public void SyncLobbySettings() {
		photonView.RPC("SyncLobby", PhotonTargets.All, leftReady, rightReady);
	}

	#endregion


	#region Photon RPC's

	[PunRPC]
	void SyncLobby(bool leftReady, bool rightReady, int bestOfIndex) {
		this.leftReady = leftReady;
		this.rightReady = rightReady;

		// Update UI
	}

	[PunRPC]
	void SyncCountDownText(int countDown) {
		SetCountDownText(countDown);
	}

	[PunRPC]
	void SyncLeaveLobby() {
		Globals.Menu.LeaveMenu();
	}

	#endregion


	#region Private API

	private void SetCountDownTextOnAllClients(int countDown) {
		photonView.RPC("SyncCountDownText", PhotonTargets.All, countDown);
	}

	private void LeaveMenuOnAllClients() {
		photonView.RPC("SyncLeaveLobby", PhotonTargets.All);
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (leftReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (rightReady) ? CheckedBox : UncheckedBox;

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
				/*
				// Hide or show all lobby buttons depending on ready status of host
				NetReady.gameObject.SetActive(!hostReady);
				LeftArrow.gameObject.SetActive(!hostReady);
				RightArrow.gameObject.SetActive(!hostReady);
				*/
			}
		}
		else {
			// Hide or show all lobby buttons depending on ready status of client
			/*
			NetReady.gameObject.SetActive(!clientReady);
			LeftArrow.gameObject.SetActive(!clientReady);
			RightArrow.gameObject.SetActive(!clientReady);
			*/
		}
	}

	// Reset ready status of both players and refresh UI
	private void ClearReadyStatus() {
		// Both players are no longer ready
		leftReady = false;
		rightReady = false;

		// Clear ready status on player objects
		foreach (PUNNetworkPlayer player in PUNNetworkPlayer.GetAllPlayers()) {
			player.ClearReadyStatus();
		}

		// Update UI
		UpdateLobbyReadyStatus();
	}

	// Check for and handle both players being ready to leave the lobby
	private void CheckReadyStatus() {
		// Check if both players are ready
		if (leftReady && rightReady) {
			// Set the win limit in the game scene
			//BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);

			if (PhotonNetwork.isMasterClient) {
				// Begin countdown to game start
				countDown = 5;								// Set countdown to 5
				countingDown = true;						// Set countdown as active
				SetCountDownTextOnAllClients(countDown);	// Update countdown UI
				StartCoroutine(CountDown());				// Begin counting interval
			}
		}
	}

	// Take a step in the countdown towards leaving the lobby
	private IEnumerator CountDown() {
		yield return new WaitForSeconds(1);

		// Check if the countdown was cancelled
		if (countingDown) {
			countDown -= 1;								// Decrement the countdown
			SetCountDownTextOnAllClients(countDown);	// Update the countdown UI
			CheckCountDownStatus();						// Check if the countdown finished
		}
	}

	// Determines the status of the countdown each interval
	private void CheckCountDownStatus() {
		// Check if countdown has expired
		if (countDown == 0) {
			LeaveMenuOnAllClients();
		}
		else {
			// Not finished, continue count down
			StartCoroutine(CountDown());
		}
	}

	// Sets the text element to display the countdown
	private void SetCountDownText(int countdown) {
		if (!CountdownText.enabled) {
			CountdownText.enabled = true;
		}
		// Show countdown
		CountdownText.text = "Game starting in  " + countdown;
	}

	#endregion
}
