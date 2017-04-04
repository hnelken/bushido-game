using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// Class used in starting and finding nearby games
public class BushidoDiscovery : NetworkDiscovery {

	public void StartNearbyGame() {
		Initialize();
		StartAsServer();
		NetworkManager.singleton.StartHost();
	}

	public void FindNearbyGame() {
		Initialize();
		StartAsClient();
	}

	public void ExitBroadcast() {
		if (running) {
			StopBroadcast();
		}
	}

	public override void OnReceivedBroadcast (string fromAddress, string data) {
		var items = data.Split(':');
		if (items.Length == 3 && items[0] == "NetworkManager") {
			// Set network address
			NetworkManager.singleton.networkAddress = items[1];

			// Set network port
			int port;
			int.TryParse(items[2], out port);
			NetworkManager.singleton.networkPort = port;

			// Start client and stop listening
			NetworkManager.singleton.StartClient();
			StopBroadcast();
		}
	}
}
