using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

	public Image FlashScreen;
	public Animator TopLine, BottomLine;

	private NaiveGameController controller;
	private bool screenFlashed;
	private bool flashingWhite;
	private bool fadingIn;

	public void SetController(NaiveGameController controller) {
		this.controller = controller;
	}

	// Use this for initialization
	void Start () {
		FlashScreen.enabled = false;
		screenFlashed = false;
		flashingWhite = true;
		fadingIn = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (screenFlashed) {
			if (!flashingWhite) {
				FadeBlack();
			}
			else {
				FlashWhiteAndFade();
			}
		}
	}

	public void FlashWhiteFadeOut() {
		flashingWhite = true;
		screenFlashed = true;
		FlashScreen.enabled = true;
	}

	public void FadeBlackFadeOut() {
		fadingIn = true;
		flashingWhite = false;
		screenFlashed = true;
		FlashScreen.enabled = true;
	}

	private void FlashWhiteAndFade() {
		if (FlashScreen.color.a > 0) {
			Color flashColor = FlashScreen.color;
			flashColor.a -= 0.01f;
			FlashScreen.color = flashColor;
		}
		else {
			screenFlashed = false;
			Color flashColor = Color.black;
			flashColor.a = 0f;
			FlashScreen.color = flashColor;
		}
	}
	
	private void FadeBlack() {
		if (fadingIn) {
			FadeBlackIn();
		}
		else {	// Fading out
			FadeBlackOut();
		}
	}

	private void FadeBlackIn() {
		if (FlashScreen.color.a < 1) {
			Color flashColor = FlashScreen.color;
			flashColor.a += 0.01f;
			FlashScreen.color = flashColor;
		}
		else {
			fadingIn = false;
			controller.SignalBlackFaded();
		}
	}

	private void FadeBlackOut() {
		if (FlashScreen.color.a > 0) {	// Decrease alpha until transparent
			Color flashColor = FlashScreen.color;
			flashColor.a -= 0.01f;
			FlashScreen.color = flashColor;
		}
		else {	// Done fading out, signal cinematics
			screenFlashed = false;
			FlashScreen.enabled = false;
			FlashScreen.color = Color.white;

			TriggerCinematics();
		}
	}

	private void TriggerCinematics() {
		TopLine.Play ("TopLine", -1, 0f);
		BottomLine.Play ("BottomLine", -1, 0f);

		StartCoroutine(WaitAndStartRound());
	}

	public IEnumerator WaitAndStartRound() {

		yield return new WaitForSeconds(3.5f);

		controller.SignalNextRoundReady();
	}
}
