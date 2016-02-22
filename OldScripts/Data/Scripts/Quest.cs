using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
public class Quest {
	public string Name;			// For example "Wolf Hunt"
	public string Description;	// "<QuestGiverName>, a local farmer, has been having difficulties with the <SpeciesName> killing his crops. He has requested you <ActionType> 12  <SpeciesName>.
	public string SpeciesName;	// <SpeciesName>for example "Human"
	public string ActionType;	// <ActionType> for example "Kill", "Hunt" for non intelligent species
	public float TimeRequested;			// Time quest giver gave the quest to someone
	public float TimeInitiatedRequest;	// time quest was available
	public float TimeLimit;				
	// reward system
	public List<Item> QuestRewards = new List<Item>();		// an example of a reward - a bronze sword
	public List<Item> QuestInsurance = new List<Item>();	// for example, costing 5 copper to start

	public Quest() {}
}

}