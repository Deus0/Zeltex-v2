using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GuiSystem {
	public class MusicList : GuiListHandler {
		//public bool IsPlayOnStart = true;
		public List<AudioClip> Music;
		AudioSource MySource;
		bool IsLooping = false;
		int CurrentSongIndex = 0;

		void Start() {
			MySource = gameObject.GetComponent<AudioSource> ();
			//UpdateMusicGuiList ();
			//if (IsPlayOnStart)
			//	PlaySongLoop (CurrentSongIndex);
		}
		void Update()
		{
			base.Update ();
			if (IsLooping && !MySource.isPlaying) {
				PlaySong(CurrentSongIndex);
			}
		}
		private int GetSongIndex(string SongName) {
			
			for (int i = 0; i < Music.Count; i++) {
				if (Music[i].name == SongName) {
					return i;
				}
			}
			return 0;
		}
		public void PlaySong(string SongName) {
			PlaySong (GetSongIndex (SongName));
		}
		public void PlaySong(int i) {
			PlaySong (i, false);
		}
		public void PlaySongLoop(int i) {
			PlaySong (i, true);
		}
		private void PlaySong(int i, bool IsLoop) {
			StopMusic ();
			MySource.PlayOneShot (Music [i]);
			CurrentSongIndex = i;
			IsLooping = IsLoop;
		}
		public void StopMusic() {
			MySource.Stop ();
		}
		override public void RefreshList() 
		{
			//Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			Clear ();
			for (int i = 0; i < Music.Count; i++) {
				AddGui(Music[i].name);
				//MusicItemHandler MyMus = MyGuis[MyGuis.Count-1].AddComponent<MusicItemHandler>();
				//MyMus.ListIndex = i;
				//MyMus.MyMusicList = this;
			}
		}
	}
}
