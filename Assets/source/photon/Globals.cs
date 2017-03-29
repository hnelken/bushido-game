using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

	public static Globals Get() {
		return FindObjectOfType<Globals>();
	}

	// Safe reference to the source of all game audio
	private static AudioManager audioManager;
	public static AudioManager Audio {
		get {
			if (!audioManager) {
				audioManager = AudioManager.Get();
			}
			return audioManager;
		}
	}
		
	// The menu manager governing this lobby
	private static PUNMenuManager menu;
	public static PUNMenuManager Menu {
		get {
			if (!menu) {
				menu = PUNMenuManager.Get();
			}
			return menu;
		}
	}

}
