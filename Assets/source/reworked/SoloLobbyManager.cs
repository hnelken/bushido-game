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
	private Color[] colorOptions = {
		Color.blue, Color.yellow, Color.white, Color.red, Color.magenta, Color.green, Color.cyan
	};
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

	// Change the current win limit selection
	protected void ChangeSpeedIndex(bool minus) {
		speedIndex = ChangeIndex(speedIndex, speedOptions.Length, minus);
		UpdateSpeedText();
	}

	protected void ChangeColorIndex(bool minus) {
		colorIndex = ChangeIndex(colorIndex, colorOptions.Length, minus);
		UpdatePlayerColor();
	}

	// Change the current win limit selection
	protected int ChangeIndex(int index, int max, bool minus) {
		// Changing match parameters resets ready status
		countdown.ClearReadyStatus();

		// Increment or decrement the index with wraparound
		if (minus) {
			if (index > 0) {
				return index - 1;
			}
			else {
				return max - 1;
			}
		}
		else {
			if (index < max - 1) {
				return index + 1;
			}
			else {
				return 0;
			}
		}
	}

	// Updates player color on the lobby
	protected void UpdateSpeedText() {
		GameSpeedText.text = speedOptions[speedIndex];
	}
	
	// Updates difficulty text on the lobby
	protected void UpdatePlayerColor() {
		PlayerImage.color = colorOptions[colorIndex];
	}

	#endregion


	#region Button Events

	public void OnColorLeftPressed() {
		Globals.Audio.PlayMenuSound();
		ChangeColorIndex(true);
	}

	public void OnColorRightPressed() {
		Globals.Audio.PlayMenuSound();
		ChangeColorIndex(false);
	}

	public void OnSpeedLeftPressed() {
		Globals.Audio.PlayMenuSound();
		ChangeSpeedIndex(true);
	}

	public void OnSpeedRightPressed() {
		Globals.Audio.PlayMenuSound();
		ChangeSpeedIndex(false);
	}

	#endregion
}
