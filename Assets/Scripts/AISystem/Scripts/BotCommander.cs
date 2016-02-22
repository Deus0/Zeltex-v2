using UnityEngine;
using System.Collections;


namespace AISystem {
	public class BotCommander : MonoBehaviour {
		public bool IsClickToCommand = false;
		public GameObject MyBot;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			if (IsClickToCommand && Input.GetMouseButtonDown (1)) {
				RaycastHit MyHit;
				if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out MyHit, 100)) {
					if (MyHit.collider.gameObject.tag == "Ground" && MyBot) {
						Movement MyBotAI = MyBot.GetComponent<Movement>();
						if (MyBotAI) {
							MyBotAI.MoveToPosition(MyHit.point);
						}
						Wander MyBotWander = MyBot.GetComponent<Wander>();
						if (MyBotWander) {
							MyBotWander.enabled = false;
						}
					}
					if (MyHit.collider.gameObject.tag == "Bot") {
						MyBot = MyHit.collider.gameObject;
					}
				}
			}
		}
	}
}
