using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class BushidoMatchMaker : MonoBehaviour {

	private MenuManager menu;

	void Start()
	{
		menu = GetComponent<MenuManager>();
		NetworkManager.singleton.StartMatchMaker();
	}

	public void QuickPlay() {
		NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnQuickPlayListMatches);
	}

	private void OnQuickPlayListMatches(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
		if (success)
		{
			if (matches.Count != 0)
			{
				Debug.Log("Match found - Joining...");

				//join the last server (just in case there are two...)
				NetworkManager.singleton.matchMaker.JoinMatch(matches[matches.Count - 1].networkId, "", "", "", 0, 0, OnJoinInternetMatch);
			}
			else
			{
				Debug.Log("No rooms found - Creating match...");
				CreateInternetMatch();
			}
		}
		else
		{
			Debug.LogError("Couldn't connect to match maker");
		}
	}

	//call this method to request a match to be created on the server
	public void CreateInternetMatch()
	{
		NetworkManager.singleton.matchMaker.CreateMatch("", 2, true, "", "", "", 0, 0, OnInternetMatchCreate);
	}

	//this method is called when your request for creating a match is returned
	private void OnInternetMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if (success)
		{
			//Debug.Log("Create match succeeded");

			MatchInfo hostInfo = matchInfo;
			NetworkServer.Listen(hostInfo, 9000);

			NetworkManager.singleton.StartHost(hostInfo);

			// Leave menu for network duel
			//menu.LeaveMenu("NetworkDuel");
		}
		else
		{
			Debug.LogError("Create match failed");
		}
	}

	//this method is called when your request to join a match is returned
	private void OnJoinInternetMatch(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if (success)
		{
			//Debug.Log("Able to join a match");

			MatchInfo hostInfo = matchInfo;
			NetworkManager.singleton.StartClient(hostInfo);

			// Leave menu for network duel
			//menu.LeaveMenu("NetworkDuel");
		}
		else
		{
			Debug.LogError("Join match failed");
		}
	}

}
