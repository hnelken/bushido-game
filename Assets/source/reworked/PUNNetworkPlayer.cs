using Photon;
using UnityEngine;
using System.Collections;

public class PUNNetworkPlayer : Photon.MonoBehaviour {

	#region Public Accessors

	public bool IsHost { get { return isHost; } }
	public bool IsReady { get { return isReady; } }

	#endregion


	#region Private Variables

	private bool inGame;					// True if this player is in a game
	private bool isHost;					// True if this player created the current room
	private bool isReady;					// True if this player is ready to leave the lobby
	private bool inputReceived;				// True if input has been received this round during gameplay

	private NetDuelManager duelManager;
	private NetDuelManager DuelManager {
		get {
			if (!duelManager) {
				duelManager = NetDuelManager.Get();
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
			photonView.RPC("TriggerReaction", PhotonTargets.All, isHost, reactionTime);
		}
	}

	// Sync this component on all clients
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(isHost);
			stream.SendNext(isReady);
			stream.SendNext(inGame);
			stream.SendNext(inputReceived);
		}
		else {
			this.isHost = (bool) stream.ReceiveNext();
			this.isReady = (bool) stream.ReceiveNext();
			this.inGame = (bool) stream.ReceiveNext();
			this.inputReceived = (bool) stream.ReceiveNext();
		}
	}

	#endregion


	#region Public API
		
	public static bool LocalPlayerIsHost() {
		return PhotonNetwork.isMasterClient;
	}

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

	public static void ResetInputForBothPlayers() {
		// Set each player to be in game
		foreach (PUNNetworkPlayer player in GetAllPlayers()) {
			player.ResetInput();
		}
	}

	public void EnterLobby() {
		photonView.RPC("SignalEnterLobby", PhotonTargets.AllBuffered);
	}

	public void ResetInput() {
		inputReceived = false;
	}

	// Set this player as ready and 
	public void SetAsReady() {
		this.isReady = true;
	}

	public void ClearReadyStatus() {
		this.isReady = false;
	}

	// Set this player as host
	public void SetAsHost() {
		this.isHost = true;
	}

	// Set this player as in-game
	public void LeaveLobby() {
		this.inGame = true;
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
