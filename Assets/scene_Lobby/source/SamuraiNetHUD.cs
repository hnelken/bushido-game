using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SamuraiNetHUD : MonoBehaviour {

	private NetworkLobbyManager manager;
	private bool showServer;

	// Use this for initialization
	void Start () {
		manager = GetComponent<NetworkLobbyManager>();
		manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
	}
	
	// Update is called once per frame
	void Update () {
		if (!NetworkServer.active && !NetworkClient.active)
		{
			if (manager.matchMaker == null)
			{
				/* CHOOSE TO START MATCH MAKER HERE
				if ("Enable Match Maker (M)")
				{
					manager.StartMatchMaker();
				}
				 */
			}
			else
			{
				if (manager.matchInfo == null)
				{
					if (manager.matches == null)
					{
						/* DECIDE TO CREATE OR JOIN A MATCH HERE
						if ("Create Internet Match")
						{
							manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
						}

						if ("Find Internet Match"))
						{
							manager.matchMaker.ListMatches(0,20, "", manager.OnMatchList);
						}
						*/
					}
					else
					{
						foreach (var match in manager.matches)
						{
							/* DISPLAY AVAILABLE MATCHES HERE
							if ("Join Match:" + match.name))
							{
								manager.matchName = match.name;
								manager.matchSize = (uint)match.currentSize;
								manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
							}
							*/
						}
					}
				}

				/* BACK OUT OF MATCHMAKING HERE
				if ("Disable Match Maker"))
				{
					manager.StopMatchMaker();
				}
				*/
			}
		}
	}


}
