using UnityEngine;
using System.Collections;

public class BushidoMatchInfo : MonoBehaviour {

	// The selected difficulty level of a solo game
	private int soloDifficulty;
	public int SoloDifficulty {
		get {
			return soloDifficulty;
		}
	}

	// The last match limit set in a lobby
	private int matchLimit;
	public int MatchLimit {
		get {
			return matchLimit;
		}
	}

	// The results struct from the last complete game
	private MatchResults results;
	public MatchResults Results {
		get { return results; }
	}

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public static BushidoMatchInfo Get() {
		return GameObject.FindObjectOfType<BushidoMatchInfo>();
	}

	public void EndMatch() {
		Destroy(gameObject);
	}

	public void SetSinglePlayerSettings(int difficulty) {
		soloDifficulty = difficulty;
	}

	public void SetMatchLimit(string limitText) {
		int.TryParse(limitText, out matchLimit);
	}

	public void SetMatchResults(int leftWins, int rightWins, int leftBest, int rightBest, bool networked) {
		results = new MatchResults(leftWins, rightWins, leftBest, rightBest, networked);
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
}
