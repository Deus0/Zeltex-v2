using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterSystem;
using UnityEngine.UI;

namespace GuiSystem 
{
	public class StatBarManager : MonoBehaviour 
	{
		[Tooltip("A prefab that will be used to spawn stat bars")]
		public GameObject StatBarPrefab;
		[Tooltip("The gameobject with CharacterStats component")]
		public GameObject TargetCharacter;
		// spawned bars
		private List<GameObject> SpawnedBars = new List<GameObject> ();
		[Tooltip("The colours used on the stat bars")]
		public List<string> MyStats = new List<string> ();

		[Tooltip("The colours used on the stat bars")]
		public List<Color32> MyColours = new List<Color32> ();
		
		[Tooltip("The Margin Between bars")]
		public float MarginY = 5f;

		void Awake()
		{
		}

		private void DefaultStats()
		{
			if (MyStats.Count == 0) {
				MyStats.Add ("Health");
				MyStats.Add ("Mana");
				MyStats.Add ("Energy");
				MyStats.Add ("Level");
			}
		}
		private bool IsInStats(string StatName) 
		{
			for (int i = 0; i < MyStats.Count; i++)
			{
				if (StatName == MyStats[i]) 
				{
					return true;
				}
			}
			return false;
		}
		// on update stats - called on things like regen etc
		public void OnUpdateStats() 
		{
			DefaultStats ();
			CharacterStats MyCharacterStats = TargetCharacter.GetComponent<CharacterStats> ();
			if (MyCharacterStats)
			{
				int StateIndex = 0;
				for (int i = 0; i < MyStats.Count; i++) 
				{
					Stat MyStat = MyCharacterStats.TempStats.GetStat(MyStats[i]);
					if (MyStat != null)
					if (MyStat.GetStatType() == "State" || MyStat.GetStatType() == "Base") 
					//if (IsInStats(MyStats.TempStats.GetStat(i).Name))
					if (StateIndex < SpawnedBars.Count)
					{
						Transform StatBar = SpawnedBars[StateIndex].transform.FindChild("StatBar");
						Transform BarState = SpawnedBars[StateIndex].transform.FindChild("BarState");
						if (StatBar && BarState 
						    && StatBar.GetComponent<Text>()
						    && BarState.GetComponent<RectTransform>()) 
						{
							// make the state bar, the normal bar*percentage of stat
							StatBar.GetComponent<Text>().text = 
								MyStat.GetState () + "/" + MyStat.GetValue();

							float NewWidth = GetRectWidth()*MyStat.GetPercentage ();
							BarState.GetComponent<RectTransform>().SetWidth(NewWidth);
							BarState.GetComponent<RectTransform>().anchoredPosition = new Vector2(NewWidth/2f, BarState.GetComponent<RectTransform>().anchoredPosition.y);
							StateIndex++;
						} else {
							Debug.LogError("Null in StatBarManager");
						}
					}

				}
			}
		}
		private Vector3 GetBarPosition(int i)
		{
			//Vector3 MyBodyBounds = new Vector3 (0, 0.8f, 0);
			Vector3 NewPosition = new Vector3();
			GUI3D.Orbitor MyOrbitor = gameObject.GetComponent<GUI3D.Orbitor>();
			if (MyOrbitor && MyOrbitor.enabled) 
			{
				NewPosition = MyOrbitor.GetGuiOffset();
			}
			NewPosition += new Vector3(0,
			                           -(i)*GetRectHeight(),
			                           0);	// was 50
			return NewPosition;
		}
		// generate the star bars here, mainly when a new state stat is added
		// called if the stats change - later on it should take in what state is changed / added removed, and only update depending on that
		public void OnNewStats()
		{
			DefaultStats ();
			// clear old ones
			for (int i = 0; i < SpawnedBars.Count; i++) 
			{
				DestroyImmediate (SpawnedBars[i]);	// otherwise they are still in the transform.childCount thingo!
			}
			SpawnedBars.Clear ();
			CharacterStats MyCharacterStats = TargetCharacter.GetComponent<CharacterStats> ();
			if (MyCharacterStats) 
			{
				//int InitialChildCount = transform.childCount-1;
				//Debug.LogError("Spawning Stat Bars: " + InitialChildCount);
				//for (int i = 0; i < MyStats.TempStats.GetSize(); i++) 
				for (int i = 0; i < MyStats.Count; i++) 
				{
					Stat MyStat = MyCharacterStats.TempStats.GetStat(MyStats[i]);
					//if (MyCharacterStats.TempStats.GetStat(i).GetStatType() == "State") 
					//if (IsInStats(MyCharacterStats.TempStats.GetStat(i).Name))
					{
						Vector3 NewPosition = GetBarPosition(transform.childCount);
						GameObject NewBar = (GameObject) Instantiate(StatBarPrefab, NewPosition, Quaternion.identity);
						NewBar.name = MyStats[i] + "_Bar_" + i;
						NewBar.transform.SetParent(transform, false);
						SpawnedBars.Add (NewBar);
						if (SpawnedBars.Count-1 < MyColours.Count) {
							NewBar.transform.FindChild("BarState").GetComponent<UnityEngine.UI.RawImage>().color = MyColours[SpawnedBars.Count-1];
						}
						GUI3D.Toggler MyToggler = gameObject.GetComponent<GUI3D.Toggler>();
						NewBar.SetActive(MyToggler.GetActive());
					}
				}
				OnUpdateStats();
			}
			RefreshFollower ();
		}
		private float GetRectWidth() {
			return StatBarPrefab.GetComponent<RectTransform> ().GetSize ().x;
		}
		private float GetRectHeight() {
			return (StatBarPrefab.GetComponent<RectTransform> ().GetSize ().y + MarginY);
		}
		private float GetTotalRectSize() 
		{
			return (transform.childCount-1) * GetRectHeight ();
		}
		private void RefreshFollower() 
		{
			//Debug.LogError ("Refreshing FOllower in StatBarManager.");
			GUI3D.Follower MyFollower = gameObject.GetComponent<GUI3D.Follower> ();
			if (MyFollower) 
			{
				MyFollower.SetTargetOffset(new Vector3 (0, 0.6f+(transform.lossyScale.y * GetTotalRectSize ()), 0));
			}
		}
	}
}
