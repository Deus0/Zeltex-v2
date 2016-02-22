using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CharacterSystem {
	public class CharacterStats : MonoBehaviour {
		private PhotonView MyPhoton;
		[Header("Debug")]
		public bool IsDebugMode = false;
		public KeyCode DecreaseStatKey;
		[Header("Audio")]
		public AudioSource MySource;
		public AudioClip OnDeathSound;

		[Header("Events")]
		public UnityEvent OnDeath = new UnityEvent();
		public UnityEvent OnNewStats = new UnityEvent();
		public UnityEvent OnUpdateStats = new UnityEvent();
		// data
		[HideInInspector] public Stats BaseStats = new Stats();
		[HideInInspector] public Stats TempStats = new Stats();	// when adding items to it
		bool IsAlive = true;

		void Start() {
			MyPhoton = gameObject.GetComponent<PhotonView> ();
			MySource = gameObject.GetComponent<AudioSource> ();
			UpdateState ();
		}
		void OnGUI() {
			if (IsDebugMode) {
				int PositionY = 0;
				GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20), name + " has " + TempStats.GetSize() + " stats.");
				for (int i = 0; i < TempStats.GetSize(); i++) {
					GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20), (i+1) +" : "  +  TempStats.GetStat(i).GuiString());
					GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20),"\t" +  TempStats.GetStat(i).GetDescription());
				}
			}
		}
		public bool HasStat(string StatName, float StatNeeded) {
			if (!IsAlive)
				return false;
			Stat MyStat = TempStats.GetStat (StatName);
			if (MyStat != null)
				return (MyStat.GetState () >= StatNeeded);
			else
				return false;
		}
		
		public float GetStatValue(string StatName) {
			return TempStats.GetStat (StatName).GetState ();
		}
		void CheckForDeath(string StatName) {
			if (StatName == "Health" && TempStats.GetStat (StatName).GetState () <= 0)
			{
				IsAlive = false;
				if (MySource && OnDeathSound) {
					MySource.PlayOneShot(OnDeathSound);
				}
				OnDeath.Invoke ();
			}
		}
		[PunRPC]
		public void UpdateStat(string StatName, float State) {
			if (!MyPhoton.isMine) 
			{
				if (TempStats.GetStat(StatName).GetState() != State) {
					TempStats.SetStat(StatName, State);
					CheckForDeath(StatName);
					OnUpdateStats.Invoke ();
				}
			}
			//Debug.LogError (TempStats.GetStat (StatName).GuiString ());
		}
		public void SynchAllStats() 
		{
			Debug.LogError ("Synching stats in: " + MyPhoton.owner.name);
			// the owner of the character needs to synch the stats
			MyPhoton.RPC ("SynchAllStats2", MyPhoton.owner);	
		}
		[PunRPC]
		public void SynchAllStats2() 
		{
			if (PhotonNetwork.connected && MyPhoton.isMine) {
				for (int i = 0; i < TempStats.GetSize(); i++) {
					SynchStat (TempStats.Data [i].Name);
				}
			}
		}
		void SynchStat(string StatName) {
			if (PhotonNetwork.connected) 
			{
				if (MyPhoton.isMine) {
					MyPhoton.RPC ("UpdateStat", PhotonTargets.All,
				             StatName, TempStats.GetStat (StatName).GetState ());
				}
				
			}
		}
		//public void OnHitByBullet(string BulletName, 
		// adds to temporary stats
		public void AddStat(string StatName, float StatAdd) {
			if (IsAlive) {
				TempStats.GetStat (StatName).AddState (StatAdd);
				CheckForDeath(StatName);
				OnUpdateStats.Invoke ();
				SynchStat(StatName);
			}
		}
		void Update() {
			HandleDebugInput ();
			if (Alive()) 
			{
				RegenStats ();
			}
		}
		public void HandleDebugInput() {
			if (IsDebugMode) {
				if (Input.GetKeyDown (DecreaseStatKey)) {
					Debug.LogError ("Decreasing health by 10.");
					AddStat ("Health", -10);
				}
			}
		}
		public bool Alive () {
			return IsAlive;
		}
		public void RegenStats() {
			bool HasRegened = false;
			for (int i = 0; i < TempStats.GetSize(); i++) {
				if (TempStats.GetStat(i).GetStatType() == "Regen") {
					if (TempStats.GetStat(i).HasTicked()) {
						Stat MyStat = TempStats.GetStat(TempStats.GetStat(i).GetModifyStatName());
						if (MyStat != null) {
							bool DidRegen = MyStat.AddState (TempStats.GetStat (i).GetRegenValue());
							HasRegened = true;
						}
						else
							Debug.LogError("Could not find the stat: " + TempStats.GetStat(i).GetModifyStatName());
					}
				}
			}
			if (HasRegened) 
			{
				OnUpdateStats.Invoke ();
			}
		}
		public void RunScript(List<string> MyData)
		{
			Debug.LogError ("Loading stats!");
			BaseStats = new Stats (MyData);
			UpdateState ();
		}
		public void UpdateState() 
		{
			TempStats.Clear ();
			for (int i = 0; i < BaseStats.GetSize(); i++) {
				//Debug.LogError ("State: " + BaseStats.Data [i].GuiString ());
				//if (BaseStats.GetStat(i).GetStatType() != "Modifier")
				TempStats.Add (BaseStats.GetStat (i));
			}
			
			// apply Armoury before stat modifiers 
			/*ItemSystem.Inventory MyInventory = gameObject.GetComponent<ItemSystem.Inventory> ();
			if (MyInventory) 
			{
				for (int i = 0; i < MyInventory.MyItems.Count; i++) 
				{
					for (int j = 0; j < MyInventory.MyItems[i].MyStats.GetSize(); j++) 
					{
						TempStats.Add (MyInventory.MyItems[i].MyStats.GetStat(j));
					}
				}
			}*/
			// stat modifiers on base stats
			for (int i = 0; i < TempStats.GetSize(); i++) {
				//Debug.LogError ("State: " + BaseStats.Data [i].GuiString ());
				if (TempStats.GetStat(i).GetStatType() == "Modifier") 
				{
					Stat MyStat = TempStats.GetStat(TempStats.GetStat(i).GetModifyStatName());
					if (MyStat != null)
						MyStat.Add (TempStats.GetStat (i).GetValue()*TempStats.GetStat (i).GetModifierValue());
					else
						Debug.LogError("Could not find the stat: " + TempStats.GetStat(i).GetModifyStatName());
				}
			}
			OnNewStats.Invoke ();
		}
	}
}