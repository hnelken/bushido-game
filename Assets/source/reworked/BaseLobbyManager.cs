using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class BaseLobbyManager : MonoBehaviour {

	#region Public References

	public Image LeftCheckbox, RightCheckbox;			// The left and right checkbox image elements
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements
	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	// "Best of" variables
	protected int bestOfIndex = 1;						// The index in the array of options for the win limit
	protected string[] bestOfOptions =  {					// The array of options for the "best of" text
		"3", "5", "7"
	};

	protected CountdownManager countdown;					// A reference to the countdown manager component for beginning a match

	#endregion


	#region Event Blocks

	protected abstract void OnAllPlayersReady();
	protected abstract void OnClearReadyStatus();
	protected abstract void OnCountdownComplete();

	#endregion


	#region Private API

	// Change visibility of the "best-of" selector and the ready button
	protected abstract void SetInteractiveUIVisible(bool visible);
		
	// Initialize the countdown component with event listeners
	protected void InitializeCountdown(bool networked) {
		this.countdown.Initialize(LobbyText, LeftCheckbox, RightCheckbox, networked);
		countdown.ClearReadyStatus();

		// Setup countdown event blocks
		CountdownManager.AllReady += OnAllPlayersReady;
		CountdownManager.ResetReady += OnClearReadyStatus;
		CountdownManager.CountdownComplete += OnCountdownComplete;
	}

	// Change the current win limit selection
	protected void ChangeBestOfIndex(bool minus) {
		// Changing match parameters resets ready status
		countdown.ClearReadyStatus();

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

	// Updates "best of" match number text from options array
	protected void UpdateBestOfText() {
		BestOfNumText.text = bestOfOptions[bestOfIndex];
	}

	#endregion
}
