using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (BasicDuelManager))]
public class UIManager : MonoBehaviour {

	// Public (editor) references
	public SpriteRenderer LeftSamurai, RightSamurai;
	public GameObject CenterPiece;
	
	public Text LeftCount, RightCount, WinText;
	public Text ReactionTimer;
	
	// Private fields
	private BasicDuelManager manager;

	// Use this for initialization
	void Start()
	{
		manager = GetComponent<BasicDuelManager>();
		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;
		CenterPiece.SetActive(false);

		EventManager.GameWin += ShowWinner;
	}
	
	public void ShowWinner()
	{
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;
		
		WinText.text = (manager.lastWinner == BasicDuelManager.LastWinner.LEFT) ? "Player 1 WINS" : "Player 2 WINS";
		WinText.enabled = true;
	}
	
	public void ToggleFlag()
	{
		CenterPiece.SetActive(!CenterPiece.activeSelf);
	}
	
	public void SignalPlayerReaction(bool valid)
	{
		CenterPiece.SetActive(false);
		if (valid)
		{	// Show successful attack

		}
		else
		{	// Show false start
			
		}
	}
	
	public void ClearForNewRound()
	{
		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;
		
		ReactionTimer.text = "--";
		
		// Reset samurai positions
	}
	
	public void UpdateTimer(int time) 
	{
		ReactionTimer.text = time.ToString();
	}

	private void ShowStrike(bool leftSamurai)
	{
		string player = (leftSamurai) ? "Player 1 " : "Player 2 ";
		WinText.text = player + "struck too early!";
		WinText.enabled = true;
	}
	
	private void RefreshWinCounts()
	{
		LeftCount.text = manager.LeftSamurai.GetWinCount();
		RightCount.text = manager.RightSamurai.GetWinCount();
	}
}
