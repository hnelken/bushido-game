using UnityEngine;
using System.Collections;

public class NaiveGameController : MonoBehaviour {

	public GameObject leftSamurai, rightSamurai;
	
	private enum LastWinner { LEFT, RIGHT }

	private bool waitingForInput;
	private bool flagPopped;
	private LastWinner lastWinner;

	// Use this for initialization
	void Start () {
		waitingForInput = false;
		flagPopped = false;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void SamuraiTrigger(bool leftSamurai) {
		if (waitingForInput) {

		}
	}
}
