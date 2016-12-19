using UnityEngine;
using System.Collections;

public class Samurai : MonoBehaviour {

	#region Inspector References + Public Properties
	
	public bool leftSamurai;					// True if this player is on the left side of the screen, false if on the right
	
	#endregion
	
	
	#region Private Variables
	
	private DuelManager manager;			// The duel manager monitoring this player
	private int winCount;						// The number of round wins this player has this match
	private int strikeCount;					// The number of strikes this player has this round
	private string displayName;					// This player's display name
	private bool readyForRound;					// True after the player has been signalled ready

	#endregion
	
	
	#region State Functions
	
	// Initialization
	void Start () {

		displayName = (leftSamurai) ? "Player 1" : "Player 2";
		
		// Set event listeners
		EventManager.GameStrike += PlayerStriked;
		EventManager.GameResult += RoundEnded;
	}
	
	#endregion
	
	
	#region Public API
	
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

	public void SignalPlayerReady() {
		readyForRound = true;
	}
	
	// Returns the display name for this player
	public string GetPlayerName() {
		return displayName;
	}

	public bool IsPlayerReady() {
		return readyForRound;
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
		return (manager.LeftPlayerCausedResult()) ? leftSamurai : !leftSamurai;
	}
	
	// Sets the players color to show the strikeout state
	private void PlayerStriked() {
		if (leftSamurai && manager.LeftPlayerCausedResult()
		    || !leftSamurai && !manager.LeftPlayerCausedResult()) {
			strikeCount++;
		}
	}
	
	// Sets the players sprite and position to show the attacking state
	private void SetPlayerAttack() {
		// Update win count if this player won this round
		if (WonLastRound()) {
			winCount++;
		}
	}
	
	// Sets the players color to show the loss state
	private void RoundEnded() {
		if (WonLastRound()) {
			winCount++;
		}
		strikeCount = 0;
	}
	
	#endregion
}
