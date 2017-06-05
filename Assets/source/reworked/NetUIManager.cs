using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetUIManager : BaseUIManager {

	// Initialization
	void Start() {
		// Get required manager component
		manager = GetComponent<BaseDuelManager>();
	}
}
