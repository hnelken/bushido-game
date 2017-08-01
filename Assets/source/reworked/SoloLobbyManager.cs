using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoloLobbyManager : MonoBehaviour {

	#region Public Variables

	public Image PlayerImage;
	public Text GameSpeedText;
	public Text CountdownText;
	public GameObject SoloReadyButton;
	public GameObject LeftColorButton, RightColorButton;
	public GameObject LeftSpeedButton, RightSpeedButton;

	#endregion


	#region Private Variables

	private int colorIndex;
	private int speedIndex;
	private string[] speedOptions = {
		"SLOW", "NORMAL", "FAST"
	};
	private CountdownManager countdown;

	#endregion


	// Use this for initialization
	void Start () {
		this.countdown = GetComponent<CountdownManager>();
	}



	#region Event Blocks

	// Called when both players are ready
	protected void OnAllPlayersReady() {
		// Hide the game settings controls
		SetInteractiveUIVisible(false);

		// Set the difficulty in the match settings
		BushidoMatchInfo.Get().SetSinglePlayerSettings(this.speedIndex);
	}	

	// Called when the ready status is reset
	protected void OnClearReadyStatus() {
		// Show the ready buttons and win limit selector
		SetInteractiveUIVisible(true);
	}

	// Called when the countdown reaches zero
	protected void OnCountdownComplete() {
		// Leave the scene and head to the duel
		Globals.Menu.LeaveForDuelScene();
	}

	#endregion


	#region Public API

	public void PrepareLobby() {

	}

	#endregion


	#region Private API

	protected void InitializeCountdown() {
		//this.countdown.Initialize(CountdownText, LeftCheckbox, RightCheckbox, networked);
		countdown.ClearReadyStatus();

		// Setup countdown event blocks
		CountdownManager.AllReady += OnAllPlayersReady;
		CountdownManager.ResetReady += OnClearReadyStatus;
		CountdownManager.CountdownComplete += OnCountdownComplete;
	}

	// Change visibility of the "best-of" selector and the ready button
	protected void SetInteractiveUIVisible(bool visible) {
		//LeftArrow.gameObject.SetActive(visible);
		//RightArrow.gameObject.SetActive(visible);
	}

	#endregion
}
