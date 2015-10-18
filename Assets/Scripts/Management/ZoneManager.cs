using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

[Serializable]
public class VectorSaver {
	float x;
	float y;
	float z;
	public VectorSaver(float X, float Y, float Z) {
		x = X; y = Y; z = Z;
	}
	public VectorSaver(Vector3 NewVector) {
		x = NewVector.x; y = NewVector.y; z = NewVector.z;
	}
	public Vector3 GetVector() {
		return new Vector3 (x, y, z);
	}
};

[Serializable]
public class ZonesSaver {
	public List<VectorSaver> ZonePositions = new List<VectorSaver>();
	public List<VectorSaver> ZoneSizes = new List<VectorSaver>();
}


public class ZoneManager : MonoBehaviour {
	// Zone Management
	public List<GameObject> BuildingZones;
	public GameObject BuildingZonePrefab;
	public bool IsShowZones = false;
	
	// Zone Manager
	public void ToggleZones() 
	{
		IsShowZones = !IsShowZones;
		ShowZones(IsShowZones);
	}

	public void DestroyZonesInCubeNotCentred(Vector3 Position, Vector3 Size) 
	{
		DestroyZonesInCube (Position + Size/2f,
		                   Size / 2f);
	}

	public void DestroyZonesInCube(Vector3 Position, Vector3 Size) 
	{
		//Debug.Log ("Destroying blocks around: " + Position.ToString () + " With a size of: " + Size.ToString () + " at time: " + Time.time);
		List<Zone> MyZones = GetZonesInCube (Position, Size);
		SaveZones (MyZones, Position-Size);
		for (int i = 0; i < MyZones.Count; i++) {
			DestroyZone (MyZones [i]);
		}
	}

	public void SaveZones(List<Zone> MyZones, Vector3 Position) 
	{
		if (MyZones.Count > 0) {
			//BinaryFormatter BinaryFile = new BinaryFormatter ();
			IFormatter BinaryFile = new BinaryFormatter ();
			//UpdateInChunk ();
			string SaveFileName = FileLocator.SaveLocation (GetManager.GetGameManager ().GameName, "Zone" + Position.ToString (), "Zones/", ".zn");
			Debug.LogError ("Saving Zone: " + SaveFileName);
			//Debug.LogError(name + " :Saving player in: " + SaveFileLocation);
			//FileStream MyFile = File.Create (SaveFileLocation);
			Stream MyFile = new FileStream (SaveFileName, FileMode.Create, FileAccess.Write, FileShare.None);
			ZonesSaver MyZonesSaveFile = new ZonesSaver ();
			for (int i = 0; i < MyZones.Count; i++) {
				MyZonesSaveFile.ZonePositions.Add (new VectorSaver(MyZones [i].transform.position));
				MyZonesSaveFile.ZoneSizes.Add (new VectorSaver(MyZones [i].Size));
			}
			BinaryFile.Serialize (MyFile, MyZonesSaveFile);
			MyFile.Close ();
		}
	}
	public bool LoadZones(Vector3 Position) 
	{
		string SaveFileName = FileLocator.SaveLocation (GetManager.GetGameManager().GameName, "Zone" + Position.ToString(), "Zones/", ".zn");
		if (File.Exists (SaveFileName)) {
			Debug.Log("Loading Zone: " + SaveFileName);
			IFormatter BinaryFile = new BinaryFormatter ();
			FileStream MyFile = new FileStream (SaveFileName, FileMode.Open);
			
			ZonesSaver MyZonesLoadFile = (ZonesSaver)BinaryFile.Deserialize (MyFile);
			MyFile.Close ();
			for (int i = 0; i < MyZonesLoadFile.ZonePositions.Count; i++) {
				PlaceBuildingZone(MyZonesLoadFile.ZonePositions[i].GetVector()-MyZonesLoadFile.ZoneSizes[i].GetVector(), 
				                  MyZonesLoadFile.ZoneSizes[i].GetVector()*2f);
			}
			return true;
		}
		return false;
	}


	// returns all the zones in a cube
	public List<Zone> GetZonesInCube(Vector3 Position, Vector3 Size) {
		List<Zone> MyZones = new List<Zone> ();
		for (int i = 0; i < BuildingZones.Count; i++) {
			if (((BuildingZones[i].gameObject.transform.position.x >= Position.x-Size.x && BuildingZones[i].gameObject.transform.position.x <= Position.x+Size.x) &&
			    (BuildingZones[i].gameObject.transform.position.y >= Position.y-Size.y && BuildingZones[i].gameObject.transform.position.y <= Position.y+Size.y) && 
			    (BuildingZones[i].gameObject.transform.position.z >= Position.z-Size.z && BuildingZones[i].gameObject.transform.position.z <= Position.z+Size.z)))
				MyZones.Add (BuildingZones[i].GetComponent<Zone>());
		}
		return MyZones;
	}

