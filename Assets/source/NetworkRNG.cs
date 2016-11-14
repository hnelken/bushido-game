using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkRNG : NetworkBehaviour {

	public DuelManager manager;
	
	[Command]
	public void CmdRandomWaitTime() {
		RpcSetRandomWaitTime(Random.Range(3, 6));
	}

	[ClientRpc]
	private void RpcSetRandomWaitTime(float waitTime) {
		manager.SetWaitTime(waitTime);
	}
}
