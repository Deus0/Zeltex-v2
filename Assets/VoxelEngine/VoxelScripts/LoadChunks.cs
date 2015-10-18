using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// if /isloadonce then just load the area that the actual map makes up!!

// this is added to the player class, and it loads the chunks around it - or any camera class really
public class LoadChunks : MonoBehaviour {
	// I need to make this non static
	// and initialize it at the start
	public List<WorldPos> ChunkPositions = new List<WorldPos>();
	public World world;
	private List<WorldPos> updateList = new List<WorldPos>();
	private List<WorldPos> buildList = new List<WorldPos>();
	List<WorldPos> chunksToDelete = new List<WorldPos>();
	public int timer = 0;
	public int MaxLoadingDistance = 5;
	public int DebugUpdateListSize;
	public int DebugBuildListSize;
	public float LastDeletedChunks = 0f;
	public bool IsLoadOnce = false;
	public bool IsDistance = false;	// shape of the deleting chunks
	public List<Text> DebugTexts = new List<Text>();
	public float TimeExisted = 0f;
	public float TimeCoolDown = 0.1f;
	private float MaxDistance = 0;	// max distance to render
	public int MaxLoadHeight = 0;
	WorldPos playerPos;


	public void Start() {
		UpdateChunkPositions();
		MaxDistance = (MaxLoadingDistance-1) * 16;
	}
	void Awake() {
		if (world == null) {
			world = GetManager.GetWorld();
		}
	}
	public void UpdateChunkPositions() {
		for (int i = 0; i < MaxLoadingDistance; i++)
			AddChunkPositions (i);		
	}

	public void AddChunkPositions(int Max) {
		for (int i = -Max; i <= Max; i++)
			for (int k = -Max; k <= Max; k++)
		{
			WorldPos NewPos = new WorldPos(i,0,k);
			if (!IsInChunkList(NewPos))
				ChunkPositions.Add (NewPos);
		}
	}
	public bool IsInChunkList(WorldPos NewChunkPos) {
		for (int i = 0; i < ChunkPositions.Count; i++) {
			if (NewChunkPos.x == ChunkPositions[i].x && 
			    NewChunkPos.y == ChunkPositions[i].y && 
			    NewChunkPos.z == ChunkPositions[i].z)
				return true;
		}
		return false;
	}
	// Update is called once per frame
	void Update() {	
		if (world != null) {
			world.ChunkLoader = this;
			TimeExisted += Time.deltaTime;
			if (TimeExisted - LastDeletedChunks > TimeCoolDown) {
				LastDeletedChunks = TimeExisted;
				// removes all excess chunks outside render range
				playerPos = new WorldPos(
					Mathf.FloorToInt(transform.position.x / Chunk.chunkSize) * Chunk.chunkSize,
					Mathf.FloorToInt(transform.position.y / Chunk.chunkSize) * Chunk.chunkSize,
					Mathf.FloorToInt(transform.position.z / Chunk.chunkSize) * Chunk.chunkSize
					);
				if (IsLoadOnce) {
					playerPos = new WorldPos(0,0,0);
				}
				//if (!IsLoadOnce)
				DeleteChunks ();
				// finds new chunks to load within the list
				//if (!IsLoadOnce)
				FindChunksToLoad ();
				// Loads new chunks and renders them
				LoadAndRenderChunks ();
			}
		
			DebugUpdateListSize = updateList.Count;
			DebugBuildListSize = buildList.Count;
		}
	}
	public void AddToUpdateList(Vector3 NewChunkPosition) {
		WorldPos newChunkPos = new WorldPos (Mathf.RoundToInt (NewChunkPosition.x), 
		                        Mathf.RoundToInt (NewChunkPosition.y), 
		                        Mathf.RoundToInt (NewChunkPosition.z));
		if (!updateList.Contains(newChunkPos))
			updateList.Add (newChunkPos);
	}
	// bool is LoadColumns	- otherwise load in every direction etc
	void FindChunksToLoad() {
		//Get the position of this gameobject to generate around

		//If there aren't already chunks to generate
		if (buildList.Count == 0) 
		{
			//Cycle through the array of positions
			//for (int i = 0; i < ChunkPositions.Length; i++)
			for (int i = 0; i < ChunkPositions.Count; i++)
			{
				//translate the player position and array position into chunk position
				WorldPos newChunkPos = new WorldPos(
					ChunkPositions[i].x * Chunk.chunkSize + playerPos.x,
					0,
					ChunkPositions[i].z * Chunk.chunkSize + playerPos.z
					);
				
				//Get the chunk in the defined position
				Chunk newChunk = world.GetChunk(
					newChunkPos.x, newChunkPos.y, newChunkPos.z);
				
				//If the chunk already exists and it's already
				//rendered or in queue to be rendered continue
				if (newChunk != null
				    && (newChunk.rendered || updateList.Contains(newChunkPos)))
					continue;
				
				//load a column of chunks in this position
				for (int y = -MaxLoadHeight; y <= MaxLoadHeight; y++)
				{
					buildList.Add(new WorldPos(
						newChunkPos.x, y * Chunk.chunkSize, newChunkPos.z));
				}
				return;
			}
		}
	}
	
	void LoadAndRenderChunks() {
		for (int i = 0; i < 2; i++)
		{
			if (buildList.Count != 0)
			{
				BuildChunk(buildList[0]);
				buildList.RemoveAt(0);
			}
		}
		
		for (int i = 0; i < updateList.Count; i++)
		{
			Chunk chunk = world.GetChunk(updateList[0].x, updateList[0].y, updateList[0].z);
			if (chunk != null)
				chunk.update = true;
			updateList.RemoveAt(0);
		}
	}
	
	void BuildChunk(WorldPos pos) {
		//int y = pos.y;
		for (int y = pos.y - Chunk.chunkSize; y <= pos.y + Chunk.chunkSize; y += Chunk.chunkSize)
		{
			if (y > 64 || y < -64)
				continue;
			
			for (int x = pos.x - Chunk.chunkSize; x <= pos.x + Chunk.chunkSize; x += Chunk.chunkSize)
			{
				for (int z = pos.z - Chunk.chunkSize; z <= pos.z + Chunk.chunkSize; z += Chunk.chunkSize)
				{
					if (world.GetChunk(x, y, z) == null)
						world.CreateChunk(x, y, z);
				}
			}
		}
		
		updateList.Add(pos);
	}

	void DeleteChunks() {
		//if (LastDeletedChunks == 0)
		//	LastDeletedChunks = TimeExisted;
		if (timer == 10) {
			//if (timer == 10) {
			chunksToDelete.Clear();
			foreach (var chunk in world.chunks) {
				if (IsDistance) {
					float distance = Vector3.Distance(
					new Vector3(chunk.Value.pos.x, 0, chunk.Value.pos.z),
					new Vector3(playerPos.x, 0, playerPos.z));
				
					if (distance > MaxDistance)
						chunksToDelete.Add(chunk.Key);
				}
				else
				{
					if (!(playerPos.x + MaxDistance >= chunk.Value.pos.x && playerPos.x - MaxDistance <= chunk.Value.pos.x &&
					    playerPos.z + MaxDistance >= chunk.Value.pos.z && playerPos.z - MaxDistance <= chunk.Value.pos.z))
							chunksToDelete.Add (chunk.Key);
				}
			}
			
			foreach (var chunk in chunksToDelete)
				world.DestroyChunk(chunk.x, chunk.y, chunk.z);
			
			timer = 0;
			LastDeletedChunks = TimeExisted;
		}
		timer++;
	}

}