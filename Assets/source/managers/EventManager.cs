using UnityEngine;
using System.Collections;

/**
 * This static class declares events for other scripts to listen for.
 */
public static class EventManager {

	#region Event Declarations

	public delegate void GameEvent();								// Event listeners must be void and take no parameters
	public static event GameEvent GameStart, GameReset;				// Events for the lifecycle of the duel
	public static event GameEvent GameStrike, GameReaction;			// Events for user reactions, early or not
	public static event GameEvent GameResult, GameOver;				// Events for round or match end
	#endregion


	#region Event Triggers

	// Triggers "game start" event
	public static void TriggerGameStart() {
		if (GameStart != null) {
			GameStart();
		}
	}

	// Triggers "game reset" event
	public static void TriggerGameReset() {
		if (GameReset != null) {
			GameReset();
		}
	}

	public static void TriggerGameReaction() {
		if (GameReaction != null) {
			GameReaction();
		}
	}

	public static void TriggerGameResult() {
		if (GameResult != null) {
			GameResult();
		}
	}

	// Triggers "game strike" event
	public static void TriggerGameStrike() {
		if (GameStrike != null) {
			GameStrike();
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
		GameOver = null;
		GameStart = null;
		GameReset = null;
		GameStrike = null;
		GameReaction = null;
	}

	#endregion
}
