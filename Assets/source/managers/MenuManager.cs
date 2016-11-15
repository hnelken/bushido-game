using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/*
 * This class is the manager for the main menu of the game
 */
public class MenuManager : MonoBehaviour {

	public Text PlayText, PlayText2, TitleText;
	public Button Local, Network;
	public Image Shade, BG;

	#region Private Variables

	private string nextSceneName;
	private bool leavingMenu;
	private bool shadeFadingIn, shadeFadingOut;
	private bool playTextFading;
	private bool openAnimsDone;
	private bool input;					// True if input was received this frame

	private int titleHeight;

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {
		titleHeight = (int)-TitleText.preferredHeight;

		HideTextAlpha(PlayText);
		HideTextAlpha(PlayText2);
		FillShade();

		Local.gameObject.SetActive(false);
		Network.gameObject.SetActive(false);

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


	#region Private API

	private void CheckForInput() {
		// Check for input on the initial menu
		if (!input && ReceivedInput()) {
			if (!openAnimsDone) {
				TitleText.rectTransform.anchoredPosition = new Vector2(0, titleHeight);
				PlayText.enabled = true;
				openAnimsDone = true;
			}
			else {
				// Register input this frame
				input = true;

				PlayText.enabled = false;
				PlayText2.enabled = false;

				Local.gameObject.SetActive(true);
				Network.gameObject.SetActive(true);
			}
		}
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	private void LeaveMenu(string sceneName) {
		nextSceneName = sceneName;
		leavingMenu = true;
		ToggleShade();
	}

	#endregion

	#region ButtonEvents

	public void OnLocalPressed() {
		// Leave menu for local duel
		LeaveMenu("LocalDuel");
	}

	public void OnNetworkPressed() {
		// Leave menu for network duel
		LeaveMenu("NetworkDuel");
	}

	#endregion

	#region BG Animations

	private void SetBGColor(Color color, float rgbValue) {
		color.r = rgbValue;
		color.g = rgbValue;
		color.b = rgbValue;
		BG.color = color;
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
			PlayText2.enabled = true;
			openAnimsDone = true;
		}
	}

	private void AnimatePlayText() {
		if (playTextFading) {
			FadeTextAlpha(PlayText);
			FadeTextAlpha(PlayText2);
		}
		else {
			RaiseTextAlpha(PlayText);
			RaiseTextAlpha(PlayText2);
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
