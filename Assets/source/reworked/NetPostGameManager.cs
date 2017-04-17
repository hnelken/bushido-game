using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetPostGameManager : MonoBehaviour {

	public GameObject RematchButton;
	public Text MainText, LeftWinner, RightWinner;
	public Text LeftWins, RightWins;
	public Text LeftBest, RightBest;
	public Image LeftCheckbox, RightCheckbox;
	public FadingShade Shade;

	private int countDown;

	private bool countingDown;
	private bool leavingScene;
	private bool rematching;
	private bool exiting;

	private bool hostRematchReady, clientRematchReady;

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

		// Clear ready status of both players
		PUNNetworkPlayer.GetLocalPlayer().ClearReadyStatus();

		// Get match results from net manager
		var matchInfo = BushidoMatchInfo.Get();

		// Change UI to show number of wins for each player
		int leftWins = matchInfo.Results.LeftWins;
		int rightWins = matchInfo.Results.RightWins;
		LeftWins.text = "" + leftWins;
		RightWins.text = "" + rightWins;
		if (leftWins > rightWins) {
			LeftWinner.enabled = true;
			RightWinner.enabled = false;
		}
		else {
			LeftWinner.enabled = false;
			RightWinner.enabled = true;
		}

		// Change UI to show the best reaction time of each player
		int leftBest = matchInfo.Results.LeftBest;
		int rightBest = matchInfo.Results.RightBest;
		LeftBest.text = (leftBest == -1) ? "xx" : "" + leftBest;
		RightBest.text = (rightBest == -1) ? "xx" : "" + rightBest;
	}

	void Update() {
		if (IsLeavingScene()) {
			CheckForRematch();
			CheckForExit();
		}
	}

	// Send rematch signal
	public void RematchPressed() {
		RematchButton.SetActive(false);

		Globals.Audio.PlayMenuSound();

		// Signal local player ready
		PUNNetworkPlayer.GetLocalPlayer().SetAsReady();
		UpdateRematchReadyStatus();
			// check ready status
			// then start countdown
	}

	// Prepare to leave the match
	public void LeaveMatchPressed() {
		Debug.Log("Leave match");
		leavingScene = true;
		exiting = true;
		Shade.Toggle();
	}

	private void UpdateRematchReadyStatus() {
		// Get lobby info from player objects
		foreach (PUNNetworkPlayer player in PUNNetworkPlayer.GetAllPlayers()) {
			if (player.IsHost) {
				hostRematchReady = player.IsReady;
				LeftCheckbox.sprite = (hostRematchReady) ? CheckedBox : UncheckedBox;
			}
			else {
				clientRematchReady = player.IsReady;
				RightCheckbox.sprite = (clientRematchReady) ? CheckedBox : UncheckedBox;
			}
		}

		if (hostRematchReady && clientRematchReady) {
			StartCountDown();
		}
	}

	private void CheckForExit() {
		if (exiting) {
			exiting = false;
			PhotonNetwork.LoadLevel(Globals.MainMenuScene);
		}
	}

	private void CheckForRematch() {
		if (rematching) {
			rematching = false;
			PhotonNetwork.LoadLevel(Globals.NetDuelScene);
		}
	}

	// Triggered when both lobby players are ready to begin a match
	private void StartCountDown() {

		// Hide checkboxes and rematch button
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
