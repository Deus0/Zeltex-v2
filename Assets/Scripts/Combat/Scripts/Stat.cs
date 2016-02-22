using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CharacterSystem 
{
	/*
		Stat System
			Base Stat - name value
			State Stat - name, value, statevalue
			Regen Stat - name, regenvalue, regencooldown

		Buffs
			Modifier - stat name, multiplier percentage
	*/
	[System.Serializable]
	public class Stat 
	{
		static private string DescriptionColorTag = "<color=#00cca4>";
		static private string NameColorTag = "<color=#474785>";
		static private string StatVariableColorTag = "<color=#989a33>";
		public string Name;									// unique identifier for stats
		public string StatType = "Base";
		[SerializeField] protected string Description;		// used for tooltip
		[SerializeField] protected List<float> Value = new List<float> ();				// the default of the value
		[SerializeField] protected List<string> Modifiers = new List<string> ();		// the default of the value
		[SerializeField] protected Texture MyTexture;		// The texture used in the gui

		public Stat(string Script) 
		{
			// types of input:
			//Base
			//	[string] [value] - Joyness 5
			//State:
			//	[string] [value] [value] - Health 50/100
			//Modifier:
			//  [string] [value] [string] [value] - Strength 10, Health x10
			// Regen:
			// [string] [string] [value] [value] - HealthRegen,Health,0,1
			
			string[] MyStrings = Script.Split (',');	// Parameters
			
			for (int j = 0; j < MyStrings.Length; j++) 
			{
				MyStrings [j] = SpeechUtilities.RemoveWhiteSpace (MyStrings [j]);
			}

			if (MyStrings.Length == 2 && SpeechUtilities.IsNumbersInput (MyStrings [1])) {
				string StatName = MyStrings [0];
				StatName = SpeechUtilities.RemoveWhiteSpace (StatName);
				float StatValue = float.Parse (MyStrings [MyStrings.Length - 1]);
				Create (StatName, StatValue);
			} else if (MyStrings.Length == 3 && SpeechUtilities.IsNumbersInput (MyStrings [2])) {
				string StatName = MyStrings [0];
				StatName = StatName.Replace (MyStrings [MyStrings.Length - 2], "");
				StatName = (StatName);
				Create (StatName, float.Parse (MyStrings [MyStrings.Length - 1]),
				                        float.Parse (MyStrings [MyStrings.Length - 2]));
			} else if (MyStrings.Length == 4) {	// string, float, string, float
				if (SpeechUtilities.IsNumbersInput (MyStrings [1]) &&
					SpeechUtilities.IsNumbersInput (MyStrings [3])) {
					Create ((MyStrings [0]), 
					                        float.Parse (MyStrings [1]),
					                        (MyStrings [2]),
					                        float.Parse (MyStrings [3]));
				} else if (SpeechUtilities.IsNumbersInput (MyStrings [2]) && // regen value
					SpeechUtilities.IsNumbersInput (MyStrings [3])) {
					Create ((MyStrings [0]), 
					                        MyStrings [1],
					                        float.Parse (MyStrings [2]),
					                        float.Parse (MyStrings [3]));
				} else {
					Description = "/Destroy";
				}
			} else {
				Description = "/Destroy";
			}
		}
		public Stat() 
		{
			Value.Add (0);
			StatType = "Base";
		}
		public Stat(Stat NewStat) 
		{
			Name = NewStat.Name;
			Description = NewStat.Description;
			StatType = NewStat.StatType;
			for (int i = 0; i < NewStat.Value.Count; i++) {
				Value.Add(NewStat.Value[i]);
			}
			for (int i = 0; i < NewStat.Modifiers.Count; i++) {
				Modifiers.Add(NewStat.Modifiers[i]);
			}
		}
		public void ActivateCommand(string MyCommand) {
			string Command = SpeechUtilities.GetCommand (MyCommand);
			string MyInput = SpeechUtilities.RemoveCommand (MyCommand);
			if (Command.Contains ("/statdata")) 
			{
				SetDescription (MyInput);
			} else if (Command.Contains ("/stattexture")) 
			{
				SetTexture (MyInput);
			}
		}
		public string GetScript() 
		{
			string MyScript = "";

			if (StatType == "Base") {
				MyScript = Name + "," + Value [0];
			}
			else if (StatType == "State") 
			{
				MyScript =  Name + "," + Value [1] + "," + Value [0];
			} 
			else if (StatType == "Regen") {
				MyScript =  Name 			+ "," + 
							Modifiers [0] 	+ "," + 
							Value [0] 		+ "," +
							Value [1];
			}
			else if (StatType == "Modifier") 
			{
				MyScript =  Name + "," + 
							Value [0] + "," + 
							Modifiers [0] + "," + 
							Value [1];
			} else {
				MyScript =  Name;
			}
			if (Description != "")
			{
				MyScript += "\n" + "/statdata " + Description;
			}
			if (MyTexture != null) 
			{
				MyScript += "\n" + "/stattexture " + MyTexture.name;
			}
			return MyScript;
		}
		// normal value
		private void Create(string NewName, float NewValue) 
		{
			Name = NewName;
			Value.Add (NewValue);
			StatType = "Base";
			Debug.LogError ("Created " + Name + " of type " + StatType);
		}
		// state value
		private void Create(string NewName, float NewState, float NewValue) 
		{
			Name = NewName;
			Value.Add (NewState);	// state first!
			Value.Add (NewValue);	// max second!
			StatType = "State";
			Debug.LogError ("Created " + Name + " of type " + StatType);
		}
		// state with regen
		private void Create(string NewName, string RegenStatName, float RegenRate, float RegenCoolDown) 
		{
			Name = NewName;
			Value.Add (RegenRate);
			Value.Add (RegenCoolDown);
			Value.Add (Time.time);	// last ticked
			Modifiers.Add (RegenStatName);
			StatType = "Regen";
			Debug.LogError ("Created " + Name + " of type " + StatType);
		}
		// modifier stat
		private void Create(string NewName, float NewValue, string ModifierType, float MultiplierValue) 
		{
			Name = NewName;
			Value.Add (NewValue);
			Modifiers.Add (ModifierType);
			Value.Add (MultiplierValue);
			StatType = "Modifier";
			Debug.LogError ("Created " + Name + " of type " + StatType);
		}

		public float GetValue() {
			if (Value.Count == 0) {
				Value.Add (0);
			}
			return Value[0];
		}
		// normal value stuff
		public void Add(float AddValue) 
		{
			if (Value.Count >= 1)
				Value [0] += AddValue;
			if (GetStatType() == "State") 
				Value [1] += AddValue;
		}
		// State stuff
		public float GetState() 
		{
			if (Value.Count >= 2)
				return Value [1];
			else if (Value.Count == 1)
				return Value [0];
			else
				return 0;
		}
		public void SetState(float NewValue) {
			if (Value.Count >= 2)
				Value[1] = NewValue;
		}
		
		public bool AddState(float AddValue) 
		{
			//if (Value [1] == Value [0])
			//	return false;
			if (Value.Count >= 2) 
			{
				Value [1] += AddValue;
				Value [1] = Mathf.Clamp(Value[1], 0, Value [0]);
				return true;	// value has changed
			}
			return false;	// weird error? no idea... how did Value List change? magic.
		}
		public void SetValue(float NewValue) 
		{
			if (Value.Count >= 1)
				Value[0] = NewValue;
		}
		public float GetPercentage() 
		{
			if (Value.Count >= 2) 
			{
				return Value[1]/Value[0];
			}
			return 1;	// by default the value is 100%
		}
		// Attributes
		public float GetModifierValue() 
		{
			return Value [1];
		}
		public string GetModifyStatName() 
		{
			return Modifiers [0];
		}
		public void Add(string ValueType, float ValueAddition) 
		{
			if (ValueType == "Base") {
				Add (ValueAddition);
			} else if (ValueType == "State") {
				Add(ValueAddition);
			}
		}
		// Regen
		// Timer stuff
		public float GetRegenValue() {
			return (Value [0] / 10f);
		}
		public float GetLastTicked() {
			if (GetStatType () == "Regen") {
				return Value[2];
			}
			return Time.time;
		}
		public float GetCoolDownTime() {
			if (GetStatType () == "Regen") 
			{
				return Value[1];
			} 
			else return 666;
		}
		public bool HasTicked() {
			if (Time.time - GetLastTicked () > GetCoolDownTime ()) {
				Value[2] = Time.time;
				return true;
			}
			return false;
		}
		// gui data stuff
		public void SetTexture(string NewTexture) {
			//MyTexture = NewTexture;
		}
		public void SetTexture(Texture NewTexture) {
			MyTexture = NewTexture;
		}
		public Texture GetTexture() {
			return MyTexture;
		}
		public void SetDescription(string Description_) {
			Description = Description_;
		}
		public string GetDescription() {
			return Description;	// if null check stat manager!
		}
		public string GetStatType() {
			return StatType;
		}
		public string GetToolTipName() {
			return NameColorTag + Name + "</color>";
		}
		public string GetToolTipText() {
			string MyTooltip =  DescriptionColorTag + Description + "</color>\n";
			if (StatType == "Modifier") {
				MyTooltip += "\t You will get an increase to " + StatVariableColorTag + "[" + Modifiers[0] + "]</color>  by a multiple of " + 
					StatVariableColorTag + "[" + Value[1] + "]</color>. Total Bonus " + StatVariableColorTag + "[" + (Value[0]*Value[1]) + "]</color>";
			}
			else if (StatType == "Regen") {
				if (Modifiers.Count == 0)
					Modifiers.Add("Invalid");
				MyTooltip += "\tThe stat " + StatVariableColorTag + "[" + Modifiers[0] + "]</color> will recover at a rate of " + StatVariableColorTag + "[" + 
					GetRegenValue() + "]</color> every " + StatVariableColorTag + "[" + Value[1] +"]</color> seconds.";
			}
			return MyTooltip;
		}
		public string GuiString() 
		{
			string MyType = GetStatType ();
			if (Value.Count == 0)
				Value.Add (0);
			if ("Base" == MyType)
				return (Name + " [" + Value [0] + "]");
			else if ("State" == MyType)
				return (Name + " [" + Value [1] + "/" + Value [0] + "]");
			else if ("Modifier" == MyType)
				return (Name + " [" + Value [0] + "]");
			else if ("Regen" == MyType)
				return (Name + " [" + Value [0] + "]");
			else
				return "Error";
		}
		public string GetGuiString() {
			string MyGuiString = GuiString () + "\n";
			if (Description != "")
				MyGuiString += Description + "\n";
			return MyGuiString;
		}
	}
	/*
	/gamemode fun
	/defaultstats
		//statregen [statname] [value] [regen] [rate]
		/statregen health 100 0.5 1
		/statregen mana 100 0.5 1
		/statregen energy 100 0.5 1
		//attribute [attributename](value) [statname](multiplier)
		/attribue strength(5) health(10)
		/attribue intelligence(5 mana(10)
		/attribue strength(5) energy(10)
	*/
	/*[System.Serializable]
	public class StatExt : Stat
	{
		protected float State;	// the state of the value
		
		// expands the max, adds to state value too
		public void AddMax(float AddValue) 
		{
			State += AddValue;
			Value += AddValue;
		}
		public void IncreaseState(float Addition) {
			State += Addition;
			State = Mathf.Clamp (State, 0f, Value);
		}
		public float GetState() {
			return State;
		}
		public void SetState(float NewState) {
			State = NewState;
		}
		public float GetPercentage() {
			return State/Value;
		}
		public override string GuiString() 
		{
			return (Name + ": " + State + "/" + Value);
		}
	}*/
	
	[System.Serializable]
	public class StatAttribute : Stat
	{
		public string StatEffected;
		public float Multiplier;
	}
	/*[System.Serializable]
	public class StatRegen : StatExt
	{
		protected float Regen;	// the state of the value
		protected float Rate;	// the rate of recovery of a stat
		private float LastRegen = 0f;
		
		public void Run() {
			if (Time.time - LastRegen >= Rate) {
				LastRegen += Rate;	// make sure it recovers exactly
				IncreaseState(Regen);
			}
		}
	}
	
	[System.Serializable]
	public class StatExtended : Stat 
	{
		public float Maximum;		// the max a value can be
		public float Recovery;		// The recovery rate / second
	} */
}