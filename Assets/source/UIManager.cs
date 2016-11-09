using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (BasicDuelManager))]
public class UIManager : MonoBehaviour {

	// Public (editor) references
	public SpriteRenderer LeftSamurai, RightSamurai;
	public Text LeftCount, RightCount, WinText;
	public Text ReactionTimer;
	public Image Flag;
	
	// Private fields
	private BasicDuelManager manager;
	private float startTime;
	private bool timing;

	// Use this for initialization
	void Start()
	{
		manager = GetComponent<BasicDuelManager>();
		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;
		Flag.enabled = false;

		EventManager.GameTie += ShowTie;
		EventManager.GameWin += ShowAttack;
		EventManager.GameStrike += ShowStrike;
		EventManager.WinResult += ShowWinResult;
		EventManager.GameReset += ClearForNewRound;
		EventManager.GameOver += ShowMatchWin;
	}

	void Update() {
		if (timing) {
			UpdateTimer();
		}
	}

	public void StartTimer(float _startTime) {
		startTime = _startTime;
		timing = true;
	}

	public void StopTimer() {
		timing = false;
	}

	public void ToggleFlag()
	{
		Flag.enabled = !Flag.enabled;
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
		
		WinText.text = (manager.roundResult == BasicDuelManager.RoundResult.WINLEFT) ? "Player 1 WINS!" : "Player 2 WINS!";
		WinText.enabled = true;
	}

	private void ShowAttack() {
		ToggleFlag();
	}

	private void ShowStrike()
	{
		bool leftSamurai = false;
		if (manager.roundResult == BasicDuelManager.RoundResult.STRIKELEFT) {
			leftSamurai = true;
		}
		string player = (leftSamurai) ? "Player 1 " : "Player 2 ";
		WinText.text = player + "struck too early!";
		WinText.enabled = true;
	}

	private void ShowMatchWin() {
		bool leftSamurai = false;
		if (manager.roundResult == BasicDuelManager.RoundResult.WINLEFT) {
			leftSamurai = true;
		}
		string player = (leftSamurai) ? "Player 1 " : "Player 2 ";
		WinText.text = player + "wins the match!";
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
		LeftCount.text = "P1: " + manager.LeftSamurai.GetWinCount();
		RightCount.text = "P2: " + manager.RightSamurai.GetWinCount();
	}

	private void UpdateTimer() 
	{
		int time = manager.GetReactionTime();
		ReactionTimer.text = time.ToString();
	}
}
