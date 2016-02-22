using UnityEngine;
using System.Collections;



[System.Serializable]
public class BaseStat {
	public string Name;
	public float State;
	public float Max;
	public float Regeneration;
	public float RegenerationCoolDown = 0.5f;
	public float LastRegeneratedTime = 0f;
	private bool IsInitialTime = true;
	
	public void SetDefaults() {
		State = 100;
		Max = 100;
		Regeneration = 0.1f;
	}
	public void Reset() {
		LastRegeneratedTime = Time.time - RegenerationCoolDown;
	}
	public bool Regenerate() {
		if (IsInitialTime) {
			Reset();
			IsInitialTime = false;
		}
		bool HasUpdated = false;
		//if (LastRegeneratedTime == 0)
		//	LastRegeneratedTime = Time.time;	// default starting point for timers
		
		if (Time.time - LastRegeneratedTime >= RegenerationCoolDown) {
			LastRegeneratedTime = Time.time;
			//IncreaseBase ((Regeneration * Time.deltaTime) / 1000f);	// each second regenerate x amount of Regeneration
			//Debug.Log ("Time: " + Time.deltaTime);
			if (State < Max)
				HasUpdated = true;
			IncreaseBase (Regeneration);	// each second regenerate x amount of Regeneration
		} 
		return HasUpdated;
	}
	public void SetState(float Amount) {
		State = Amount;
	}
	// increases the state towards the max by the generation rate
	public void IncreaseBase(float IncreaseAmount) {
		State += IncreaseAmount;
		CheckStateLimit ();
	}
	// limits stats between 0 and their max
	void CheckStateLimit() {
		if (State < 0)
			State = 0;
		else if (State > Max)
			State = Max;
	}
};