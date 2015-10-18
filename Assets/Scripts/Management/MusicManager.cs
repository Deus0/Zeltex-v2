using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {
	public int SelectedMusic;
	private int MusicPlaying = -1;
	public List<AudioClip> MyMusic = new List<AudioClip>();
	AudioSource MySource;

	void Start() {
		MySource = GetComponent<AudioSource>();
		UpdateMusic ("MainMenu");
	}

	void Update () {
		if (SelectedMusic != MusicPlaying) {
			UpdateSourceWithMusic(SelectedMusic);
		}
	}

	public void UpdateSourceWithMusic(int NewMusicIndex) {
		if (MusicPlaying != NewMusicIndex) {
			MusicPlaying = SelectedMusic;
			Debug.Log ("Playing New Song: " + MyMusic [MusicPlaying].name);
			MySource.Stop ();
			MySource.clip = MyMusic [MusicPlaying];
			MySource.Play ();
		}
	}
	public void UpdateMusic(string NewMusicType) {
		if (NewMusicType == "Combat") {
			UpdateSourceWithMusic(2);
		} else if (NewMusicType == "Peace") {
			UpdateSourceWithMusic(0);
		} else if (NewMusicType == "MainMenu") {
			UpdateSourceWithMusic(1);
		}
	}
}
