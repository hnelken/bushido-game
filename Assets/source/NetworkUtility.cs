using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class NetworkUtility : NetworkBehaviour {

	public DuelManager manager;			// Reference to the manager using this utility

	public static NetworkUtility Get() {
		return FindObjectOfType<NetworkUtility>();
	}

	#region Server Commands

	[Command]
	public void CmdSetRandomWaitTime() {
		RpcSetRandomWaitTime(Random.Range(4, 7));
	}

	#endregion


	#region Client RPC's

	[ClientRpc]
	public void RpcTriggerGameStart() {
		manager.TriggerGameStart();
	}

	[ClientRpc]
	public void RpcPopFlag() {
		manager.PopFlag();
	}

	[ClientRpc]
	private void RpcSetRandomWaitTime(float waitTime) {
		manager.SetWaitTime(waitTime);
	}

	[ClientRpc]
	private void RpcSetStartTime(float startTime) {
		manager.SetStartTime(startTime);
	}

	#endregion
}
