using UnityEngine;
using System.Collections;

/**
 * This class manages a player entity's appearance and logistics
 */
[RequireComponent (typeof (SpriteRenderer))]
public class Player : MonoBehaviour {

	#region Inspector References + Public Properties

	public bool leftSamurai;								// True if this player is on the left side of the screen, false if on the right
	public Sprite idleSprite, attackSprite, tiedSprite;		// The references to the sprites for different states

	#endregion


	#region Private Variables

	private SpriteRenderer spriteRenderer;					// The required sprite renderer component
	private BasicDuelManager manager;						// The duel manager monitoring this player
	private int winCount;									// The number of round wins this player has this match
	private int strikeCount;								// The number of strikes this player has this round
	private string displayName;									// This player's display name

	#endregion


	#region State Functions

	// Initialization
	void Start () {
		// Get required sprite renderer component
		spriteRenderer = GetComponent<SpriteRenderer> ();
		
		// Initialize players in scene
		SetPlayerIdle();
		displayName = (leftSamurai) ? "Player 1" : "Player 2";
		
		// Set event listeners
		EventManager.GameTie += SetPlayerTied;
		EventManager.GameStrike += PlayerStriked;
		EventManager.GameWin += SetPlayerAttack;
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

		// Check the round result and compare with this player's side
		switch(manager.roundResult) {
		case BasicDuelManager.RoundResult.WINLEFT:
			return leftSamurai;
		case BasicDuelManager.RoundResult.WINRIGHT:
			return !leftSamurai;
		}
		return false;
	}

	// Sets this players position to one of two options depending on this player's side
	private void SetPlayerPositions(Vector2 leftPlayer, Vector2 rightPlayer) {
		if (leftSamurai) {
			transform.position = leftPlayer;
		}
		else {
			transform.position = rightPlayer;
		}
	}

	// Sets the players sprite, color, and position to show the idle state
	private void SetPlayerIdle() {
		spriteRenderer.sprite = idleSprite;
		spriteRenderer.color = (leftSamurai) ? Color.white : Color.yellow;
		SetPlayerPositions(manager.leftIdlePosition, manager.rightIdlePosition);
	}
	
	// Sets the players color to show the strikeout state
	private void PlayerStriked() {
		if ((leftSamurai && manager.roundResult == BasicDuelManager.RoundResult.STRIKELEFT) ||
		    (!leftSamurai && manager.roundResult == BasicDuelManager.RoundResult.STRIKERIGHT)) {
			spriteRenderer.color = Color.black;
			strikeCount++;
		}
	}
	
	// Sets the players sprite and position to show the tied state
	private void SetPlayerTied() {
		spriteRenderer.sprite = tiedSprite;
		SetPlayerPositions(manager.leftTiePosition, manager.rightTiePosition);
	}
	
	// Sets the players sprite and position to show the attacking state
	private void SetPlayerAttack() {
		spriteRenderer.sprite = attackSprite;
		SetPlayerPositions(manager.rightIdlePosition, manager.leftIdlePosition);

		// Update win count if this player won this round
		if (WonLastRound()) {
			winCount++;
		}
	}
	
	// Sets the players color to show the loss state
	private void RoundEnded() {
		if (!WonLastRound()) {
			// Player lost
			spriteRenderer.color = Color.black;
		}
		strikeCount = 0;
	}

	#endregion
}
