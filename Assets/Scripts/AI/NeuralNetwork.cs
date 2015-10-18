using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NeuralNetwork : MonoBehaviour { 
	// The Network has a list of neurons
	public List<Neuron> MyNeurons = new List<Neuron>();
	// The Network now keeps a duplicate list of all Connection objects.
	// This makes it easier to draw everything in this class
	public List<Connection> MyConnections = new List<Connection>();
	public float LastTime = 0;
	public Material MyMaterial;
	public float FireSpeed = 2.5f;
	public int BeginNodeIndex = 0;
	public int Amount = 3;
	public int InputsMax = 3;
	public List<int> MyInputs = new List<int>();

	// Use this for initialization
	void Start () {
		LastTime = Time.time;
		CreateRandomNetwork ();
	}
	public void Update() {
		if (Time.time-LastTime > FireSpeed)
		{
			LastTime = Time.time;
			FeedForward(0.15f + Random.Range (-0.1f,0.1f));
		}
		/*	if (Input.GetKeyDown(KeyCode.C)) {
			LastTime = Time.time;
			ClearNetwork();
		}
		if (Input.GetKeyDown(KeyCode.G)) {
			ClearNetwork();
			CreateRandomNetwork();
		}*/
		if (Time.time-LastGenerated > 5f)
		{
			LastGenerated = Time.time;
			//ClearNetwork();
			//CreateRandomNetwork();
		}
	}
	float LastGenerated;
	public void ClearNetwork() {
		for (int i = MyNeurons.Count-1; i >= 0; i--) {
			Destroy(MyNeurons[i].gameObject);
			MyNeurons.RemoveAt (i);
		}
		for (int i = MyConnections.Count-1; i >= 0; i--) {
			Destroy(MyConnections[i].gameObject);
			MyConnections.RemoveAt (i);
		}
	}

	public void CreateDefaultNetwork() {
		Neuron a = CreateNeuron(-350, 0);
		Neuron b = CreateNeuron(-200, 0);
		Neuron c = CreateNeuron(0, 75);
		Neuron d = CreateNeuron(0, -75);
		Neuron e = CreateNeuron(200, 0);
		Neuron f = CreateNeuron(350, 0);
		// Connect them
		Connect(a, b, 1);
		Connect(b, c, GetRandom());
		Connect(b, d, GetRandom());
		Connect(c, e, GetRandom());
		Connect(d, e, GetRandom());
		Connect(e, f, 1);
	}

	public void CreateRandomNetwork() {
		for (int i = 0; i < Amount; i++) {
			RectTransform MyRect = gameObject.GetComponent<RectTransform>();
			
			//CreateNeuron(Random.Range(-MyRect.sizeDelta.x/2f, MyRect.sizeDelta.x/2f), Random.Range(-MyRect.sizeDelta.y/2f, MyRect.sizeDelta.y/2f));
			CreateNeuron(Random.Range(-MyRect.sizeDelta.x/2f, MyRect.sizeDelta.x/2f), 
			             Random.Range(-MyRect.sizeDelta.y/2f, MyRect.sizeDelta.y/2f),
			             Random.Range(-MyRect.sizeDelta.x/2f, MyRect.sizeDelta.x/2f));
		}
		CreateConnectionsClosestNode2 ();
		LastTime = Time.time;
	}

	public void CreateConnectionsClosestNode2() {
		for (int i = 0; i < InputsMax; i++) {
			MyInputs.Add (Mathf.FloorToInt (Random.Range (0f, MyNeurons.Count)));
		}
		BeginNodeIndex = Mathf.FloorToInt (Random.Range (0f, MyNeurons.Count));
		Neuron StartNeuron = MyNeurons [BeginNodeIndex];
		StartNeuron.HasAddedToBrain = true;
		Neuron MyNeuron2 = FindClosestNeuron (StartNeuron);
		MyNeuron2.HasAddedToBrain = true;
		Connect (MyNeuron2, StartNeuron, 1f);
		int CheckCount = 0;
		while (!HasAllNeuronsBeenAdded()) 
		{
			//int NeuronIndex = Mathf.FloorToInt(Random.Range (0f,MyNeurons.Count));
			Neuron MyNeuron = GetUnUsedNeuron();
			if (MyNeuron == null)
				break;	//idk whats going on
			//if (!MyNeuron.HasAddedToBrain)
			Connect (FindClosestNeuron2(MyNeuron), MyNeuron, GetRandom());
			MyNeuron.HasAddedToBrain = true;
			CheckCount++;
			if (CheckCount > 1000)
				break;
		}
	}
	public Neuron GetUnUsedNeuron() {
		for (int i = 0; i < MyNeurons.Count; i++) {
			if (!MyNeurons[i].HasAddedToBrain)
				return MyNeurons[i];
		}
		return null;
	}

	public bool HasAllNeuronsBeenAdded() {
		for (int i = 0; i < MyNeurons.Count; i++) 
		{
			if (!MyNeurons[i].HasAddedToBrain) {
				return false;
			}
		}
		return true;
	}

	public Neuron FindClosestNeuron2(Neuron MyNeuron) 
	{
		bool IsFirstAdd = true;
		int ClosestIndex = -1;
		float ClosestDistance = 0;
		for (int i = 0; i < MyNeurons.Count; i++) 
		{
			if (MyNeuron != MyNeurons[i] && MyNeurons[i].HasAddedToBrain) 
			{
				float DistanceBetween = Vector3.Distance(MyNeuron.transform.position, MyNeurons [i].transform.position);
				if (ClosestDistance > DistanceBetween || IsFirstAdd) 
				{
					ClosestIndex = i;
					ClosestDistance = DistanceBetween;
					IsFirstAdd = false;
				}
			}
		}
		//MyNeurons[ClosestIndex].HasAddedToBrain = true;
		return MyNeurons [ClosestIndex];
	}

	public void CreateConnectionsClosestNode() {		
		for (int i = 0; i < MyNeurons.Count; i++) {
			Connect (MyNeurons[i], FindClosestNeuron(MyNeurons[i]), Random.Range(0.2f, 0.8f));
		}
	}

	public Neuron FindClosestNeuron(Neuron MyNeuron) {
		int ClosestIndex = 0;
		float ClosestDistance = Vector3.Distance (MyNeuron.transform.position, MyNeurons [ClosestIndex].transform.position);
		for (int i = 1; i < MyNeurons.Count; i++) {
			if (MyNeuron != MyNeurons[i]) {
				float DistanceBetween = Vector3.Distance(MyNeuron.transform.position, MyNeurons [i].transform.position);
				if (ClosestDistance > DistanceBetween) 
				{
					ClosestIndex = i;
					ClosestDistance = DistanceBetween;
				}
			}
		}
		return MyNeurons [ClosestIndex];
	}

	public float GetRandom() {
		return Random.Range (0.01f, 1.01f) - 0.01f;
	}
	
	public Neuron CreateNeuron(float x, float y) 
	{
		return CreateNeuron (x, y, 0);
	}
	public Neuron CreateNeuron(float x, float y, float z) 
	{
		GameObject NewNeuron = new GameObject ();
		NewNeuron.transform.position = new Vector3 (x, y, z);
		NewNeuron.transform.SetParent (gameObject.transform, false);
		NewNeuron.name = "Neuron " + MyNeurons.Count;
		Neuron MyNeuron = NewNeuron.AddComponent<Neuron> ();
		MyNeurons.Add (MyNeuron);
		return MyNeuron;
	}

	public Connection CreateConnection(Neuron A, Neuron B, float Weighting) 
	{
		GameObject NewConnection = new GameObject ();
		NewConnection.transform.SetParent (gameObject.transform, false);
		Connection MyConnection = NewConnection.AddComponent<Connection> ();
		MyConnection.NeuronA = A;
		MyConnection.NeuronB = B;
		MyConnection.weight = Weighting;
		NewConnection.name = "Connection " + MyConnections.Count;
		MyConnection.MyMaterial = MyMaterial;
		MyConnection.CreateLine ();
		MyConnection.MyNeuralNetwork = this;
		return MyConnection;
	}

	// We can connection two Neurons
	void Connect(Neuron a, Neuron b, float weight) 
	{
		Connection c = CreateConnection(a, b, weight);
		a.AddConnection(c);
		//b.AddConnection(c);
		// Also add the Connection here
		MyConnections.Add(c);
	} 
	
	// Sending an input to the first Neuron
	// We should do something better to track multiple inputs
	void FeedForward(float input) 
	{
		Debug.Log ("Weighting: " + input.ToString() + " -> Feeding Neural Network from Beginning point: " + Time.time);
		//Neuron start = MyNeurons[BeginNodeIndex];
		//start.FeedForward(input);
		for (int i = 0; i < MyInputs.Count; i++) {
			MyNeurons[MyInputs[i]].IsInput = true;
			MyNeurons[MyInputs[i]].FeedForward(input);
		}
	}
}
