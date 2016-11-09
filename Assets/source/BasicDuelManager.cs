using UnityEngine;
using System.Collections;

/**
 * This class manages a basic local duel lifecycle and event triggering
 */
[RequireComponent (typeof (UIManager))]
public class BasicDuelManager : MonoBehaviour {

	#region Public References

	[HideInInspector]
	public RoundResult roundResult;							
	public enum RoundResult {								// Enum rep. of last round result
		WINLEFT, WINRIGHT, TIE, STRIKELEFT, STRIKERIGHT 
	}

	[HideInInspector]
	public Vector3 leftIdlePosition, rightIdlePosition;		// Default idle position of players
	public Vector3 leftTiePosition, rightTiePosition;		// Default position of sprites during a tie

	public Player LeftSamurai, RightSamurai;				// Reference to the two player entities
	public int strikeLimit;									// Number of strikes required to lose a round
	public int winLimit;									// Number of wins required to win the match

	#endregion


	#region Private Variables

	private bool waitingForInput;							// True when input will be reacted to
	private bool waitingForTie;								// True when waiting for tying input before calling the round
	private bool tyingInput;								// True if there was tying input while waiting for it
	private bool flagPopped;								// True if the centerpiece icon has been displayed
	private bool playerStrike;								// True if a player struck too early

	private float startTime;								// The time at which the centerpiece icon was displayed
	private float reactTime;								// The time at which the first valid input was received
	private float tieTime;									// The time at which the potentially tying input was received
	
	private UIManager gui;									// The manager of all non-player UI elements

	#endregion


	#region State Functions

	// Initialization
	void Awake() {
		// Get UI manager
		gui = GetComponent<UIManager>();

		// Get initial samurai positions and set their managers
		leftIdlePosition = LeftSamurai.transform.position;
		rightIdlePosition = RightSamurai.transform.position;
		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);

		// Set event listeners
		EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;

		// Delay the beginning of the round
		StartCoroutine(WaitAndStartRound());
	}

	#endregion


	#region Public API

	// Signal a player reaction during a round
	// - leftSamurai: A boolean representing which player triggered this event
	public void TriggerReaction(bool leftSamurai) {
		// Check if the input is valid
		if (waitingForInput){

			// Stop the updating of the timer UI element
			gui.StopTimer();

			// Check if the player was early or not
			if (flagPopped) {
				// Flag was out, reaction counts. Save reaction time.
				reactTime = GetReactionTime();
				flagPopped = false;
				
				// Set all further input as tying input and wait to call the round.
				waitingForInput = false;
				waitingForTie = true;
				StartCoroutine(WaitForTyingInput(leftSamurai));
			}
			else {
				// The flag was not out, input is a strike.
				TriggerStrike(leftSamurai);
			}
		}
		// Check if input counts as a tie
		else if (waitingForTie) {
			// Get time of tying reaction
			tieTime = GetReactionTime();
			waitingForTie = false;
			tyingInput = true;
		}
	}

	// Returns whether or not input will count as a reaction
	public bool WaitingForInput() {
		return waitingForInput;
	}

	// Calculates the rounded-off reaction time since the flag popped
	public int GetReactionTime() {
		return (int)(100 * (Time.realtimeSinceStartup - startTime));
	}

	#endregion


	#region Private API

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
		reactTime = 0;
		tieTime = 0;

		// Start delayed wait before round start
		StartCoroutine(WaitAndStartRound());
	}

	// Signal that a player's input was too early
	// - leftSamurai: A boolean representing which player triggered this event
	private void TriggerStrike(bool leftSamurai) 
	{
		// Set the round result following the strike
		roundResult = (leftSamurai) ? RoundResult.STRIKELEFT : RoundResult.STRIKERIGHT;

		// Halt input and register early reaction
		waitingForInput = false;
		playerStrike = true;

		// Signal player strike to system
		EventManager.TriggerGameStrike();

		// Check if either player exceeds the strike limit (strikes out)
		if (LeftSamurai.StrikeOut(strikeLimit, RightSamurai)) {
			// Change the result to be a win
			// Left player struck out, right player gets a win
			roundResult = RoundResult.WINRIGHT;

			// Show resulting winner after a delay
			StartCoroutine(WaitAndShowWinner());
		}
		else if (RightSamurai.StrikeOut(strikeLimit, LeftSamurai)) {
			// Change the result to be a win
			// Right player struck out, left player gets a win
			roundResult = RoundResult.WINLEFT;

			// Show resulting winner after a delay
			StartCoroutine(WaitAndShowWinner());
		}
		else {
			// Neither player struck out, just reset round
			StartCoroutine(WaitAndRestartGame());
		}
	}
	
	// Signal that a player's input was fastest and counts as a win
	// - leftSamurai: A boolean representing which player triggered this event
	private void TriggerWin(bool leftSamurai) 
	{
		// Set the round result following the win
		roundResult = (leftSamurai) ? RoundResult.WINLEFT : RoundResult.WINRIGHT;

		// Signal the win to the system
		EventManager.TriggerGameWin();

		// Show the winner after a delay
		StartCoroutine(WaitAndShowWinner());
	}

	// Singal that the players tied
	private void TriggerTie()
	{
		// Set the round result following the tie
		roundResult = RoundResult.TIE;

		// Signal the tie to the system
		EventManager.TriggerGameTie();

		// Reset for a new round after a delay
		StartCoroutine(WaitAndRestartGame());
	}

	// Checks if either player has enough wins to claim the match
	private bool MatchWon() {
		return LeftSamurai.GetWinCount() >= winLimit
			|| RightSamurai.GetWinCount() >= winLimit;
	}

	#endregion


	#region Delayed Routines

	public IEnumerator WaitAndStartRound() {
		yield return new WaitForSeconds(2);

		EventManager.TriggerGameStart();
	}

	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(3, 6);
		yield return new WaitForSeconds(randomWait);

		if (!playerStrike) {
			startTime = Time.realtimeSinceStartup;
			gui.StartTimer(startTime);
			gui.ToggleFlag();
			flagPopped = true;
		}
	}

	public IEnumerator WaitForTyingInput(bool leftSamurai) {
		yield return new WaitForSeconds(0.05f);

		if (tyingInput && reactTime == tieTime) {
			TriggerTie();
		}
		else {
			TriggerWin(leftSamurai);
		}
	}
	
	public IEnumerator WaitAndShowWinner() {
		yield return new WaitForSeconds(3);
		
		EventManager.TriggerWinResult();
		StartCoroutine(WaitAndRestartGame());
	}

	public IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		if (MatchWon()) {
			EventManager.TriggerGameOver();
			StartCoroutine(WaitAndEndGame());
		}
		else {
			EventManager.TriggerGameReset();
		}
	}

	public IEnumerator WaitAndEndGame() {
		yield return new WaitForSeconds(4);

		EventManager.Nullify();
		Application.LoadLevel("Menu");
	}

	#endregion
}
