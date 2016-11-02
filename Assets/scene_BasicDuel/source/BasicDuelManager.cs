using UnityEngine;
using System.Collections;

[RequireComponent (typeof (UIManager))]
public class BasicDuelManager : MonoBehaviour {

	public Player LeftSamurai, RightSamurai;
	private UIManager gui;

	[HideInInspector]
	public Vector3 leftIdlePosition, rightIdlePosition;
	public Vector3 leftTiePosition, rightTiePosition;

	private bool waitingForRestart;
	private bool waitingForInput;
	private bool waitingForTie;
	private bool tyingInput;
	private bool flagPopped;
	private bool playerStrike;
	private float startTime;
	
	[HideInInspector]
	public LastWinner lastWinner;
	public enum LastWinner { LEFT, RIGHT, TIE, STRIKELEFT, STRIKERIGHT }
	
	// Use this for initialization
	void Awake () 
	{
		gui = GetComponent<UIManager>();
		leftIdlePosition = LeftSamurai.transform.position;
		rightIdlePosition = RightSamurai.transform.position;
		LeftSamurai.SetManager(this);
		RightSamurai.SetManager(this);

		EventManager.GameStart += BeginRound;
		EventManager.GameReset += ResetGame;

		StartCoroutine(WaitAndStartRound());
	}

	void Update() {
		if (flagPopped) {
			gui.UpdateTimer(startTime);
		}
	}

	public bool CanRestartRound() {
		return waitingForRestart;
	}
	
	public void TriggerReaction(bool leftSamurai) 
	{
		if (waitingForInput)
		{
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
	
	private void BeginRound() {
		waitingForInput = true;
		StartCoroutine(WaitAndPopFlag());
	}

	private void TriggerWin(bool leftSamurai) 
	{
		lastWinner = (leftSamurai) ? LastWinner.LEFT : LastWinner.RIGHT;
		EventManager.TriggerGameWin();
		StartCoroutine(WaitAndShowWinner());
	}

	private void TriggerTie()
	{
		waitingForRestart = true;
		lastWinner = LastWinner.TIE;
		EventManager.TriggerGameTie();
	}
	
	private void TriggerStrike(bool leftSamurai) 
	{
		playerStrike = true;
		waitingForRestart = true;
		EventManager.TriggerGameStrike();
	}

	private void ResetGame() {
		tyingInput = false;
		flagPopped = false;
		playerStrike = false;
		waitingForRestart = false;
		StartCoroutine(WaitAndStartRound());
	}

	public IEnumerator WaitAndStartRound() {
		yield return new WaitForSeconds(2f);

		EventManager.TriggerGameStart();
	}

	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(2, 6);
		yield return new WaitForSeconds(randomWait);

		if (!playerStrike) {
			startTime = Time.realtimeSinceStartup;
			flagPopped = true;
			gui.ToggleFlag();
		}
	}

	public IEnumerator WaitForTyingInput(bool leftSamurai) {
		yield return new WaitForSeconds(0.02f);

		if (tyingInput) {
			TriggerTie();
		}
		else {
			TriggerWin (leftSamurai);
		}
	}
	
	public IEnumerator WaitAndShowWinner() {
		yield return new WaitForSeconds(2);
		
		EventManager.TriggerWinResult();
		waitingForRestart = true;
	}
}
