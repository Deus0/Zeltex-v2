using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// used to generate the social stats of bots
// as well as the:
//		Bone Sizes
//		Texture Maps
//		Voxel Model Data
//		any modifiers on their mesh
//		deepness of their voice - pitch - i will modify a sound effect based on the dna values

[System.Serializable]
public class DNA : MonoBehaviour {
	//The genetic sequence
	public List<float> Genomes;
	public float MutationRate = 0.35f;  // A pretty high mutation rate here, our population is rather small we need to enforce variety
	public int maxDNA = 255;
	public KeyCode MyKey;

	public DNA(List<float> NewGenomes) {
		CloneGenomes (NewGenomes);
	}
	// Use this for initialization
	void Start () {
		FillGenomes ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (MyKey)) {
			Mutate (MutationRate);
		}
	}

	public void FillGenomes() {
		//DNA is random floating point values between 0 and 1 (!!)
		for (int i = 0; i < maxDNA; i++) {
			Genomes.Add(Random.value);
		}
	}

	public void CloneGenomes(List<float> NewGenomes) {
		Genomes.Clear ();
		for (int i = 0; i < NewGenomes.Count; i++)
			Genomes.Add (NewGenomes [i]);
	}
	//returns one element from array 
	float GetGene(int index) {
		return Genomes[index];
	}
	
	//creates new DNA sequence from two (this & 
	DNA Breed(DNA Lover) {
		List<float> ChildGenomes = new List<float>();

		// this splites the dna in half, i probaly want to do something better then this
		int crossover = Mathf.RoundToInt(Random.Range(1, maxDNA));
		for (int i = 0; i < maxDNA; i++) {
			if (i > crossover) {
				ChildGenomes.Add(GetGene(i));
			} else               
				ChildGenomes.Add(Lover.GetGene(i));
		}
		DNA newdna = new DNA(ChildGenomes);
		return newdna;
	}
	
	//based on a mutation probability, picks a new random character in array spots
	void Mutate(float MutationChance) {
		for (int i = 0; i < Genomes.Count; i++) {
			if (Random.value < MutationChance) {
				Genomes[i] = Random.value;
			}
		}
	}
}
// Fill out your copyright notice in the Description page of Project Settings.

// This algorithm should vary
// for instance one gene says, 4-12 bones
// next genes will be for each bone
// Therefor will have parent and child genes
// it will only mutate the genes if they are the same size
// if one has 4 bones, the other 6, it will mutate 4/6 of the bones, that have corresponding indexes
