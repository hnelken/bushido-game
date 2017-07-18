﻿using Photon;
using UnityEngine;
using System.Collections;

public class PUNNetworkPlayer : Photon.MonoBehaviour {

	#region Public Accessors

	public bool IsReadyForMatchStart { get { return isReadyForMatchStart; } }
	public bool IsReadyForRoundStart { get { return isReadyForRoundStart; } }

	#endregion


	#region Private Variables

	private bool inGame;					// True if this player is in a game
	private bool inputReceived;				// True if input has been received this round during gameplay
	private bool isReadyForMatchStart;		// True if this player is ready to leave the lobby
	private bool isReadyForRoundStart;		// True if this player is ready for the round to begin

	private NetDuelManager duelManager;
	private NetDuelManager DuelManager {
		get {
			if (!duelManager) {
				duelManager = (NetDuelManager)NetDuelManager.Get();
			}
			return duelManager;
		}
	}

	#endregion


	#region Photon Behaviour API

	void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	void Update() {
		// Stop if not the local player or input has been received
		if (!ShouldCheckForInput()) {
			return;
		}

		// Check for touch or keyboard input
		if (TouchInput() || Input.GetKeyDown(KeyCode.Space)) {
			inputReceived = true;

			// Get reaction time from duel manager
			int reactionTime = DuelManager.GetCurrentTime();

			// Call reaction RPC to trigger input on all clients
			photonView.RPC("TriggerReaction", PhotonTargets.All, IsPlayerHost(), reactionTime);
		}
	}

	// Sync this component on all clients
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(inGame);
			stream.SendNext(inputReceived);
			stream.SendNext(isReadyForMatchStart);
			stream.SendNext(isReadyForRoundStart);
		}
		else {
			this.inGame = (bool) stream.ReceiveNext();
			this.inputReceived = (bool) stream.ReceiveNext();
			this.isReadyForMatchStart = (bool) stream.ReceiveNext();
			this.isReadyForRoundStart = (bool) stream.ReceiveNext();
		}
	}

	#endregion


	#region Public API
		
	//public static bool LocalPlayerIsHost() {
	//	return PhotonNetwork.isMasterClient;
	//}

	public static PUNNetworkPlayer GetLocalPlayer() {
		foreach (PUNNetworkPlayer player in GetAllPlayers()) {
			if (player.photonView.isMine) {
				return player;
			}
		}
		Debug.Log("Couldn't find local player");
		return null;
	}

	public static PUNNetworkPlayer[] GetAllPlayers() {
		return GameObject.FindObjectsOfType<PUNNetworkPlayer>();
	}

	// Signal boths players as leaving the lobby
	public static void SignalBothPlayersLeaveLobby() {
		// Set each player to be in game
		foreach (PUNNetworkPlayer player in GetAllPlayers()) {
			player.LeaveLobby();
		}
	}

	public static void ResetBothPlayersForNewRound() {
		// Set each player to be in game
		foreach (PUNNetworkPlayer player in GetAllPlayers()) {
			player.ResetInput();
			player.ClearRoundReadyStatus();
		}
	}

	public void EnterLobby() {
		photonView.RPC("SignalEnterLobby", PhotonTargets.AllBuffered);
	}

	public void ResetInput() {
		inputReceived = false;
	}

	// Set this player as ready and 
	public void SetAsReadyForRoundStart() {
		this.isReadyForRoundStart = true;
	}

	public void ClearRoundReadyStatus() {
		this.isReadyForRoundStart = false;
	}

	// Set this player as ready and 
	public void SetAsReadyForMatchStart() {
		this.isReadyForMatchStart = true;
	}

	public void ClearMatchReadyStatus() {
		this.isReadyForMatchStart = false;
	}

	// Set this player as host
	//public void SetAsHost() {
	//	this.isHost = true;
	//}

	// Set this player as in-game
	public void LeaveLobby() {
		this.inGame = true;
	}

	#endregion


	#region Private API

	private bool IsPlayerHost() {
		return Globals.IsNetPlayerHost(this);
	}

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
		
		// Check for active game status
		bool gameNotPaused = (DuelManager)
			? !DuelManager.IsGamePaused()
			: false;

		// Check for input if...
		return inGame						//...player is in an active game
			&& gameNotPaused				//...the active game is not paused
			&& photonView.isMine 			//...player is the owner of this component
			&& !inputReceived;				//...player has not triggered input this round already
	}

	#endregion


	#region PUN RPC's

	[PunRPC]
	void SignalEnterLobby() {
		Globals.NetLobby.OnPlayerEnteredLobby(this);
	}

	[PunRPC]	// RPC to trigger reaction from this player on all clients
	void TriggerReaction(bool hostSamurai, int reactionTime) {
		DuelManager.TriggerReaction(hostSamurai, reactionTime);
	}

	#endregion
}
