using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetPostGameManager : BasePostGameManager {

	#region Public References

	public Button Exit;							// The button element used to leave the post game scene
	public Image LeftSamurai, RightSamurai;		// The left and right samurai image elements
	public Image LeftCheckbox, RightCheckbox;	// The left and right checkbox image elements
	#endregion


	#region Private Variables

	private PhotonView photonView;				// A private reference to the photon view for making RPC's
	private PopupManager popup;					// A private reference to the popup manager for alerting the player
	private CountdownManager countdown;			// A private reference to the countdown manager for a potential rematch

	private bool bothPlayersPresent;			// True when both players are present in the post game

	#endregion 


	#region MonoBehaviour API

	// Use this for initialization
	public override void Start() {
		// Initialize some UI
		Shade.Initialize();

		// Get component references
		this.photonView = GetComponent<PhotonView>();
		this.popup = GetComponent<PopupManager>();

		// Setup PUN event listener
		Globals.MatchMaker.InitializeForNewScene();
		PUNQuickPlay.Disconnect += OnOpponentLeftMatch;

		// Setup countdown reference
		countdown = GetComponent<CountdownManager>();
		countdown.Initialize(MainText, LeftCheckbox, RightCheckbox, true);

		// Setup countdown event block
		CountdownManager.ResetReady += ShowRematchButton;
		CountdownManager.CountdownComplete += OnCountdownComplete;

		// Clear ready status of both players
		PUNNetworkPlayer.GetLocalPlayer().ClearReadyStatus();

		// Update UI if any player left during the game
		CheckForMissingPlayers();

		// Fill UI with stats from finished match
		UpdateUIWithGameStats(BushidoMatchInfo.Get());
	}

	#endregion


	#region Photon RPC's

	[PunRPC]
	void SyncClearReadyStatus() {
		// Reset the countdown and ready status
		countdown.ClearReadyStatus();
	}

	// Reset the countdown and ready status for both players
	private void ClearReadyStatusOnAllClients() {
		photonView.RPC("SyncClearReadyStatus", PhotonTargets.All);
	}

	#endregion


	#region Public API

	public void OnOpponentLeftMatch() {
		// A player has left
		bothPlayersPresent = false;

		// Hide rematch controls
		countdown.ShowControls(false);
		RematchButton.SetActive(false);

		// Prevent players from joining while we look at the popup menu
		PhotonNetwork.room.IsVisible = false;
		PhotonNetwork.room.IsOpen = false;

		// Trigger popup menu
		ShowPopup(false);
	}

	public void OnPlayerLeftMatch() {
		// Disconnect player and leave post game scene
		PhotonNetwork.Disconnect();
		exiting = true;
		StartLeavingScene();
	}

	public void HidePopup() {
		SetExitButtonInteractable(true);
	}

	public void ShowRematchButton() {
		if (bothPlayersPresent) {
			RematchButton.SetActive(true);
		}
	}

	#endregion


	#region Derived API

	// Check if there will be a rematch when the scene changes
	protected override void CheckForRematch() {
		if (rematching) {
			// Go back to local duel scene if not leaving match
			rematching = false;
			SceneManager.LoadScene(Globals.NetDuelScene);
		}
	}

	#endregion


	#region Private API

	private void SetExitButtonInteractable(bool interactable) {
		Exit.interactable = interactable;
	}

	private void ShowPopup(bool manualExit) {
		// Disable lobby interactive UI
		SetExitButtonInteractable(false);

		if (manualExit) {
			// Reset the ready status and stop countdown if in progress
			ClearReadyStatusOnAllClients();

			// Popup the notification asking if you want to leave the game
			popup.Initialize(OnPlayerLeftMatch, HidePopup, "You  are  leaving\nthe  game", true);
		}
		else if (!countdown.IsFinished()) {
			Debug.Log("finished: " + countdown.IsFinished());
			// Popup the notification that a player has left
			popup.Initialize(HidePopup, null, "Your  opponent\nhas  left", false);
		}
		else {
			Debug.Log("finished: " + countdown.IsFinished());
		}

	}

	private void CheckForMissingPlayers() {
		// Hide any missing player's samurai
		bool leftInRoom = false, rightInRoom = false;
		foreach (PUNNetworkPlayer player in PUNNetworkPlayer.GetAllPlayers()) {
			if (player.IsHost) {
				leftInRoom = true;
			}
			else {
				rightInRoom = true;
			}
		}

		// Change UI based on player presence
		if (!leftInRoom) {	// Left player missing
			LeftSamurai.enabled = false;
			LeftWinner.enabled = false;
			RightWinner.enabled = true;
		}
		else if (!rightInRoom) {	// Right player missing
			RightSamurai.enabled = false;
			LeftWinner.enabled = true;
			RightWinner.enabled = false;
		}
		else {
			// Show rematch controls
			countdown.ShowControls(true);
			RematchButton.SetActive(true);
			bothPlayersPresent = true;
		}
	}

	private void OnCountdownComplete() {
		rematching = true;
		StartLeavingScene();
	}

	#endregion


	#region Button Events

	// Send rematch signal
	public override void RematchPressed() {
		Globals.Audio.PlayMenuSound();

		// Signal local player ready
		RematchButton.SetActive(false);
		PUNNetworkPlayer.GetLocalPlayer().SetAsReady();
		countdown.SignalPlayerReady(PhotonNetwork.isMasterClient);
	}

	// Prepare to leave the match
	public override void LeaveMatchPressed() {
		Globals.Audio.PlayMenuSound();

		// Show popup for leaving match
		ShowPopup(true);
	}

	#endregion
}
