using UnityEngine;
using System.Collections;

/*
 * This class is the manager for the main menu of the game
 */
public class Menu : MonoBehaviour {

	#region Private Variables

	private bool input = false;					// True if input was received this frame

	#endregion


	#region State Functions

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// Check for input on the initial menu
		if (!input && ReceivedInput()) {
			// Register input this frame
			input = true;

			// Nullify event listeners and load the local duel scene
			EventManager.Nullify();
			Application.LoadLevel("BasicDuel");
		}
	}

	#endregion


	#region Private API

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	#endregion
}
