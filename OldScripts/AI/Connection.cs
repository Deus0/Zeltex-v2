using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Connection : MonoBehaviour {
	// Connection is from Neuron A to B
	public Neuron NeuronA;
	public Neuron NeuronB;
	public Vector3 SendingLocation;
	public float TimeBegan;
	// Connection has a weight
	public float weight;
	// Variables to track the animation
	public bool IsSending = false;
	// Need to store the output for when its time to pass along
	public float output = 0;
	LineRenderer MyLine;
	public Material MyMaterial;
	public NeuralNetwork MyNeuralNetwork;
	public float LineWidth = 0.01f;

	// Use this for initialization
	void Start () {
		gameObject.AddComponent <RawImage> ();
	}
	public void CreateLine() {
		MyLine = gameObject.AddComponent<LineRenderer> ();
		MyLine.SetWidth (LineWidth,LineWidth);
		MyLine.SetVertexCount (2);
		MyLine.SetPosition (0, NeuronA.transform.position);
		MyLine.SetPosition (1, NeuronB.transform.position);
		MyLine.material = MyMaterial;
	}

	// Update is called once per frame
	void Update () {
		if (IsSending) 
		{
			SendingLocation = Vector3.Lerp(NeuronA.transform.position, NeuronB.transform.position, (1f/MyNeuralNetwork.FireSpeed)*(Time.time-TimeBegan));
			float d = Vector3.Distance(SendingLocation, NeuronB.transform.position);
			// If we've reached the end
			if (d < 0.01f) 
			{
				// Pass along the output!
				NeuronB.FeedForward(output);
				IsSending = false;
			}
		}
		if (MyLine) {
			MyLine.SetPosition (0, NeuronA.transform.position);
			MyLine.SetPosition (1, NeuronB.transform.position);
		}
		Render ();
	}
	
	/*public Connection(Neuron from, Neuron to, float w) {
		weight = w;
		NeuronA = from;
		NeuronB = to;
	}*/

	// The Connection is active
	public void FeedForward(float val) {
		output = val*weight;        // Compute output
		//sender = a.location.get();  // Start animation at Neuron A
		IsSending = true;             // Turn on sending
		TimeBegan = Time.time;
	}
	
	// Draw line and traveling circle
	void Render() {
		MyLine.SetWidth (0.005f+0.005f*weight, 0.005f+0.005f*weight);
		if (IsSending) {
			gameObject.GetComponent <RawImage> ().enabled = true;
			gameObject.GetComponent <RawImage> ().color = new Color32(0,0,0,105);
			gameObject.GetComponent <RectTransform>().sizeDelta = new Vector2(NeuronA.Radius,NeuronA.Radius);
			transform.position = SendingLocation;

		} else {
			gameObject.GetComponent <RawImage> ().enabled = false;
		}
		/*stroke(0);
		strokeWeight(1+weight*4);
		line(a.location.x, a.location.y, b.location.x, b.location.y);
		
		if (sending) {
			fill(0);
			strokeWeight(1);
			ellipse(sender.x, sender.y, 16, 16);
		}*/
	}
}
