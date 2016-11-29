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

	public Sprite CheckedBox, UncheckedBox;

	public Image Shade;
	public Image LeftCheckbox, RightCheckbox;
	public Image LeftLobbySamurai, RightLobbySamurai;

	public Text InfoText;
	public Text PlayText, TitleText;
	public Text LocalBestOf, NetworkBestOf;

	public GameObject PlayMenu, LocalDialog, NetworkDialog, InfoDialog, LobbyDialog;

	#region Private Variables

	private string nextSceneName;

	private BushidoMatchMaker matchMaker;

	private bool shadeFadingIn, shadeFadingOut;
	private bool playTextFading;
	private bool openAnimsDone;
	private bool localSettings;
	private bool leavingMenu;
	private bool lobbyFull;
	private bool inLobby;
	private bool input;

	private int titleHeight;
	private int bestOfIndex = 0;
	private string[] bestOfOptions = {"3", "5", "7"};

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {
		matchMaker = GetComponent<BushidoMatchMaker>();

		titleHeight = ((int)-TitleText.preferredHeight * 3) / 4;

		HideTextAlpha(PlayText);
		HideTextAlpha(GetTextComponentInChild(PlayText));
		FillShade();

		PlayMenu.SetActive(false);
		InfoDialog.SetActive(false);
		LobbyDialog.SetActive(false);
		LocalDialog.SetActive(false);
		NetworkDialog.SetActive(false);

		LeftLobbySamurai.enabled = false;
		RightLobbySamurai.enabled = false;

		shadeFadingOut = true;
		playTextFading = true;
	}
	
	// Update is called once per frame
	void Update () {

		CheckForInput();

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

		// Manage shade fading in
		if (shadeFadingIn) {
			RaiseShadeAlpha();
		}
		// After shade is black, leave menu
		else if (leavingMenu) {
			leavingMenu = false;
			EventManager.Nullify();
			SceneManager.LoadScene(nextSceneName);
		}
	}

	#endregion


	#region Public API

	public static MenuManager Get() {
		return FindObjectOfType<MenuManager>();
	}

	public void UpdateLobbySamurai(bool hostInLobby, bool clientInLobby) {
		LeftLobbySamurai.enabled = hostInLobby;
		RightLobbySamurai.enabled = clientInLobby;
		if (!LobbyDialog.activeSelf) {
			ActivateLobby();
		}
	}

	public void UpdateLobbyReadyBoxes(bool hostReady, bool clientReady) {
		// Left checkbox
		LeftCheckbox.sprite =
			(hostReady) 
			? CheckedBox 
			: UncheckedBox;

		// Right checkbox
		RightCheckbox.sprite = 
			(clientReady)
			? CheckedBox 
			: UncheckedBox;

		// Check if both players are ready
		if (hostReady && clientReady) {
			StartCoroutine(CountDown());
		}
	}

	public void LeaveMenu(string sceneName) {
		nextSceneName = sceneName;
		leavingMenu = true;
		ToggleShade();
	}

	public IEnumerator CountDown() {
		yield return new WaitForSeconds(5);
		Debug.Log("Changing scene");

		BushidoNetManager.Get().OnBothPlayersReady();
	}

	#endregion


	#region Private API

	private void ActivateLobby() {
		PlayText.gameObject.SetActive(false);
		ToggleLobbyDialog();
	}

	private void CheckForInput() {
		// Check for input on the initial menu
		if (!input && ReceivedInput()) {
			if (!openAnimsDone) {
				// Fast forward title animation and begin animating play text
				TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
				PlayText.gameObject.SetActive(true);
				openAnimsDone = true;
			}
			else {
				AudioManager.Get().PlayMenuSound();

				// Hide play text and show play menu
				PlayText.gameObject.SetActive(false);
				PlayMenu.SetActive(true);

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

	private void UpdateBestOfText() {
		if (localSettings) {
			ChangeTextInChildText(bestOfOptions[bestOfIndex], LocalBestOf);
		}
		else {
			ChangeTextInChildText(bestOfOptions[bestOfIndex], NetworkBestOf);
		}
	}

	public void MatchFound() {
		ChangeTextInChildText("Joining game", PlayText);
	}

	private void TogglePlayMenu() {
		PlayMenu.SetActive(!PlayMenu.activeSelf);
	}

	private void ToggleInfoDialog() {
		InfoDialog.SetActive(!InfoDialog.activeSelf);
	}

	private void ToggleLocalMenu() {
		LocalDialog.SetActive(!LocalDialog.activeSelf);
	}

	private void ToggleNetworkMenu() {
		NetworkDialog.SetActive(!NetworkDialog.activeSelf);
	}

	private void ToggleLobbyDialog() {
		LobbyDialog.SetActive(!LobbyDialog.activeSelf);
	}

	#endregion


	#region ButtonEvents

	public void OnGameMenuExit() {
		AudioManager.Get().PlayMenuSound();

		// Close the menu that was open
		if (localSettings) {
			ToggleLocalMenu();
		}
		else {
			ToggleNetworkMenu();
		}

		// Bring up the play menu
		TogglePlayMenu();
	}

	public void OnInfoDialogOk() {
		AudioManager.Get().PlayMenuSound();

		// Close info dialog and open next menu
		ToggleInfoDialog();

		if (localSettings) {
			ToggleLocalMenu();
		}
		else {
			// Toggle lobby dialog
		}
	}

	public void OnLocalGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localSettings = true;

		// Customize info dialog for local play
		ChangeTextInChildText("Touch your side\nof the screen\nas soon as you see", InfoText);

		// Hide play menu and show info dialog
		TogglePlayMenu();
		ToggleInfoDialog();
	}

	public void OnLocalMenuConfirm() {
		AudioManager.Get().PlayMenuSound();

		// Set game wins based on UI
		int.TryParse(LocalBestOf.text, out BushidoNetManager.Get().matchLimit);
		LeaveMenu("LocalDuel");
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
		matchMaker.QuickPlay();
		ToggleNetworkMenu();

		ChangeTextInChildText("Finding a game", PlayText);
		PlayText.gameObject.SetActive(true);
	}

	public void OnLobbyReady() {
		AudioManager.Get().PlayMenuSound();

		// Find local player and give ready signal
		foreach (LobbyPlayer player in LobbyPlayer.GetAll()) {
			if (player.isLocalPlayer) {
				player.CmdGiveReadySignal();
				return;
			}
		}
	}

	public void OnLobbyExit() {
		AudioManager.Get().PlayMenuSound();

		// Quit match
		if (matchMaker.PlayingAsHost) {
			NetworkManager.singleton.StopHost();
		}
		else {
			NetworkManager.singleton.StopClient();
		}

		//ToggleLobbyDialog();
		//ToggleNetworkMenu();
	}

	public void OnCreateGameToggle() {
		AudioManager.Get().PlayMenuSound();

		// Switch between network menu and create game dialog
		ToggleNetworkMenu();
		//ToggleCreateGameDialog();
	}

	public void OnFindGamePressed() {
		AudioManager.Get().PlayMenuSound();

		//ToggleNetworkMenu();
	}


	public void OnLeftPressed() {
		AudioManager.Get().PlayMenuSound();
		bestOfIndex = (bestOfIndex > 0)
			? bestOfIndex - 1
			: bestOfOptions.Length - 1;
		UpdateBestOfText();
	}

	public void OnRightPressed() {
		AudioManager.Get().PlayMenuSound();
		bestOfIndex = (bestOfIndex < bestOfOptions.Length - 1) 
			? bestOfIndex + 1
			: 0;
		UpdateBestOfText();
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

	private Text GetTextComponentInChild(Text text) {
		return text.transform.GetChild(0).GetComponentInChildren<Text>();
	}

	private void ChangeTextInChildText(string newText, Text text) {
		Text child = GetTextComponentInChild(text);
		text.text = newText;
		child.text = newText;
	}

	private void AnimateTitle() {
		var titleY = TitleText.rectTransform.anchoredPosition.y;
		if (titleY > titleHeight) {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleY - 5);
		}
		else {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
			PlayText.gameObject.SetActive(true);
			openAnimsDone = true;
		}
	}

	private void AnimatePlayText() {
		if (playTextFading) {
			FadeTextAlpha(PlayText);
			FadeTextAlpha(GetTextComponentInChild(PlayText));
		}
		else {
			RaiseTextAlpha(PlayText);
			RaiseTextAlpha(GetTextComponentInChild(PlayText));
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
