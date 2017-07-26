using UnityEngine;
using System.Collections;

public class SoloInput : MonoBehaviour {

	#region Private Variables

	private SoloDuelManager manager;		// The current duel manager
	private bool playerInput;			// True if left player input this frame

	#endregion


	#region State Functions

	// Initialization
	void Start () {
		manager = GetComponent<SoloDuelManager>();
	}

	// Update is called once per frame
	void Update () {

		// Get input status
		CheckInput();

		// Submit reaction inputs from either player
		if (manager.WaitingForInput()) {
			int reactionTime = manager.GetCurrentTime();
			if (playerInput) {
				manager.TriggerReaction(true, reactionTime);
			}
		}
	}

	#endregion


	#region Private API

	// Checks for touch input from both players
	private void CheckInput() {
		// Get input from keyboard keys or touch during testing
		playerInput = Input.GetKeyDown(KeyCode.Space);

		for (int i = 0; i < Input.touchCount && !playerInput; i++) {
			playerInput = playerInput || Input.touches[i].phase == TouchPhase.Began;
		}
	}

	#endregion
}
