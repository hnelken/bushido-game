using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class BushidoNetManager : NetworkLobbyManager {

	[HideInInspector]
	public int matchLimit;

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	void OnClientEnterLobby() {

	}

	void OnClientExitLobby() {

	}

	void OnClientReady(bool readyState) {

	}
}
