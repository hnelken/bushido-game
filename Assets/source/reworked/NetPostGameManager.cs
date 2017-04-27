using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetPostGameManager : MonoBehaviour {

	#region Public References

	public GameObject RematchButton;			// The button element used to vote for a rematch
	public Text LeftWinner, RightWinner;		// The text elements used to indicate the winner of the match
	public Text LeftWins, RightWins;			// The text elements displaying each player's number of wins
	public Text LeftBest, RightBest;			// The text elements displaying each player's best reaction time
	public Text MainText;						// The text element used to show the countdown to a rematch
	public FadingShade Shade;					// The UI element used to fade in and out of scenes

	#endregion


	#region Private Variables

	private CountdownManager countdown;			// A private reference to the countdown manager for a potential rematch

	private bool rematching;					// True when a rematch has been confirmed by both players
	private bool exiting;						// True when a player is exiting the post game with no rematch

	#endregion 


	#region MonoBehaviour API

	// Use this for initialization
	void Start() {
		// Initialize some UI
		Shade.Initialize();

		// Setup countdown reference
		countdown = GetComponent<CountdownManager>();
		countdown.Initialize(MainText);

		// Setup countdown event block
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

	#endregion


	#region Public API

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
		exiting = true;
		Shade.Toggle();
	}

	#endregion


	#region Private API

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
			AudioManager.Get().BackToMenu();
			BushidoMatchInfo.Get().EndMatch();
			PhotonNetwork.LoadLevel(Globals.MainMenuScene);
		}
	}

	private void CheckForRematch() {
		if (rematching) {
			rematching = false;
			PhotonNetwork.LoadLevel(Globals.NetDuelScene);
		}
	}
		
	private void StartLeavingScene() {
		rematching = true;
		Shade.Toggle();
	}

	private bool IsLeavingScene() {
		return (exiting || rematching) && !Shade.IsHidden && !Shade.IsBusy;
	}

	#endregion
}
