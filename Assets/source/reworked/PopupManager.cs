using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour {

	// The delegate for action when "OK" is pressed
	public delegate void Action();
	public Action OkAction, CancelAction;

	// UI references
	public GameObject Popup;
	public GameObject LeftOKButton, CenterOKButton, CancelButton;
	public Text MessageText;

	// Use this for initialization
	void Start () {
		this.Popup.SetActive(false);
	}

	public void Initialize(Action okAction, string message) {
		Initialize(okAction, null, message, false);
	}

	// Initialize the popup action and message
	public void Initialize(Action okAction, Action cancelAction, string message, bool cancellable) {
		// Set actions for "Ok" and Cancel"
		this.OkAction = okAction;
		this.CancelAction = cancelAction;

		// Set popup message
		this.MessageText.text = message;

		// Hide/Show UI
		LeftOKButton.SetActive(cancellable);
		CenterOKButton.SetActive(!cancellable);
		CancelButton.SetActive(cancellable);

		// Show popup
		TogglePopup();
	}

	// Handles player "ok-ing" the popup
	public void OkPressed() {
		Globals.Audio.PlayMenuSound();

		// Hide popup menu
		TogglePopup();

		// Trigger popup "OK" reaction
		if (OkAction != null) {
			OkAction();
		}
	}

	public void CancelPressed() {
		Globals.Audio.PlayMenuSound();

		// Hide popup menu
		TogglePopup();

		if (CancelAction != null) {
			CancelAction();
		}
	}

	// Toggle the popup visibility
	private void TogglePopup() {
		this.Popup.SetActive(!Popup.activeSelf);
	}
}