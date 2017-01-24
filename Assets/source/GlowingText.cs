using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GlowingText : Text {

	private bool fading;

	// Update is called once per frame
	void Update () {
		// Fade text in or out (back and forth)
		if (fading) {
			FadeTextAlpha();
		}
		else {
			RaiseTextAlpha();
		}
	}

	// Initialize text to fade in from nothing
	public void Initialize() {
		SetAlphaValue(0);
		enabled = true;
		fading = false;
	}

	// Handle "play" text fading in
	private void RaiseTextAlpha() {

		// Raise text alpha to 1 then stop
		if (color.a < 1) {
			SetAlphaValue(color.a + .03f);
		}
		else {
			SetAlphaValue(1);
			fading = true;
		}
	}

	// Handle "play" text fading out
	private void FadeTextAlpha() {
		// Fade text alpha to 0 then stop
		if (color.a > 0) {
			SetAlphaValue(color.a - .03f);
		}
		else {
			SetAlphaValue(0);
			fading = false;
		}
	}

	// Set play text alpha to desired value
	private void SetAlphaValue(float alphaValue) {
		var tmpColor = this.color;
		tmpColor.a = alphaValue;
		this.color = tmpColor;
	}
}
