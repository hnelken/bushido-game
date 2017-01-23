using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class BushidoNetManager : NetworkLobbyManager {


	#region Public Accessors

	public int matchLimit;

	public MatchResults Results {
		get { return results; }
	}

	public struct MatchResults {
		int leftWins, rightWins;
		int leftBest, rightBest;

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

		public MatchResults(int _leftWins, int _rightWins, int _leftBest, int _rightBest) {
			leftWins = _leftWins;
			rightWins = _rightWins;
			leftBest = _leftBest;
			rightBest = _rightBest;
		}
	}

	#endregion


	#region Private Variables

	private MatchResults results;

	#endregion


	#region Public API

	public void SetMatchResults(int leftWins, int rightWins, int leftBest, int rightBest) {
		results = new MatchResults(leftWins, rightWins, leftBest, rightBest);
	}

	public override void OnLobbyClientExit ()
	{
		base.OnLobbyClientExit ();
		Debug.Log("Client left lobby");
	}

	public static BushidoNetManager Get() {
		return GameObject.FindObjectOfType<BushidoNetManager>();
	}

	public override void OnLobbyServerPlayersReady() {
		Debug.Log("Both players ready");
	}

	public void OnBothPlayersReady() {
		ServerChangeScene("NetworkDuel");
	}

	#endregion

}
