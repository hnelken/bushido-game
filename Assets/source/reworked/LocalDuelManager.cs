﻿using UnityEngine;
using System.Collections;

public class LocalDuelManager : MonoBehaviour {

	#region Public References

	public LocalSamurai LeftSamurai, RightSamurai;

	// Safe reference to the manager of all UI elements
	private LocalUIManager gui;
	public LocalUIManager GUI {
		get {
			if (!gui) {
				gui = GetComponent<LocalUIManager>();
			}
			return gui;
		}
	}

	#endregion


	#region Private Variables

	private const int strikeLimit = 2;						// Number of strikes required to lose a round
	private int winLimit = 3;								// Number of wins required to win the match

	private bool resultWasTie;
	private bool leftPlayerCausedResult; 					// True if the left samurai caused the latest round result
	private bool waitingForInput;							// True from when round starts until after first input
	private bool waitingForTie;								// True when waiting for tying input after first input
	private bool playerStrike;								// True if a player has committed a strike
	private bool tyingInput;								// True if there was tying input
	private bool flagPopped;								// True if the flag is showing
	private bool timeRanOut;								// True if the round exceeded the time limit

	private float randomWait;								// The random wait time
	private float startTime;								// The time at which the centerpiece icon was displayed

	private int reactTime;									// The time at which the first valid input was received
	private int tieTime;									// The time at which the potentially tying input was received
	private int currTime;									// The current synchronized time elapsed this round
	private int maxTime = 100;								// The time limit for a round to be called before player input

	#endregion


	#region MonoBehaviour API

	// Initialization
	void Awake() {

		// Get match limit from net manager
		//winLimit = BushidoNetManager.Get().MatchLimit;

		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);

		// Set event listeners
		EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;

