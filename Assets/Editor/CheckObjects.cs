#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

class CheckObjects
{
	[MenuItem("Marz/Worlds/CheckObjects")]
	static void CheckAllObjects()
	{
		//GameObject[] MyObjects =
		
		VoxelEngine.World[] MyObjects = GameObject.FindObjectsOfType(typeof(VoxelEngine.World)) as VoxelEngine.World[];
		if (MyObjects.Length == 0) 
		{
			// add new game object here
			Debug.Log("Creating World.");
		} else {
			Debug.Log("Scene already contains a world.");
		}
	}
}
#endif
