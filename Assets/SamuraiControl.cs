using UnityEngine;
using System.Collections;

public class SamuraiControl : MonoBehaviour {

	public bool leftSamurai;
	public Sprite idle, attacking;

	private int winCount;
	private bool isIdle;
	private NaiveGameController controller;
	private SpriteRenderer spriteRenderer;

	public void addWin() {
		winCount++;
	}

	public string getWinCount() {
		return winCount.ToString();
	}

	public void SetController(NaiveGameController controller) {
		this.controller = controller;
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		spriteRenderer.sprite = idle;
		isIdle = true;
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

	public void ToggleSprite() {
		if (isIdle) {
			spriteRenderer.sprite = attacking;
		}
		else {
			spriteRenderer.sprite = idle;
		}
		isIdle = !isIdle;
	}

	// Signal a player input to the game controller
	private void SamuraiReact() {
		controller.TriggerReaction(leftSamurai);
	}

}
