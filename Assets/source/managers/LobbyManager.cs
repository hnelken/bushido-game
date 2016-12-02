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

	[Range (0, 2)]
	private int bestOfIndex = 1;						// The current index in the array of options for the "best of" text
	private string[] bestOfOptions = {					// The array of options for the "best of" text
		"3", "5", "7"
	};

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

	[SyncVar(hook = "OnHostReadyStatusChanged")]
	private bool hostReady;

	[SyncVar(hook = "OnClientReadyStatusChanged")]
	private bool clientReady;

	[SyncVar(hook = "OnHostEnteredLobby")]
	private bool hostInLobby;

	[SyncVar(hook = "OnClientEnteredLobby")]
	private bool clientInLobby;

	private bool localLobby;

	#endregion


	#region Behaviour API

	// Use this for initialization
	void Start() {
		LeftCheckbox.sprite = UncheckedBox;
		RightCheckbox.sprite = UncheckedBox;

		if (!localLobby) {
			LeftSamurai.enabled = false;
			RightSamurai.enabled = false;
		}
		else {
			LeftSamurai.enabled = true;
			RightSamurai.enabled = true;
		}
	}

	#endregion


	#region Public API

	public IEnumerator CountDown() {
		yield return new WaitForSeconds(5);
		Debug.Log("Changing scene");

		if (localLobby) {
			Menu.LeaveMenu("LocalDuel");
		}
		else {
			Menu.LeaveMenu("NetworkDuel");
		}
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

	public void GoToLocalScene() {
		// Set game wins based on UI and begin countdown
		int.TryParse(BestOfText.text, out BushidoNetManager.Get().matchLimit);
		StartCoroutine(CountDown());
	}

	// Prepare the lobby menu for a local lobby
	public void PrepareLocalLobby() {
		// Set preferences for local settings
		localLobby = true;
		bestOfIndex = 1;

		hostInLobby = true;
		hostReady = false;

		clientInLobby = true;
		clientReady = false;

		// Update UI
		UpdateBestOfText();
		UpdateLobbySamurai();
		UpdateLobbyReadyBoxes();
		NetReady.gameObject.SetActive(false);
		LeftReady.gameObject.SetActive(true);
		RightReady.gameObject.SetActive(true);
	}

	// Prepare the lobby menu for a network lobby
	public void PrepareNetworkLobby() {
		// Set preferences for network settings
		localLobby = false;
		bestOfIndex = 1;

		hostInLobby = false;
		hostReady = false;

		clientInLobby = false;
		clientReady = false;

		// Update UI
		UpdateBestOfText();
		UpdateLobbySamurai();
		UpdateLobbyReadyBoxes();
		NetReady.gameObject.SetActive(true);
		LeftReady.gameObject.SetActive(false);
		RightReady.gameObject.SetActive(false);
	}

	#endregion


	#region SyncVar Hooks

	private void OnHostReadyStatusChanged(bool newHostReady) {
		hostReady = newHostReady;
		UpdateLobbyReadyBoxes();
	}

	private void OnClientReadyStatusChanged(bool newClientReady) {
		clientReady = newClientReady;
		UpdateLobbyReadyBoxes();
	}

	private void OnHostEnteredLobby(bool newHostInLobby) {
		hostInLobby = newHostInLobby;
		UpdateLobbySamurai();
	}

	private void OnClientEnteredLobby(bool newClientInLobby) {
		clientInLobby = newClientInLobby;
		UpdateLobbySamurai();
	}

	#endregion


	#region Button Events 

	public void OnNetworkReady() {
		Menu.Audio.PlayMenuSound();

		// Find local player and give ready signal
		foreach (LobbyPlayer player in LobbyPlayer.GetAll()) {
			if (player.isLocalPlayer) {
				player.CmdGiveReadySignal();
				return;
			}
		}
	}

	public void OnLocalLobbyReadyLeft() {
		Menu.Audio.PlayMenuSound();

		// Set host as ready
		hostReady = true;

		// Update UI
		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);
		LeftReady.gameObject.SetActive(false);
		UpdateLobbyReadyBoxes();
	}

	public void OnLocalLobbyReadyRight() {
		Menu.Audio.PlayMenuSound();

		// Set client as ready
		clientReady = true;

		// Update UI
		LeftArrow.gameObject.SetActive(false);
		RightArrow.gameObject.SetActive(false);
		RightReady.gameObject.SetActive(false);
		UpdateLobbyReadyBoxes();
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
		bestOfIndex = (bestOfIndex > 0)
			? bestOfIndex - 1
			: bestOfOptions.Length - 1;
		UpdateBestOfText();
	}

	public void OnRightPressed() {
		Menu.Audio.PlayMenuSound();
		bestOfIndex = (bestOfIndex < bestOfOptions.Length - 1) 
			? bestOfIndex + 1
			: 0;
		UpdateBestOfText();
	}

	#endregion


	#region Private API

	// Updates the samurai image elements based on presence of players
	private void UpdateLobbySamurai() {
		// Enable or disable the samurai images
		LeftSamurai.enabled = hostInLobby;
		RightSamurai.enabled = clientInLobby;
	}

	// Updates the lobby ready checkbox images based on player ready status
	private void UpdateLobbyReadyBoxes() {
		// Update the checkbox images
		LeftCheckbox.sprite = (hostReady) ? CheckedBox : UncheckedBox;
		RightCheckbox.sprite = (clientReady) ? CheckedBox : UncheckedBox;

		// Check if both players are ready
		if (hostReady && clientReady) {
			// Begin countdown to round begin
			Debug.Log("Countdown to scene launch");
			StartCoroutine(CountDown());
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
