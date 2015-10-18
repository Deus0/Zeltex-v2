using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Need stats to directly affect: - for turrets
//	Spell fire rate
// 	Spell damage
//	Movement speed
//	Accuracy of firing

[System.Serializable]
public class Stats { 
	// The concept of lives is important, NPCs will respawn a given amount of time, and may do things that increase their lives
	// However when their lives run out, they will not respawn
	// instead another npc, with much mutated genetics will respawn (if refilling the population!)
	public int LivesRemaining = 1;			// how many lives remaining, when 0 game over

	public bool IsDead;						// set to true if the player is dead - disables controls
	public bool HasUpdated;					// if updated the stats, set to true, the gui uses this variable to update
	public bool HasEffectsChanged = true;	// the same thing, although only for effects
	bool HasUpdatedStats = false;			// okay why is there 3 of these... lolol

	public List<BaseStat> StatsList;
	public List<Attribute> AttributeList;
	public List<Effect> EffectsList;
	public LevelUp MyLevel;
	public bool IsAutomateIncreaseStats = true;
	// goes into creating more stats
	//public float BaseRegen = 0.2f;

	public Stats() {

	}
	public void ResetCoolDowns() {
		for (int i = 0; i < StatsList.Count; i++) {
			StatsList[i].Reset();
		}
	}
	// restores all stats to a percent
	public void Ressurect(float Percentage) {
		for (int i = 0; i < StatsList.Count; i++) {
			StatsList[i].SetState(StatsList[i].Max*Percentage);
		}
		IsDead = false;
	}
	
	public void Regenerate() {
		if (!IsDead) {
			if (!HasUpdatedStats) {
				HasUpdatedStats = true;
				HasEffectsChanged = true;
			}
			if (IsAutomateIncreaseStats)
				if (MyLevel.SkillPoints > 0) {
					IncreaseRandomAttribute ();
					HasUpdated = true;
				}

			for (int i = 0; i < StatsList.Count; i++) {
				//Debug.Log("Regeneration: " + i);
				if (StatsList [i].Regenerate ())	// if changes in stats, it returns true
					HasUpdated = true;
			}

			for (int i = 0; i < EffectsList.Count; i++) {
				if (EffectsList [i].IsTick ()) {
					GetStatFromList(EffectsList[i].StatName).IncreaseBase(EffectsList[i].Stacks*EffectsList[i].Increment);
				}
				if (EffectsList[i].IsFinish ()) {
					HasEffectsChanged = true;
					EffectsList.RemoveAt (i);
				}
			}
		}
	}

