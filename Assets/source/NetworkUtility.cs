using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkUtility : NetworkBehaviour {

	public DuelManager manager;			// Reference to the manager using this utility

	#region Server Commands

	[Command]
	public void CmdSetLatency(float clientStartTime) {

		Debug.Log("Server: " + manager.GetStartTime() + " - Client: " + clientStartTime);
		//RpcSetLatency(clientStartTime);
	}

	[Command]
	public void CmdSetStartTime() {
		RpcSetStartTime(Time.realtimeSinceStartup);
	}

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
	private void RpcSetLatency(float clientStartTime) {
		
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
