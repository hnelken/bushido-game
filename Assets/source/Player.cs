using UnityEngine;
using System.Collections;

/**
 * This class manages a player entity's appearance and logistics
 */
[RequireComponent (typeof (SpriteRenderer))]
public class Player : MonoBehaviour {

	#region Editor Properties

	public bool leftSamurai;
	public Sprite idleSprite, attackSprite, tiedSprite;

	#endregion


	#region Private Variables

	private SpriteRenderer spriteRenderer;
	private BasicDuelManager manager;
	private int winCount;
	private int strikeCount;

	#endregion


	#region State Functions

	// Initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		
		// Initialize players in scene
		SetPlayerIdle();
		
		// Set event listeners
		EventManager.GameTie += SetPlayerTied;
		EventManager.GameStrike += PlayerStriked;
		EventManager.GameWin += SetPlayerAttack;
		EventManager.WinResult += RoundEnded;
		EventManager.GameReset += SetPlayerIdle;
	}

	#endregion


	#region Public API

	// Returns whether 
	public bool StrikeOut(int strikeLimit, Player opponent) {
		bool strikeOut = strikeCount >= strikeLimit;
		if (strikeOut) {
			opponent.winCount++;
		}
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
