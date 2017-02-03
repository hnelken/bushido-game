using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// Class used in starting and finding nearby games
public class BushidoDiscovery : NetworkDiscovery {

	public void StartNearbyGame() {
		Initialize();
		StartAsServer();
		//NetworkManager.singleton.StartHost();
	}

	public void FindNearbyGame() {
		Initialize();
		StartAsClient();
	}

	public override void OnReceivedBroadcast (string fromAddress, string data) {
		NetworkManager.singleton.networkAddress = fromAddress;
		NetworkManager.singleton.StartClient();
		StopBroadcast();
	}
}
