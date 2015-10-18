using UnityEngine;
using System.Collections;

//  Attributes directly affect base stats
//	Having a certain amount of them can also unlock abilities and spells
//		ie: intellect, 100 intellect will unlock a wizard based ability - 200% mana regen or something

// For NPCs, attributes chosen will be done by genetics
//		and their first spells/abiliities will be based on these attributes
//	ie level 20 NPC spawns, its given 15 strength, 3 wisdom, 3 intellect, 5 agility, 1 luck
//		It will be given skills like rush, or enrage, to complement its attributes

[System.Serializable]
public class Attribute {
	public string Name = "NewAttribute";
	public float State = 0f;
	public float Base = 10f;	// before effects are applied
	
	public Attribute() {
		Reset ();
	}
	public void SetDefaults() {
		Reset ();
	}
	public void Reset(){
		State = Base;
	}
	public void ApplyIncrease(float Multiplier) {
		State += Base * Multiplier;
	}
	public void SetBase(float Amount) {
		Base = Amount;
	}
	public void IncreaseBase(float IncreaseAmount) {
		Base += IncreaseAmount;
		CheckStateLimit ();
	}
	void CheckStateLimit() {
		if (Base < 0)
			Base = 0;
		if (State < 0)
			State = 0;
	}
};
