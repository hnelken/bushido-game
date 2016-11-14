using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/*
 * This class is the manager for the main menu of the game
 */
public class MenuManager : MonoBehaviour {

	public Text PlayText, PlayText2, TitleText;

	#region Private Variables

	private bool alphaFading;
	private bool opened;
	private bool input;					// True if input was received this frame

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {
		HideAlpha(PlayText);
		HideAlpha(PlayText2);

		alphaFading = true;
	}
	
	// Update is called once per frame
	void Update () {

		CheckForInput();

		if (!opened) {
			AnimateTitle();
		}
		else {
			AnimatePlayText();
		}
	}

	#endregion


	#region Private API

	private void AnimateTitle() {
		var titleY = TitleText.rectTransform.anchoredPosition.y;
		if (titleY > -100) {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, titleY - 5);
		}
		else {
			TitleText.rectTransform.anchoredPosition = new Vector2(0, -100);
			PlayText.enabled = true;
			PlayText2.enabled = true;
			opened = true;
		}
	}

	private void AnimatePlayText() {
		if (alphaFading) {
			FadeAlpha(PlayText);
			FadeAlpha(PlayText2);
		}
		else {
			RaiseAlpha(PlayText);
			RaiseAlpha(PlayText2);
		}
	}

	private void CheckForInput() {
		// Check for input on the initial menu
		if (!input && ReceivedInput()) {
			if (!opened) {
				TitleText.rectTransform.anchoredPosition = new Vector2(0, -100);
				PlayText.enabled = true;
				opened = true;
			}
			else {
				// Register input this frame
				input = true;

				// Nullify event listeners and load the local duel scene
				EventManager.Nullify();
				SceneManager.LoadScene("LocalDuel");
			}
		}
	}

	private void RaiseAlpha(Text text) {
		var color = text.color;
		if (color.a < 1) {
			color.a += 0.03f;
			text.color = color;
		}
		else {
			color.a = 1;
			text.color = color;
			alphaFading = true;
		}
	}

	private void FadeAlpha(Text text) {
		var color = text.color;
		if (color.a > 0) {
			color.a -= 0.03f;
			text.color = color;
		}
		else {
			color.a = 0;
			text.color = color;
			alphaFading = false;
		}
	}

	private void HideAlpha(Text text) {
		var color = text.color;
		color.a = 0;
		text.color = color;
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	#endregion
}
