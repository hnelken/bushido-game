using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalLobbyManager : BaseLobbyManager {

	#region Editor References

	public Button LeftReady, RightReady;				// The left, right, and center ready button element

	#endregion


	#region Life Cycle API

	void Start() {
		this.countdown = GetComponent<CountdownManager>();
	}

	#endregion


	#region Event Blocks

	// Called when both players are ready
	protected override void OnAllPlayersReady() {
		// Hide the game settings controls
		HideInteractiveUI();

		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
	}	

	// Called when the ready status is reset
	protected override void OnClearReadyStatus() {
		// Show the ready buttons and win limit selector
		ShowInteractiveUI();
	}

	// Called when the countdown has finished and the scene is being left
	protected override void OnCountdownComplete() {
		// Leave the scene and head to the duel
		Globals.Menu.LeaveForDuelScene();
	}


	#endregion


	#region Public API

	public override void PrepareLobby() {
		bestOfIndex = 1;
		UpdateBestOfText();
		ShowInteractiveUI();
		InitializeCountdown(false);
	}

	#endregion


	#region Private API

	// Change visibility of the "best-of" selector and the ready button
	protected override void SetInteractiveUIVisible(bool visible) {
		LeftArrow.gameObject.SetActive(visible);
		RightArrow.gameObject.SetActive(visible);
		LeftReady.gameObject.SetActive(visible);
		RightReady.gameObject.SetActive(visible);
	}

	#endregion


	#region Button Events

	// Handle the local left-side player signalling ready
	public void OnLobbyReadyLeft() {
		Globals.Audio.PlayMenuSound();

		// Set host as ready
		countdown.SignalPlayerReady(true);

		// Update UI
		LeftReady.gameObject.SetActive(false);
	}

	// Handle the local right-side player signalling ready
	public void OnLobbyReadyRight() {
		Globals.Audio.PlayMenuSound();

		// Set client as ready
		countdown.SignalPlayerReady(false);

		// Update UI
		RightReady.gameObject.SetActive(false);
	}

	// Handle the left arrow button being pressed
	public void OnLeftPressed() {
		Globals.Audio.PlayMenuSound();

		// Decrement the "best-of" index
		ChangeBestOfIndex(true);
	}

	// Handle the right arrow button being pressed
	public void OnRightPressed() {
		Globals.Audio.PlayMenuSound();

		// Increment the "best-of" index
		ChangeBestOfIndex(false);
	}

	// Handle the lobby exit button being pressed
	public void OnLobbyExit() {
		Globals.Audio.PlayMenuSound();

		// Stop count down if it was in progress
		LobbyText.enabled = false;
		countdown.HaltCountdown();

		// Exit the local lobby
		Globals.Menu.ExitLocalLobby();
	}

	#endregion

}
