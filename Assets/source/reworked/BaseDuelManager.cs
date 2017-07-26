using UnityEngine;
using System.Collections;

public class BaseDuelManager : MonoBehaviour {

	#region Public References

	public BaseSamurai LeftSamurai, RightSamurai;

	// Safe reference to the manager of all UI elements
	protected BaseUIManager gui;
	public BaseUIManager GUI {
		get {
			if (!gui) {
				gui = GetComponent<BaseUIManager>();
			}
			return gui;
		}
	}

	#endregion


	#region protected Variables

	protected const int strikeLimit = 2;					// Number of strikes required to lose a round
	protected int winLimit = 3;								// Number of wins required to win the match

	protected bool resultWasTie;
	protected bool leftPlayerCausedResult; 					// True if the left samurai caused the latest round result
	protected bool waitingForInput;							// True from when round starts until after first input
	protected bool waitingForTie;								// True when waiting for tying input after first input
	protected bool playerStrike;								// True if a player has committed a strike
	protected bool tyingInput;								// True if there was tying input
	protected bool flagPopped;								// True if the flag is showing
	protected bool timeRanOut;								// True if the round exceeded the time limit

	protected float randomWait;								// The random wait time
	protected float startTime;								// The time at which the centerpiece icon was displayed

	protected int reactTime;									// The time at which the first valid input was received
	protected int tieTime;									// The time at which the potentially tying input was received
	protected int currTime;									// The current synchronized time elapsed this round
	protected int maxTime = 100;							// The time limit for a round to be called before player input

	#endregion


	#region MonoBehaviour API

	// Initialization
	public void Awake() {
		SetupMatch();

		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);

		// Set event listeners
		EventManager.Nullify();
		EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;

		Get().StartCoroutine(WaitAndSetupRound());
	}

	public void Update() {
		if (flagPopped) {
			UpdateCurrentTime();
			if (waitingForInput && !timeRanOut) {
				CheckForTimeout();
			}
		}
	}

	#endregion


	#region Public API

	public static BaseDuelManager Get() {
		return FindObjectOfType<BaseDuelManager>();
	}

	// Signal a player reaction during a round
	// - leftSamurai: A boolean representing which player triggered this event
	public virtual void TriggerReaction(bool leftSamurai, int reactionTime) {
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

				Get().StartCoroutine(WaitAndShowReaction(leftSamurai));
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

	// Enables input and begins the randomly timed wait before popping the flag
	public virtual void BeginRound() {
		// Delayed negation of strike status to avoid UI issues
		playerStrike = false;

		// Allow input and begin delayed flag display
		waitingForInput = true;

		Get().StartCoroutine(WaitAndPopFlag());
	}

	// Starts the reaction timer and shows the flag image
	public virtual void PopFlag() {
		// No strike, record time of flag pop and start timer
		startTime = Time.realtimeSinceStartup;

		GUI.ToggleTimer();

		// "Pop" the flag 
		GUI.ToggleFlag();
		flagPopped = true;

		AudioManager.Get().PlayPopSound();
	}

	public virtual void TriggerGameStart() {
		GUI.ToggleShadeForRoundStart();
		AudioManager.Get().StartMusic();
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

	protected virtual void SetupMatch() {
		// Get match limit from match info
		winLimit = BushidoMatchInfo.Get().MatchLimit;
	}

	protected virtual void SetupRound() {
		Debug.Log("Starting round");
		TriggerGameStart();
	}

	protected virtual void CheckForTimeout() {
		if (currTime >= maxTime) {
			AudioManager.Get().PlayStrikeSound();

			waitingForInput = false;
			timeRanOut = true;
			resultWasTie = true;

			// Stop GUI
			GUI.ToggleTimer();

			// Show round result and prepare to restart game
			EventManager.TriggerGameResult();
			Get().StartCoroutine(WaitAndRestartGame());
		}
	}

	protected void RecordReactionTime(bool leftSamurai, int time) {
		if (leftSamurai) {
			LeftSamurai.RecordReactionTime(time);
		}
		else {
			RightSamurai.RecordReactionTime(time);
		}
	}

	// Reset the parameters of the manager before setting up for a new round
	protected void ResetGame() {
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
		Get().StartCoroutine(WaitAndSetupRound());
	}

	// Signal that a player's input was too early
	// - leftSamurai: A boolean representing which player triggered this event
	protected void TriggerStrike(bool leftSamurai) {
		// Set the round result following the strike
		leftPlayerCausedResult = leftSamurai;

		// Halt input and register early reaction
		waitingForInput = false;
		playerStrike = true;

		AudioManager.Get().PlayStrikeSound();

		// Signal player strike to system
		EventManager.TriggerGameStrike();

		// Check if either player exceeds the strike limit (strikes out)
		if (LeftSamurai.StrikeOut(strikeLimit)) {
			// Change the result to be a win
			// Left player struck out, right player gets a win
			leftPlayerCausedResult = false;

			// Show resulting winner after a delay
			Get().StartCoroutine(WaitAndShowResult(false, true));
		}
		else if (RightSamurai.StrikeOut(strikeLimit)) {
			// Change the result to be a win
			// Right player struck out, left player gets a win
			leftPlayerCausedResult = true;

			// Show resulting winner after a delay
			Get().StartCoroutine(WaitAndShowResult(false, true));
		}
		else {
			Debug.Log("Waiting to restart round after strike");
			// Neither player struck out, just reset round
			Get().StartCoroutine(WaitAndRestartGame());
		}
	}

	protected void LeaveForPostGame() {
		// Set match results
		BushidoMatchInfo.Get().SetMatchResults(
			LeftSamurai.WinCount, RightSamurai.WinCount,
			LeftSamurai.BestTime, RightSamurai.BestTime,
			true);

		GUI.ToggleShadeForMatchEnd();
	}

	protected void UpdateCurrentTime() {
		currTime = GetReactionTime();
		if (!timeRanOut) {
			GUI.UpdateTimer();
		}
	}

	// Calculates the rounded-off reaction time since the flag popped
	protected int GetReactionTime() {
		return (int)((100 * (Time.realtimeSinceStartup - startTime)) / 2);
	}

	// Checks if either player has enough wins to claim the match
	protected bool MatchWon() {
		return LeftSamurai.WinCount > winLimit / 2
			|| RightSamurai.WinCount > winLimit / 2;
	}

	#endregion


	#region Delayed Routines

	// Displays the flag after a randomized wait time
	public virtual IEnumerator WaitAndPopFlag() {

		randomWait = Random.Range(4, 7);

		yield return new WaitForSeconds(randomWait);

		// Only pop flag if the player has not struck early
		if (!playerStrike) {
			PopFlag();
		}
	}

	// Triggers the "game start" event after 2 second
	public IEnumerator WaitAndSetupRound() {

		yield return new WaitForSeconds(1);

		SetupRound();
	}

	public IEnumerator WaitAndShowReaction(bool leftSamurai) {
		AudioManager.Get().PlayHitSound();

		yield return new WaitForSeconds(0.1f);

		// Signal the win to the system
		EventManager.TriggerGameReaction();

		// Show the winner after a delay
		Get().StartCoroutine(WaitAndShowResult(leftSamurai, false));
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
		Get().StartCoroutine(WaitAndRestartGame());
	}

	// Resets for a new round after 4 seconds
	public IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		Debug.Log("Restarting round");
		// Checks if either player has won the match after this round
		if (MatchWon()) {
			// Trigger the "match win" event
			EventManager.TriggerGameOver();

			// Leave the duel scene after a delay
			Get().StartCoroutine(WaitAndEndGame());
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

		LeaveForPostGame();
	}

	#endregion

}
