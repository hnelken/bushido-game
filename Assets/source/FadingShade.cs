using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadingShade : Image {

	private bool fadingIn, fadingOut;
	
	// Manage animations
	void Update() {
		// Manage shade fading out
		if (fadingOut) {
			FadeAlpha();
		}
		// Manage shade fading in
		else if (fadingIn) {
			RaiseAlpha();
		}
	}

	// Initialize the shade for a scene entry
	public void Initialize() {
		// Set shade to full color
		SetAlpha(color, 1);
		enabled = true;

		// Begin fading out
		fadingOut = true;
	}

	// Trigger animation fading in or out
	public void Toggle() {
		if (!enabled) {
			// Fade  in
			enabled = true;
			fadingIn = true;
			fadingOut = false;
		}
		else {
			// Fade  out
			fadingOut = true;
			fadingIn = false;
		}
	}

	// Handle fading in
	private void RaiseAlpha() {
		var tmpColor = color;
		var limit = 1;

		// Check if fade-in is complete
		if (tmpColor.a < limit) {
			// Fade in a little bit this frame
			SetAlpha(tmpColor, tmpColor.a + .02f);
		}
		else {	// Fade-in complete
			// Clamp alpha and stop fading
			SetAlpha(tmpColor, limit);
			fadingIn = false;
		}
	}

	// Handle fading out
	private void FadeAlpha() {
		var tmpColor = color;

		// Check if fade-out is complete
		if (tmpColor.a > 0) {
			// Fade the alpha a little bit this frame
			SetAlpha(tmpColor, tmpColor.a - .02f);
		}
		else {	// Shade is faded out
			// Clamp alpha and stop fading
			SetAlpha(tmpColor, 0);
			fadingOut = false;
			enabled = false;
		}
	}

	// Set alpha to specified value
	private void SetAlpha(Color tmpColor, float alphaValue) {
		tmpColor.a = alphaValue;
		color = tmpColor;
	}
}
