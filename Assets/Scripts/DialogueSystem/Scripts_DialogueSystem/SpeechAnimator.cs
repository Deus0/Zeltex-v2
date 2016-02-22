using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpeechAnimator : MonoBehaviour {
	public Text AnimationTextReference;	// just a reference to the npc's dialogue
	private string MySpeech = "";	// used to animate the text
	private float LastUpdatedTime = 0;
	private float UpdateCoolDown = 0.1f;
	public int SoundInterval = 3;
	private int CharacterIndex;
	public List<AudioClip> MySpeechSounds = new List<AudioClip>();
	public AudioSource MyAudioSource;
	public UnityEngine.Events.UnityEvent OnFinishedAnimationFunction;
	// settings
	private float SoundMin = 0.25f;
	private float SoundMax = 0.8f;
	private float AnimationSpeedMin = 0.02f;
	private float AnimationSpeedMax = 0.06f;
	private bool IsSound = true;

	// Use this for initialization
	void Awake () {
		AnimationTextReference = gameObject.GetComponent<Text> ();
		if (MyAudioSource == null) 
			MyAudioSource = gameObject.GetComponent<AudioSource> ();
		AnimationSpeedMin = GameSettings.SpeedSpeed;
		AnimationSpeedMax = GameSettings.SpeedSpeed;
		IsSound = GameSettings.IsSpeechSound;
	}
	
	void Update ()
	{
		UpdateAnimation ();
	}

	// animates from AnimationTextReference.text to MySpeech (both strings)
	private void UpdateAnimation() {
		if (MySpeech != null) {
			if (CharacterIndex < MySpeech.Length && 
				Time.time - LastUpdatedTime > UpdateCoolDown) {
				// make sound - different sound for each character added - its meant to sound like a type writer
				if (IsSound) {
					if (CharacterIndex % SoundInterval == 0 && MySpeechSounds.Count > 0) {
						MyAudioSource.PlayOneShot (MySpeechSounds [Random.Range (0, MySpeechSounds.Count)]);
						if (MySpeech [CharacterIndex] == ' ')
							MyAudioSource.volume = 0.3f;
						else
							MyAudioSource.volume = Random.Range (SoundMin, SoundMax);
					}
				}
				// add another character to it
				AnimationTextReference.text += MySpeech [CharacterIndex];
				CharacterIndex++;
				LastUpdatedTime = Time.time;
				UpdateCoolDown = Random.Range (AnimationSpeedMin, AnimationSpeedMax);

				if (CharacterIndex == MySpeech.Length) {	// end of animation
					//Debug.LogError("Calling function: " + Time.time);	// debugging
					OnFinishedAnimationFunction.Invoke ();
				}
			}
		} else {
			Debug.LogError("NoSpeech bubble reference in animator: " + gameObject.name);
		}
	}

	// begins animation a new, normally use new line instead to change what it animates too
	public void ResetAnimation() {
		if (AnimationTextReference != null) {
			AnimationTextReference.text = "";	// set to nothing
			LastUpdatedTime = Time.time;
			CharacterIndex = 0;
			UpdateCoolDown = Random.Range (AnimationSpeedMin, AnimationSpeedMax);
		} else {
			Debug.LogError("Problem with Speech Animation: " + gameObject.name);
		}
	}

	public void NewLine(string NewLine) {
		MySpeech = NewLine;
		ResetAnimation ();
	}
}
