using UnityEngine;
using System.Collections;

/*
	public class AnimateCharacter : MonoBehaviour {
		public AnimationState MyAnimationState;	// 0 for idle, 1 for walking, 2 for punching
		public Animation MyAnimationSingleClip;
		public Animator MyAnimation;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			if (MyAnimationSingleClip != null) {
				MyAnimationSingleClip.wrapMode = WrapMode.Loop;
			}
				//Debug.Log ("    Animation: " + MyAnimationState);
				if (MyAnimationState == AnimationState.Idle) {
					if (MyAnimation != null) MyAnimation.Play ("Idle");
					if (MyAnimationSingleClip != null) MyAnimationSingleClip.Play ("idle");
				} else if (MyAnimationState == AnimationState.Walking) {
					if (MyAnimation != null) MyAnimation.Play ("Walking");
					if (MyAnimationSingleClip != null) MyAnimationSingleClip.Play ("walk");
				} else if (MyAnimationState == AnimationState.Punching) {
					if (MyAnimation != null) MyAnimation.Play ("Punch");
					if (MyAnimationSingleClip != null) MyAnimationSingleClip.Play ("attack_1");
				} else if (MyAnimationState == AnimationState.Dead) {
					if (MyAnimation != null) MyAnimation.Play ("Dead");
					if (MyAnimationSingleClip != null) MyAnimationSingleClip.Play ("death");
				}
		}
	}
 */