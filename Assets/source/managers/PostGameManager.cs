using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PostGameManager : MonoBehaviour {

	public GameObject RematchButton;
	public Text MainText;
	public Text LeftWins, RightWins;
	public Text LeftBest, RightBest;
	public Image LeftCheckbox, RightCheckbox;
	public FadingShade Shade;

	private int countDown;

	private bool countingDown;
	private bool leavingScene;
	private bool networked;
	private bool rematching;
	private bool exiting;

	// The sprite for the checked box
	private Sprite checkedBox;
	private Sprite CheckedBox {
		get {
			if (!checkedBox) {
				checkedBox = Resources.Load<Sprite>("sprites/checkbox-checked");
			}
			return checkedBox;
		}
	}

	// The sprite for the unchecked box
	private Sprite uncheckedBox;
	private Sprite UncheckedBox {
		get {
			if (!uncheckedBox) {
				uncheckedBox = Resources.Load<Sprite>("sprites/checkbox-unchecked");
			}
			return uncheckedBox;
		}
	}

	// Use this for initialization
	void Start() {
		// Initialize some UI
		Shade.Initialize();
		MainText.enabled = false;
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;

		// Get match results from net manager
		var netManager = BushidoNetManager.Get();

		if (netManager) {
		// Change UI to show number of wins for each player
		LeftWins.text = "" + netManager.Results.LeftWins;
		RightWins.text = "" + netManager.Results.RightWins;

		// Change UI to show the best reaction time of each player
		int leftBest = netManager.Results.LeftBest;
		int rightBest = netManager.Results.RightBest;
		LeftBest.text = (leftBest == -1) ? "xx" : "" + leftBest;
		RightBest.text = (rightBest == -1) ? "xx" : "" + rightBest;
		}

		networked = netManager.Results.Networked;
		if (!networked) {
			LeftCheckbox.enabled = false;
			RightCheckbox.enabled = false;
		}
	}

	void Update() {
		if (IsLeavingScene()) {
			CheckForRematch();
			CheckForExit();
		}
	}

	// Send rematch signal
	public void RematchPressed() {
		Debug.Log("Rematch");

		if (!networked) {
			StartCountDown();
		}
		else {
			// check ready status

		}

	}

	// Prepare to leave the match
	public void LeaveMatchPressed() {
		Debug.Log("Leave match");
		leavingScene = true;
		exiting = true;
		Shade.Toggle();
	}

	private void CheckForExit() {
		if (exiting) {
			exiting = false;
			SceneManager.LoadScene("Menu");
		}
	}

	private void CheckForRematch() {
		if (rematching) {
			rematching = false;
			if (networked) {
				BushidoNetManager.Get().LaunchNetworkDuel();
			}
			else {
				SceneManager.LoadScene("LocalDuel");
			}
		}
	}

	// Triggered when both lobby players are ready to begin a match
	private void StartCountDown() {

		// Hide checkboxes and rematch button
		RematchButton.SetActive(false);
		LeftCheckbox.enabled = false;
		RightCheckbox.enabled = false;

		// Start counting down from 5
		countDown = 5;
		countingDown = true;
		UpdateMainText(countDown);
		StartCoroutine(CountDown());
	}

	private IEnumerator CountDown() {

		yield return new WaitForSeconds(1);

		if (countingDown) {
			countDown -= 1;
			UpdateMainText(countDown);
			CheckCountDownStatus();
		}
	}

	// Determines the status of the countdown after the counter changes
	private void CheckCountDownStatus() {

		// Not canceled, check if countdown is over
		if (countDown == 0) {
			leavingScene = true;
			rematching = true;
			Shade.Toggle();
		}
		else {
			// Not canceled or complete, continue count down
			StartCoroutine(CountDown());
		}
	}

	private void UpdateMainText(int countdown) {
		if (!MainText.enabled) {
			MainText.enabled = true;
		}
		MainText.text = "Game starting in  " + countdown;
	}

	private bool IsLeavingScene() {
		return leavingScene && !Shade.IsHidden && !Shade.IsBusy;
	}
}
