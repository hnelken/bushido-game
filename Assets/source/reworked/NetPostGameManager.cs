using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetPostGameManager : MonoBehaviour {

	public GameObject RematchButton;
	public Text MainText, LeftWinner, RightWinner;
	public Text LeftWins, RightWins;
	public Text LeftBest, RightBest;
	public FadingShade Shade;

	private CountdownManager countdown;

	private bool leavingScene;
	private bool rematching;
	private bool exiting;

	// Use this for initialization
	void Start() {
		// Initialize some UI
		Shade.Initialize();
		MainText.enabled = false;

		countdown = GetComponent<CountdownManager>();
		CountdownManager.CountdownComplete += StartLeavingScene;

		// Clear ready status of both players
		PUNNetworkPlayer.GetLocalPlayer().ClearReadyStatus();

		// Fill UI with stats from finished match
		UpdateUIWithGameStats(BushidoMatchInfo.Get());
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
		countdown.SignalPlayerReady(PhotonNetwork.isMasterClient);
	}

	// Prepare to leave the match
	public void LeaveMatchPressed() {
		PhotonNetwork.Disconnect();
		Debug.Log("Leave match");
		leavingScene = true;
		exiting = true;
		Shade.Toggle();
	}

	private void UpdateUIWithGameStats(BushidoMatchInfo matchInfo) {
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

	private void CheckForExit() {
		if (exiting) {
			exiting = false;
			LeaveScene(Globals.MainMenuScene);
			//PhotonNetwork.LoadLevel(Globals.MainMenuScene);
		}
	}

	private void CheckForRematch() {
		if (rematching) {
			rematching = false;
			LeaveScene(Globals.NetDuelScene);
			//PhotonNetwork.LoadLevel(Globals.NetDuelScene);
		}
	}
		
	private void StartLeavingScene() {
		leavingScene = true;
		rematching = true;
		Shade.Toggle();
	}

	private void LeaveScene(string sceneName) {
		AudioManager.Get().BackToMenu();
		BushidoMatchInfo.Get().EndMatch();
		PhotonNetwork.LoadLevel(sceneName);
	}

	private bool IsLeavingScene() {
		return leavingScene && !Shade.IsHidden && !Shade.IsBusy;
	}
}
