using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BaseUIManager : MonoBehaviour {

	#region Public References

	public Color blueColor, yellowColor;

	// The sprite for the checked box
	private Sprite idleSprite;
	private Sprite IdleSprite {
		get {
			if (!idleSprite) {
				idleSprite = Resources.Load<Sprite>("sprites/samurai-standing");
			}
			return idleSprite;
		}
	}

	// The sprite for the checked box
	private Sprite attackSprite;
	private Sprite AttackSprite {
		get {
			if (!attackSprite) {
				attackSprite = Resources.Load<Sprite>("sprites/samurai-slash");
			}
			return attackSprite;
		}
	}

	// The image element for the left samurai
	private Image leftSamurai;
	public Image LeftSamurai {
		get {
			if (!leftSamurai) {
				leftSamurai = GameObject.Find("LeftSamurai").GetComponent<Image>();
			}
			return leftSamurai;
		}
	}

	// The image element for the right samurai
	private Image rightSamurai;
	public Image RightSamurai {
		get {
			if (!rightSamurai) {
				rightSamurai = GameObject.Find("RightSamurai").GetComponent<Image>();
			}
			return rightSamurai;
		}
	}

	// The image element for the flag 
	private Image flag;
	public Image Flag {
		get {
			if (!flag) {
				flag = GameObject.Find("Flag").GetComponent<Image>();
			}
			return flag;
		}
	}

	// The image element for the white flash animation
	private Image flash;
	public Image Flash {
		get {
			if (!flash) {
				flash = GameObject.Find("Flash").GetComponent<Image>();
			}
			return flash;
		}
	}

	// The custom image element for the black fading animation
	private FadingShade shade;
	public FadingShade Shade {
		get {
			if (!shade) {
				shade = GameObject.Find("Shade").GetComponent<FadingShade>();
			}
			return shade;
		}
	}

	// The text element for the reaction timer
	private Text timer;
	public Text ReactionTimer {
		get {
			if (!timer) {
				timer = GameObject.Find("Timer").GetComponent<Text>();
			}
			return timer;
		}
	}

	// The text element for the central text message
	private Text mainText;
	public Text MainText {
		get {
			if (!mainText) {
				mainText = GameObject.Find("MainText").GetComponent<Text>();
			}
			return mainText;
		}
	}

	#endregion


	#region Private Variables

	private WinCountManager winCount;							// The win count UI manager
	private BaseDuelManager manager;							// The required duel manager component

	private bool timing;										// True if the timer is active, false otherwise
	private bool roundStart, roundEnd, matchEnd;				// Status variables that are true depending on the state of the duel
	private bool flashFadingOut, flashShouldFade;				// Status variables governing the animation of the white flash

	private Vector3 leftIdlePosition, rightIdlePosition;		// Default idle position of players

	#endregion

	#region MonoBehaviour API

	// Initialization
	void Start() {
		// Get required manager component
		winCount = GetComponent<WinCountManager>();

		// Set idle positions based on initial image positions
		leftIdlePosition = LeftSamurai.rectTransform.anchoredPosition;
		rightIdlePosition = RightSamurai.rectTransform.anchoredPosition;

		// Disable some UI elements for the start of the round
		MainText.enabled = false;
		Flag.enabled = false;

		// Set shade over screen and hide flash screen
		Shade.Fill();
		Flash.enabled = false;

		// Set event listeners
		EventManager.GameStart += Shade.Toggle;
		EventManager.GameReset += ClearForNewRound;
		EventManager.GameOver += ShowMatchWin;
		EventManager.GameResult += ShowResult;
		EventManager.GameStrike += ShowStrike;
		EventManager.GameReaction += ShowAttack;
	}

	// Update is called once per frame
	void Update() {

		// Handle flash animation
		if (flashFadingOut) {
			FadeFlashAlpha();
		}

		// Handle shade animation
		if (Shade.IsHidden) {
			// Do nothing
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
			SceneManager.LoadScene(Globals.NetPostScene);
		}
	}

	#endregion


	#region Public API

	public void UpdateTimer() {
		// Format and set the time on the timer text element
		int time = manager.GetCurrentTime();
		string timerText = (time < 10) 
			? "0" + time.ToString() 
			: time.ToString();
		ReactionTimer.text = timerText;
	}

	// Toggles the timer activity
	public void ToggleTimer() {
		timing = !timing;
	}

	// Stop the timer at a desired time
	public void StopTimer(int time) {
		timing = false;

		// Format time as string
		string timerText = (time < 10) 
			? "0" + time.ToString() 
			: time.ToString();
		ReactionTimer.text = timerText;
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
		Shade.Toggle();
	}

	public void ToggleShadeForRoundEnd() {
		roundEnd = true;
		Shade.Toggle();
	}

	public void ToggleShadeForMatchEnd() {
		matchEnd = true;
		Shade.Toggle();
	}

	#endregion


	#region Private API

	// Returns the display name of the player that caused the given result
	private string GetPlayerString(bool roundWon) {
		// Get the players name depending if the left or right player caused the result
		return (manager.LeftPlayerCausedResult()) 
			? manager.LeftSamurai.DisplayName 
				: manager.RightSamurai.DisplayName;
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

	// Displays UI representing a valid attack
	private void ShowAttack() {

		// Sets the players sprite and position to show the attacking state
		LeftSamurai.sprite = AttackSprite;
		RightSamurai.sprite = AttackSprite;
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

	private void ShowResult() {

		// Check result of round
		if (manager.ResultWasTie()) {
			// Change the sprite color of both players
			ShowPlayersTied();

			// Set main text element to reflect tie
			MainText.text = "Tie!";
		}
		else {
			// Change the sprite color of the player who lost the round
			ShowPlayerLoss(true);

			// Set main text element to reflect round win
			string player = GetPlayerString(true);
			MainText.text = player + " wins!";
		}

		MainText.enabled = true;
	}

	private void ShowPlayersTied() {
		LeftSamurai.color = Color.black;
		RightSamurai.color = Color.black;
		winCount.SignalTie();
	}

	// Sets the color of the player that lost to black
	private void ShowPlayerLoss(bool roundWon) {
		// Check if the round ended in a win or a strike
		if (roundWon) {
			// Round was won, change the color of the losing player to black
			winCount.SignalWin(manager.LeftPlayerCausedResult());
			LeftSamurai.color = (manager.LeftPlayerCausedResult()) ? blueColor : Color.black;
			RightSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.black : yellowColor;
		}
		else {
			// Round was not won, change color of player that struck early to black
			LeftSamurai.color = (manager.LeftPlayerCausedResult()) ? Color.black : blueColor;
			RightSamurai.color = (manager.LeftPlayerCausedResult()) ? yellowColor : Color.black;
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
		// Disable text element
		MainText.enabled = false;

		// Set player sprites and positions to show idle state
		LeftSamurai.sprite = IdleSprite;
		RightSamurai.sprite = IdleSprite;
		LeftSamurai.color = blueColor;
		RightSamurai.color = yellowColor;
		SetPlayerPositions(leftIdlePosition, rightIdlePosition);

		// Hide flag if not already hidden
		if (Flag.enabled) {
			ToggleFlag();
		}

		// Sets timer text to default
		ReactionTimer.text = "00";
	}

	#endregion
}
