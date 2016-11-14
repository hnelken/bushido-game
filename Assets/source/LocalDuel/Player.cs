using UnityEngine;
using System.Collections;

/**
 * This class manages a player entity's appearance and logistics
 */
public class Player : MonoBehaviour {

	#region Inspector References + Public Properties

	public bool leftSamurai;								// True if this player is on the left side of the screen, false if on the right

	#endregion


	#region Private Variables

	private BasicDuelManager manager;						// The duel manager monitoring this player
	private int winCount;									// The number of round wins this player has this match
	private int strikeCount;								// The number of strikes this player has this round
	private string displayName;									// This player's display name

	#endregion


	#region State Functions

	// Initialization
	void Start () {
		
		// Initialize players in scene
		SetPlayerIdle();
		displayName = (leftSamurai) ? "Player 1" : "Player 2";
		
		// Set event listeners
		EventManager.GameTie += SetPlayerTied;
		EventManager.GameWin += SetPlayerAttack;
		EventManager.GameStrike += PlayerStriked;
		EventManager.WinResult += RoundEnded;
		EventManager.GameReset += SetPlayerIdle;
	}

	#endregion


	#region Public API

	// Returns whether a player has struck out following the last round
	public bool StrikeOut(int strikeLimit, Player opponent) {
		// Player's strike count must exceed limit to strike out
		bool strikeOut = strikeCount >= strikeLimit;

		// Player's oppenent receives a win if a strike out occured
		if (strikeOut) {
			opponent.winCount++;
		}

		// Return whether strikeout occured
		return strikeOut;
	}

	// Returns a string containing player win count
	public int GetWinCount() {
		return winCount;
	}

	// Sets the manager for the duel this player is in
	public void SetManager(BasicDuelManager _manager) {
		manager = _manager;
	}

	// Returns the display name for this player
	public string GetPlayerName() {
		return displayName;
	}

	#endregion


	#region Private API

	// Checks if this player won the last complete round
	private bool WonLastRound() {
		// Player's side must match player that caused result
		if (manager.LeftPlayerCausedResult()) {
			return leftSamurai;
		}
		else {
			return !leftSamurai;
		}
	}

	// Sets the players sprite, color, and position to show the idle state
	private void SetPlayerIdle() {

	}
	
	// Sets the players color to show the strikeout state
	private void PlayerStriked() {
		if (leftSamurai && manager.LeftPlayerCausedResult()
		    || !leftSamurai && !manager.LeftPlayerCausedResult()) {
			strikeCount++;
		}
	}
	
	// Sets the players sprite and position to show the tied state
	private void SetPlayerTied() {

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
		strikeCount = 0;
	}

	#endregion
}
