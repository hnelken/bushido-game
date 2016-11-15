using UnityEngine;
using System.Collections;

public class DuelManager : MonoBehaviour {

	#region Inspector References + Public Properties

	public NetworkRNG Generator;
	public Samurai LeftSamurai, RightSamurai;
	public bool networking;

	#endregion
	
	
	#region Private Variables

	private const int strikeLimit = 2;						// Number of strikes required to lose a round
	private const int winLimit = 3;							// Number of wins required to win the match

	private bool leftPlayerCausedResult; 					// True if the left samurai caused the latest round result
	private bool waitingForInput;							// True from when round starts until after first input
	private bool waitingForTie;								// True when waiting for tying input after first input
	private bool playerStrike;								// True if a player has committed a strike
	private bool tyingInput;								// True if there was tying input
	private bool flagPopped;								// True if the flag is showing

	private float randomWait;								// The random wait time
	private float startTime;								// The time at which the centerpiece icon was displayed
	private float reactTime;								// The time at which the first valid input was received
	private float tieTime;									// The time at which the potentially tying input was received
	
	private UIManager gui;								// The manager of all UI elements
	
	#endregion
	
	
	#region State Functions
	
	// Initialization
	void Awake() {
		// Get UI manager
		gui = GetComponent<UIManager>();

		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);
		
		// Set event listeners
		EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;
	}
	
	#endregion
	
	
	#region Public API

	public static DuelManager Get() {
		return FindObjectOfType<DuelManager>();
	}

	public void SetWaitTime(float waitTime) {
		randomWait = waitTime;
		StartCoroutine(WaitAndPopFlag());
	}

	public bool IsReadyToDuel() {
		return LeftSamurai.IsPlayerReady() && RightSamurai.IsPlayerReady();
	}

	public bool SignalPlayerReady(bool leftSamurai) {

		if (networking && !BothPlayersInMatch()) {
			return false;
		}

		if (leftSamurai) {
			Debug.Log("Left Player Ready");
			LeftSamurai.SignalPlayerReady();
		}
		else {
			Debug.Log("Right Player Ready");
			RightSamurai.SignalPlayerReady();
		}
		// Check the players checkbox
		gui.SignalPlayerReady(leftSamurai);

		if (IsReadyToDuel()) {

			// Delay the beginning of the round
			StartCoroutine(WaitAndStartRound());
		}

		return true;
	}

	// Signal a player reaction during a round
	// - leftSamurai: A boolean representing which player triggered this event
	public void TriggerReaction(bool leftSamurai, float reactionTime) {

		// Check if the input is valid
		if (waitingForInput){
			
			// Check if the player was early or not
			if (flagPopped) {
				// Stop the updating of the timer UI element
				gui.ToggleTimer();
				
				// Flag was out, reaction counts. Save reaction time.
				reactTime = GetReactionTime(reactionTime);
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
			tieTime = GetReactionTime(reactionTime);
			waitingForTie = false;
			tyingInput = true;
		}
	}

	public float GetStartTime() {
		return startTime;
	}

	// Returns whether or not input will count as a reaction
	public bool WaitingForInput() {
		return waitingForInput;
	}
	
	// Calculates the rounded-off reaction time since the flag popped
	public int GetReactionTime(float reactionTime) {
		return (int)((100 * (reactionTime - startTime)) / 2);
	}
	
	// Returns which player caused the last round result
	public bool LeftPlayerCausedResult() {
		return leftPlayerCausedResult;
	}
	
	#endregion
	
	
	#region Private API

	// Enables input and begins the randomly timed wait before popping the flag
	private void BeginRound() {
		// Delayed negation of strike status to avoid UI issues
		playerStrike = false;
		
		// Allow input and begin delayed flag display
		waitingForInput = true;

		if (networking) {
			// Get random wait time
			Generator.CmdRandomWaitTime();
		}
		else {
			StartCoroutine(WaitAndPopFlag());
		}
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
		leftPlayerCausedResult = leftSamurai;
		
		// Halt input and register early reaction
		waitingForInput = false;
		playerStrike = true;
		
		// Signal player strike to system
		EventManager.TriggerGameStrike();
		
		// Check if either player exceeds the strike limit (strikes out)
		if (LeftSamurai.StrikeOut(strikeLimit, RightSamurai)) {
			// Change the result to be a win
			// Left player struck out, right player gets a win
			leftPlayerCausedResult = false;
			
			// Show resulting winner after a delay
			StartCoroutine(WaitAndShowWinner());
		}
		else if (RightSamurai.StrikeOut(strikeLimit, LeftSamurai)) {
			// Change the result to be a win
			// Right player struck out, left player gets a win
			leftPlayerCausedResult = true;
			
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
		leftPlayerCausedResult = leftSamurai;
		
		// Signal the win to the system
		EventManager.TriggerGameWin();
		
		// Show the winner after a delay
		StartCoroutine(WaitAndShowWinner());
	}
	
	// Singal that the players tied
	private void TriggerTie()
	{
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

	// Checks if both players have entered the room
	private bool BothPlayersInMatch() {
		return FindObjectsOfType<NetworkInput>().Length == 2;
	}

	#endregion
	
	
	#region Delayed Routines
	
	// Triggers the "game start" event after 2 second
	public IEnumerator WaitAndStartRound() {

		yield return new WaitForSeconds(2);

		gui.OnBothPlayersReady();
		gui.ToggleShadeForRoundStart();

		//EventManager.TriggerGameStart();
	}
	
	// Displays the flag after a randomized wait time
	public IEnumerator WaitAndPopFlag() {

		if (!networking) {
			randomWait = Random.Range(4, 7);
		}

		yield return new WaitForSeconds(randomWait);
		
		// Only pop flag if the player has not struck early
		if (!playerStrike) {
			// No strike, record time of flag pop and start timer
			startTime = Time.realtimeSinceStartup;
			gui.ToggleTimer();
			
			// "Pop" the flag 
			gui.ToggleFlag();
			flagPopped = true;
		}
	}
	
	// Determines the result of a round with no strike after a slight delay to wait for tying input
	public IEnumerator WaitForTyingInput(bool leftSamurai) {
		yield return new WaitForSeconds(0.02f);
		
		// Check if there was a tying input of equal reaction time
		if (tyingInput && reactTime == tieTime) {
			// Players tied
			TriggerTie();
		}
		else {
			// No tie, winner is delared
			TriggerWin(leftSamurai);
		}
	}
	
	// Triggers the "show win result" event after 3 seconds
	public IEnumerator WaitAndShowWinner() {
		yield return new WaitForSeconds(3);
		
		EventManager.TriggerWinResult();
		
		// Reset for new round after some time
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
			gui.ToggleShadeForRoundEnd();
		}
	}
	
	// Leaves the duel scene after 4 seconds
	public IEnumerator WaitAndEndGame() {
		yield return new WaitForSeconds(4);

		gui.ToggleShadeForMatchEnd();
	}
	
	#endregion
}
