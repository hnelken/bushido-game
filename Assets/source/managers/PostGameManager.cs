using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PostGameManager : MonoBehaviour {

	public Text MainText;
	public Text LeftWins, RightWins;
	public Text LeftBest, RightBest;
	public Image LeftCheckbox, RightCheckbox;

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
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;

		var netManager = BushidoNetManager.Get();
		LeftWins.text = "" + netManager.Results.LeftWins;
		RightWins.text = "" + netManager.Results.RightWins;
		LeftBest.text = "" + netManager.Results.LeftBest;
		RightBest.text = "" + netManager.Results.RightBest;
	}

	public void RematchPressed() {
		Debug.Log("Rematch");
	}
}
