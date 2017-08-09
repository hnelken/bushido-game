using UnityEngine;
using System.Collections;

public class SoloDuelManager : BaseDuelManager {

	#region Private Variables

	private int[] timesToBeat =	new int[] {
		32, 26, 20,		// Slow reactions
		26, 20, 14,		// Normal reactions
		20, 14, 8		// Fast reactions
	};
		
	private Globals.Difficulty currentDifficulty;
	private int currentLevel;

	#endregion


	#region Private API

	protected override void SetupMatch() {
		SetMatchDifficulty(BushidoMatchInfo.Get().SoloDifficulty);
		UpdateMatchSettings();
	}

	protected void SetMatchDifficulty(int difficulty) {
		if (difficulty >= 0 && difficulty < 3) {
			currentDifficulty = Globals.Difficulties[difficulty];
			currentLevel = (int)currentDifficulty;
			switch (currentDifficulty) {
			case Globals.Difficulty.SLOW:
				winLimit = 3;
				break;
			case Globals.Difficulty.NORMAL:
				winLimit = 5;
				break;
			case Globals.Difficulty.FAST:
				winLimit = 7;
				break;
			}
		}
		else {
			Debug.Log("ERROR: Invalid difficulty");
		}
	}

	protected void UpdateMatchSettings() {
		maxTime = timesToBeat[(int)currentDifficulty + currentLevel];
		maxTime += (int)Random.Range(-2, 2);
	}

	protected override void CheckForTimeout () {
		if (currTime >= maxTime) {

			// Trigger NPC reaction
			TriggerReaction(false, maxTime);
		}
	}

	#endregion


	#region Delayed Routines

	// Resets for a new round after 4 seconds
	public override IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		Debug.Log("Restarting round");
		// Checks if either player has won the match after this round
		if (MatchWon()) {
			if (currentLevel == (int)currentDifficulty + 2) {
				// Trigger the "match win" event
				EventManager.TriggerGameOver();

				// Leave the duel scene after a delay
				Get().StartCoroutine(WaitAndEndGame());
			}
			else {
				currentLevel++;
				UpdateMatchSettings();

				GUI.ToggleShadeForRoundEnd();
			}
		}
		else {
			// No player has won the match
			// Trigger the "reset for new round" event
			GUI.ToggleShadeForRoundEnd();
		}
	}

	#endregion
}
