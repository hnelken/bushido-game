using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BasicDuelManager))]
public class LocalPlayerInput : MonoBehaviour {

	private BasicDuelManager manager;
	private bool leftPlayerInput;
	private bool rightPlayerInput;

	void Start () {
		manager = GetComponent<BasicDuelManager>();
	}

	// Update is called once per frame
	void Update () {

		CheckInput();

		if (leftPlayerInput) {
			manager.TriggerReaction(true);
		}
		if (rightPlayerInput) {
			manager.TriggerReaction(false);
		}
	}

	// Checks for touch input from both players
	private void CheckInput() {
		leftPlayerInput = Input.GetKeyDown(KeyCode.S);
		rightPlayerInput = Input.GetKeyDown(KeyCode.K);
		List<float> beganTouchesX = GetTouchPositionsX();

		// Check all touches for location on screen
		for (int i = 0; i < beganTouchesX.Count; i++) {
			if (TouchIsInLeftScreen(beganTouchesX[i])) {
				leftPlayerInput = true;
			}
			if (TouchIsInRightScreen(beganTouchesX[i])) {
				rightPlayerInput = true;
			}
		}
		
	}

	// Gets a list of positions of touches that began this frame
	private List<float> GetTouchPositionsX() {
		List<float> touchesX = new List<float>();
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
		return !leftPlayerInput && touchX < Screen.width / 2;
	}
	
	// Checks if a touch is in the right side of the screen
	private bool TouchIsInRightScreen(float touchX) {
		return !rightPlayerInput && touchX > Screen.width / 2;
	}
}
