﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkRNG : NetworkBehaviour {

	public DuelManager manager;

	[Command]
	public void CmdUpdateTimer() {
		RpcUpdateTimer(manager.GetReactionTime(Time.realtimeSinceStartup));
	}

	[ClientRpc]
	private void RpcUpdateTimer(int time) {
		manager.gui.UpdateTimer(time);
	}

	[Command]
	public void CmdRandomWaitTime() {
		RpcSetRandomWaitTime(Random.Range(3, 6));
	}

	[ClientRpc]
	private void RpcSetRandomWaitTime(float waitTime) {
		manager.SetWaitTime(waitTime);
	}
}
