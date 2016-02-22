using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Neuron : MonoBehaviour {
	// Neuron has a list of connections
	public List<Connection> MyConnections = new List<Connection>();
	// We now track the inputs and sum them
	public float Sum = 0;
	// The Neuron's size can be animated
	public float OriginalRadius = 0.1f;
	public float Radius;
	public bool HasAddedToBrain = false;
	public bool IsInputNode = false;

	// Use this for initialization
	void Start () {
		gameObject.AddComponent<RawImage> ();
		Radius = OriginalRadius;
	}
	
	// Update is called once per frame
	void Update () {
		Display ();
	}
	
	// Add a Connection
	public void AddConnection(Connection c) {
		MyConnections.Add(c);
	} 
	
	// Receive an input
	public void FeedForward(float input) {
		// Accumulate it
		Sum += input;
		// Activate it?
		if (Sum > 1)
		{
			Fire();
			Sum = 0;  // Reset the sum to 0 if it fires
		}
	}
	float TimeFired;
	// The Neuron fires
	void Fire() 
	{
		TimeFired = Time.time;
		Radius = OriginalRadius*2f;   // It suddenly is bigger
		
		// We send the output through all connections
		for (int i = 0; i < MyConnections.Count; i++) {
			MyConnections[i].FeedForward(Sum);
		}
	}
	public bool IsInput = false;
	// Draw it as a circle
	void Display() 
	{
		Radius = Mathf.Lerp(Radius,OriginalRadius,Time.time-TimeFired);
		if (Radius == OriginalRadius) {
			Radius += OriginalRadius*Sum;
		}
		RectTransform MyRect = gameObject.GetComponent<RectTransform> ();
		MyRect.sizeDelta = new Vector2 (Radius, Radius);
		RawImage MyImage =  gameObject.GetComponent<RawImage> ();
		byte Percentage = (byte) Mathf.RoundToInt(Sum*255);
		MyImage.color = new Color32 (Percentage, Percentage, Percentage, 255);
		if (IsInput) {
			MyImage.color = new Color32 (255, Percentage, Percentage, 255);
		}
		/*stroke(0);
		strokeWeight(1);
		// Brightness is mapped to sum
		float b = map(sum,0,1,255,0);
		fill(b);
		ellipse(location.x, location.y, r, r);
		
		// Size shrinks down back to original dimensions
		r = lerp(r,32,0.1);*/
	}
}
