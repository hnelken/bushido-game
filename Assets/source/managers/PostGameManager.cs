using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PostGameManager : MonoBehaviour {

	public Text MainText;
	public Text LeftWins, RightWins;
	public Text LeftBest, RightBest;
	public Image LeftCheckbox, RightCheckbox;
	public FadingShade Shade;

	// The sprite for the checked box
	private Sprite checkedBox;
	private Sprite CheckedBox {
		get {
			if (!checkedBox) {
				checkedBox = Resources.Load<Sprite>("sprites/checkbox-checked");
			}
			return checkedBox;
		}
	}

	// The sprite for the unchecked box
	private Sprite uncheckedBox;
	private Sprite UncheckedBox {
		get {
			if (!uncheckedBox) {
				uncheckedBox = Resources.Load<Sprite>("sprites/checkbox-unchecked");
			}
			return uncheckedBox;
		}
	}

	// Use this for initialization
	void Start() {
		// Initialize some UI
		Shade.Initialize();
		MainText.gameObject.SetActive(false);
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;

		// Get match results from net manager
		var netManager = BushidoNetManager.Get();

		if (netManager) {
		// Change UI to show number of wins for each player
		LeftWins.text = "" + netManager.Results.LeftWins;
		RightWins.text = "" + netManager.Results.RightWins;

		// Change UI to show the best reaction time of each player
		int leftBest = netManager.Results.LeftBest;
		int rightBest = netManager.Results.RightBest;
		LeftBest.text = (leftBest == -1) ? "xx" : "" + leftBest;
		RightBest.text = (rightBest == -1) ? "xx" : "" + rightBest;
		}

	}

	// Send rematch signal
	public void RematchPressed() {
		Debug.Log("Rematch");
	}

	// Prepare to leave the match
	public void LeaveMatchPressed() {
		Debug.Log("Leave match");
		Shade.Toggle();
	}
}