	public void DestroyZone(Zone ZoneToDestroy) {
		for (int i = 0; i < BuildingZones.Count; i++) {
			if (BuildingZones[i].GetComponent<Zone>() == ZoneToDestroy) {
				Destroy (ZoneToDestroy.gameObject);
				BuildingZones.RemoveAt (i);
				i = BuildingZones.Count;
			}
		}
	}

	public Zone GetZoneFromBlock(Vector3 BlockPosition) {
		Zone BlockInZone = new Zone ();
		for (int i = 0; i < BuildingZones.Count; i++) {
			Vector3 ZoneBlockPosition = BuildingZones [i].GetComponent<Zone>().InBlockPosition;
			Vector3 ZoneSize = BuildingZones [i].GetComponent<Zone>().Size;
			if (ZoneBlockPosition.x - ZoneSize.x <= BlockPosition.x && ZoneBlockPosition.x + ZoneSize.x >= BlockPosition.x
			    && ZoneBlockPosition.y - ZoneSize.y <= BlockPosition.y && ZoneBlockPosition.y + ZoneSize.y >= BlockPosition.y
			    &&ZoneBlockPosition.z - ZoneSize.z <= BlockPosition.z && ZoneBlockPosition.z + ZoneSize.z >= BlockPosition.z)
				return BuildingZones[i].GetComponent<Zone>();
		}
		return BlockInZone;
	}

	public void ShowZones(bool IsShow) {
		for (int i = 0; i < BuildingZones.Count; i++) {
			BuildingZones[i].GetComponent<MeshRenderer>().enabled = IsShow;
		}
	}
	
	public bool PlaceBuildingZone(Vector3 BuildingPlacePosition, Vector3 BuildingSize) {
		if (BuildingZonePrefab != null) {
			float SizeIncrease = 0.25f;
			Zone MyZone = ((GameObject)Instantiate (BuildingZonePrefab, BuildingPlacePosition, Quaternion.identity)).GetComponent<Zone> ();
		
			MyZone.transform.localScale = new Vector3 (BuildingSize.x + SizeIncrease, BuildingSize.y + SizeIncrease, BuildingSize.z + SizeIncrease);
			MyZone.transform.position += new Vector3 ((BuildingSize.x) / 2.0f, (BuildingSize.y + SizeIncrease / 2.0f) / 2.0f, (BuildingSize.z) / 2.0f);
			MyZone.InBlockPosition = BuildingPlacePosition + new Vector3 ((BuildingSize.x) / 2.0f, (BuildingSize.y) / 2.0f, (BuildingSize.z) / 2.0f);
			MyZone.Size = BuildingSize / 2f;
			bool CanPlace = CanPlaceBuilding (MyZone);
			if (!CanPlace) {
				Destroy (MyZone.gameObject);
				//BuildingZones.RemoveAt (BuildingZones.Count - 1);
			} else {
				BuildingZones.Add (MyZone.gameObject);
				MyZone.GetComponent<MeshRenderer> ().enabled = IsShowZones;
			}
			return CanPlace;
		} else {
			Debug.LogError ("No Zone Prefab is linked with ZoneManager");
			return false;
		}
	}
	
	public bool CanPlaceBuilding(Zone MyZone) {
		bool IsInsideBox = false;
		for (int i = 0; i < BuildingZones.Count; i++) {
			Vector3 Building1Position = BuildingZones[i].gameObject.transform.position;
			Vector3 Building1Size = BuildingZones[i].gameObject.transform.localScale;
			Vector3 Building2Position = MyZone.transform.position;
			Vector3 Building2Size = MyZone.transform.localScale;
			Debug.Log ("Checking Zone " + i + " 1: " + Building1Position.ToString() + " : " + Building1Size.ToString()
			           +  " 2: " + Building2Position.ToString() + " : " + Building2Size.ToString());
			IsInsideBox = IsInBox (Building1Position, Building1Size, Building2Position, Building2Size);
			if (IsInsideBox) return !IsInsideBox;
		}
		return !IsInsideBox;
	}
	
	public bool IsInBox(Vector3 Position1, Vector3 Size1, Vector3 Position2, Vector3 Size2) {
		// if out side of box
		if (!((Position1.x + Size1.x / 2.0f < Position2.x - Size2.x / 2.0f) ||
		      (Position1.x - Size1.x / 2.0f > Position2.x + Size2.x / 2.0f) ||
		      (Position1.z + Size1.z / 2.0f < Position2.z - Size2.z / 2.0f) ||
		      (Position1.z - Size1.z / 2.0f > Position2.z + Size2.z / 2.0f)))	// left side greater then right side
			return true;
		return false;
	}
	
	// zone stuff end
}
