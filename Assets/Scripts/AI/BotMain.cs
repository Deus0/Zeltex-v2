using UnityEngine;
using System.Collections;

// Bot AI is split accross 3 classes:
//	BotMovement		- Adds movement to the bot
//	BotBrain		- Makes decisions, remembers things
//	BotPerception	- Can percieve enemies and obstacles

// Maybe I should put commands in here and the mode

public class BotMain : MonoBehaviour {
	public bool CanMove = true;		// adds a bot movement class
	public bool CanPercieve = true;	// adds the bot perception class
	public bool CanRemember = true;	// adds the bot perception class

	// Use this for initialization
	void Start () {
		AddScripts ();
	}
	
	// Update is called once per frame
	void Update () {
		if (CanPercieve && CanMove) {
			gameObject.GetComponent<BotPerception>().MyBehaviour = gameObject.GetComponent<BotMovement>().MyBehaviour;
		}
		if (GetComponent<BaseCharacter> ().MyStats.IsDead) {
			SetActiveScripts (false);
		} else {
			SetActiveScripts (true);
		}
	}

	// if dead, remove scripts
	public void AddScripts() {
		if (CanMove && gameObject.GetComponent<BotMovement> () == null) {
			gameObject.AddComponent<BotMovement>();
		}
		if (CanPercieve && gameObject.GetComponent<BotPerception> () == null) {
			gameObject.AddComponent<BotPerception>();
		}
		if (CanRemember && gameObject.GetComponent<BotBrain> () == null) {
			gameObject.AddComponent<BotBrain>();
		}
	}
		
	public void RemoveScripts() {
		if (gameObject.GetComponent<BotMovement> () != null) {
			Destroy(gameObject.GetComponent<BotMovement>());
		}
		if (gameObject.GetComponent<BotPerception> () != null) {
			Destroy(gameObject.GetComponent<BotPerception>());
		}
		if (gameObject.GetComponent<BotBrain> () != null) {
			Destroy(gameObject.GetComponent<BotBrain>());
		}
	}
	public void SetActiveScripts(bool IsEnabled) {
		if (gameObject.GetComponent<BotMovement> () != null) {
				gameObject.GetComponent<BotMovement>().enabled = IsEnabled;
		}
			if (gameObject.GetComponent<BotPerception> () != null) {
				gameObject.GetComponent<BotPerception>().enabled = IsEnabled;
		}
			if (gameObject.GetComponent<BotBrain> () != null) {
				gameObject.GetComponent<BotBrain>().enabled = IsEnabled;
		}
	}
}
