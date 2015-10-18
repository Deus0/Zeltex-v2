using UnityEngine;
using System.Collections;

//		Decisions:
//					- A list of goals that it wants to achieve, with priorities
//					- Performs goal of highest priority first, things like commands will influence priorities
//					- Neural network will be kept here

// it will be aware of the time of day
// it will schedule tasks for that day depending on priorities like:
//		Gathering food
//		Repairing buildings
//		Chopping trees
//		building shelters

// If the NPC is part of a herd, it will share these tasks with others

public enum BehaviourPattern {
	HyperAggressive,	// never retreats
	Passive,			// never attacks, only retreats
	PassiveAggressive,	// retreats if loses too much health
	Docile				// never retreats or attacks, just keeps wandering
};

public class BotBrain : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
