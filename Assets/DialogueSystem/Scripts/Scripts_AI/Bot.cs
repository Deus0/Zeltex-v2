using UnityEngine;
using System.Collections;

// use this in Bot class, similar to player but for npcs
// make a basic bot class that can be used for things like animations etc

namespace AISystem {
	public enum MovementState {
		Waiting,
		WalkToPosition,
		Following,
		Wander,
		Flee
	};

	public class Bot : MonoBehaviour
	{
		public MovementState MyMovementState = MovementState.Waiting;
		public Movement MyMovement;
		public Wander MyWander;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}