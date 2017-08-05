using UnityEngine;
using System.Collections;

public class SoloDuelManager : BaseDuelManager {

	#region Private Variables

	private Difficulty[] difficulties = new Difficulty[] {
		Difficulty.SLOW, Difficulty.NORMAL, Difficulty.FAST
	};

	private int[] timesToBeat =	new int[] {
		32, 26, 20,		// Slow reactions
		26, 20, 14,		// Normal reactions
		20, 14, 8		// Fast reactions
	};
		
	public enum Difficulty {
		SLOW = 0, 
		NORMAL = 3, 
		FAST = 6
	}

	private Difficulty currentDifficulty;
	private int currentLevel;

	#endregion


	#region Private API

	protected override void SetupMatch() {
		SetMatchDifficulty(BushidoMatchInfo.Get().SoloDifficulty);
		UpdateMatchSettings(-1);
	}

	protected void SetMatchDifficulty(int difficulty) {
		if (difficulty >= 0 && difficulty < 3) {
			currentDifficulty = difficulties[difficulty];
		}
		else {
			Debug.Log("ERROR: Invalid difficulty");
		}
	}

	protected void UpdateMatchSettings(int level) {
		currentLevel = level + 1;
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
}
