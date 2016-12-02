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

	public Image Shade;
	public Text PlayText, TitleText;
	public GameObject MultiPlayMenu, NetworkMenu, LobbyMenu;

	public AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}

	public BushidoMatchMaker MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = GetComponent<BushidoMatchMaker>();
			}
			return matchMaker;
		}
	}

	public LobbyManager Lobby;

	#region Private Variables

	private AudioManager audioManager;
	private BushidoMatchMaker matchMaker;

	private bool shadeFadingIn, shadeFadingOut;
	private bool playTextFading;
	private bool openAnimsDone;
	private bool localSettings;
	private bool leavingMenu;
	private bool input;

	private int titleHeight;
	private string nextSceneName;

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {

		titleHeight = ((int)-TitleText.preferredHeight * 3) / 4;

		HideTextAlpha(PlayText);
		HideTextAlpha(GetTextComponentInChild(PlayText));
		FillShade();

		shadeFadingOut = true;
		playTextFading = true;
	}
	
	// Update is called once per frame
	void Update () {

		CheckForInput();

		/* ---- MANAGE ANIMATIONS EVERY FRAME ---- */

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

			// Change scene
			if (localSettings) {
				SceneManager.LoadScene(nextSceneName);
			}
			else {
				BushidoNetManager.Get().OnBothPlayersReady();
			}
		}

	}

	#endregion


	#region Public API

	public static MenuManager Get() {
		return FindObjectOfType<MenuManager>();
	}

	public void OnBothPlayersReady(bool localLobby) {
		StartCoroutine(CountDown(localLobby));
	}

	public IEnumerator CountDown(bool localLobby) {
		yield return new WaitForSeconds(5);
		Debug.Log("Changing scene");

		if (localLobby) {
			LeaveMenu("LocalDuel");
		}
		else {
			LeaveMenu("NetworkDuel");
		}
	}

	public bool OnNetworkPlayerEnteredLobby() {
		Debug.Log("Lobby: " + LobbyMenu.activeSelf);
		if (!LobbyMenu.activeSelf) {
			ShowNetworkLobby();
		}
		return Lobby.OnPlayerEnteredLobby();
	}

	public void ShowNetworkLobby() {
		PlayText.gameObject.SetActive(false);
		ToggleNetworkLobby();
	}

	public void ExitLocalLobby() {
		ToggleLocalLobby();
		TogglePlayMenu();
	}

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

	public void LeaveMenu(string sceneName) {
		nextSceneName = sceneName;
		leavingMenu = true;
		ToggleShade();
	}

	#endregion


	#region Private API

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
			Lobby.PrepareNetworkLobby();
		}
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	private void ToggleLocalLobby() {
		if (!LobbyMenu.activeSelf) {
			Lobby.PrepareLocalLobby();
		}
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

		ChangeTextInChildText("Finding a game", PlayText);
		PlayText.gameObject.SetActive(true);
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
