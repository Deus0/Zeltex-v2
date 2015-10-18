using UnityEngine;
using System.Collections;

public class CharacterBasic : MonoBehaviour {
	// :: All Base Character variables ::
	public int PlayerIndex = 0;			// each player has a unique one, given by the Character manager
	public int ClanIndex = 0;			// Each player belons to a different clan - clan 0 is every man for themself

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// all basic players will be lone wolfs
	public int GetClanIndex() {
		return ClanIndex;
	}
}
