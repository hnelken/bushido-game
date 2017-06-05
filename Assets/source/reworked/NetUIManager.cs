using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetUIManager : BaseUIManager {

	// Provide correct scene for post game
	protected override void LeaveScene() {
		EventManager.Nullify();
		SceneManager.LoadScene(Globals.NetPostScene);
	}
}
