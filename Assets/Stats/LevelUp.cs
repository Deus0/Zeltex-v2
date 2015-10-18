using UnityEngine;
using System.Collections;

// Levels can be obtained by characters, or items
//	- Characters get experience from killing, when the final blow is dealt, if they are in a party the xp will be shared
//	- once the level increases, Skill points are given, which can be put into attributes, increasing the base stats

[System.Serializable]
public class LevelUp {
	public float TotalExperiencePoints;	// Total amount of overall experience points gained	- used for things like decreasing the level
	public float ExperiencePoints;		// Current amount of experience points in the level
	public float Level;					// Increases the power of the character each level
	public float ExperienceRequired;	// used per level up
	public float ExperienceMultiplier;	// multiplied by the Experience Required per level
	public int SkillPoints;					// can be used to increase attributes
	public LevelUp() {
		Level = 1.0f;
		ExperienceRequired = 10.0f;
		ExperiencePoints = 0.0f;
		TotalExperiencePoints = 0.0f;
		ExperienceMultiplier = 2.0f;
		SkillPoints = 0;
	}
	public bool HasEffectsChanged = true;
	
	public float GetPercentage() {
		return (ExperiencePoints / ExperienceRequired);
	}
	public void SpendSkillPoint(int SpendAmount) {
		if (SkillPoints - SpendAmount >= 0)
			SkillPoints -= SpendAmount;
	}
	// returns true if levels up
	public bool IncreaseExperiencePoints(float IncreasePoints, float EnemyLevel) {
		float Difference = (EnemyLevel - Level);
		if (Difference > 0)
			IncreasePoints += Difference * 1.5f;	// modify points x10 percent per extra level
		if (Difference < 0)
			IncreasePoints -= Difference / 1.5f;	// modify points x10 percent per extra level
		return IncreaseExperiencePoints (IncreasePoints);
	}
	public bool IncreaseExperiencePoints(float IncreasePoints) {
		if (IncreasePoints < 0)
			IncreasePoints = 0;
		ExperiencePoints += IncreasePoints;
		TotalExperiencePoints += IncreasePoints;
		
		if (ExperiencePoints >= ExperienceRequired) {
			Level++;
			SkillPoints++;
			ExperiencePoints -= ExperienceRequired;
			ExperienceRequired *= ExperienceMultiplier;
			return true;
		} else
			return false;
	}
};