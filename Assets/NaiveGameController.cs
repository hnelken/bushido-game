using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NaiveGameController : MonoBehaviour {

	public SamuraiControl LeftSamurai, RightSamurai;
	public GameObject CenterPiece;
	public GUIController gui;

	public Text LeftCount, RightCount, WinText;
	private enum LastWinner { LEFT, RIGHT }

	private Vector3 leftPosition, rightPosition;
	private bool waitingForRestart;
	private bool waitingForInput;
	private bool flagPopped;
	private bool restarting;

	private LastWinner lastWinner;

	// Use this for initialization
	void Start () 
	{
		leftPosition = LeftSamurai.transform.position;
		rightPosition = RightSamurai.transform.position;
		
		LeftSamurai.GetComponent<SpriteRenderer>().color = Color.white;
		RightSamurai.GetComponent<SpriteRenderer>().color = Color.yellow;

		waitingForRestart = false;
		waitingForInput = false;
		flagPopped = false;
		restarting = false;

		CenterPiece.SetActive (false);

		LeftSamurai.SetController(this);
		RightSamurai.SetController(this);
		gui.SetController(this);

		WinText.enabled = false;
		LeftCount.enabled = false;
		RightCount.enabled = false;

		StartCoroutine(WaitAndStartRound ());
	}
	
	// Update is called once per frame
	void Update () 
	{
		// If restarting, space triggers fade to black.
		if (waitingForRestart && Input.GetKeyDown(KeyCode.Space)) 
		{
			waitingForRestart = false;
			restarting = true;
			gui.FadeBlackFadeOut();
		}
		// Space otherwise triggers beginning of timer for the central signal
		else if (!restarting && !waitingForInput && Input.GetKeyDown(KeyCode.Space))
		{
			waitingForInput = true;
			StartCoroutine(WaitAndPopFlag());
		}
	}

	public void SignalBlackFaded() {
		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;

		ResetPlayerPositions();
	}

	public void SignalNextRoundReady() {
		restarting = false;
		waitingForInput = true;
		StartCoroutine(WaitAndPopFlag());
	}

	public IEnumerator WaitAndStartRound() {
		
		yield return new WaitForSeconds(3f);

		SignalNextRoundReady();
	}

	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(2, 8);
		yield return new WaitForSeconds(randomWait);
		
		CenterPiece.SetActive(true);
		flagPopped = true;
	}

	public IEnumerator WaitAndShowWinner() {
		yield return new WaitForSeconds(2);

		switch (lastWinner) {
		case LastWinner.LEFT:
			RightSamurai.GetComponent<SpriteRenderer>().color = Color.black;
			LeftSamurai.addWin();
			WinText.text = "BLUE SAMURAI WINS";
			break;
		case LastWinner.RIGHT:
			LeftSamurai.GetComponent<SpriteRenderer>().color = Color.black;
			RightSamurai.addWin();
			WinText.text = "YELLOW SAMURAI WINS";
			break;
		}

		LeftCount.text = LeftSamurai.getWinCount();
		RightCount.text = RightSamurai.getWinCount();

		LeftCount.enabled = true;
		RightCount.enabled = true;
		WinText.enabled = true;

		waitingForRestart = true;
	}

	public void TriggerReaction(bool leftSamurai) 
	{
		if (waitingForInput) 
		{
			// Accept no more input
			waitingForInput = false;

			if (flagPopped)
			{	// Player reacted after flag popped (win)
				TriggerWin(leftSamurai);
			}
			else 
			{	// Player reacted before flag popped (strike)
				//TriggerStrike(leftSamurai);
				waitingForInput = true;
			}
		}
	}

	private void TriggerWin(bool leftSamurai) 
	{
		flagPopped = false;
		CenterPiece.SetActive(false);
		lastWinner = (leftSamurai) ? LastWinner.LEFT : LastWinner.RIGHT;

		// Trigger gui cues
		gui.FlashWhiteFadeOut();

		SwapPlayerPositions();

		StartCoroutine(WaitAndShowWinner());
	}

	private void TriggerStrike(bool leftSamurai) 
	{
		flagPopped = false;
		CenterPiece.SetActive(false);
		waitingForRestart = true;
	}

	private void SwapPlayerPositions() {
		LeftSamurai.ToggleSprite();
		RightSamurai.ToggleSprite();

		LeftSamurai.transform.position = rightPosition;
		RightSamurai.transform.position = leftPosition;
	}

	private void ResetPlayerPositions() {
		LeftSamurai.ToggleSprite();
		RightSamurai.ToggleSprite();

		LeftSamurai.GetComponent<SpriteRenderer>().color = Color.white;
		RightSamurai.GetComponent<SpriteRenderer>().color = Color.yellow;

		LeftSamurai.transform.position = leftPosition;
		RightSamurai.transform.position = rightPosition;
	}
}
