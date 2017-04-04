using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class recognizes any player input in a local duel scene
 * and sends the appropriate messages to the duel manager to handle.
 */

public class LocalInput : MonoBehaviour {

	#region Private Variables

	private DuelManager manager;			// The current duel manager
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
		if (manager.WaitingForInput()) {
			int reactionTime = DuelManager.Get().GetCurrentTime();
			if (leftPlayerInput) {
				manager.TriggerReaction(true, reactionTime);
			}
			if (rightPlayerInput) {
				manager.TriggerReaction(false, reactionTime);
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

		for (int i = 0; i < Input.touchCount && (!leftPlayerInput || !rightPlayerInput); i++) {
			if (Input.touches[i].phase == TouchPhase.Began) {
				Debug.LogError(Input.touches[i].position.x);
				leftPlayerInput = leftPlayerInput || Input.touches[i].position.x < Screen.width / 2;
				rightPlayerInput = rightPlayerInput || Input.touches[i].position.x > Screen.width / 2;
			}
		}
	}

	#endregion
}
