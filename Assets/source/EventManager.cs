using UnityEngine;
using System.Collections;

/**
 * This static class declares events for other scripts to listen for.
 */
public static class EventManager {

	#region Event Declarations

	public delegate void GameEvent();									// Event listeners must be void and take no parameters
	public static event GameEvent GameStart, WinResult, GameReset;		// These events cover the lifecycle of the duel
	public static event GameEvent GameWin, GameStrike, GameTie;			// Only one of these events occurs each round
	public static event GameEvent GameOver;								// Occurs at the end of a match when the win cap is reached
	#endregion


	#region Event Triggers

	// Triggers "game start" event
	public static void TriggerGameStart() {
		if (GameStart != null) {
			GameStart();
		}
	}
	
	// Triggers "show win result" event
	public static void TriggerWinResult() {
		if (WinResult != null) {
			WinResult();
		}
	}

	// Triggers "game reset" event
	public static void TriggerGameReset() {
		if (GameReset != null) {
			GameReset();
		}
	}

	// Triggers "game won" event
	public static void TriggerGameWin() {
		if (GameWin != null) {
			GameWin();
		}
	}

	// Triggers "game strike" event
	public static void TriggerGameStrike() {
		if (GameStrike != null) {
			GameStrike();
		}
	}
	
	// Triggers "game tied" event
	public static void TriggerGameTie() {
		if (GameTie != null) {
			GameTie();
		}
	}

	// Triggers "game over" event
	public static void TriggerGameOver() {
		if (GameOver != null) {
			GameOver();
		}
	}

	// Empties event responses for loading a new scene
	public static void Nullify() {
		GameStart = null;
		WinResult = null;
		GameReset = null;
		GameWin = null;
		GameTie = null;
		GameStrike = null;
		GameOver = null;
	}

	#endregion
}
