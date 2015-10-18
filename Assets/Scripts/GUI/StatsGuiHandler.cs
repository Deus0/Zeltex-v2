using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsGuiHandler : MonoBehaviour {
	public BaseCharacter MyStatsCharacter;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Text LevelText = transform.FindChild ("LevelBackground").FindChild ("LevelText").GetComponent<Text>();
		LevelText.text = " LVL [" + Mathf.Round (MyStatsCharacter.MyStats.MyLevel.Level) + "]";
		Text ExperienceText = transform.FindChild ("ExperienceBackground").FindChild ("ExperienceText").GetComponent<Text>();
		ExperienceText.text = "XP [" + Mathf.Round (MyStatsCharacter.MyStats.MyLevel.ExperiencePoints) + " / " + Mathf.Round (MyStatsCharacter.MyStats.MyLevel.ExperienceRequired) + "]";
		Text HealthText = transform.FindChild ("HealthBackground").FindChild ("HealthText").GetComponent<Text>();
		HealthText.text =  "HP [" + Mathf.Round (MyStatsCharacter.MyStats.GetStat ("Health")) + "]";
		Text ManaText = transform.FindChild ("ManaBackground").FindChild ("ManaText").GetComponent<Text>();
		ManaText.text =  "M [" + Mathf.Round (MyStatsCharacter.MyStats.GetStat ("Mana")) + "]";

		//MyGui.HealthBar.sizeDelta = new Vector2( Mathf.Round (MyPlayer.MyStats.GetPercentage ("Health") * 100f), 20); 
		//MyGui.ManaBar.sizeDelta = new Vector2(  Mathf.Round (MyPlayer.MyStats.GetPercentage ("Mana") * 100f), 20); 
	}
}
