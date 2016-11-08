using UnityEngine;
using System.Collections;

/**
 * This class manages a basic local duel lifecycle and event triggering
 */
[RequireComponent (typeof (UIManager))]
public class BasicDuelManager : MonoBehaviour {

	#region Public References

	[HideInInspector]
	public LastWinner lastWinner;
	public enum LastWinner { LEFT, RIGHT, TIE, STRIKELEFT, STRIKERIGHT }

	[HideInInspector]
	public Vector3 leftIdlePosition, rightIdlePosition;
	public Vector3 leftTiePosition, rightTiePosition;

	public Player LeftSamurai, RightSamurai;
	public int strikeLimit;
	public int winLimit;

	#endregion


	#region Private Variables

	private bool waitingForInput;
	private bool waitingForTie;
	private bool tyingInput;
	private bool flagPopped;
	private bool playerStrike;
	private float startTime;
	
	private UIManager gui;

	#endregion


	#region State Functions

	// Initialization
	void Awake() {
		gui = GetComponent<UIManager>();

		// Get initial samurai positions and set their managers
		leftIdlePosition = LeftSamurai.transform.position;
		rightIdlePosition = RightSamurai.transform.position;
		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);

		// Set event listeners
		//EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;

		// Start round after some time
		StartCoroutine(WaitAndStartRound());
	}

	#endregion


	#region Public API

	public void TriggerReaction(bool leftSamurai) {
		if (waitingForInput){
			gui.StopTimer();
			if (flagPopped) {
				flagPopped = false;
				waitingForTie = true;
				waitingForInput = false;
				StartCoroutine(WaitForTyingInput(leftSamurai));
			}
			else {
				TriggerStrike(leftSamurai);
			}
		}
		else if (waitingForTie) {
			tyingInput = true;
			waitingForTie = false;
		}
	}

	public bool WaitingForInput() {
		return waitingForInput;
	}

	#endregion
	
	private void BeginRound() {
		playerStrike = false;
		waitingForInput = true;
		StartCoroutine(WaitAndPopFlag());
	}

	private void ResetGame() {
		print ("Restarted Round");
		tyingInput = false;
		flagPopped = false;
		waitingForInput = false;
		waitingForTie = false;
		StartCoroutine(WaitAndStartRound());
	}

	private void TriggerStrike(bool leftSamurai) 
	{
		lastWinner = (leftSamurai) ? LastWinner.STRIKELEFT : LastWinner.STRIKERIGHT;
		
		playerStrike = true;
		waitingForInput = false;

		EventManager.TriggerGameStrike();

		if (leftSamurai && LeftSamurai.StrikeOut(strikeLimit)) {
			lastWinner = LastWinner.RIGHT;
			RightSamurai.WinRound();
			StartCoroutine(WaitAndShowWinner());
		}
		else if (!leftSamurai && RightSamurai.StrikeOut(strikeLimit)) {
			lastWinner = LastWinner.LEFT;
			LeftSamurai.WinRound();
			StartCoroutine(WaitAndShowWinner());
		}
		else {
			StartCoroutine(WaitAndRestartGame());
		}
	}

	private void TriggerWin(bool leftSamurai) 
	{
		lastWinner = (leftSamurai) ? LastWinner.LEFT : LastWinner.RIGHT;

		EventManager.TriggerGameWin();

		StartCoroutine(WaitAndShowWinner());
	}

	private void TriggerTie()
	{
		lastWinner = LastWinner.TIE;

		EventManager.TriggerGameTie();
		StartCoroutine(WaitAndRestartGame());
	}

	private bool MatchWon() {
		return LeftSamurai.GetWinCount() >= winLimit
			|| RightSamurai.GetWinCount() >= winLimit;
	}

	public IEnumerator WaitAndStartRound() {
		yield return new WaitForSeconds(2);

		BeginRound();
		//EventManager.TriggerGameStart();
	}

	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(3, 6);
		yield return new WaitForSeconds(randomWait);

		if (!playerStrike) {
			startTime = Time.realtimeSinceStartup;
			gui.StartTimer(startTime);
			gui.ToggleFlag();
			flagPopped = true;
		}
	}

	public IEnumerator WaitForTyingInput(bool leftSamurai) {
		yield return new WaitForSeconds(0.0001f);

		if (tyingInput) {
			TriggerTie();
		}
		else {
			TriggerWin (leftSamurai);
		}
	}
	
	public IEnumerator WaitAndShowWinner() {
		yield return new WaitForSeconds(3);
		
		EventManager.TriggerWinResult();
		StartCoroutine(WaitAndRestartGame());
	}

	public IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		if (MatchWon()) {
			EventManager.TriggerGameOver();
			StartCoroutine(WaitAndEndGame());
		}
		else {
			EventManager.TriggerGameReset();
		}
	}

	public IEnumerator WaitAndEndGame() {
		yield return new WaitForSeconds(4);

		EventManager.Nullify();
		Application.LoadLevel("Menu");
	}
}
