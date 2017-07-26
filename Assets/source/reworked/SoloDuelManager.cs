using UnityEngine;
using System.Collections;

public class SoloDuelManager : BaseDuelManager {


	private int[] timesToBeat =	new int[] {
		32, 26, 20,		// Slow reactions
		26, 20, 14,		// Normal reactions
		20, 14, 8		// Fast reactions
	};
		
	private enum Difficulty {
		SLOW = 1, 
		NORMAL = 4, 
		FAST = 7
	}

	private Difficulty currentDifficulty;
	private int currentLevel;
	private int timeToBeat;

	// Use this for initialization
	void Start () {
		currentDifficulty = Difficulty.NORMAL;
		currentLevel = 1;
		SetTimeToBeat();
	}

	public override void TriggerReaction (bool leftSamurai, int reactionTime) {
		// Check if the input is valid
		if (waitingForInput) {

			// Check if the player was early or not
			if (flagPopped) {
				// Stop the updating of the timer UI element
				GUI.StopTimer(reactionTime);

				// Hide the "!" flag
				GUI.ToggleFlag();

				// Flash a white screen
				GUI.ShowFlash();

				// Flag was out, reaction counts. Save reaction time.
				reactTime = reactionTime;
				flagPopped = false;

				// Set all further input as tying input and wait to call the round.
				waitingForInput = false;
				waitingForTie = true;

				// Compare time against samurai's current best
				RecordReactionTime(leftSamurai, reactionTime);

				Get().StartCoroutine(WaitAndShowReaction(leftSamurai));
			}
			else {
				// The flag was not out, input is a strike.
				TriggerStrike(leftSamurai);
			}
		}
		// Check if input counts as a tie
		else if (GetCurrentTime() > timeToBeat) {//waitingForTie) {
			// Get time of tying reaction
			tieTime = reactionTime;
			waitingForTie = false;
			tyingInput = true;

			// Compare time against samurai's current best
			RecordReactionTime(leftSamurai, reactionTime);
		}
	}

	private void SetTimeToBeat() {
		Debug.Log((int)currentDifficulty);
		timeToBeat = timesToBeat[(int)currentDifficulty + currentLevel];
	}

}
