using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TouchControl : NetworkBehaviour {

	private bool touching;
	public override void OnStartLocalPlayer()
	{
		// Initialize
		touching = false;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) 
		{
			return;
		}

		CheckInput();
	}

	private void CheckInput() 
	{
		// Check for input via touch or keyboard
		if (!touching && (Input.touchCount > 0 || 
		                  Input.GetKeyDown(KeyCode.Space)))
		{
			touching = true;
			CmdTriggerReaction();
			
		}	// Also check for end of input
		else if (touching && (Input.touchCount == 0 ||
		                      Input.GetKeyUp(KeyCode.Space)))
		{
			touching = false;
		}
	}

	[Command]
	void CmdTriggerReaction()
	{
		RpcReceiveReaction();
	}

	[ClientRpc]
	void RpcReceiveReaction()
	{
		GameManager.Get().TriggerReaction();
	}


}
