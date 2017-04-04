using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/*
 * This class is the manager for the main menu of the game
 */
public class MenuManager : MonoBehaviour {
	
	#region Editor References

	public Text TitleText;										// The title text element
	public GlowingText PlayText;								// The glowing "tap-to-play" text element
	public FadingShade Shade;									// The black image used for fading in and out of scenes
	public GameObject MultiPlayMenu, NetworkMenu;				// The parent objects for some menu segments
	public GameObject LobbyMenu, NearbyMenu;					// The parent objects for other menu segments
	public GameObject ExitButton;								// The parent object of the exit button UI element

	public LobbyManager Lobby;									// Reference to the lobby manager object

	#endregion



	// Safe reference to the source of all game audio
	public AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}

	// Safe reference to the match maker for net games
	public BushidoMatchMaker MatchMaker {
		get {
			if (!matchMaker) {
				matchMaker = GetComponent<BushidoMatchMaker>();
			}
			return matchMaker;
		}
	}


	#region Private Variables

	private AudioManager audioManager;				// Unsafe reference to audio source
	private BushidoMatchMaker matchMaker;			// Unsafe reference to the match maker

	private bool nearbyMenuOpen;					// True if the nearby menu is open, false if not
	private bool localMenuOpen;						// True if a local-play menu is open, false if a net-play menu is open
	private bool leavingMenu;						// True if a game is about to begin and the menu scene must be left
	private bool pastOpenMenu;						// True if the opening menu is no longer showing

	private string nextSceneName;					// The string name of the scene the menu should transition to next

	#endregion


	#region Behaviour API

	// Use this for initialization
	void Start () {

		// Enable lobby manager
		Lobby.gameObject.SetActive(true);

		// Initialize some UI elements
		Shade.Initialize();
		PlayText.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

		// Handle interactions before opening menu is past
		if (!pastOpenMenu) {
			// Enable glowing "tap-to-play" text when shade is gone
			if (Shade.IsHidden && !PlayText.enabled) {
				PlayText.Initialize();
			}

			// Check for input that exits opening menu
			CheckForIntroSkip();
		}
		else {
			// Check if the scene is being exited
			CheckForMenuExit();
		}
	}

	#endregion


	#region Public API

	// Used to gain a reliable reference to this manager
	public static MenuManager Get() {
		return FindObjectOfType<MenuManager>();
	}

	// Triggered when a network player enters the lobby
	// Returns true if the player is the host, false if the client
	public bool OnNetworkPlayerEnteredLobby() {
		return Lobby.OnPlayerEnteredLobby();
	}

	// Manage UI to show a network lobby
	public void ShowNetworkLobby() {
		PlayText.enabled = false;
		ToggleNetworkLobby();
	}

	// Manage UI to leave a local lobby
	public void ExitLocalLobby() {
		ToggleLocalLobby();
		TogglePlayMenu();
	}

	// Manage leaving a network lobby
	public void ExitNetworkLobby() {
		/*
		// Quit match
		if (matchMaker.PlayingAsHost) {
			NetworkManager.singleton.StopHost();
		}
		else {
			NetworkManager.singleton.StopClient();
		}
		*/
	}

	// Begins the animations that lead to desired scene change
	public void LeaveMenu() {
		nextSceneName = localMenuOpen ? "LocalDuel" : "NetworkDuel";
		leavingMenu = true;
		Shade.Toggle();
	}

	#endregion


	#region Private API

	// Handle animating UI elements when leaving menu scene
	private void CheckForMenuExit() {
		// After shade is black, leave menu
		if (!Shade.IsHidden && leavingMenu) {
			leavingMenu = false;

			// Clear event listeners
			EventManager.Nullify();

			// Change scene
			if (localMenuOpen) {
				SceneManager.LoadScene(nextSceneName);
			}
			else {
				BushidoNetManager.Get().LaunchNetworkDuel();
			}
		}
	}

	// Check for tap input that skips opening menu
	private void CheckForIntroSkip() {
		if (ReceivedInput()) {
			// Input will either skip title slide or open main menu
			if (Shade.IsHidden) {
				AudioManager.Get().PlayMenuSound();

				// Hide play text and show play menu
				PlayText.enabled = false;
				MultiPlayMenu.SetActive(true);

				// Prevent further non-button input
				pastOpenMenu = true;
			}
		}
	}

	// Checks for any input this frame (touch or spacebar)
	private bool ReceivedInput() {
		return (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
			|| Input.GetKeyDown(KeyCode.Space);
	}

	// Toggle visibility of multiplayer menu
	private void TogglePlayMenu() {
		MultiPlayMenu.SetActive(!MultiPlayMenu.activeSelf);
	}

	// Toggle visibility of network match menu
	private void ToggleNetworkMenu() {
		NetworkMenu.SetActive(!NetworkMenu.activeSelf);
	}

	private void ToggleNearbyMenu() {
		NearbyMenu.SetActive(!NearbyMenu.activeSelf);
	}

	private void ToggleExitButton() {
		ExitButton.SetActive(!ExitButton.activeSelf);
	}

	// Toggle visibility of lobby menu
	private void ToggleLobbyMenu() {
		TitleText.gameObject.SetActive(LobbyMenu.activeSelf);
		LobbyMenu.SetActive(!LobbyMenu.activeSelf);
	}

	// Prepare network lobby and toggle menu
	private void ToggleNetworkLobby() {
		// Prepare network lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			Lobby.PrepareNetworkLobby();
		}
		ToggleLobbyMenu();
	}

	// Prepare local lobby and toggle menu
	private void ToggleLocalLobby() {
		// Prepare local lobby if menu is not visible
		if (!LobbyMenu.activeSelf) {
			Lobby.PrepareLocalLobby();
		}
		ToggleLobbyMenu();
	}

	private void QuickPlay() {
		// Find opponent via quick play matchaking
		MatchMaker.QuickPlay();

		// Setup UI to show matchmaking process
		PlayText.text = "Finding a game";
		PlayText.enabled = true;
	}

	#endregion


	#region ButtonEvents

	// Close the network match menu
	public void OnMenuExit() {
		AudioManager.Get().PlayMenuSound();

		if (!nearbyMenuOpen) {
			// Close the network menu and show multi-play menu
			ToggleNetworkMenu();
			ToggleExitButton();
			TogglePlayMenu();
		}
		else {
			// Stop any running broadcasts
			//BushidoNetManager.Get().Discovery.ExitBroadcast();

			// Close the nearby game menu and show net-game menu
			ToggleNearbyMenu();
			ToggleNetworkMenu();
		}
	}

	// Open the local game lobby
	public void OnLocalGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = true;

		// Hide multiplay menu and show local lobby
		TogglePlayMenu();
		ToggleLocalLobby();
	}

	// Open the network game menu
	public void OnNetworkGameSelect() {
		AudioManager.Get().PlayMenuSound();
		localMenuOpen = false;

		// Hide multiplay menu and show network lobby
		TogglePlayMenu();
		QuickPlay();

		/* 
		 * --UNCOMMENT WHEN NEARBY GAME FEATURE IS AVAILABLE--
		 * 
		// Hide multiplay menu and show network game menu
		TogglePlayMenu();
		ToggleNetworkMenu();
		ToggleExitButton();
		*/
	}

	// Trigger quick play matchmaking and open lobby
	public void OnQuickPlayPressed() {
		AudioManager.Get().PlayMenuSound();

		// Setup UI to show matchmaking process
		ToggleNetworkMenu();
		ToggleExitButton();

		// Find opponent via quick play matchaking
		QuickPlay();
	}

	// Trigger nearby matchmaking process and open lobby
	public void OnNearbyGamePressed() {
		// TODO: use local discovery to find nearby opponent
		AudioManager.Get().PlayMenuSound();

		// Toggle discovery game menu
		ToggleNetworkMenu();
		ToggleNearbyMenu();
		nearbyMenuOpen = true;
	}

	public void OnStartNearbyGamePressed() {
		// Begin broadcasting on local network
		//BushidoNetManager.Get().Discovery.StartNearbyGame();
		ToggleNearbyMenu();
		ToggleExitButton();
	}

	public void OnFindNearbyGamePressed() {
		//BushidoNetManager.Get().Discovery.FindNearbyGame();
		ToggleNearbyMenu();
		ToggleExitButton();

		// Setup UI to show search process
		PlayText.text = "Finding a game nearby";
		PlayText.enabled = true;
	}

	#endregion
}
