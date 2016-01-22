using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

	// Public (editor) references
	public SpriteRenderer LeftSamurai, RightSamurai;
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
	private Vector3 leftPosition, rightPosition;

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

		leftPosition = LeftSamurai.transform.position;
		rightPosition = RightSamurai.transform.position;
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

	public void ShowWinner(bool leftWin)
	{
		RefreshWinCounts();
		LeftCount.enabled = true;
		RightCount.enabled = true;
		
		WinText.text = (leftWin) ? "PLAYER 1 WINS" : "PLAYER 2 WINS";
		WinText.enabled = true;
	}

	public void PopFlag()
	{
		CenterPiece.SetActive(true);
	}

	public void SignalPlayerReaction(bool valid)
	{
		CenterPiece.SetActive(false);
		if (valid)
		{	// Show successful attack
			FlashWhiteFadeOut();
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
		ResetSamuraiPositions();
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
		SwapSamuraiPositions();
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

	private void SwapSamuraiPositions() 
	{
		LeftSamurai.transform.position = rightPosition;
		RightSamurai.transform.position = leftPosition;
	}

	private void ResetSamuraiPositions()
	{
		LeftSamurai.transform.position = leftPosition;
		RightSamurai.transform.position = rightPosition;
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

			
			manager.BeginNewRound();
			//StartCoroutine(WaitAndSignalRoundStart());
			//TriggerCinematics();
		}
	}

	private void ShowStrike()
	{

	}	

	private IEnumerator WaitAndSignalRoundStart()
	{	
		yield return new WaitForSeconds (3.5f);
		
		manager.BeginNewRound();
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
