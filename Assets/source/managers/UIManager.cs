using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour {

	#region Editor References + Public Properties
	
	public Sprite idleSprite, attackSprite, tiedSprite;			// The sprites for different player states

	public Image LeftSamurai, RightSamurai;						// The image elements for the left and right samurai
	public Image Shade;											// The transparent black image element for visual effects
	public Image Flag;											// The centerpiece flag image element
	public Image BG;
	
	public Text ReactionTimer, MainText;						// The timer and main text elements
	public Text LeftCount, RightCount;							// The win count text elements
	
	#endregion
	
	
	#region Private Variables
	
	private DuelManager manager;								// The required duel manager component
	private bool timing;										// True if the timer is active, false otherwise
	private bool roundEnd, matchEnd;
	private bool shadeFadingIn, shadeFadingOut;
	private bool bgIsLight, bgLighten, bgDarken;
	private Vector3 leftIdlePosition, rightIdlePosition;		// Default idle position of players
	
	private Vector3 leftTiePosition = new Vector2(-100, -50);	// Default position of left sprite during a tie
	private Vector3 rightTiePosition = new Vector2(100, -50);	// Default position of right sprite during a tie
	
	#endregion
	
	
	#region State Functions
	
	// Initialization
	void Start() {
		// Get required manager component
		manager = GetComponent<DuelManager>();

		// Set idle positions based on initial image positions
		leftIdlePosition = LeftSamurai.rectTransform.anchoredPosition;
		rightIdlePosition = RightSamurai.rectTransform.anchoredPosition;
		
		// Disable all text elements at beginning of round
		LeftCount.enabled = false;
		RightCount.enabled = false;
		MainText.enabled = false;
		Flag.enabled = false;
		
		// Set event listeners
		EventManager.GameStart += ToggleShade;
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

		if (bgDarken) {
			FadeBGColor();
		}
		if (bgLighten) {
			RaiseBGColor();
		}

		if (shadeFadingIn) {
			RaiseShadeAlpha();
		}
		else if (roundEnd) {
			roundEnd = false;
			EventManager.TriggerGameReset();
		}
		else if (matchEnd) {
			matchEnd = false;

			EventManager.Nullify();
			SceneManager.LoadScene("Menu");
		}
		if (shadeFadingOut) {
			FadeShadeAlpha();
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

	public void ToggleShadeForMatchEnd() {
		matchEnd = true;
		ToggleShade();
	}

	public void ToggleShadeForRoundEnd() {
		roundEnd = true;
		ToggleShade();
	}
	
	#endregion


	#region Private API

	// Update the win count text elements
	private void RefreshWinCounts() {
		// Set text elements to show latest win counts
		LeftCount.text = "P1: " + manager.LeftSamurai.GetWinCount();
		RightCount.text = "P2: " + manager.RightSamurai.GetWinCount();
	}

	// Updates the timer text element
	private void UpdateTimer() {
		// Get the reaction time
		int time = manager.GetReactionTime(Time.realtimeSinceStartup);

		// Format and set the time on the timer text element
		ReactionTimer.text = (time < 10) 
			? "0" + time.ToString() 
			: time.ToString();
	}

	// Returns the display name of the player that caused the given result
	private string GetPlayerString(bool roundWon) {
		// Get the players name depending if the left or right player caused the result
		return (manager.LeftPlayerCausedResult()) 
			? manager.LeftSamurai.GetPlayerName() 
				: manager.RightSamurai.GetPlayerName();
	}

	// Sets both samurai image elements to given positions
	private void SetPlayerPositions(Vector2 leftPlayer, Vector2 rightPlayer) {
		LeftSamurai.rectTransform.anchoredPosition = leftPlayer;
		RightSamurai.rectTransform.anchoredPosition = rightPlayer;
	}

	private void ToggleShade() {


		/*if (bgIsLight) {
			bgDarken = true;
			bgLighten = false;
		}
		else {
			bgLighten = true;
			bgDarken = false;
		}*/
		if (!Shade.enabled) {
			Shade.enabled = true;
			shadeFadingIn = true;
			shadeFadingOut = false;
		}
		else {
			shadeFadingOut = true;
			shadeFadingIn = false;
		}
	}	

	private void SetShadeAlpha(Color color, float alphaValue) {
		color.a = alphaValue;
		Shade.color = color;
	}

	private void SetBGColor(Color color, float rgbValue) {
		color.r = rgbValue;
		color.g = rgbValue;
		color.b = rgbValue;
		BG.color = color;
	}
		
	#endregion

	
	#region Game Event Listeners

	// Displays UI representing a tied round
	private void ShowTie() {
		// Hide the flag
		ToggleFlag();
		
		// Set samurai sprites and positions to show the tied state
		LeftSamurai.sprite = tiedSprite;
		RightSamurai.sprite = tiedSprite;
		SetPlayerPositions(leftTiePosition, rightTiePosition);
		
		// Show main text element
		MainText.text = "Tie!";
		MainText.enabled = true;
	}
	
	// Displays UI representing a valid attack
	private void ShowAttack() {
		// Hide the flag
		ToggleFlag();
		
		// Sets the players sprite and position to show the attacking state
		LeftSamurai.sprite = attackSprite;
		RightSamurai.sprite = attackSprite;
		SetPlayerPositions(rightIdlePosition, leftIdlePosition);
	}
	
	// Displays UI representing an early attack
	private void ShowStrike() {
		// Change the sprite color of the player who struck early
		ShowPlayerLoss(false);
		
		// Set the main text element to reflect early strike
		string player = GetPlayerString(false);
		MainText.text = player + " struck too early!";
		MainText.enabled = true;
	}
	
	// Displays UI representing a round win
	private void ShowWinResult() {
		// Refresh and display win count text elements
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;
		
		// Change the sprite color of the player who lost the round
		ShowPlayerLoss(true);
		
		// Set main text element to reflect round win
		string player = GetPlayerString(true);
		MainText.text = player + " wins!";
		MainText.enabled = true;
	}

	// Sets the color of the player that lost to black
	private void ShowPlayerLoss(bool roundWon) {
		// Check if the round ended in a win or a strike
		if (roundWon) {
			// Round was won, change the color of the losing player to black
			LeftSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.white : Color.black;
			RightSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.black : Color.yellow;
		}
		else {
			// Round was not won, change color of player that struck early to black
			LeftSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.black : Color.white;
			RightSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.yellow : Color.black;
		}
	}
	
	// Displays UI representing a match win
	private void ShowMatchWin() {
		// Set main text element to reflect match win
		string player = GetPlayerString(true);
		MainText.text = player + " wins the match!";
		MainText.enabled = true;
	}
	
	// Clears the UI elements for a new round
	private void ClearForNewRound() {
		// Disables text elements
		LeftCount.enabled = false;
		RightCount.enabled = false;
		MainText.enabled = false;
		
		// Set player sprites and positions to show idle state
		LeftSamurai.sprite = idleSprite;
		RightSamurai.sprite = idleSprite;
		LeftSamurai.color = Color.white;
		RightSamurai.color = Color.yellow;
		SetPlayerPositions(leftIdlePosition, rightIdlePosition);
		
		// Sets timer text to default
		ReactionTimer.text = "00";
	}

	#endregion


	#region UI Animations

	private void FadeBGColor() {
		var color = BG.color;
		if (color.r > (184 / 255)) {
			SetBGColor(color, color.r - 0.02f);
		}
		else {
			SetBGColor(color, (184 / 255));
			bgDarken = false;
			bgIsLight = false;
		}
	}

	private void RaiseBGColor() {
		var color = BG.color;
		if (color.r < 1) {
			SetBGColor(color, color.r + 0.02f);
		}
		else {
			SetBGColor(color, 1);
			bgLighten = false;
			bgIsLight = true;
		}
	}

	private void FadeShadeAlpha() {
		var color = Shade.color;
		if (color.a > 0) {
			SetShadeAlpha(color, color.a - 0.03f);
		}
		else {
			SetShadeAlpha(color, 0);
			Shade.enabled = false;
			shadeFadingOut = false;
		}
	}

	private void RaiseShadeAlpha() {
		var color = Shade.color;
		var limit = (matchEnd || roundEnd) ? 1 : 0.5f;
		if (color.a < limit) {
			SetShadeAlpha(color, color.a + 0.03f);
		}
		else {
			SetShadeAlpha(color, limit);
			shadeFadingIn = false;
		}
	}
	
	#endregion
}
