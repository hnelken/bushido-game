using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	private enum LastWinner { LEFT, RIGHT }
	
	private LastWinner lastWinner;

	private bool waitingForRestart;
	private bool waitingForInput;
	private bool flagPopped;
	private bool restarting;

	#region State Methods

	// Use this for initialization
	void Start () {
		waitingForRestart = false;
		waitingForInput = false;
		flagPopped = false;
		restarting = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// End of overrides methods
	#endregion


	#region Public API Methods

	// Returns an instance of the game manager
	public static GameManager Get()
	{
		return GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	// Signal game manager to refresh for new 
	public void RefreshForNewRound() 
	{

	}

	// Signal game manager that new round timer can begin
	public void BeginNewRound() {
		restarting = false;
		waitingForInput = true;
		StartCoroutine(WaitAndPopFlag());
	}

	public void TriggerReaction()
	{
		if (waitingForInput) 
		{
			// Accept no more input
			waitingForInput = false;
			
			if (flagPopped)
			{	// Player reacted after flag popped (win)
				TriggerWin();	
			}
			else 
			{	// Player reacted before flag popped (strike)
				TriggerStrike();
			}
		}
	}

	// End of public API
	#endregion


	#region Yield Methods

	// Waits for the initial animations to complete and begins a new round
	public IEnumerator WaitAndStartRound() {
		
		yield return new WaitForSeconds(5f);
		
		BeginNewRound();
	}

	// Waits a random time and pops the center flag
	public IEnumerator WaitAndPopFlag()
	{
		float randomWait = Random.Range(2, 8);
		yield return new WaitForSeconds(randomWait);

		flagPopped = true;
	}

	// Waits a short time after samurais cross to show winner
	public IEnumerator WaitAndShowWinner() 
	{
		yield return new WaitForSeconds(2);
		
		switch (lastWinner)
		{
		case LastWinner.LEFT:
			break;
		case LastWinner.RIGHT:
			break;
		}
		
		waitingForRestart = true;
	}

	// End of yields
	#endregion


	#region Private Methods

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
		//waitingForRestart = true;
	}

	// End of private methods
	#endregion
}
