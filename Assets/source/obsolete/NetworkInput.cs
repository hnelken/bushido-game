using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkInput : NetworkBehaviour {
	
	//private bool localPlayerInput;			// True if local player input this frame
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}
		
		// Check for touch or keyboard input
		if (TouchInput() || Input.GetKeyDown(KeyCode.Space)) {
			int reactionTime = DuelManager.Get().GetCurrentTime();
			CmdTriggerReaction(isServer, reactionTime);
		}
	}


	#region Private API

	[Command]
	private void CmdTriggerReaction(bool hostSamurai, int inputTime) {
		RpcTriggerReaction(hostSamurai, inputTime);
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
