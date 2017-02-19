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
		leftCount = 0;
		rightCount = 0;
		RefreshCircles();
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
		RefreshCircles();
	}

	private void SignalRightWin() {
		rightCount++;
		RefreshCircles();
	}

	private void RefreshCircles() {
		LeftIcon1.enabled = leftCount > 0;
		LeftIcon2.enabled = leftCount > 1;
		LeftIcon3.enabled = leftCount > 2;
		LeftIcon4.enabled = leftCount > 3;

		RightIcon1.enabled = rightCount > 0;
		RightIcon2.enabled = rightCount > 1;
		RightIcon3.enabled = rightCount > 2;
		RightIcon4.enabled = rightCount > 3;
	}
}
