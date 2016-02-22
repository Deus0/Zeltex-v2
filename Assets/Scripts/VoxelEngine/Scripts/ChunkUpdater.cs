using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Update Cycle -> Primary Chunk, Outer chunks for light, Primary Chunk Outer part! (remove all light propogation from inner chunk, out of outer chunk)
// Or maybe just reverse propogate light(in primary chunk), where it goes darkness to bright, and then update the surrounding chunks again
namespace VoxelEngine 
{
	[ExecuteInEditMode]
	public class ChunkUpdater : MonoBehaviour 
	{
		public bool IsDebugMode = false;
		public List<Chunk> UpdateList;
		public List<Chunk> LightsUpdateList;
		public float UpdateSpeed = 0.1f;
		public bool IsUpdating = false;
		IEnumerator MyUpdater;
		private float TimeStartedUpdating;
		private float AverageLoadTime;
		private List<float> LoadTimes = new List<float>();
		
		
		void OnGUI() {
			if (IsDebugMode) 
			{
				GUILayout.Label ("Average Load Time: " + AverageLoadTime + " - with count: " + LoadTimes.Count);
				GUILayout.Label ("UpdateList Count: " + UpdateList.Count);
				GUILayout.Label ("LightsUpdateList Count: " + LightsUpdateList.Count);
				VoxelLoader MyLoader = GetComponent<VoxelLoader>();
				GUILayout.Label ("Load Distance: " + MyLoader.LoadDistance.ToString());

				GUILayout.Label ("Chunks Loaded: " + transform.childCount);
			}
		} 
		void Start() 
		{
			Reset ();
		}
		public void Reset() 
		{
			MyUpdater = UpdateChunks();
			IsUpdating = false;
			UpdateList.Clear ();
			LightsUpdateList.Clear ();
		}
		#if UNITY_EDITOR
		public void ManuallyRunUpdate() 
		{
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				if (MyUpdater != null) 
				{
					MyUpdater.MoveNext ();
				}
			}
		}
		#endif
		IEnumerator UpdateChunks() 
		{
			if (gameObject.GetComponent<World>().IsDebugMode)
				Debug.LogError ("[" + GetCurrentTime() + "s]" + " - Begin Updating [" + name + "]");

			#if UNITY_EDITOR
			float TimeStarted = (float)UnityEditor.EditorApplication.timeSinceStartup;
			while (UnityEditor.EditorApplication.timeSinceStartup-TimeStarted < UpdateSpeed) 
			{
				yield return new WaitForSeconds(0.1f);
			}
			#else
			yield return new WaitForSeconds(UpdateSpeed);
			#endif

			while (UpdateList.Count > 0 || LightsUpdateList.Count > 0) 
			{
				if (UpdateList.Count > 0) {
					if (UpdateList[0])
						UpdateList[0].RunUpdates();
					UpdateList.RemoveAt(0);
				}
				else {
					if (LightsUpdateList.Count > 0) 
					{
						if (LightsUpdateList[0])
							LightsUpdateList[0].RunUpdateLights();
						LightsUpdateList.RemoveAt(0);
					}
				}
				#if UNITY_EDITOR
				float TimeStarted2 = (float)UnityEditor.EditorApplication.timeSinceStartup;
				while (UnityEditor.EditorApplication.timeSinceStartup-TimeStarted2 < UpdateSpeed) 
				{
					yield return new WaitForSeconds(0.1f);
				}
				#else
				yield return new WaitForSeconds(UpdateSpeed);
				#endif

			}
			IsUpdating = false;
			// performs check to see if both lists are empty
			AddLoadTime ();
			if (gameObject.GetComponent<World>().IsDebugMode)
				Debug.LogError ("Total Time [" + ((int)LoadTimes[LoadTimes.Count-1]) + "ms]");
			yield return null;  
		}
		public static float GetCurrentTime() {
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
				return Time.time;
			else
				return (float)UnityEditor.EditorApplication.timeSinceStartup;
			#endif
			return Time.time;
		}
		void AddLoadTime() {
			LoadTimes.Add ((GetCurrentTime() - TimeStartedUpdating)*1000);
			AverageLoadTime = 0;
			for (int i = 0; i < LoadTimes.Count; i++) {
				AverageLoadTime += LoadTimes[i];
			}
			AverageLoadTime /= LoadTimes.Count;
		}
		public void Add(Chunk NewChunk)
		{
			Add (NewChunk, UpdateList);
		}
		public void AddLights(Chunk NewChunk) 
		{
			//if (GetComponent<World>().IsLighting)
				Add (NewChunk, LightsUpdateList);
		}

		private void Add(Chunk NewChunk, List<Chunk> MyList) 
		{
			//Debug.LogError ("Adding new chunk: " + NewChunk.name);
			if (MyList.Count == 0) {
				TimeStartedUpdating = GetCurrentTime();
			}
			for (int i = 0; i < MyList.Count; i++) 
			{
				if (MyList[i] == NewChunk)
					return;
			}
			MyList.Add (NewChunk);
			if (!IsUpdating) 
			{
				IsUpdating = true;
				MyUpdater = UpdateChunks();
				#if UNITY_EDITOR
					if (UnityEditor.EditorApplication.isPlaying)
						StartCoroutine(MyUpdater);
					//else
					////anuallyRunUpdate ();
				#else
					//UpdateChunks();
					StartCoroutine(MyUpdater);
					//StartCoroutine("UpdateChunks");
				#endif
			}
		}
	}
}
