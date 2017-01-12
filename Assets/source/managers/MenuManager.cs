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
	private bool localSettings;						// True if a local-play menu is open, false if a net-play menu is open
	private bool leavingMenu;						// True if a game is about to begin and the menu scene must be left
	private bool input;								// True if input has been received that should skip the opening animations

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
		HideTextAlpha(PlayText);
		FillShade();

		// Set status of UI animations
		shadeFadingOut = true;
		playTextFading = true;
	}
	
	// Update is called once per frame
	void Update () {

		// Check if the user wants to skip the opening animations
		if (!input) {
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

	private void CheckCountDownStatus() {
		// Check if the countdown has been canceled
		if (cancelSceneChange) {
			cancelSceneChange = false;
			countDown = 0;
		}
		// Not canceled, check if countdown is over
		else if (countDown == 0) {
			// Launch the appropriate game scene
			if (localSettings) {
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
		else {
			AnimatePlayText();
		}
	}

	private void ManageLeavingAnimations() {
		// Manage shade fading in
		if (shadeFadingIn) {
			RaiseShadeAlpha();
		}
		// After shade is black, leave menu
		else if (leavingMenu) {
			leavingMenu = false;
			EventManager.Nullify();

			// Change scene
			if (localSettings) {
				SceneManager.LoadScene(nextSceneName);
			}
			else {
				BushidoNetManager.Get().OnBothPlayersReady();
			}
		}
	}

	private void CheckForInput() {
		// Check for input on the initial menu
		if (ReceivedInput()) {
			if (!openAnimsDone) {
				// Fast forward title animation and begin animating play text
				TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
				PlayText.enabled = true;
				openAnimsDone = true;
			}
			else {
				AudioManager.Get().PlayMenuSound();

				// Hide play text and show play menu
				playTextFading = false;
				PlayText.enabled = false;
				MultiPlayMenu.SetActive(true);

				// Prevent further non-button input
				input = true;
			}
		}
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	private void TogglePlayMenu() {
		MultiPlayMenu.SetActive(!MultiPlayMenu.activeSelf);
	}

	private void ToggleNetworkMenu() {
		NetworkMenu.SetActive(!NetworkMenu.activeSelf);
	}

	private void ToggleNetworkLobby() {
		if (!LobbyMenu.activeSelf) {
			cancelSceneChange = false;
			Lobby.PrepareNetworkLobby();
		}
		TitleText.gameObject.SetActive(LobbyMenu.activeSelf);
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	private void ToggleLocalLobby() {
		if (!LobbyMenu.activeSelf) {
			cancelSceneChange = false;
			Lobby.PrepareLocalLobby();
		}
		TitleText.gameObject.SetActive(LobbyMenu.activeSelf);
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	#endregion


	#region ButtonEvents

	public void OnGameMenuExit() {
		AudioManager.Get().PlayMenuSound();

		// Close the menu that was open
		if (localSettings) {
			ToggleLocalLobby();
		}
		else {
			ToggleNetworkMenu();
		}

		// Bring up the play menu
		TogglePlayMenu();
	}

	public void OnLocalGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localSettings = true;

		// Hide play menu and show local lobby
		TogglePlayMenu();
		ToggleLocalLobby();
	}

	public void OnNetworkGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localSettings = false;

		// Hide play menu and show info dialog
		TogglePlayMenu();
		ToggleNetworkMenu();
	}

	public void OnQuickPlayPressed() {
		AudioManager.Get().PlayMenuSound();
		MatchMaker.QuickPlay();
		ToggleNetworkMenu();

		PlayText.text = "Finding a game";
		PlayText.enabled = true;
	}

	public void OnCreateGameToggle() {
		AudioManager.Get().PlayMenuSound();

	}

	public void OnFindGamePressed() {
		AudioManager.Get().PlayMenuSound();
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

	private void RaiseShadeAlpha() {
		var color = Shade.color;
		var limit = 1;
		if (color.a < limit) {
			SetShadeAlpha(color, color.a + .02f);
		}
		else {
			SetShadeAlpha(color, limit);
			shadeFadingIn = false;
		}
	}

	private void FadeShadeAlpha() {
		var color = Shade.color;
		if (color.a > 0) {
			SetShadeAlpha(color, color.a - .02f);
		}
		else {
			SetShadeAlpha(color, 0);
			shadeFadingOut = false;
			Shade.enabled = false;
		}
	}

	private void FillShade() {
		Shade.enabled = true;
		var color = Shade.color;
		color.a = 1;
		Shade.color = color;
	}

	private void SetShadeAlpha(Color color, float alphaValue) {
		color.a = alphaValue;
		Shade.color = color;
	}

	#endregion


	#region Text Animations

	private void AnimateTitle() {
		var titleY = TitleText.rectTransform.anchoredPosition.y;
		if (titleY > titleHeight) {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleY - 5);
		}
		else {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
			PlayText.enabled = true;
			openAnimsDone = true;
		}
	}

	private void AnimatePlayText() {
		if (playTextFading) {
			FadeTextAlpha(PlayText);
		}
		else {
			RaiseTextAlpha(PlayText);
		}
	}

	private void RaiseTextAlpha(Text text) {
		var color = text.color;
		if (color.a < 1) {
			color.a += 0.03f;
			text.color = color;
		}
		else {
			color.a = 1;
			text.color = color;
			playTextFading = true;
		}
	}

	private void FadeTextAlpha(Text text) {
		var color = text.color;
		if (color.a > 0) {
			color.a -= 0.03f;
			text.color = color;
		}
		else {
			color.a = 0;
			text.color = color;
			playTextFading = false;
		}
	}

	private void HideTextAlpha(Text text) {
		var color = text.color;
		color.a = 0;
		text.color = color;
	}

	#endregion
}