	public void AddEffect(Effect NewEffect) {
		bool IsInList = false;
		for (int i = 0; i < EffectsList.Count; i++) {
			if (NewEffect.Name == EffectsList[i].Name) {
				if (EffectsList[i].Stacks < EffectsList[i].MaxStacks) {
					HasEffectsChanged = true;
					EffectsList[i].IncreaseStacks();
				}
				IsInList = true;
				break;
			}
		}
		if (!IsInList) {
			EffectsList.Add (NewEffect);
			HasEffectsChanged = true;
		}
	}
	public void IncreaseExperiencePoints(float EnemyLevel) {
		float IncreasePoints = 3;
		if (MyLevel.IncreaseExperiencePoints (IncreasePoints, EnemyLevel))
			HasUpdated = true;
	}
	public BaseStat GetStatFromList(string StatName) {
		for (int i = 0; i < StatsList.Count; i++) {
			if (StatsList[i].Name == StatName) {
				return StatsList [i];
			}
		}
		return new BaseStat();
	}
	public int GetStatIndexFromList(string StatName) {
		//return 0;
		for (int i = 0; i < StatsList.Count; i++) {
			if (StatsList[i].Name == StatName) {
				return i;
			}
		}
		return 0;
	}
	public Attribute GetAttributeFromList(string AttributeName) {
		for (int i = 0; i < AttributeList.Count; i++) {
			if (AttributeList[i].Name == AttributeName) {
				return AttributeList [i];
			}
		}
		return new Attribute();
	}
	public int GetAttributeIndexFromList(string AttributeName) {
		//return 0;
		for (int i = 0; i < AttributeList.Count; i++) {
			if (AttributeList[i].Name == AttributeName) {
				return i;
			}
		}
		return 0;
	}
	public void IncreaseRandomAttribute() {
		int RandomAttribute = Random.Range (1, 8);
		if (RandomAttribute == 1) {
			IncreaseAttribute ("Strength", 1);
		} else if (RandomAttribute == 2){
			IncreaseAttribute ("Vitality", 1);
		} else if (RandomAttribute == 3){
			IncreaseAttribute ("Intelligence", 1);
		} else if (RandomAttribute == 4){
			IncreaseAttribute ("Wisdom", 1);
		} else if (RandomAttribute == 5){
			IncreaseAttribute ("Agility", 1);
		} else if (RandomAttribute == 6){
			IncreaseAttribute ("Dexterity", 1);
		} else if (RandomAttribute == 7){
			IncreaseAttribute ("Luck", 1);
		} else if (RandomAttribute == 8){
			IncreaseAttribute ("Charisma", 1);
		}
	}
	public void SetAttribute(string AttributeName, float Amount) {
		int Indexu = GetAttributeIndexFromList (AttributeName);
		Attribute NewAttribute = AttributeList [Indexu];
	    NewAttribute.SetBase(Amount);
		NewAttribute.Reset ();
		AttributeList [Indexu] = NewAttribute;
		UpdateBaseStatsWithAttribute(AttributeName);
	}
	public float GetPercentage(string StatName) {
		return GetStatFromList(StatName).State / GetStatFromList(StatName).Max;
	}
	public float GetStat(string StatName) {
		return GetStatFromList(StatName).State;
	}
	public void SetState(string StatName, float Amount) {
		int Indexu = GetStatIndexFromList (StatName);
		BaseStat NewStat = StatsList [Indexu];
		NewStat.SetState(Amount);
		StatsList [Indexu] = NewStat;
	}
	public void SetStateMax(string StatName) {
		int Indexu = GetStatIndexFromList (StatName);
		BaseStat NewStat = StatsList [Indexu];
		NewStat.SetState(NewStat.Max);
		StatsList [Indexu] = NewStat;
	}
	public void IncreaseState(string StatName, float IncreaseAmount) {
		StatsList[GetStatIndexFromList(StatName)].IncreaseBase(IncreaseAmount);
	}
	public void IncreaseAttribute(int AttributeIndex, float IncreaseAmount) {
		if (AttributeIndex >= 0 && AttributeIndex < AttributeList.Count) {
			AttributeList [AttributeIndex].IncreaseBase (IncreaseAmount);
			UpdateBaseStatsWithAttribute (AttributeList [AttributeIndex].Name);
			MyLevel.SpendSkillPoint (1);
			HasUpdated = true;
		}
	}
	public void IncreaseAttribute(string AttributeName, float IncreaseAmount) {
		AttributeList[GetAttributeIndexFromList(AttributeName)].IncreaseBase(IncreaseAmount);
		UpdateBaseStatsWithAttribute(AttributeName);		
		MyLevel.SpendSkillPoint (1);
		HasUpdated = true;
	}
	public void UpdateAllBaseStats() {
		for (int i = 0; i < AttributeList.Count; i++) {
			UpdateBaseStatsWithAttribute(AttributeList[i].Name);
		}
	}
	public void UpdateBaseStatsWithAttribute(string AttributeName) {
		if (AttributeName == "Strength") 
		{
			GetStatFromList ("Health").Max = GetAttributeFromList ("Strength").Base * 10;
		} 
		else if (AttributeName == "Vitality") 
		{
			GetStatFromList ("Health").Regeneration = GetAttributeFromList ("Vitality").Base / 100f;
		} 
		else if (AttributeName == "Intelligence") 
		{
			GetStatFromList ("Mana").Max = GetAttributeFromList ("Intelligence").Base * 10;
		} 
		else if (AttributeName == "Wisdom") 
		{
			GetStatFromList ("Mana").Regeneration = GetAttributeFromList ("Wisdom").Base / 100f;
		} 
		else if (AttributeName == "Agility") 
		{
			GetStatFromList ("Energy").Max = GetAttributeFromList ("Agility").Base * 10;
		} 
		else if (AttributeName == "Dexterity") 
		{
			GetStatFromList ("Energy").Regeneration = GetAttributeFromList ("Dexterity").Base / 100f;
		}
	}
	public void SetDefaults() {
		if (StatsList.Count == 0) 
		{
			StatsList = new List<BaseStat> ();
			BaseStat NewHealthStat = new BaseStat ();
			NewHealthStat.Name = "Health";
			NewHealthStat.SetDefaults ();
			//NewHealthStat.Regeneration = BaseRegen;
			StatsList.Add (NewHealthStat);
		
			BaseStat NewManaStat = new BaseStat ();
			NewManaStat.Name = "Mana";
			NewManaStat.SetDefaults ();
			//NewManaStat.Regeneration = BaseRegen;
			StatsList.Add (NewManaStat);
		
			BaseStat NewEnergy = new BaseStat ();
			NewEnergy.Name = "Energy";
			NewEnergy.SetDefaults ();
			//NewEnergy.Regeneration = BaseRegen;
			StatsList.Add (NewEnergy);
		
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Strength";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Vitality";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Intelligence";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Wisdom";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Agility";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Dexterity";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Luck";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			{
				Attribute NewAttribute = new Attribute ();
				NewAttribute.Name = "Charisma";
				NewAttribute.SetDefaults ();
				AttributeList.Add (NewAttribute);
			}
			UpdateAllBaseStats ();
		}
	}
};