using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkUtility : NetworkBehaviour {

	public DuelManager manager;			// Reference to the manager using this utility

	[SyncVar]
	private int currentTime;

	#region Public API

	public int GetCurrentTime() {
		return currentTime;
	}

	public void UpdateCurrentTime() {
		if (isServer) {
			currentTime = manager.GetReactionTime();
			RpcSetCurrentTime(currentTime);
		}
	}

	#endregion


	#region Server Commands

	[Command]
	public void CmdSetStartTime() {
		RpcSetStartTime(Time.realtimeSinceStartup);
	}

	[Command]
	public void CmdSetRandomWaitTime() {
		RpcSetRandomWaitTime(Random.Range(3, 6));
	}

	#endregion


	#region Client RPC's

	[ClientRpc]
	private void RpcSetRandomWaitTime(float waitTime) {
		manager.SetWaitTime(waitTime);
	}

	[ClientRpc]
	private void RpcSetStartTime(float startTime) {
		manager.SetStartTime(startTime);
	}

	[ClientRpc]
	private void RpcSetCurrentTime(int currTime) {
		manager.SetCurrentTime(currTime);
	}

	#endregion
}
