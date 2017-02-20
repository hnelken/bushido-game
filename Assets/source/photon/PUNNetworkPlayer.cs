using Photon;
using UnityEngine;
using System.Collections;

public class PUNNetworkPlayer : Photon.MonoBehaviour {

	#region Public Accessors

	public bool IsHost { get { return isHost; } }

	#endregion


	#region Private Variables

	private bool inGame;					// True if this player is in a game
	private bool isHost;					// True if this player created the current room
	private bool inputReceived;				// True if input has been received this round during gameplay

	#endregion


	#region Photon Behaviour API

	void Update() {
		// Stop if not the local player or input has been received
		if (!ShouldCheckForInput()) {
			return;
		}

		// Check for touch or keyboard input
		if (TouchInput() || Input.GetKeyDown(KeyCode.Space)) {
			inputReceived = true;

			// Trigger player reaction at the current time
			int reactionTime = DuelManager.Get().GetCurrentTime();
			TriggerReaction(isHost, reactionTime);
		}
	}

	// Sync this component on all clients
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(isHost);
		}
		else {
			this.isHost = (bool) stream.ReceiveNext();
		}
	}

	#endregion


	#region Public API
		
	// Set this player as in-game
	public void LeaveLobby() {
		this.inGame = true;
	}

	// Set this player as host
	public void SetAsHost() {
		this.isHost = true;
	}

	#endregion


	#region Private API

	// Checks for touch input beginning this frame
	private bool TouchInput() {
		// Check all touches for ones beginning this frame
		for (int i = 0; i < Input.touchCount; i++) {
			Touch touch = Input.GetTouch(i);

			// Touch must have just begun
			if (touch.phase == TouchPhase.Began) {
				return true;
			}
		}
		return false;
	}

	// Decides if input should be checked for
	private bool ShouldCheckForInput() {
		// Check for input if...
		return inGame	// ...player is not in lobby
			&& photonView.isMine 	//...player is the owner of this component
			&& !inputReceived;		//...player has not triggered input this round already
	}

	#endregion


	#region PUN RPC's

	[PunRPC]	// RPC to trigger reaction from this player on all clients
	void TriggerReaction(bool hostSamurai, int reactionTime) {
		DuelManager.Get().TriggerReaction(hostSamurai, reactionTime);
	}

	#endregion
}
