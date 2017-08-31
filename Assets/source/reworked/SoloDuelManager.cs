using UnityEngine;
using System.Collections;

public class SoloDuelManager : BaseDuelManager {

	#region Private Variables

	private bool firstDuelSetup = true;
	private int[] timesToBeat =	new int[] {
		32, 26, 20,		// Slow reactions
		26, 20, 14,		// Normal reactions
		20, 14, 8		// Fast reactions
	};
		
	private Globals.Difficulty currentDifficulty;
	private int currentLevel;

	#endregion


	#region Private API

	protected void ResetWinCounts() {
		LeftSamurai.ResetWinCount();
		RightSamurai.ResetWinCount();
	}

	protected override void SetupMatch() {
		if (firstDuelSetup) {
			SetMatchDifficulty(BushidoMatchInfo.Get().SoloDifficulty);
			firstDuelSetup = false;
		}
		UpdateMatchSettings();
	}

	protected void SetMatchDifficulty(int difficulty) {
		if (difficulty >= 0 && difficulty < 3) {
			currentDifficulty = Globals.Difficulties[difficulty];
			currentLevel = 0;
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
		maxTime += (int)Random.Range(-2, 1);
	}

	protected override void CheckForTimeout () {
		if (currTime >= maxTime) {

			Debug.Log("Speed: " + currentDifficulty + " - Level: " + currentLevel + " - Time elapsed: " + maxTime + " - OTime: " + (timesToBeat[(int)currentDifficulty + currentLevel]));
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
		// Checks if the player has won the match after this round
		if (MatchWon()) {
			// Check if the player has completed the final level
			Debug.Log("Diff: " + currentDifficulty + " Level: " + currentLevel);
			if (currentLevel == 2) {
				// Trigger the "match win" event
				EventManager.TriggerGameOver();

				// Leave the duel scene after a delay
				Get().StartCoroutine(WaitAndEndGame());
			}
			else {	// Final level not complete, increase difficulty
				currentLevel++;
				UpdateMatchSettings();
				ResetWinCounts();
				GUI.ToggleShadeForRoundEnd(true);
			}
		}
		else {
			// No player has won the match
			// Trigger the "reset for new round" event
			GUI.ToggleShadeForRoundEnd(false);
		}
	}

	#endregion
}
