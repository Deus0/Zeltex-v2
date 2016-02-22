using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace OldCode {
[System.Serializable]
public enum Element {
	Fire,
	Water,
	Nature,
	Frost,
	Earth,
	Dark,
	Light,
	Electricity,
	Magma,
	Generic
};

[System.Serializable]
public enum DamageType {
	Peirce,	// ignore magic defence - arrows
	Splash,	// does aoe damage - explosions
	Chaos,	// ignores alot of armor - ie magic
	Wave	// doesn't destroy on hit, flows through enemies
};

// whenever I update the variables - make sure i update the copy constructor lel
[System.Serializable]
public class Spell {
	//public int IconIndex;		// which icon will display for this
	public string Name;			// what is the name of this finishing move
	public string ToolTip;		// the text displayed when mouse is over it
	public DamageType MyDamageType;	// what kind of damage it will do
	public int MyTextureId;
	public int MySpawnSoundId;
	public int SpawnIndex;
	public int FunctionIndex;	// whether its projectile, laser, etc
	
	public Element ElementType;
	public List<int> Effects;
	//public int EffectIndex;		// Later make a list of this
	public float Damage;		// how much damage will be done to the pour soul that gets hit
	public float ChargeTime;	// how long will it take to charge
	public string StatType = "Mana";	// which type of stat will it cost
	public float ManaCost;		// how much will the mana cost
	public float Range;		// basic variable for how far it will travel (unhindered)
	public float LifeTime;		// how long will it last for
	public float Size;			// What will its final Size be
	public float TravelSpeed;			// What will its final Size be
	public bool IsInstant = false;
	public float CoolDown;			// What will its final Size be
	public int ModelIndex;
	public float LastActivated;
	public int Ammo;			// Some spells require ammo- like bombs (they are basically the same thing!)
	public string AnimationType = "Shooting";	// which type of stat will it cost

	//public bool IsTeleport = false;
	
	//public Texture MyTexture;
	//public AudioClip SpawnSoundEffect;
	//public GameObject SpawnObject;
	
	public Spell() {
		ResetCoolDown ();
	}
	// used for GUI
	public string ConvertToText() {
		string MySpellData = Name + ": " +
			"\n" + ToolTip.ToString () + 
			"\n Function Type: " + FunctionIndex.ToString () + 
			"\t\t\t Cost: " + StatType.ToString () + ": " + ManaCost +
			"\n ChargeTime: " + ChargeTime.ToString () + 
			"\t\t\t CoolDown: " + CoolDown.ToString ();
		// if projectile, add projectil stats
		
		MySpellData +=
			"\n Damage: " + Damage.ToString() + 
				"\t\t Range: " + Range.ToString() + 
				"\n TravelSpeed: " + TravelSpeed.ToString() +
				"\t\t\t LifeTime: " + LifeTime.ToString() + 
				"\t\t\t Size: " + Size.ToString() + "\n"
				;
		if ( Effects.Count > 0) {
			MySpellData += "Effects:\n";
			for (int i = 0; i < Effects.Count; i++) {
				if (GetManager.GetDataManager())
					MySpellData += GetManager.GetDataManager().EffectsList[Effects[i]].Name + "\t ";
			}
		}
		return MySpellData;
	}


	public void ResetCoolDown() {
		LastActivated = -CoolDown;	//Time.time - CoolDown;
	}

	public bool HasEnoughResources(float StatBase) {
		if (StatBase > ManaCost)
			return true;
		else
			return false;
	}
	public bool CanCast() {
		// when updating the code during execution, sometimes lastactivated variable gets stuck
		//Debug.Log ("Time.Time: " + Time.time + "LastActivated: " + LastActivated + "TimeSinceCast: " + TimeSinceCast().ToString() + " CooLDown: " + CoolDown.ToString());
		if (TimeSinceCast () > CoolDown) {
			LastActivated = Time.time;
			return true;
		} else
			return false;
	}
	public float TimeSinceCast() {
		//Debug.Log (Time.time.ToString ());
		return Time.time-LastActivated;
	}
	public float TimeLeft() {
		float TimeLeftToCast = CoolDown - TimeSinceCast ();
		if (TimeLeftToCast < 0)
			TimeLeftToCast = 0;
		return TimeLeftToCast;
	}
	public Spell(Spell NewSpell) {
		Name = NewSpell.Name;
		IsInstant = NewSpell.IsInstant;
		FunctionIndex = NewSpell.FunctionIndex;
		//MyTexture = NewSpell.MyTexture;
		ElementType = NewSpell.ElementType;
		MySpawnSoundId = NewSpell.MySpawnSoundId;

		Effects = NewSpell.Effects;
		Damage = NewSpell.Damage;
		MyDamageType = NewSpell.MyDamageType;
		ChargeTime = NewSpell.ChargeTime;
		StatType = NewSpell.StatType;
		ManaCost = NewSpell.ManaCost;
		LifeTime = NewSpell.LifeTime;
		Size = NewSpell.Size;
		TravelSpeed = NewSpell.TravelSpeed;
		CoolDown = NewSpell.CoolDown;
		ModelIndex = NewSpell.ModelIndex;

		//SpawnSoundEffect = NewSpell.SpawnSoundEffect;
		AnimationType = NewSpell.AnimationType;
		ToolTip = NewSpell.ToolTip;
		//IsTeleport = NewSpell.IsTeleport;
		Range = NewSpell.Range;
		MyTextureId = NewSpell.MyTextureId;
		SpawnIndex = NewSpell.SpawnIndex;
		ResetCoolDown ();

		//MyTexture = GetManager.GetDataManager().GetSpellTexture (MyTextureId);
		//SpawnSoundEffect = GetManager.GetDataManager().GetSpellSpawnSound(MySpawnSoundId);
		//SpawnObject = GetManager.GetDataManager().GetSpellPrefab (SpawnIndex);
	}
	public Spell(SpellData NewSpell) {
		Name = NewSpell.Name;
		IsInstant = NewSpell.IsInstant;
		Effects = NewSpell.Effects;
		Damage = NewSpell.Damage;
		MySpawnSoundId = NewSpell.MySpawnSoundId;
		FunctionIndex = NewSpell.FunctionIndex;

		ChargeTime = NewSpell.ChargeTime;
		StatType = NewSpell.StatType;
		ManaCost = NewSpell.ManaCost;
		LifeTime = NewSpell.LifeTime;
		Size = NewSpell.Size;
		TravelSpeed = NewSpell.TravelSpeed;
		CoolDown = NewSpell.CoolDown;
		ModelIndex = NewSpell.ModelIndex;

		AnimationType = NewSpell.AnimationType;
		ToolTip = NewSpell.ToolTip;
		//IsTeleport = NewSpell.IsTeleport;
		Range = NewSpell.Range;
		MyTextureId = NewSpell.MyTextureId;
		SpawnIndex = NewSpell.SpawnIndex;
		ResetCoolDown ();
		//MyTexture = GetManager.GetDataManager().GetSpellTexture (MyTextureId);
		//SpawnSoundEffect = GetManager.GetDataManager().GetSpellSpawnSound(MySpawnSoundId);
		//SpawnObject = GetManager.GetDataManager().GetSpellPrefab (SpawnIndex);
	}
};

// convert is instant to another function

// whenever I update the variables - make sure i update the copy constructor lel
[System.Serializable]
public class SpellData {
	public string Name;			// what is the name of this finishing move
	public string ToolTip;		// the text displayed when mouse is over it
	public int MyTextureId;
	public int SpawnIndex;
	public int FunctionIndex;
	public int MySpawnSoundId;
	
	//public Element ElementType;
	public List<int> Effects = new List<int>();
	//public int EffectIndex;		// Later make a list of this
	public float Damage;		// how much damage will be done to the pour soul that gets hit
	//public DamageType MyDamageType;	// what kind of damage it will do
	public float ChargeTime;	// how long will it take to charge
	public string StatType = "Mana";	// which type of stat will it cost
	public float ManaCost;		// how much will the mana cost
	public float Range;		// basic variable for how far it will travel (unhindered)
	public float LifeTime;		// how long will it last for
	public float Size;			// What will its final Size be
	public float TravelSpeed;			// What will its final Size be
	public bool IsInstant = false;
	public float CoolDown;			// What will its final Size be
	public int ModelIndex;

	public float LastActivated;
	public int Ammo;			// Some spells require ammo- like bombs (they are basically the same thing!)
	public string AnimationType = "Shooting";	// which type of stat will it cost
	
	public bool IsTeleport = false;

	public SpellData(Spell NewSpell) {
		// general spell properties
		Name = NewSpell.Name;
		ToolTip = NewSpell.ToolTip;
		FunctionIndex = NewSpell.FunctionIndex;
		MySpawnSoundId = NewSpell.MySpawnSoundId;
		MyTextureId = NewSpell.MyTextureId;
		ModelIndex = NewSpell.ModelIndex;
		SpawnIndex = NewSpell.SpawnIndex;
		StatType = NewSpell.StatType;
		ManaCost = NewSpell.ManaCost;
		CoolDown = NewSpell.CoolDown;
		AnimationType = NewSpell.AnimationType;

		// projectile properties
		Damage = NewSpell.Damage;
		Range = NewSpell.Range;
		TravelSpeed = NewSpell.TravelSpeed;
		LifeTime = NewSpell.LifeTime;
		Size = NewSpell.Size;
		ChargeTime = NewSpell.ChargeTime;
		IsInstant = NewSpell.IsInstant;
		// on collision properties
		Effects = NewSpell.Effects;
	}
	public SpellData(List<string> MyData) {
		int i = 0;
		string NewLine = "";
		Name = MyData [i++].Replace("<Name>", "");
		ToolTip = MyData [i++].Replace("<ToolTip>","");

		NewLine = MyData [i++].Replace("<Fun>","");
		int.TryParse(NewLine, out FunctionIndex);
		NewLine = MyData [i++].Replace("<SpawnSound>","");
		int.TryParse(NewLine, out MySpawnSoundId);
		NewLine = MyData [i++].Replace("<TextureId>","");
		int.TryParse(NewLine, out MyTextureId);
		NewLine = MyData [i++].Replace("<ModelId>","");
		int.TryParse(NewLine, out ModelIndex);
		NewLine = MyData [i++].Replace("<SpawnId>","");
		int.TryParse(NewLine, out SpawnIndex);
		StatType = MyData [i++].Replace("<Stat>", "");
		NewLine = MyData [i++].Replace("<Cost>","");
		float.TryParse(NewLine, out ManaCost);
		NewLine = MyData [i++].Replace("<CoolDown>","");
		float.TryParse(NewLine, out CoolDown);
		AnimationType = MyData [i++].Replace("<Animation>", "");

		NewLine = MyData [i++].Replace("<Damage>","");
		float.TryParse(NewLine, out Damage);
		NewLine = MyData [i++].Replace("<Range>","");
		float.TryParse(NewLine, out Range);
		NewLine = MyData [i++].Replace("<Speed>","");
		float.TryParse(NewLine, out TravelSpeed);
		NewLine = MyData [i++].Replace("<LifeTime>","");
		float.TryParse(NewLine, out LifeTime);
		NewLine = MyData [i++].Replace("<Size>","");
		float.TryParse(NewLine, out Size);
		NewLine = MyData [i++].Replace("<CastTime>","");
		float.TryParse(NewLine, out ChargeTime);
		NewLine = MyData [i++].Replace("<IsInstant>","");
		bool.TryParse(NewLine, out IsInstant);
		while (i < MyData.Count) {
			int NewEffect = 0;
			NewLine = MyData [i].Replace("<Effect>","");
			int.TryParse(NewLine, out NewEffect);
			Effects.Add (NewEffect);
			i++;
		}
	}

	public List<string> ConvertToTextData() {
		List<string> MyData = new List<string> ();
		MyData.Add ("<Name>" + Name);
		MyData.Add ("<ToolTip>" + ToolTip);
		MyData.Add ("<Fun>" + FunctionIndex.ToString());
		MyData.Add ("<SpawnSound>" + MySpawnSoundId.ToString());
		MyData.Add ("<TextureId>" + MyTextureId.ToString());
		MyData.Add ("<ModelId>" + ModelIndex.ToString());
		MyData.Add ("<SpawnId>" + SpawnIndex.ToString());
		MyData.Add ("<Stat>" + StatType);
		MyData.Add ("<Cost>" + ManaCost.ToString());
		MyData.Add ("<CoolDown>" + CoolDown.ToString());
		MyData.Add ("<Animation>" + AnimationType.ToString());
		
		MyData.Add ("<Damage>" + Damage.ToString());
		MyData.Add ("<Range>" + Range.ToString());
		MyData.Add ("<Speed>" + TravelSpeed.ToString());
		MyData.Add ("<LifeTime>" + LifeTime.ToString());
		MyData.Add ("<Size>" + Size.ToString());
		MyData.Add ("<CastTime>" + ChargeTime.ToString());
		MyData.Add ("<IsInstant>" + IsInstant.ToString());
		for (int i = 0; i < Effects.Count; i++) {
			MyData.Add ("<Effect>" + Effects[i].ToString());
		}
		return MyData;
	}
}
}