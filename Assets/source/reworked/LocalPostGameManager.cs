using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LocalPostGameManager : MonoBehaviour {

	#region Public References

	public GameObject RematchButton;				// The button element that begins the rematch process
	public Text LeftWinner, RightWinner;			// The text elements showing which player won the match
	public Text LeftWins, RightWins; 				// The text elements showing the number of rounds won by each player
	public Text LeftBest, RightBest;				// The text elements showing the best reaction time each player had
	public Text MainText;							// The central text element used to display the countdown

	public FadingShade Shade;						// The UI element used to fade in and out of the scene

	#endregion


	#region Private Variables

	private int countDown;							// The current time remaining in the countdown to a rematch

	private bool countingDown;						// True if the countdown to a rematch is in progress
	private bool leavingScene;						// True if the scene will change when the shade is not busy
	private bool rematching;						// True if a rematch will occur when the scene changes
	private bool exiting;							// True if returning to the menu when the scene changes

	#endregion


	#region MonoBehaviour API

	// Use this for initialization
	void Start() {
		// Initialize some UI
		Shade.Initialize();
		MainText.enabled = false;

		// Get match results from net manager
		var matchResults = BushidoMatchInfo.Get();

		// Change UI to show number of wins for each player
		int leftWins = matchResults.Results.LeftWins;
		int rightWins = matchResults.Results.RightWins;
		LeftWins.text = "" + leftWins;
		RightWins.text = "" + rightWins;

		// Show which player won the match
		if (leftWins > rightWins) {
			LeftWinner.enabled = true;
			RightWinner.enabled = false;
		}
		else {
			LeftWinner.enabled = false;
			RightWinner.enabled = true;
		}

		// Change UI to show the best reaction time of each player
		int leftBest = matchResults.Results.LeftBest;
		int rightBest = matchResults.Results.RightBest;
		LeftBest.text = (leftBest == -1) ? "xx" : "" + leftBest;
		RightBest.text = (rightBest == -1) ? "xx" : "" + rightBest;
	}

	void Update() {
		// Check if the scene should be left 
		if (ShouldLeaveScene()) {
			CheckForRematch();
			CheckForExit();
		}
	}

	#endregion


	#region Private API

	// Check if the match should be exited when the scene changes
	private void CheckForExit() {
		if (exiting) {
			// Go back to menu if not rematching
			exiting = false;
			BushidoMatchInfo.Get().EndMatch();
			AudioManager.Get().BackToMenu();
			SceneManager.LoadScene(Globals.MainMenuScene);
		}
	}

	// Check if there will be a rematch when the scene changes
	private void CheckForRematch() {
		if (rematching) {
			// Go back to local duel scene if not leaving match
			rematching = false;
			SceneManager.LoadScene(Globals.LocalDuelScene);
		}
	}

	// Triggered when a rematch has been signalled
	private void StartCountDown() {

		// Hide rematch button
		RematchButton.SetActive(false);

		// Start counting down from 5
		countDown = 5;
		countingDown = true;
		UpdateMainText(countDown);
		StartCoroutine(CountDown());
	}

	// Countdown towards beginning a rematch
	private IEnumerator CountDown() {

		yield return new WaitForSeconds(1);

		// Check if the countdown process should continue
		if (countingDown) {
			// Countdown continues
			countDown -= 1;
			UpdateMainText(countDown);
			CheckCountDownStatus();
		}
	}

	// Determines the status of the countdown after the counter changes
	private void CheckCountDownStatus() {

		// Check if countdown is over
		if (countDown == 0) {
			leavingScene = true;
			rematching = true;
			Shade.Toggle();
		}
		else {
			// Not canceled or complete, continue count down
			StartCoroutine(CountDown());
		}
	}

	// Updates the countdown text element
	private void UpdateMainText(int countdown) {
		if (!MainText.enabled) {
			MainText.enabled = true;
		}
		MainText.text = "Game starting in  " + countdown;
	}

	// Returns whether the scene should change this frame
	private bool ShouldLeaveScene() {
		return leavingScene && !Shade.IsHidden && !Shade.IsBusy;
	}

	#endregion


	#region Button Events

	// Begin countdown to rematch for local game
	public void RematchPressed() {
		StartCountDown();
	}

	// Prepare to leave the match
	public void LeaveMatchPressed() {
		leavingScene = true;
		exiting = true;
		Shade.Toggle();
	}

	#endregion
}
