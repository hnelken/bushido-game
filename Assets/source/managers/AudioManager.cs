using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public AudioSource music;
	public AudioSource sfx;

	public AudioClip menuSound, popSound, hitSound, strikeSound, tieSound;

	public static AudioManager Get() {
		return FindObjectOfType<AudioManager>();
	}

	public void BackToMenu() {
		Destroy(gameObject);
	}

	void Start() {
		DontDestroyOnLoad(gameObject);
		StartMusic();
	}

	public void StartMusic() {
		if (!music.isPlaying) {
			music.Play();
		}
	}

	public void StopMusic() {
		music.Stop();
	}

	public void PlayPopSound() {
		music.Stop();
		sfx.PlayOneShot(popSound);
	}

	public void PlayTieSound() {
		sfx.PlayOneShot(tieSound);
	}

	public void PlayHitSound() {
		sfx.PlayOneShot(hitSound);
	}

	public void PlayStrikeSound() {
		sfx.PlayOneShot(strikeSound);
	}

	public void PlayMenuSound() {
		sfx.PlayOneShot(menuSound);
	}
}
