using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[Serializable]
public class SaveChunk
{
	public Dictionary<WorldPos, BlockBase> BlocksDictionary = new Dictionary<WorldPos, BlockBase>();
	
	public SaveChunk(Chunk chunk)
	{
		for (int x = 0; x < Chunk.chunkSize; x++)
		{
			for (int y = 0; y < Chunk.chunkSize; y++)
			{
				for (int z = 0; z < Chunk.chunkSize; z++)
				{
					if (!chunk.blocks[x, y, z].changed)
						continue;
					
					WorldPos pos = new WorldPos(x, y, z);
					BlocksDictionary.Add(pos, chunk.blocks[x, y, z]);
				}
			}
		}
	}
}

public static class Serialization {
    public static string saveFolderName = "SavedData";

    public static string SaveLocation(string SaveFileName, string WorldName) {
		string saveLocation = saveFolderName + "/" + SaveFileName + "/" + WorldName + "/";

        if (!Directory.Exists(saveLocation))
        {
            Directory.CreateDirectory(saveLocation);
        }
		saveLocation += "chunks/";
		
		if (!Directory.Exists(saveLocation))
		{
			Directory.CreateDirectory(saveLocation);
		}
        return saveLocation;
    }

    public static string FileName(WorldPos chunkLocation)
    {
        string fileName = chunkLocation.x + "," + chunkLocation.y + "," + chunkLocation.z + ".chk";
        return fileName;
    }

    public static void SaveChunk(Chunk chunk)
    {
		SaveChunk MyChunkSave = new SaveChunk(chunk);
		if (MyChunkSave.BlocksDictionary.Count == 0)
            return;

        string saveFile = SaveLocation(chunk.world.SaveFileName, chunk.world.worldName);
        saveFile += FileName(chunk.pos);

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, MyChunkSave);
        stream.Close();
    }

	// if server - load from memory
	// else - ask server for data.... how do i do this? Each client needs unique data sent to them
	// need a way for client to send //can i has this chunk - messages to the server, and server to respond back!
	// once block changes are made on the client, the server will record these changes and save them into chunks when it can - if the chunks are not loaded!
    public static bool Load(Chunk chunk)
    {
		string saveFile = SaveLocation(chunk.world.SaveFileName, chunk.world.worldName);
        saveFile += FileName(chunk.pos);

        if (!File.Exists(saveFile))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

		SaveChunk save = (SaveChunk)formatter.Deserialize(stream);

        foreach (var block in save.BlocksDictionary)
        {
            chunk.blocks[block.Key.x, block.Key.y, block.Key.z] = block.Value;
        }

        stream.Close();
        return true;
    }
}