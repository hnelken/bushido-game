using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	public SamuraiControl LeftSamurai, RightSamurai;
	public GUIController UI;

	private enum LastWinner { LEFT, RIGHT }
	private LastWinner lastWinner;
	private float startTime;
	private bool waitingForInput;
	private bool flagPopped;

	private const int LOWER_WAIT_BOUND = 5;
	private const int UPPER_WAIT_BOUND = 10;
	#region State Methods

	// Use this for initialization
	void Start ()
	{
		waitingForInput = false;
		flagPopped = false;

		UI.SetController(this);

		BeginNewRound();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (flagPopped)
		{
			int time = ((int)(100 * (Time.time - startTime))/2);
			UI.UpdateTimer(time);
		}
	}

	// Used for players to check if input is valid
	public bool IsInputValid() 
	{
		return waitingForInput;
	}

	// End of overrides methods
	#endregion


	#region Public API Methods

	// Returns an instance of the game manager
	public static GameManager Get()
	{
		return GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	// Reset everything for new round 
	public void RefreshForNewRound() 
	{
		UI.ClearForNewRound();	
		Debug.Log ("Stage Cleared");
	}

	// Begin the wait-period before the flag pops
	public void BeginNewRound() 
	{
		Debug.Log ("New Round");
		waitingForInput = true;
		StartCoroutine(WaitAndPopFlag());
	}

	// Receive input from players and decide validity of timing
	public void TriggerReaction()
	{
		if (waitingForInput) 
		{
			Debug.Log ("Reaction");

			// Accept no more input
			waitingForInput = false;
			
			if (flagPopped)
			{	// Player reacted after flag popped (win)
				UI.SignalPlayerReaction(true);
				TriggerWin();
				Debug.Log ("Win");
			}
			else 
			{	// Player reacted before flag popped (strike)
				UI.SignalPlayerReaction(false);
				TriggerStrike();
				Debug.Log ("Strike");
			}
			//StartCoroutine(WaitAndShowWinner());
		}
	}

	// End of public API
	#endregion


	#region Yield Methods

	// Waits for the initial animations to complete and begins a new round
	public IEnumerator WaitAndStartRound() 
	{
		yield return new WaitForSeconds(3f);

		Debug.Log ("Beginning match");
		BeginNewRound();
	}

	// Waits a random time and pops the center flag
	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(LOWER_WAIT_BOUND, UPPER_WAIT_BOUND);
		yield return new WaitForSeconds(randomWait);

		Debug.Log ("Flag popped");
		UI.PopFlag();
		flagPopped = true;

		startTime = Time.time;
	}

	// Waits a short time after samurais cross to show winner
	public IEnumerator WaitAndShowWinner() 
	{
		yield return new WaitForSeconds(3);

		switch (lastWinner)
		{
		case LastWinner.LEFT:
			break;
		case LastWinner.RIGHT:
			break;
		}

		bool leftWin = true;
		UI.ShowWinner(leftWin);
		StartCoroutine(WaitAndRestartRound());
	}

	public IEnumerator WaitAndRestartRound()
	{
		yield return new WaitForSeconds (5);

		RestartDuel();
	}

	// End of yields
	#endregion


	#region Private Methods

	private void RestartDuel() 
	{
		UI.FadeBlackFadeOut();
	}

	// Signal that a player reacted after the flag was popped
	private void TriggerWin() 
	{
		flagPopped = false;
		
		StartCoroutine(WaitAndShowWinner());
	}

	// Signal that a player reacted preemptively
	private void TriggerStrike()
	{
		// BYPASSING STRIKE CURRENTLY
		waitingForInput = true;

		//flagPopped = false;
	}

	// End of private methods
	#endregion
}
