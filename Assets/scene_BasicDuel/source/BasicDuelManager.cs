using UnityEngine;
using System.Collections;

[RequireComponent (typeof (UIManager))]
public class BasicDuelManager : MonoBehaviour {

	[HideInInspector]
	public LastWinner lastWinner;
	public enum LastWinner { LEFT, RIGHT, TIE, STRIKELEFT, STRIKERIGHT }

	[HideInInspector]
	public Vector3 leftIdlePosition, rightIdlePosition;
	public Vector3 leftTiePosition, rightTiePosition;

	public Player LeftSamurai, RightSamurai;

	private bool waitingForInput;
	private bool waitingForTie;
	private bool tyingInput;
	private bool flagPopped;
	private bool playerStrike;
	private float startTime;
	
	private UIManager gui;
	
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

	private void ResetGame() {
		print ("Restarted Round");
		tyingInput = false;
		flagPopped = false;
		playerStrike = false;
		StartCoroutine(WaitAndStartRound());
	}

	private void TriggerStrike(bool leftSamurai) 
	{
		lastWinner = (leftSamurai) ? LastWinner.STRIKELEFT : LastWinner.STRIKERIGHT;
		
		playerStrike = true;
		waitingForInput = false;

		EventManager.TriggerGameStrike();
		StartCoroutine(WaitAndRestartGame());
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

	public IEnumerator WaitAndStartRound() {
		yield return new WaitForSeconds(3);

		EventManager.TriggerGameStart();
	}

	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(3, 6);
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
		StartCoroutine(WaitAndRestartGame());
	}

	public IEnumerator WaitAndRestartGame() {
		yield return new WaitForSeconds(4);

		EventManager.TriggerGameReset();
	}
}
