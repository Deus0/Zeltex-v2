using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CustomEvents {
	[System.Serializable]
	public class MyEvent : UnityEvent<GameObject> {}
	
	[System.Serializable]
	public class MyEvent2 : UnityEvent<GameObject,GameObject> {
		
	}
	[System.Serializable]
	public class MyEvent3 : UnityEvent<GameObject,string> {
		
	}
	[System.Serializable]
	public class MyEventString : UnityEvent<string> {
		
	}
}