using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class LobbyManager : NetworkBehaviour {

	#region Public Accessors

	public MenuManager Menu {
		get {
			if (!menu) {
				menu = MenuManager.Get();
			}
			return menu;
		}
	}

	#endregion


	#region Editor References

	public Image LeftCheckbox, RightCheckbox;			// The left and right checkbox image elements
	public Image LeftSamurai, RightSamurai;				// The left and right samurai image elements

	public Button LeftReady, RightReady, NetReady;		// The left, right, and center ready button elements
	public Button LeftArrow, RightArrow;				// The left and right arrow button elements

	public Text BestOfText;								// The text element displaying the number of matches to be played

	#endregion


	#region Private Variables

	private MenuManager menu;							// The menu manager governing this lobby

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

	private bool localLobby;
	private bool playerIsHost;

	[SyncVar(hook = "OnHostReadyStatusChanged")]
	private bool hostReady;

	[SyncVar(hook = "OnClientReadyStatusChanged")]
	private bool clientReady;

	[SyncVar(hook = "OnHostEnteredLobby")]
	private bool hostInLobby;

	[SyncVar(hook = "OnClientEnteredLobby")]
	private bool clientInLobby;

	[Range (0, 2)]
	[SyncVar(hook = "OnBestOfIndexChanged")]
	private int bestOfIndex = 1;						// The current index in the array of options for the "best of" text

	private string[] bestOfOptions = {					// The array of options for the "best of" text
		"3", "5", "7"
	};


	#endregion


	#region Behaviour API

	// Use this for initialization
	void Start() {
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;

		/*
		if (!localLobby) {
			LeftSamurai.enabled = false;
			RightSamurai.enabled = false;
		}
		else {
			LeftSamurai.enabled = true;
			RightSamurai.enabled = true;
		}*/
	}

	#endregion


	#region Public API

	public static LobbyManager Get() {
		return FindObjectOfType<LobbyManager>();
	}

	// Returns whether the player was host or client
	public bool OnPlayerEnteredLobby() {
		// Update lobby status
		if (!hostInLobby) {
			hostInLobby = true;
			return true;
		}
		else {
			clientInLobby = true;
			return false;
		}
	}

	public void OnPlayerReady(bool hostSamurai) {
		// Update lobby ready status
		if (hostSamurai) {
			hostReady = true;
		}
		else {
			clientReady = true;
		}
	}

	// Prepare the lobby menu for a local lobby
	public void PrepareLocalLobby() {
		// Set preferences for local settings
		localLobby = true;
		bestOfIndex = 1;

		hostInLobby = true;
		clientInLobby = true;

		hostReady = false;
		clientReady = false;

		// Update UI
		UpdateBestOfText();
		NetReady.gameObject.SetActive(false);
		LeftReady.gameObject.SetActive(true);
		RightReady.gameObject.SetActive(true);
	}

	// Prepare the lobby menu for a network lobby
	public void PrepareNetworkLobby() {
		// Set preferences for network settings
		localLobby = false;
		bestOfIndex = 1;

		// Update UI
		UpdateBestOfText();
		NetReady.gameObject.SetActive(true);
		LeftReady.gameObject.SetActive(false);
		RightReady.gameObject.SetActive(false);
	}

	#endregion


	#region SyncVar Hooks

	private void OnHostReadyStatusChanged(bool newHostReady) {
		hostReady = newHostReady;
		UpdateLobbyReadyStatus();
	}

	private void OnClientReadyStatusChanged(bool newClientReady) {
		clientReady = newClientReady;
		UpdateLobbyReadyStatus();
	}

	private void OnHostEnteredLobby(bool newHostInLobby) {
		hostInLobby = newHostInLobby;
		UpdateLobbySamurai();
	}

	private void OnClientEnteredLobby(bool newClientInLobby) {
		clientInLobby = newClientInLobby;
		UpdateLobbySamurai();
	}

	private void OnBestOfIndexChanged(int newBestOfIndex) {
		bestOfIndex = newBestOfIndex;
		UpdateBestOfText();
	}

	#endregion


	#region Button Events 

	public void OnNetworkReady() {
		Menu.Audio.PlayMenuSound();

		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);

		// Find local player and give ready signal
		LobbyPlayer.GetLocalPlayer().CmdGiveReadySignal();
	}

	public void OnLocalLobbyReadyLeft() {
		Menu.Audio.PlayMenuSound();

		// Set host as ready
		hostReady = true;

		// Update UI
		LeftReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
	}

	public void OnLocalLobbyReadyRight() {
		Menu.Audio.PlayMenuSound();

		// Set client as ready
		clientReady = true;

		// Update UI
		RightReady.gameObject.SetActive(false);
		UpdateLobbyReadyStatus();
	}

	public void OnLobbyExit() {
		Menu.Audio.PlayMenuSound();

		if (localLobby) {
			Menu.ExitLocalLobby();
		}
		else {
			Menu.ExitNetworkLobby();
		}
	}

	public void OnLeftPressed() {
		Menu.Audio.PlayMenuSound();

		if (!localLobby) {
			LobbyPlayer.GetLocalPlayer().CmdChangeBestOfIndex(true);
		}
		else {
			ChangeBestOfIndex(true);
		}
	}

	public void OnRightPressed() {
		Menu.Audio.PlayMenuSound();
		if (!localLobby) {
			LobbyPlayer.GetLocalPlayer().CmdChangeBestOfIndex(false);
		}
		else {
			ChangeBestOfIndex(false);
		}
	}

	#endregion


	#region Private API

	void ClearReadyStatus() {
		hostReady = false;
		clientReady = false;

		if (localLobby) {
			UpdateLobbyReadyStatus();
		}
	}

	public void ChangeBestOfIndex(bool minus) {
		ClearReadyStatus();

		if (minus) {
			bestOfIndex = (bestOfIndex > 0) ? bestOfIndex - 1 : bestOfOptions.Length - 1;
		}
		else {
			bestOfIndex = (bestOfIndex < bestOfOptions.Length - 1) ? bestOfIndex + 1 : 0;
		}

		if (localLobby) {
			UpdateBestOfText();
		}
	}

	// Updates the samurai image elements based on presence of players
	private void UpdateLobbySamurai() {
		// Enable or disable the samurai images
		Debug.Log("left: " + hostInLobby);
		LeftSamurai.enabled = hostInLobby;
		RightSamurai.enabled = clientInLobby;
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyStatus() {
		// Update the checkbox images
		LeftCheckbox.sprite = (hostReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (clientReady) ? CheckedBox : UncheckedBox;

		// Update ready buttons
		if (localLobby) {
			LeftReady.gameObject.SetActive(!hostReady);
			RightReady.gameObject.SetActive(!clientReady);
		}
		else {
			UpdateNetworkReadyStatus();
		}
			
		// Check if both players are ready
		CheckReadyStatus();
	}

	private void UpdateNetworkReadyStatus() {
		if (Menu.MatchMaker.PlayingAsHost) {
			NetReady.gameObject.SetActive(!hostReady);
			LeftArrow.gameObject.SetActive(!hostReady);
			RightArrow.gameObject.SetActive(!hostReady);
		}
		else {
			NetReady.gameObject.SetActive(!clientReady);
			LeftArrow.gameObject.SetActive(!clientReady);
			RightArrow.gameObject.SetActive(!clientReady);
		}
	}

	private void CheckReadyStatus() {
		// Check if both players are ready
		if (hostReady && clientReady) {
			// Turn off arrow buttons in local lobby
			if (localLobby) {
				LeftArrow.gameObject.SetActive(false);
				RightArrow.gameObject.SetActive(false);
			}

			// Begin countdown to round begin
			Debug.Log("Countdown to scene launch");
			int.TryParse(BestOfText.text, out BushidoNetManager.Get().matchLimit);
			Menu.OnBothPlayersReady(localLobby);
		}
	}

	// Updates "best of" match number text from options array
	private void UpdateBestOfText() {
		ChangeTextInChildText(bestOfOptions[bestOfIndex], BestOfText);
	}

	// Changes text for a label and its child (shadow) label
	private void ChangeTextInChildText(string newText, Text text) {
		Text child = GetTextComponentInChild(text);
		text.text = newText;
		child.text = newText;
	}

	// Returns the child text component of a text component
	private Text GetTextComponentInChild(Text text) {
		return text.transform.GetChild(0).GetComponentInChildren<Text>();
	}

	#endregion
}
