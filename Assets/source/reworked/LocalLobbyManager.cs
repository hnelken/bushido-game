using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalLobbyManager : MonoBehaviour {

	#region Editor References

	public Button LeftReady, RightReady;				// The left, right, and center ready button elements
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Image LeftCheckbox, RightCheckbox;			// The left and right checkbox image elements

	public Text BestOfNumText;							// The text element displaying the number of matches to be played
	public Text LobbyText;								// The text element displaying the status of the lobby

	#endregion


	#region Private Variables

	// Ready toggle variables
	private bool leftReady, rightReady;					// True if a player has signalled they are ready to begin a match, false if not

	// Countdown variables
	private bool countingDown;							// True if the countdown to match start is active, false if not
	private int countDown;								// The remaining number of seconds in the countdown

	// "Best of" variables
	private int bestOfIndex = 1;						// The index in the array of options for the win limit
	private string[] bestOfOptions =  {					// The array of options for the "best of" text
		"3", "5", "7"
	};

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
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;
	}

	#endregion


	#region Public API

	public static LocalLobbyManager Get() {
		return FindObjectOfType<LocalLobbyManager>();
	}

	public void PrepareLocalLobby() {
		bestOfIndex = 1;
		UpdateBestOfText();
		ClearReadyStatus();
	}

	#endregion


	#region Private API

	// Change the current win limit selection
	private void ChangeBestOfIndex(bool minus) {
		// Changing match parameters resets ready status
		ClearReadyStatus();

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
	private void UpdateBestOfText() {
		BestOfNumText.text = bestOfOptions[bestOfIndex];
	}

	// Update the big text element that displays the countdown when leaving the lobby
	private void UpdateLobbyText(int countdown) {
		Debug.Log("Update countdown");
		LobbyText.enabled = true;
		if (!LobbyText.enabled) {
			LobbyText.enabled = true;
		}
		// Show countdown
		LobbyText.text = "Game starting in  " + countdown;
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (leftReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (rightReady) ? CheckedBox : UncheckedBox;

		// Hide/show local lobby ready buttons
		LeftReady.gameObject.SetActive(!leftReady);
		RightReady.gameObject.SetActive(!rightReady);

		// Check if both players are ready
		CheckReadyStatus();
	}

	// Reset ready status of both players and refresh UI
	private void ClearReadyStatus() {
		// Both players are no longer ready
		leftReady = false;
		rightReady = false;

		// Best-of selector arrows become available again
		LeftArrow.gameObject.SetActive(true);
		RightArrow.gameObject.SetActive(true);

		// Update UI
		UpdateLobbyReadyStatus();
	}

	// Check for and handle both players being ready to leave the lobby
	private void CheckReadyStatus() {
		// Check if both players are ready
		if (leftReady && rightReady) {
			// Begin countdown to round begin
			OnBothPlayersReady();
		}
	}

	// Triggered when both lobby players are ready to begin a match
	private void OnBothPlayersReady() {
		// Turn off arrow buttons in local lobby
		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);

		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);

		// Begin countdown to game start
		countDown = 5;					// Set countdown to 5
		countingDown = true;			// Set countdown as active
		UpdateLobbyText(countDown);		// Update countdown UI
		StartCoroutine(CountDown());	// Begin counting
	}

	// Take a step in the countdown towards leaving the lobby
	private IEnumerator CountDown() {

		yield return new WaitForSeconds(1);

		// Check if the countdown was cancelled
		if (countingDown) {
			countDown -= 1;				// Decrement the countdown
			UpdateLobbyText(countDown);	// Update the countdown UI
			CheckCountDownStatus();		// Check if the countdown finished
		}
	}

	// Determines the status of the countdown after the counter changes
	private void CheckCountDownStatus() {
		// Check if countdown has expired
		if (countDown == 0) {
			// Countdown complete, head to game scene
			Globals.Menu.LeaveForDuelScene();
		}
		else {
			// Not finished, continue count down
			StartCoroutine(CountDown());
		}
	}

	#endregion


	#region Button Events

	// Handle the local left-side player signalling ready
	public void OnLobbyReadyLeft() {
		Globals.Audio.PlayMenuSound();

		// Set host as ready
		leftReady = true;

		// Update UI
		LeftReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
	}

	// Handle the local right-side player signalling ready
	public void OnLobbyReadyRight() {
		Globals.Audio.PlayMenuSound();

		// Set client as ready
		rightReady = true;

		// Update UI
		RightReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
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
		countingDown = false;

		// Exit the local lobby
		Globals.Menu.ExitLocalLobby();
	}

	#endregion

}
