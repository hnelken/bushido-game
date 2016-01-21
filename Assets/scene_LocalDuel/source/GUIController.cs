using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

	// Public (editor) references
	public GameObject CenterPiece;

	public Text LeftCount, RightCount, WinText;
	public Text ReactionTimer;
	public Image FlashScreen;
	public Animator TopLine, BottomLine;

	// Private fields
	//private NaiveGameController controller;
	private GameManager manager;
	private bool screenFlashed;
	private bool flashingWhite;
	private bool fadingIn;

	public void SetController(GameManager manager) 
	{
		this.manager = manager;
	}

	// Use this for initialization
	void Start()
	{
		FlashScreen.enabled = false;
		screenFlashed = false;
		flashingWhite = true;
		fadingIn = true;

		LeftCount.enabled = false;
		RightCount.enabled = false;
		WinText.enabled = false;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (screenFlashed) 
		{
			if (!flashingWhite) 
			{
				FadeBlack();
			}
			else
			{
				FlashWhiteAndFade();
			}
		}
	}
	
	private IEnumerator WaitAndSignalRoundStart()
	{	
		yield return new WaitForSeconds (3.5f);
		
		manager.BeginNewRound();
	}

	public void PopFlag()
	{
		CenterPiece.SetActive(true);
	}

	public void SignalPlayerReaction(bool win)
	{
		CenterPiece.SetActive(false);
		if (win)
		{	// Show player win
			ShowWin();
		}
		else
		{	// Show player strike

		}
	}

	public void UpdateTimer(int time) 
	{
		ReactionTimer.text = time.ToString();
	}

	public void FlashWhiteFadeOut() 
	{
		flashingWhite = true;
		screenFlashed = true;
		FlashScreen.enabled = true;
	}

	public void FadeBlackFadeOut() 
	{
		fadingIn = true;
		flashingWhite = false;
		screenFlashed = true;
		FlashScreen.enabled = true;
	}
	
	private void FlashWhiteAndFade()
	{
		if (FlashScreen.color.a > 0) 
		{
			Color flashColor = FlashScreen.color;
			flashColor.a -= 0.01f;
			FlashScreen.color = flashColor;
		}
		else
		{
			screenFlashed = false;
			Color flashColor = Color.black;
			flashColor.a = 0f;
			FlashScreen.color = flashColor;
		}
	}
	
	private void FadeBlack() 
	{
		if (fadingIn) 
		{
			FadeBlackIn();
		}
		else 
		{	// Fading out
			FadeBlackOut();
		}
	}

	private void FadeBlackIn() 
	{
		if (FlashScreen.color.a < 1) 
		{
			Color flashColor = FlashScreen.color;
			flashColor.a += 0.01f;
			FlashScreen.color = flashColor;
		}
		else
		{
			fadingIn = false;
			manager.RefreshForNewRound();
		}
	}

	private void FadeBlackOut() 
	{
		if (FlashScreen.color.a > 0)
		{	// Decrease alpha until transparent
			Color flashColor = FlashScreen.color;
			flashColor.a -= 0.01f;
			FlashScreen.color = flashColor;
		}
		else 
		{	// Done fading out, signal cinematics
			screenFlashed = false;
			FlashScreen.enabled = false;
			FlashScreen.color = Color.white;

			//StartCoroutine(WaitAndStartRound());
			TriggerCinematics();
		}
	}

	private void ShowWin()
	{
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;

		WinText.text = "PLAYER 1 WINS";
		WinText.enabled = true;
	}

	private void ShowStrike()
	{

	}

	private void RefreshWinCounts() 
	{
		LeftCount.text = manager.LeftSamurai.getWinCount();
		RightCount.text = manager.RightSamurai.getWinCount();
	}

	private void TriggerCinematics()
	{
		TopLine.Play ("TopLine", -1, 0f);
		BottomLine.Play ("BottomLine", -1, 0f);

		StartCoroutine(WaitAndSignalRoundStart());
	}
}
