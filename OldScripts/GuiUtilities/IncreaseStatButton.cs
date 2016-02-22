using UnityEngine;
using System.Collections;

namespace OldCode {
public class IncreaseStatButton : MonoBehaviour {
	public int StatIndex;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void IncreaseStats() {
		GetManager.GetGuiCreator ().IncreasePlayerStats (StatIndex);
	}
}

}