using UnityEngine;
using System.Collections;

public class Samurai : MonoBehaviour {

	#region Inspector References + Public Properties
	
	public bool leftSamurai;					// True if this player is on the left side of the screen, false if on the right
	
	#endregion
	
	
	#region Private Variables
	
	private DuelManager manager;				// The duel manager monitoring this player

	private int bestTime;						// This samurai's best reaction time of the game
	private int winCount;						// The number of round wins this player has this match
	private int strikeCount;					// The number of strikes this player has in the current round

	private bool readyForRound;					// True after the player has been signalled ready
	private string displayName;					// This player's display name

	#endregion
	
	
	#region State Functions
	
	// Initialization
	void Start () {
		// Set initial best time
		bestTime = -1;

		// Set display name for each samurai
		displayName = (leftSamurai) ? "Player 1" : "Player 2";
		
		// Set event listeners
		EventManager.GameStrike += PlayerStriked;
		EventManager.GameResult += RoundEnded;
	}
	
	#endregion
	
	
	#region Public API

	public int BestTime {
		get {
			return bestTime;
		}
	}

	public int WinCount {
		get {
			return winCount;
		}
	}
	
	// Returns whether a player has struck out following the last round
	public bool StrikeOut(int strikeLimit, Samurai opponent) {
		// Player's strike count must exceed limit to strike out
		bool strikeOut = strikeCount >= strikeLimit;
		
		// Player's oppenent receives a win if a strike out occured
		if (strikeOut) {
			opponent.winCount++;
		}
		
		// Return whether strikeout occured
		return strikeOut;
	}
	
	// Sets the manager for the duel this player is in
	public void SetManager(DuelManager _manager) {
		manager = _manager;
	}

	// Compare a reaction time with the current best
	public void RecordReactionTime(int time) {
		if (bestTime == -1) {
			bestTime = time;
		}
		else {
			bestTime = (time < bestTime) ? time : bestTime;
		}
	}
	
	// Returns the display name for this player
	public string GetPlayerName() {
		return displayName;
	}

	// Returns player win count
	public int GetWinCount() {
		return winCount;
	}
	
	#endregion
	
	
	#region Private API
	
	// Checks if this player won the last complete round
	private bool WonLastRound() {
		// Player's side must match player that caused result
		if (manager.ResultWasTie()) {
			return false;
		}
		else {
			return (manager.LeftPlayerCausedResult()) ? leftSamurai : !leftSamurai;
		}
	}

	private void PlayerStriked() {
		if (leftSamurai && manager.LeftPlayerCausedResult()
		    || !leftSamurai && !manager.LeftPlayerCausedResult()) {
			strikeCount++;
		}
	}

	private void RoundEnded() {
		if (WonLastRound()) {
			winCount++;
		}
		strikeCount = 0;
	}
	
	#endregion
}
