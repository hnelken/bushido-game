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

		EventManager.GameTie += ShowTie;
		EventManager.GameWin += ShowAttack;
		EventManager.GameStrike += ShowStrike;
		EventManager.WinResult += ShowWinResult;
		EventManager.GameReset += ClearForNewRound;
	}

	public void UpdateTimer(float startTime) 
	{
		int time = (int)(100 * (Time.realtimeSinceStartup - startTime));
		ReactionTimer.text = time.ToString();
	}

	public void ToggleFlag()
	{
		CenterPiece.SetActive(!CenterPiece.activeSelf);
	}

	private void ShowTie() {
		ToggleFlag();
		WinText.text = "TIE!";
		WinText.enabled = true;
	}

	private void ShowWinResult()
	{
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;
		
		WinText.text = (manager.lastWinner == BasicDuelManager.LastWinner.LEFT) ? "Player 1 WINS!" : "Player 2 WINS!";
		WinText.enabled = true;
	}

	private void ShowAttack() {
		ToggleFlag();
	}

	private void ShowStrike()
	{
		bool leftSamurai = false;
		if (manager.lastWinner == BasicDuelManager.LastWinner.STRIKELEFT) {
			leftSamurai = true;
		}
		string player = (leftSamurai) ? "Player 1 " : "Player 2 ";
		WinText.text = player + "struck too early!";
		WinText.enabled = true;
	}

	private void ClearForNewRound()
	{
		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;
		
		ReactionTimer.text = "--";
	}
	
	private void RefreshWinCounts()
	{
		LeftCount.text = manager.LeftSamurai.GetWinCount();
		RightCount.text = manager.RightSamurai.GetWinCount();
	}
}
