using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkInput : NetworkBehaviour {
	
	private bool localPlayerInput;			// True if local player input this frame
	private bool playerReady;				// True if the player has given the ready signal
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}
		
		// Check for touch or keyboard input
		if (TouchInput() || Input.GetKeyDown(KeyCode.Space)) {
			if (playerReady) {
				CmdTriggerReaction(isServer, Time.realtimeSinceStartup);
			}
			else {
				CmdSignalPlayerReady(isServer);
			}
		}
	}


	#region Private API

 	[Command]
	private void CmdSignalPlayerReady(bool hostSamurai) {
		RpcSignalPlayerReady(hostSamurai);
	}
	
	[ClientRpc]
	private void RpcSignalPlayerReady(bool hostSamurai) {
		playerReady = DuelManager.Get().SignalPlayerReady(hostSamurai);
	}

	[Command]
	private void CmdTriggerReaction(bool hostSamurai, float inputTime) {
		
		Debug.Log("Host: " + hostSamurai);
		Debug.Log("Input: " + inputTime);
		int reactionTime = DuelManager.Get().GetCurrentTime();
		Debug.Log("Reaction: " + reactionTime);
		RpcTriggerReaction(hostSamurai, reactionTime);
	}
	
	[ClientRpc]
	private void RpcTriggerReaction(bool hostSamurai, int reactionTime) {
		DuelManager.Get().TriggerReaction(hostSamurai, reactionTime);
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
	
	#endregion
}
