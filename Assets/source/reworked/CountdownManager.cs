﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : MonoBehaviour {

	#region Delegate Events

	public delegate void CountdownEvent();
	public static event CountdownEvent AllReady;						// Event for both players being ready
	public static event CountdownEvent ResetReady;						// Event for the ready status being reset
	public static event CountdownEvent LeftReady, RightReady;			// Events for each player signalling ready
	public static event CountdownEvent CountdownComplete;				// Event for the end of the countdown

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

	public static void TriggerCountdownComplete() {
		if (CountdownComplete != null) {
			CountdownComplete();
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


	#region MonoBehaviour API

	void Start() {
		photonView = GetComponent<PhotonView>();
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

		SyncReadyStatusOnAllClients();
	}

	#endregion


	#region Photon RPC's

	[PunRPC]
	void SyncReadyStatus(bool leftReady, bool rightReady) {
		this.leftReady = leftReady;
		this.rightReady = rightReady;

		// Update UI
		UpdateReadyStatus();
	}

	[PunRPC]
	void SyncCountDownText(int countDown) {
		SetCountDownText(countDown);
	}

	[PunRPC]
	void SyncLeaveLobby() {
		TriggerCountdownComplete();
	}

	#endregion


	#region Private API

	// Synchronize lobby settings for players just entering a network game
	private void SyncReadyStatusOnAllClients() {
		photonView.RPC("SyncReadyStatus", PhotonTargets.All, leftReady, rightReady);
	}

	private void SetCountDownTextOnAllClients(int countDown) {
		photonView.RPC("SyncCountDownText", PhotonTargets.All, countDown);
	}

	private void LeaveMenuOnAllClients() {
		photonView.RPC("SyncLeaveLobby", PhotonTargets.All);
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
		UpdateReadyStatus();
		TriggerResetReady();
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (leftReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (rightReady) ? CheckedBox : UncheckedBox;

		// Check if both players are ready
		CheckReadyStatus();
	}

	// Check for and handle both players being ready to leave the lobby
	private void CheckReadyStatus() {
		// Check if both players are ready
		if (leftReady && rightReady) {
			// Set the win limit in the game scene
			//BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
			TriggerAllReady();

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
