using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/*
 * This class is the manager for the main menu of the game
 */
public class MenuManager : MonoBehaviour {
	
	#region Editor References

	public Image Shade;											// The black image used for fading in and out of scenes
	public Text PlayText, TitleText;							// The opening screen text elements
	public GameObject MultiPlayMenu, NetworkMenu, LobbyMenu;	// The parent objects for the menu segments

	public LobbyManager Lobby;									// Reference to the lobby manager object

	#endregion

	// Safe reference to the source of all game audio
	public AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}

	// Safe reference to the match maker for net games
	public BushidoMatchMaker MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = GetComponent<BushidoMatchMaker>();
			}
			return matchMaker;
		}
	}


	#region Private Variables

	private AudioManager audioManager;				// Unsafe reference to audio source
	private BushidoMatchMaker matchMaker;			// Unsafe reference to the match maker

	private bool shadeFadingIn, shadeFadingOut;		// Status of fade-in/fade-out animations
	private bool playTextFading;					// Status of "tap-to-play" text fading animation
	private bool cancelSceneChange;					// True if a match-start-countdown was canceled
	private bool openAnimsDone;						// True if the opening title descent has finished
	private bool localMenuOpen;						// True if a local-play menu is open, false if a net-play menu is open
	private bool leavingMenu;						// True if a game is about to begin and the menu scene must be left
	private bool pastOpenMenu;						// True if the opening menu is no longer showing

	private int countDown;							// The remaining seconds in the countdown to leave the menu scene
	private int titleHeight;						// The pixel height of the title text object for animation purposes
	private string nextSceneName;					// The string name of the scene the menu should transition to next

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {
		// Connect to photon network
		PhotonNetwork.ConnectUsingSettings("1");

		// Record the height of the title text object at runtime
		titleHeight = ((int)-TitleText.preferredHeight * 3) / 4;

		// Set the alpha of UI elements for opening
		SetPlayTextAlpha(PlayText.color, 0);
		SetShadeAlpha(Shade.color, 1);
		Shade.enabled = true;

		// Set status of UI animations
		shadeFadingOut = true;
		playTextFading = true;
	}
	
	// Update is called once per frame
	void Update () {

		// Check if the user wants to skip the opening animations
		if (!pastOpenMenu) {
			CheckForInput();
		}

		// Manage opening and leaving animations
		ManageOpeningAnimations();
		ManageLeavingAnimations();

	}

	#endregion


	#region Public API

	// Used to gain a reliable reference to this manager
	public static MenuManager Get() {
		return FindObjectOfType<MenuManager>();
	}

	// Triggered when both lobby players are ready to begin a match
	public void OnBothPlayersReady() {
		// Start counting down from 5
		countDown = 5;
		Lobby.UpdateLobbyText(countDown);
		StartCoroutine(CountDown());
	}

	// Used to cancel an in-progress countdown
	public void CancelCountDown() {
		cancelSceneChange = true;
	}

	// Called once every second during a countdown
	public IEnumerator CountDown() {

		yield return new WaitForSeconds(1);

		// Count down one second and update UI
		if (countDown > 0) {
			countDown -= 1;
			Lobby.UpdateLobbyText(countDown);
		}

		CheckCountDownStatus();
	}

	// Triggered when a network player enters the lobby
	// Returns true if the player is the host, false if the client
	public bool OnNetworkPlayerEnteredLobby() {
		return Lobby.OnPlayerEnteredLobby();
	}

	// Manage UI to show a network lobby
	public void ShowNetworkLobby() {
		PlayText.enabled = false;
		ToggleNetworkLobby();
	}

	// Manage UI to leave a local lobby
	public void ExitLocalLobby() {
		ToggleLocalLobby();
		TogglePlayMenu();
	}

	// Manage leaving a network lobby
	public void ExitNetworkLobby() {
		/*
		// Quit match
		if (matchMaker.PlayingAsHost) {
			NetworkManager.singleton.StopHost();
		}
		else {
			NetworkManager.singleton.StopClient();
		}
		*/
	}

	// Begins the animations that lead to desired scene change
	public void LeaveMenu(string sceneName) {
		nextSceneName = sceneName;
		leavingMenu = true;
		ToggleShade();
	}

	#endregion


	#region Private API

	// Determines the status of the countdown after the counter changes
	private void CheckCountDownStatus() {
		// Check if the countdown has been canceled
		if (cancelSceneChange) {
			cancelSceneChange = false;
			countDown = 0;
		}
		// Not canceled, check if countdown is over
		else if (countDown == 0) {
			// Launch the appropriate game scene
			if (localMenuOpen) {
				LeaveMenu("LocalDuel");
			}
			else {
				LeaveMenu("NetworkDuel");
			}
		}
		else {
			// Not canceled or complete, continue count down
			StartCoroutine(CountDown());
		}
	}

	// Handle animating UI elements during opening sequence
	private void ManageOpeningAnimations() {
		// Manage shade fading out
		if (shadeFadingOut) {
			FadeShadeAlpha();
		}
		// Once shade is gone, animate title
		else if (!openAnimsDone) {
			AnimateTitle();
		}
		// After title is steady, animate play text
		else if (!pastOpenMenu) {
			AnimatePlayText();
		}
	}

	// Handle animating UI elements when leaving menu scene
	private void ManageLeavingAnimations() {
		// Manage shade fading in
		if (shadeFadingIn) {
			RaiseShadeAlpha();
		}
		// After shade is black, leave menu
		else if (leavingMenu) {
			leavingMenu = false;

			// Clear event listeners
			EventManager.Nullify();

			// Change scene
			if (localMenuOpen) {
				SceneManager.LoadScene(nextSceneName);
			}
			else {
				BushidoNetManager.Get().OnBothPlayersReady();
			}
		}
	}

	// Check for tap input that skips opening menu
	private void CheckForInput() {
		if (ReceivedInput()) {
			// Input will either skip title slide or open main menu
			if (!openAnimsDone) {
				// Fast forward title animation and begin animating play text
				FinishOpeningAnimations();
			}
			else {
				AudioManager.Get().PlayMenuSound();

				// Hide play text and show play menu
				PlayText.enabled = false;
				MultiPlayMenu.SetActive(true);

				// Prevent further non-button input
				pastOpenMenu = true;
			}
		}
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	// Toggle visibility of multiplayer menu
	private void TogglePlayMenu() {
		MultiPlayMenu.SetActive(!MultiPlayMenu.activeSelf);
	}

	// Toggle visibility of network match menu
	private void ToggleNetworkMenu() {
		NetworkMenu.SetActive(!NetworkMenu.activeSelf);
	}

	// Toggle visibility of lobby menu
	private void ToggleLobbyMenu() {
		TitleText.gameObject.SetActive(LobbyMenu.activeSelf);
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	// Prepare network lobby and toggle menu
	private void ToggleNetworkLobby() {
		// Prepare network lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			cancelSceneChange = false;
			Lobby.PrepareNetworkLobby();
		}
		ToggleLobbyMenu();
	}

	// Prepare local lobby and toggle menu
	private void ToggleLocalLobby() {
		// Prepare local lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			cancelSceneChange = false;
			Lobby.PrepareLocalLobby();
		}
		ToggleLobbyMenu();
	}

	#endregion


	#region ButtonEvents

	// Close the network match menu
	public void OnNetworkMenuExit() {
		AudioManager.Get().PlayMenuSound();

		// Close the network menu and show multi-play menu
		ToggleNetworkMenu();
		TogglePlayMenu();
	}

	// Open the local game lobby
	public void OnLocalGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = true;

		// Hide multiplay menu and show local lobby
		TogglePlayMenu();
		ToggleLocalLobby();
	}

	// Open the network game menu
	public void OnNetworkGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = false;

		// Hide multiplay menu and show network game menu
		TogglePlayMenu();
		ToggleNetworkMenu();
	}

	// Trigger quick play matchmaking and open lobby
	public void OnQuickPlayPressed() {
		AudioManager.Get().PlayMenuSound();

		// Find opponent via quick play matchaking
		MatchMaker.QuickPlay();
		ToggleNetworkMenu();

		// Setup UI to show matchmaking process
		PlayText.text = "Finding a game";
		PlayText.enabled = true;
	}

	// Trigger nearby matchmaking process and open lobby
	public void OnNearbyGamePressed() {
		// TODO: use local discovery to find nearby opponent
	}

	#endregion


	#region Shade Animations

	// Trigger shade animation fading in or out
	private void ToggleShade() {
		if (!Shade.enabled) {
			// Fade shade in
			Shade.enabled = true;
			shadeFadingIn = true;
			shadeFadingOut = false;
		}
		else {
			// Fade shade out
			shadeFadingOut = true;
			shadeFadingIn = false;
		}
	}

	// Handle shade fading in
	private void RaiseShadeAlpha() {
		var color = Shade.color;
		var limit = 1;

		// Raise alpha to 1 then stop
		if (color.a < limit) {
			SetShadeAlpha(color, color.a + .02f);
		}
		else {
			SetShadeAlpha(color, limit);
			shadeFadingIn = false;
		}
	}

	// Handle shade fading out
	private void FadeShadeAlpha() {
		var color = Shade.color;

		// Fade alpha to 0 then stop
		if (color.a > 0) {
			SetShadeAlpha(color, color.a - .02f);
		}
		else {
			SetShadeAlpha(color, 0);
			shadeFadingOut = false;
			Shade.enabled = false;
		}
	}

	// Set shade alpha to specified value
	private void SetShadeAlpha(Color color, float alphaValue) {
		color.a = alphaValue;
		Shade.color = color;
	}

	#endregion


	#region Text Animations

	// Skip title slide animation
	private void FinishOpeningAnimations() {
		// Set title UI element to desired height, toggle flashing "play" text element
		TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
		PlayText.enabled = true;
		openAnimsDone = true;
	}

	// Handle title slide animation
	private void AnimateTitle() {
		// Get title height
		var titleY = TitleText.rectTransform.anchoredPosition.y;

		// Slide title to desired height and stop
		if (titleY > titleHeight) {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleY - 5);
		}
		else {
			FinishOpeningAnimations();
		}
	}

	// Handle flashing "play" animation
	private void AnimatePlayText() {
		// Fade text in or out (back and forth)
		if (playTextFading) {
			FadeTextAlpha(PlayText);
		}
		else {
			RaiseTextAlpha(PlayText);
		}
	}

	// Handle "play" text fading in
	private void RaiseTextAlpha(Text text) {
		var color = text.color;

		// Raise text alpha to 1 then stop
		if (color.a < 1) {
			SetPlayTextAlpha(color, color.a + .03f);
		}
		else {
			SetPlayTextAlpha(color, 1);
			playTextFading = true;
		}
	}

	// Handle "play" text fading out
	private void FadeTextAlpha(Text text) {
		var color = text.color;

		// Fade text alpha to 0 then stop
		if (color.a > 0) {
			SetPlayTextAlpha(color, color.a - .03f);
		}
		else {
			SetPlayTextAlpha(color, 0);
			playTextFading = false;
		}
	}

	// Set play text alpha to desired value
	private void SetPlayTextAlpha(Color color, float alphaValue) {
		color.a = alphaValue;
		PlayText.color = color;
	}

	#endregion
}
