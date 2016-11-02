using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Player : MonoBehaviour {

	public bool leftSamurai;
	public Sprite idleSprite, attackSprite, tiedSprite;
	
	private SpriteRenderer spriteRenderer;
	private BasicDuelManager manager;
	private int winCount;

	public string GetWinCount() {
		if (leftSamurai) {
			return "P1: " + winCount;
		}
		else {
			return "P2: " + winCount;
		}
	}

	public void SetManager(BasicDuelManager _manager) {
		manager = _manager;
	}

	// Use this for initialization
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

	private void PlayerStriked() {
		if ((leftSamurai && manager.lastWinner == BasicDuelManager.LastWinner.STRIKELEFT) ||
		    (!leftSamurai && manager.lastWinner == BasicDuelManager.LastWinner.STRIKERIGHT)) {
			spriteRenderer.color = Color.black;
		}
	}

	private void RoundEnded() {
		if (!WonLastRound()) {
			// Player lost
			spriteRenderer.color = Color.black;
		}
	}

	private void SetPlayerTied() {
		spriteRenderer.sprite = tiedSprite;
		SetPlayerPositions(manager.leftTiePosition, manager.rightTiePosition);
	}

	private void SetPlayerAttack() {
		spriteRenderer.sprite = attackSprite;
		SetPlayerPositions(manager.rightIdlePosition, manager.leftIdlePosition);
		
		if (WonLastRound()) {
			winCount++;
		}
	}

	private void SetPlayerIdle() {
		spriteRenderer.sprite = idleSprite;
		spriteRenderer.color = (leftSamurai) ? Color.white : Color.yellow;
		SetPlayerPositions(manager.leftIdlePosition, manager.rightIdlePosition);
	}

	private void SetPlayerPositions(Vector2 leftPlayer, Vector2 rightPlayer) {
		if (leftSamurai) {
			transform.position = leftPlayer;
		}
		else {
			transform.position = rightPlayer;
		}
	}

	private bool WonLastRound() {
		switch(manager.lastWinner) {
		case BasicDuelManager.LastWinner.LEFT:
			return leftSamurai;
		case BasicDuelManager.LastWinner.RIGHT:
			return !leftSamurai;
		}
		return false;
	}
}
