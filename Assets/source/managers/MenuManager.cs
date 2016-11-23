using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/*
 * This class is the manager for the main menu of the game
 */
public class MenuManager : MonoBehaviour {

	public Image Shade;
	public Text PlayText, TitleText;
	public Text LocalBestOf, NetworkBestOf;
	public GameObject PlayMenu, LocalDialog, NetworkDialog;

	#region Private Variables

	private string nextSceneName;

	private BushidoMatchMaker matchMaker;

	private bool shadeFadingIn, shadeFadingOut;
	private bool playTextFading;
	private bool openAnimsDone;
	private bool localSettings;
	private bool leavingMenu;
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
		LocalDialog.SetActive(false);
		NetworkDialog.SetActive(false);

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
				TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
				PlayText.gameObject.SetActive(true);
				openAnimsDone = true;
			}
			else {
				PlayText.gameObject.SetActive(false);
				AudioManager.Get().PlayMenuSound();
				PlayMenu.SetActive(true);

				// Register input this frame
				input = true;
			}
		}
	}

	private void UpdateBestOfText() {
		if (localSettings) {
			ChangeTextInChildText(bestOfOptions[bestOfIndex], LocalBestOf);
		}
		else {
			ChangeTextInChildText(bestOfOptions[bestOfIndex], NetworkBestOf);
		}
	}

	private void TogglePlayMenu() {
		PlayMenu.SetActive(!PlayMenu.activeSelf);
	}

	private void ToggleLocalMenu() {
		localSettings = true;
		LocalDialog.SetActive(!LocalDialog.activeSelf);
	}

	private void ToggleNetworkMenu() {
		localSettings = false;
		NetworkDialog.SetActive(!NetworkDialog.activeSelf);
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	public void MatchFound() {
		ChangeTextInChildText("Joining game", PlayText);
	}

	#endregion


	#region ButtonEvents

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

	public void OnLocalToggle() {
		// Leave menu for local duel
		AudioManager.Get().PlayMenuSound();
		TogglePlayMenu();
		ToggleLocalMenu();
	}

	public void OnNetworkToggle() {
		AudioManager.Get().PlayMenuSound();
		TogglePlayMenu();
		ToggleNetworkMenu();
	}

	public void OnLocalMenuConfirm() {
		AudioManager.Get().PlayMenuSound();

		// Set game wins based on UI
		int.TryParse(LocalBestOf.text, out BushidoNetManager.Get().matchLimit);
		LeaveMenu("LocalDuel");
	}

	public void OnQuickPlayPressed() {
		AudioManager.Get().PlayMenuSound();
		matchMaker.QuickPlay();
		ToggleNetworkMenu();

		ChangeTextInChildText("Finding a game", PlayText);
		PlayText.gameObject.SetActive(true);
	}

	public void OnCreateGamePressed() {
		AudioManager.Get().PlayMenuSound();
		matchMaker.CreateInternetMatch();
		ToggleNetworkMenu();

		ChangeTextInChildText("Waiting for another player", PlayText);
		PlayText.gameObject.SetActive(true);
	}

	public void OnFindGamePressed() {
		AudioManager.Get().PlayMenuSound();

		//ToggleNetworkMenu();
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
