using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadingShade : Image {

	public bool IsHidden, IsBusy;
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

	public void Fill() {
		// Set shade to full color
		SetAlphaValue(1);
		enabled = true;
	}

	// Initialize the shade for a scene entry
	public void Initialize() {
		// Set shade to full color
		Fill();

		// Begin fading out
		Toggle();
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
		IsBusy = true;
	}

	public void ToggleHalfAlpha() {
		if (!enabled) {
			enabled = true;
			SetAlphaValue(0.5f);
		}
		else {
			SetAlphaValue(0f);
			enabled = false;
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

			// Set public status
			IsHidden = false;
			IsBusy = false;
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

			// Set public status
			IsHidden = true;
			IsBusy = false;
		}
	}

	// Set alpha to specified value
	private void SetAlphaValue(float alphaValue) {
		var tmpColor = this.color;
		tmpColor.a = alphaValue;
		this.color = tmpColor;
	}
}
