using UnityEngine;
using System.Collections;

public static class EventManager {

	public delegate void GameEvent();
	public static event GameEvent GameStart, GameStrike, GameWin, GameTie, GameReset;

	public static void TriggerGameStart() {
		if (GameStart != null) {
			GameStart();
		}
	}

	public static void TriggerGameStrike() {
		if (GameStrike != null) {
			GameStrike();
		}
	}

	public static void TriggerGameWin() {
		if (GameWin != null) {
			GameWin();
		}
	}
	
	public static void TriggerGameTie() {
		if (GameTie != null) {
			GameTie();
		}
	}

	public static void TriggerGameReset() {
		if (GameReset != null) {
			GameReset();
		}
	}
}
