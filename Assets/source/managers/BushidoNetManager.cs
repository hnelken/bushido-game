using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class BushidoNetManager : NetworkManager {

	public void StartupHost() {
		SetPort();
		NetworkManager.singleton.StartHost();
	}

	public void JoinGame() {
		SetIPAddress();
		SetPort();
		NetworkManager.singleton.StartClient();
	}

	private void SetIPAddress() {
		string ipAddress = GameObject.Find("ipAddressField").transform.FindChild("Text").GetComponent<Text>().text;
		NetworkManager.singleton.networkAddress = ipAddress;
	}

	private void SetPort() {
		NetworkManager.singleton.networkPort = 7777;
	}


}
