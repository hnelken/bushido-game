using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class BushidoNetManager : NetworkLobbyManager {


	#region Public Accessors

	public BushidoDiscovery Discovery;				// The network discovery component for nearby games

	// The last match limit set in a lobby
	public int MatchLimit {
		get {
			return matchLimit;
		}
	}

	// The results struct from the last complete game
	public MatchResults Results {
		get { return results; }
	}

	// The struct representing the results of a complete match
	public struct MatchResults {
		int leftWins, rightWins;
		int leftBest, rightBest;
		bool networked;

		public int LeftWins {
			get { return leftWins; }
		}

		public int RightWins {
			get { return rightWins; }
		}

		public int LeftBest {
			get { return leftBest; }
		}

		public int RightBest {
			get { return rightBest; }
		}

		public bool Networked {
			get { return networked; }
		}

		public MatchResults(int _leftWins, int _rightWins, int _leftBest, int _rightBest, bool _networked) {
			leftWins = _leftWins;
			rightWins = _rightWins;
			leftBest = _leftBest;
			rightBest = _rightBest;
			networked = _networked;
		}
	}

	#endregion


	#region Private Variables

	private MatchResults results;
	private int matchLimit;

	#endregion


	#region Public API

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	public override void OnLobbyClientExit ()
	{
		base.OnLobbyClientExit ();
		Debug.Log("Client left lobby");
	}

	public override void OnLobbyServerPlayersReady() {
		Debug.Log("Both players ready");
	}

	public void SetMatchLimit(string limitText) {
		int.TryParse(limitText, out matchLimit);
	}

	public void SetMatchResults(int leftWins, int rightWins, int leftBest, int rightBest, bool networked) {
		results = new MatchResults(leftWins, rightWins, leftBest, rightBest, networked);
	}

	public void LaunchNetworkDuel() {
		ServerChangeScene("NetworkDuel");
	}

	#endregion

}
