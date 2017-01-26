using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinCountManager : MonoBehaviour {

	private Image leftIcon1;
	private Image LeftIcon1 {
		get {
			if (!leftIcon1) {
				leftIcon1 = GameObject.Find("LeftWinIcon1").GetComponent<Image>();
			}
			return leftIcon1;
		}
	}

	private Image leftIcon2;
	private Image LeftIcon2 {
		get {
			if (!leftIcon2) {
				leftIcon2 = GameObject.Find("LeftWinIcon2").GetComponent<Image>();
			}
			return leftIcon2;
		}
	}

	private Image leftIcon3;
	private Image LeftIcon3 {
		get {
			if (!leftIcon3) {
				leftIcon3 = GameObject.Find("LeftWinIcon3").GetComponent<Image>();
			}
			return leftIcon3;
		}
	}

	private Image leftIcon4;
	private Image LeftIcon4 {
		get {
			if (!leftIcon4) {
				leftIcon4 = GameObject.Find("LeftWinIcon4").GetComponent<Image>();
			}
			return leftIcon4;
		}
	}

	private Image rightIcon1;
	private Image RightIcon1 {
		get {
			if (!rightIcon1) {
				rightIcon1 = GameObject.Find("RightWinIcon1").GetComponent<Image>();
			}
			return rightIcon1;
		}
	}

	private Image rightIcon2;
	private Image RightIcon2 {
		get {
			if (!rightIcon2) {
				rightIcon2 = GameObject.Find("RightWinIcon2").GetComponent<Image>();
			}
			return rightIcon2;
		}
	}

	private Image rightIcon3;
	private Image RightIcon3 {
		get {
			if (!rightIcon3) {
				rightIcon3 = GameObject.Find("RightWinIcon3").GetComponent<Image>();
			}
			return rightIcon3;
		}
	}

	private Image rightIcon4;
	private Image RightIcon4 {
		get {
			if (!rightIcon4) {
				rightIcon4 = GameObject.Find("RightWinIcon4").GetComponent<Image>();
			}
			return rightIcon4;
		}
	}

	private int leftCount, rightCount;

	// Use this for initialization
	void Start () {
		LeftIcon1.enabled = false;
		LeftIcon2.enabled = false;
		LeftIcon3.enabled = false;
		LeftIcon4.enabled = false;

		RightIcon1.enabled = false;
		RightIcon2.enabled = false;
		RightIcon3.enabled = false;
		RightIcon4.enabled = false;
	}

	public void SignalWin(bool leftWon) {
		if (leftWon) {
			SignalLeftWin();
		}
		else {
			SignalRightWin();
		}
	}

	private void SignalLeftWin() {
		leftCount++;
		switch (leftCount) {
		case 1:
			LeftIcon1.enabled = true;
			break;
		case 2:
			LeftIcon2.enabled = true;
			break;
		case 3:
			LeftIcon3.enabled = true;
			break;
		case 4:
			LeftIcon4.enabled = true;
			break;
		default:
			break;
		}
	}

	private void SignalRightWin() {
		rightCount++;
		switch (rightCount) {
		case 1:
			RightIcon1.enabled = true;
			break;
		case 2:
			RightIcon2.enabled = true;
			break;
		case 3:
			RightIcon3.enabled = true;
			break;
		case 4:
			RightIcon4.enabled = true;
			break;
		default:
			break;
		}
	}
}
