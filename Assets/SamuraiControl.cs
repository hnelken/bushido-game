using UnityEngine;
using System.Collections;

public class SamuraiControl : MonoBehaviour {

	public bool leftSamurai;

	private NaiveGameController controller;

	public void SetController(NaiveGameController controller) {
		this.controller = controller;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Player 1 input
		if (leftSamurai) {
			if (Input.GetKeyDown(KeyCode.S)) {
				SamuraiReact();
			}
		} 
		else {	// Player 2 input
			if (Input.GetKeyDown(KeyCode.K)) {
				SamuraiReact();
			}
		}
	}

	// Signal a player input to the game controller
	private void SamuraiReact() {
		controller.SamuraiTrigger(leftSamurai);
	}

}