		StartCoroutine(WaitAndStartRound());
	}

	void Update() {
		if (flagPopped) {
			UpdateCurrentTime();
			if (waitingForInput && !timeRanOut) {
				CheckForTimeout();
			}
		}
	}

	#endregion


	#region Public API

	// Signal a player reaction during a round
	// - leftSamurai: A boolean representing which player triggered this event
	public void TriggerReaction(bool leftSamurai, int reactionTime) {
		// Check if the input is valid
		if (waitingForInput){

			// Check if the player was early or not
			if (flagPopped) {
				// Stop the updating of the timer UI element
				GUI.StopTimer(reactionTime);

				// Hide the "!" flag
				GUI.ToggleFlag();

				// Flash a white screen
				GUI.ShowFlash();

				// Flag was out, reaction counts. Save reaction time.
				reactTime = reactionTime;
				flagPopped = false;

				// Set all further input as tying input and wait to call the round.
				waitingForInput = false;
				waitingForTie = true;

				// Compare time against samurai's current best
				RecordReactionTime(leftSamurai, reactionTime);

				StartCoroutine(WaitAndShowReaction(leftSamurai));
			}
			else {
				// The flag was not out, input is a strike.
				TriggerStrike(leftSamurai);
			}
		}
		// Check if input counts as a tie
		else if (waitingForTie) {
			// Get time of tying reaction
			tieTime = reactionTime;
			waitingForTie = false;
			tyingInput = true;

			// Compare time against samurai's current best
			RecordReactionTime(leftSamurai, reactionTime);
		}
	}

	// Returns whether or not input will count as a reaction
	public bool WaitingForInput() {
		return waitingForInput;
	}

	public bool ResultWasTie() {
		return resultWasTie;
	}

	public float GetStartTime() {
		return startTime;
	}

	public int GetCurrentTime() {
		return currTime;
	}

	// Returns which player caused the last round result
	public bool LeftPlayerCausedResult() {
		return leftPlayerCausedResult;
	}

	#endregion


	#region Private API

	private void PopFlag() {
		// No strike, record time of flag pop and start timer
		startTime = Time.realtimeSinceStartup;

		GUI.ToggleTimer();

		// "Pop" the flag 
		GUI.ToggleFlag();
		flagPopped = true;

		AudioManager.Get().PlayPopSound();
	}

	private void TriggerGameStart() {
		GUI.ToggleShadeForRoundStart();
		AudioManager.Get().StartMusic();
	}

	private void RecordReactionTime(bool leftSamurai, int time) {
		if (leftSamurai) {
			LeftSamurai.RecordReactionTime(time);
		}
		else {
			RightSamurai.RecordReactionTime(time);
		}
	}

	private void CheckForTimeout() {
		if (currTime >= maxTime) {
			AudioManager.Get().PlayStrikeSound();

			waitingForInput = false;
			timeRanOut = true;
			resultWasTie = true;

			// Stop GUI
			GUI.ToggleTimer();

			// Show round result and prepare to restart game
			EventManager.TriggerGameResult();
			StartCoroutine(WaitAndRestartGame());
		}
	}

	// Enables input and begins the randomly timed wait before popping the flag
	private void BeginRound() {
		// Delayed negation of strike status to avoid UI issues
		playerStrike = false;

		// Allow input and begin delayed flag display
		waitingForInput = true;

		StartCoroutine(WaitAndPopFlag());
	}

	// Reset the parameters of the manager before setting up for a new round
	private void ResetGame() {
		// Reset parameters
		tyingInput = false;
		flagPopped = false;
		waitingForInput = false;
		waitingForTie = false;
		resultWasTie = false;
		timeRanOut = false;
		startTime = 0;
		reactTime = 0;
		tieTime = 0;
		currTime = 0;


		// Start delayed wait before round start
		StartCoroutine(WaitAndStartRound());
	}

	// Signal that a player's input was too early
	// - leftSamurai: A boolean representing which player triggered this event
	private void TriggerStrike(bool leftSamurai) {
		// Set the round result following the strike
		leftPlayerCausedResult = leftSamurai;

		// Halt input and register early reaction
		waitingForInput = false;
		playerStrike = true;

		AudioManager.Get().PlayStrikeSound();

		// Signal player strike to system
		EventManager.TriggerGameStrike();

		// Check if either player exceeds the strike limit (strikes out)
		if (LeftSamurai.StrikeOut(strikeLimit, RightSamurai)) {
			// Change the result to be a win
			// Left player struck out, right player gets a win
			leftPlayerCausedResult = false;

			// Show resulting winner after a delay
			StartCoroutine(WaitAndShowResult(false, true));
		}
		else if (RightSamurai.StrikeOut(strikeLimit, LeftSamurai)) {
			// Change the result to be a win
			// Right player struck out, left player gets a win
			leftPlayerCausedResult = true;

			// Show resulting winner after a delay
			StartCoroutine(WaitAndShowResult(false, true));
		}
		else {
			// Neither player struck out, just reset round
			StartCoroutine(WaitAndRestartGame());
		}
	}

	private void UpdateCurrentTime() {
		currTime = GetReactionTime();
		if (!timeRanOut) {
			GUI.UpdateTimer();
		}
	}

	// Calculates the rounded-off reaction time since the flag popped
	private int GetReactionTime() {
		return (int)((100 * (Time.realtimeSinceStartup - startTime)) / 2);
	}

	// Checks if either player has enough wins to claim the match
	private bool MatchWon() {
		return LeftSamurai.GetWinCount() > winLimit / 2
			|| RightSamurai.GetWinCount() > winLimit / 2;
	}

	// Checks if both players have entered the room
	private bool BothPlayersInMatch() {
		return FindObjectsOfType<NetworkInput>().Length == 2;
	}

	#endregion


	#region Delayed Routines

	// Triggers the "game start" event after 2 second
	public IEnumerator WaitAndStartRound() {

		yield return new WaitForSeconds(2);

		TriggerGameStart();
	}

	// Displays the flag after a randomized wait time
	public IEnumerator WaitAndPopFlag() {

		randomWait = Random.Range(4, 7);

		yield return new WaitForSeconds(randomWait);

		// Only pop flag if the player has not struck early
		if (!playerStrike) {
			PopFlag();
		}
	}

	public IEnumerator WaitAndShowReaction(bool leftSamurai) {
		AudioManager.Get().PlayHitSound();

		yield return new WaitForSeconds(0.1f);

		// Signal the win to the system
		EventManager.TriggerGameReaction();

		// Show the winner after a delay
		StartCoroutine(WaitAndShowResult(leftSamurai, false));
	}

	public IEnumerator WaitAndShowResult(bool leftSamurai, bool resultWasStrike) {
		yield return new WaitForSeconds(3);

		// Check if the round was a tie
		if (!tyingInput || reactTime != tieTime) {
			resultWasTie = false;

			if (!resultWasStrike) {
				if (leftSamurai) {
					leftPlayerCausedResult = (reactTime > tieTime);
				}
				else {
					leftPlayerCausedResult = (reactTime < tieTime);
				}
			}
		}
		else {
			resultWasTie = true;
		}

		// Show round result and prepare to restart game
		EventManager.TriggerGameResult();
		StartCoroutine(WaitAndRestartGame());
	}

	// Resets for a new round after 4 seconds
	public IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		// Checks if either player has won the match after this round
		if (MatchWon()) {
			// Trigger the "match win" event
			EventManager.TriggerGameOver();

			// Leave the duel scene after a delay
			StartCoroutine(WaitAndEndGame());
		}
		else {
			// No player has won the match
			// Trigger the "reset for new round" event
			GUI.ToggleShadeForRoundEnd();
		}
	}

	// Leaves the duel scene after 4 seconds
	public IEnumerator WaitAndEndGame() {
		yield return new WaitForSeconds(4);

		// Set match results
		BushidoMatchInfo.Get().SetMatchResults(
			LeftSamurai.WinCount, RightSamurai.WinCount,
			LeftSamurai.BestTime, RightSamurai.BestTime,
			false);

		GUI.ToggleShadeForMatchEnd();
	}

	#endregion

}