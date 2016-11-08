using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	private bool input = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (!input && ReceivedInput()) {
			input = true;
			EventManager.Nullify();
			Application.LoadLevel("BasicDuel");
		}

	}

	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetKeyDown(KeyCode.Space);
	}
}
