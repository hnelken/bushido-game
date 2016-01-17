using UnityEngine;
using System.Collections;

public class TouchControl : MonoBehaviour {

	private bool touching;

	// Use this for initialization
	void Start () {
		touching = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!touching && Input.touchCount > 0) {
			touching = true;
		}
	}
}
