using UnityEngine;
//using System.Collections;

// List of things to make:
//		Bleeding		-	Keeps up until wounds are healed with bandage, caused by physical attacks
//		Burning			-	Damage over time, quicker then curse magic, does more damage initially
//		Cold			- 	Slower movement slower physical attacks
//		Cursed 			-  	Damage over time
//		Renew			-	Heals over time

// Buffs:
//		Buffs are permanent effects, although they do not show up in the same gui location
//		Some basic ones are:
//			Basic Warrior Class Buffs: 25% damage to all melee weapons, 50% decrease in mana regeneration and max mana - stuff like this
//			Job buffs are dependent on jobs, and will be removed if the job is changed

// things like healing over time or bleeding effects etc
[System.Serializable]
public class Effect {
	public string Name;
	public Texture MyTexture;
	public float Duration;
	public float MaxTicks;
	public int Stacks;
	public int MaxStacks;
	public float Increment;
	public string StatName;
	public float StatMultiplier;
	float TicksCount;
	float LastTickTime;
	float CoolDown;

	public Effect() {
		TicksCount = 0;
		LastTickTime = 0;
		CoolDown = 3f;
		Stacks = 1;
		MaxStacks = 3;
	}
	public void IncreaseStacks() {
		Stacks++;
		Refresh ();
	}
	public void Refresh() {
		TicksCount = 0;
		LastTickTime = Time.time;
	}
	public bool IsTick() {
		CoolDown = Duration / MaxTicks;
		if (TicksCount < MaxTicks) {
			if (Time.time - LastTickTime > CoolDown) {
				LastTickTime = Time.time;
				TicksCount++;
				return true;
			}
		}
		return false;
	}

	public bool IsFinish() {
		if (TicksCount < MaxTicks) {
			return false;
		} else
			return true;
	}
};