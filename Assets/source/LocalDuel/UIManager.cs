using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * This class manages all UI elements of a basic local duel
 */
[RequireComponent (typeof (BasicDuelManager))]
public class UIManager : MonoBehaviour {

	#region Editor References + Public Properties

	public Text LeftCount, RightCount, MainText;		// The text UI elements
	public Text ReactionTimer;							// The timer UI element
	public Image Flag;									// The centerpiece flag UI element

	#endregion


	#region Private Variables
	
	private BasicDuelManager manager;					// The required duel manager component
	private bool timing;								// True if the timer is active, false otherwise

	#endregion


	#region State Functions

	// Initialization
	void Start() {
		// Get required manager component
		manager = GetComponent<BasicDuelManager>();

		// Disable all text elements at beginning of round
		LeftCount.enabled = false;
		RightCount.enabled = false;
		MainText.enabled = false;
		Flag.enabled = false;

		// Set event listeners
		EventManager.GameTie += ShowTie;
		EventManager.GameWin += ShowAttack;
		EventManager.GameStrike += ShowStrike;
		EventManager.WinResult += ShowWinResult;
		EventManager.GameReset += ClearForNewRound;
		EventManager.GameOver += ShowMatchWin;
	}
	
	// Update is called once per frame
	void Update() {
		// Update the timer if its active
		if (timing) {
			UpdateTimer();
		}
	}

	#endregion


	#region Public API

	// Toggles the timer activity
	public void ToggleTimer() {
		timing = !timing;
	}

	// Toggles the display of the centerpiece flag UI element
	public void ToggleFlag() {
		Flag.enabled = !Flag.enabled;
	}

	#endregion


	#region Private API

	// Displays UI representing a valid attack
	private void ShowAttack() {
		// Hide the flag
		ToggleFlag();
	}

	// Displays UI representing a tied round
	private void ShowTie() {
		// Hide the flag
		ToggleFlag();

		// Show text element
		MainText.text = "Tie!";
		MainText.enabled = true;
	}

	// Displays UI representing a round win
	private void ShowWinResult() {
		// Refresh and display win count text elements
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;

		// Show winner text element
		string player = GetPlayerString(BasicDuelManager.RoundResult.WINLEFT);
		MainText.text = player + " wins!";
		MainText.enabled = true;
	}

	// Displays UI representing an early attack
	private void ShowStrike() {
		// Show player strike text element
		string player = GetPlayerString(BasicDuelManager.RoundResult.STRIKELEFT);
		MainText.text = player + " struck too early!";
		MainText.enabled = true;
	}

	// Displays UI representing a match win
	private void ShowMatchWin() {
		// Show match winner text element
		string player = GetPlayerString(BasicDuelManager.RoundResult.WINLEFT);
		MainText.text = player + " wins the match!";
		MainText.enabled = true;
	}

	// Clears the UI elements for a new round
	private void ClearForNewRound() {
		// Disables text elements
		LeftCount.enabled = false;
		RightCount.enabled = false;
		MainText.enabled = false;

		// Sets timer text to default
		ReactionTimer.text = "00";
	}

	// Update the win count text elements
	private void RefreshWinCounts() {
		// Set text elements to show latest win counts
		LeftCount.text = "P1: " + manager.LeftSamurai.GetWinCount();
		RightCount.text = "P2: " + manager.RightSamurai.GetWinCount();
	}

	// Updates the timer text element
	private void UpdateTimer() {
		// Get the reaction time
		int time = manager.GetReactionTime();

		// Format and set the time on the timer text element
		ReactionTimer.text = (time < 10) 
			? "0" + time.ToString() 
			: time.ToString();
	}

	// Returns the display name of the player that caused the given result
	// - result: The sided round result to check against the manager's recorded result
	private string GetPlayerString(BasicDuelManager.RoundResult result) {
		// Get the players name depending if the left or right player caused the result
		return (manager.roundResult == result) 
			? manager.LeftSamurai.GetPlayerName() 
			: manager.RightSamurai.GetPlayerName();
	}

	#endregion
}
