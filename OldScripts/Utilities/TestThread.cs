using UnityEngine;
//using System.Collections;

public class TestThread : MonoBehaviour {
	UnityThreading.ActionThread MyThread;
	void Start()
	{
		
		//myThread = UnityThreadHelper.CreateThread(DoThreadWork);
		MyThread = UnityThreadHelper.CreateThread (() =>
		{
			DoThreadWork (MyThread);
		});
	}
	
	void DoThreadWork(UnityThreading.ActionThread myThread)
	{
		// processing
		for (int i = 0; i < 10000; i++) {
			// processing
			//Debug.LogError("Testing thread: " + i);
			//Debug.LogError("Testing thread: " + Time.time);
			//return new WaitForSeconds(2);
			//yield return new WaitForSeconds(2);
		}
	}

}
