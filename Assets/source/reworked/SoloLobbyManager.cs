using UnityEngine;
using System.Collections;

public class SoloLobbyManager : BaseLobbyManager {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	#region Event Blocks

	// Called when both players are ready
	protected override void OnAllPlayersReady() {
		// Hide the game settings controls
		SetInteractiveUIVisible(false);

		// Set the win limit in the game scene
		BushidoMatchInfo.Get().SetMatchLimit(BestOfNumText.text);
	}	

	// Called when the ready status is reset
	protected override void OnClearReadyStatus() {
		// Show the ready buttons and win limit selector
		SetInteractiveUIVisible(true);
	}

	// Called when the countdown reaches zero
	protected override void OnCountdownComplete() {
		// Leave the scene and head to the duel
		Globals.Menu.LeaveForDuelScene();
	}

	#endregion


	#region Public API

	public void PrepareLobby() {

	}

	#endregion


	#region Private API

	// Change visibility of the "best-of" selector and the ready button
	protected override void SetInteractiveUIVisible(bool visible) {
		LeftArrow.gameObject.SetActive(visible);
		RightArrow.gameObject.SetActive(visible);
	}

	#endregion
}
