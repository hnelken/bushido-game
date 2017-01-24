using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadingShade : Image {

	public bool IsHidden;
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
		SetAlphaValue(1);
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
		var limit = 1;

		// Check if fade-in is complete
		if (color.a < limit) {
			// Fade in a little bit this frame
			SetAlphaValue(color.a + .02f);
		}
		else {	// Fade-in complete
			// Clamp alpha and stop fading
			SetAlphaValue(limit);
			fadingIn = false;
			IsHidden = false;
		}
	}

	// Handle fading out
	private void FadeAlpha() {
		var limit = 0;

		// Check if fade-out is complete
		if (color.a > limit) {
			// Fade the alpha a little bit this frame
			SetAlphaValue(color.a - .02f);
		}
		else {	// Shade is faded out
			// Clamp alpha and stop fading
			SetAlphaValue(limit);
			fadingOut = false;
			enabled = false;
			IsHidden = true;
		}
	}

	// Set alpha to specified value
	private void SetAlphaValue(float alphaValue) {
		var tmpColor = this.color;
		tmpColor.a = alphaValue;
		this.color = tmpColor;
	}
}
