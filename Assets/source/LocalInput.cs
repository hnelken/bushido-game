using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class recognizes any player input in a local duel scene
 * and sends the appropriate messages to the duel manager to handle.
 */

public class LocalInput : MonoBehaviour {

	#region Private Variables

	private DuelManager manager;		// The current duel manager
	private bool leftPlayerInput;			// True if left player input this frame
	private bool rightPlayerInput;			// True if right player input this frame

	#endregion


	#region State Functions

	// Initialization
	void Start () {
		manager = GetComponent<DuelManager>();
	}

	// Update is called once per frame
	void Update () {

		// Get input status
		CheckInput();

		// Submit reaction inputs from either player
		if (!manager.IsReadyToDuel()) {
			if (leftPlayerInput) {
				manager.SignalPlayerReady(true);
			}
			if (rightPlayerInput) {
				manager.SignalPlayerReady(false);
			}
		}
		else if (manager.WaitingForInput()) {
			if (leftPlayerInput) {
				manager.TriggerReaction(true, Time.realtimeSinceStartup);
			}
			if (rightPlayerInput) {
				manager.TriggerReaction(false, Time.realtimeSinceStartup);
			}
		}
	}

	#endregion


	#region Private API

	// Checks for touch input from both players
	private void CheckInput() {
		// Get input from keyboard keys or touch during testing
		leftPlayerInput = Input.GetKeyDown(KeyCode.S);
		rightPlayerInput = Input.GetKeyDown(KeyCode.K);

		// Assemble all touches' x coordinates
		List<float> beganTouchesX = GetTouchPositionsX();

		// Check all touches for location on screen
		for (int i = 0; i < beganTouchesX.Count; i++) {
			// Touches on left side of screen are from player 1
			if (TouchIsInLeftScreen(beganTouchesX[i])) {
				leftPlayerInput = true;
			}
			// Touches on right side of screen are from player 2
			if (TouchIsInRightScreen(beganTouchesX[i])) {
				rightPlayerInput = true;
			}
		}
	}

	// Gets a list of positions of touches that began this frame
	private List<float> GetTouchPositionsX() {
		List<float> touchesX = new List<float>();

		// Assemble all touches' x coordinates
		for (int i = 0; i < touchesX.Count; i++) {
			Touch touch = Input.GetTouch(i);

			// Touch must have just begun
			if (touch.phase == TouchPhase.Began) {
				touchesX.Add(touch.position.x);
			}
		}
		return touchesX;
	}

	// Checks if a touch is in the left side of the screen
	private bool TouchIsInLeftScreen(float touchX) {
		// True if touch is on left half of screen
		// Player input clause speeds up loop in GetTouchPositionsX
		return !leftPlayerInput && touchX < Screen.width / 2;
	}
	
	// Checks if a touch is in the right side of the screen
	private bool TouchIsInRightScreen(float touchX) {
		// True if touch is on right half of screen
		// Player input clause speeds up loop in GetTouchPositionsX
		return !rightPlayerInput && touchX > Screen.width / 2;
	}

	#endregion
}
