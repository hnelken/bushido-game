using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour {

	#region Editor References + Public Properties

	public Sprite checkedBox, uncheckedBox;						// The sprites for the checkbox states
	public Sprite idleSprite, attackSprite, tiedSprite;			// The sprites for different player states

	public Image LeftCheckbox, RightCheckbox;					// The left and right checkbox image elements
	public Image LeftSamurai, RightSamurai;						// The image elements for the left and right samurai
	public Image Shade, Flash;									// The black and white image elements for visual effects
	public Image Flag;											// The centerpiece flag image element
	public Image BG;											// The background image element

	public Text ReadyText, P1Text, P2Text;
	public Text ReactionTimer, MainText;						// The timer and main text elements
	public Text LeftCount, RightCount;							// The win count text elements
	
	#endregion
	
	
	#region Private Variables
	
	private DuelManager manager;								// The required duel manager component

	private bool roundStart, roundEnd, matchEnd;
	private bool shadeFadingIn, shadeFadingOut;
	private bool flashFadingOut, flashShouldFade;

	private bool timing;										// True if the timer is active, false otherwise

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

		// Set shade over screen and hide flash screen
		FillShade();
		Flash.enabled = false;
		
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

		// Flash

		if (flashFadingOut) {
			FadeFlashAlpha();
		}


		// Shade

		if (shadeFadingOut) {
			FadeShadeAlpha();
		}

		if (shadeFadingIn) {
			RaiseShadeAlpha();
		}
		else if (roundStart) {
			roundStart = false;
			EventManager.TriggerGameStart();
		}
		else if (roundEnd) {
			roundEnd = false;
			EventManager.TriggerGameReset();
		}
		else if (matchEnd) {
			matchEnd = false;

			EventManager.Nullify();
			AudioManager.Get().BackToMenu();
			SceneManager.LoadScene("Menu");
		}
	}
	
	#endregion
	
	
	#region Public API

	public void SignalPlayerReady(bool leftSamurai) {
		if (leftSamurai) {
			LeftCheckbox.sprite = checkedBox;
		}
		else {
			RightCheckbox.sprite = checkedBox;
		}
	}

	public void OnBothPlayersReady() {
		LeftCheckbox.enabled = false;
		RightCheckbox.enabled = false;
		ReadyText.enabled = false;
		P1Text.enabled = false;
		P2Text.enabled = false;
	}

	private void UpdateTimer() {
		// Format and set the time on the timer text element
		int time = manager.GetCurrentTime();
		ReactionTimer.text = (time < 10)
			? "0" + time.ToString()
			: time.ToString();
	}

	// Toggles the timer activity
	public void ToggleTimer() {
		timing = !timing;
	}
	
	// Toggles the display of the centerpiece flag UI element
	public void ToggleFlag() {
		Flag.enabled = !Flag.enabled;
	}

	public void ShowFlash() {
		ToggleFlash();
	}

	public void ToggleShadeForRoundStart() {
		roundStart = true;
		ToggleShade();
	}

	public void ToggleShadeForRoundEnd() {
		roundEnd = true;
		ToggleShade();
	}

	public void ToggleShadeForMatchEnd() {
		matchEnd = true;
		ToggleShade();
	}
	
	#endregion


	#region Private API

	// Update the win count text elements
	private void RefreshWinCounts(bool leftSamurai) {
		// Set text elements to show latest win counts
		LeftCount.text = "P1\n" + manager.LeftSamurai.GetWinCount();
		RightCount.text = "P2\n" + manager.RightSamurai.GetWinCount();

		LeftCount.color = leftSamurai ? Color.blue : Color.black;
		RightCount.color = leftSamurai ? Color.black : Color.yellow;

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

	// Sets a given UI element's color to have a designated alpha value
	private void SetGraphicAlpha(Graphic graphic, float alphaValue) {
		var color = graphic.color;
		color.a = alphaValue;
		graphic.color = color;
	}

	#endregion


	#region Shade Animations

	private void ToggleShade() {
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

	private void FillShade() {
		Shade.enabled = true;
		SetGraphicAlpha(Shade, 1);
	}

	private void FadeShadeAlpha() {
		var alpha = Shade.color.a;
		if (alpha > 0) {
			SetGraphicAlpha(Shade, alpha - .03f);
		}
		else {
			SetGraphicAlpha(Shade, 0);
			Shade.enabled = false;
			shadeFadingOut = false;
		}
	}

	private void RaiseShadeAlpha() {
		var alpha = Shade.color.a;
		if (alpha < 1) {
			SetGraphicAlpha(Shade, alpha + .03f);
		}
		else {
			SetGraphicAlpha(Shade, 1);
			shadeFadingIn = false;
		}
	}
	
	#endregion


	#region Flash Animations

	private void ToggleFlash() {
		if (!Flash.enabled) {
			SetGraphicAlpha(Flash, 1);
			Flash.enabled = true;
		}
		else {
			flashFadingOut = true;
		}
	}	

	private void FadeFlashAlpha() {
		var alpha = Flash.color.a;
		if (alpha > 0) {
			SetGraphicAlpha(Flash, alpha - .05f);
		}
		else {
			SetGraphicAlpha(Flash, 0);
			Flash.enabled = false;
			flashFadingOut = false;
		}
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

		// Begin fading flash element
		ToggleFlash();
	}

	// Displays UI representing a valid attack
	private void ShowAttack() {
		// Hide the flag
		ToggleFlag();

		// Sets the players sprite and position to show the attacking state
		LeftSamurai.sprite = attackSprite;
		RightSamurai.sprite = attackSprite;
		SetPlayerPositions(rightIdlePosition, leftIdlePosition);

		// Begin fading flash element
		ToggleFlash();
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
		RefreshWinCounts(manager.LeftPlayerCausedResult());
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
}

