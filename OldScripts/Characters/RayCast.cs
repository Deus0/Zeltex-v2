using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// This will eventually be the main raycast script for characters
// it will use layer masks to raycast behind walls for some things


public class RayCast : MonoBehaviour {
	public GameObject terrain;
	public GameObject target;
	private LayerMask layerMask = (1 << 0);
	public float RayCastRange = 50;
	//private TerrainGenerator TerrainGeneratorScript;
	public Camera MyCamera;
	public Vector3 HitBlock;
	public Vector3 HitNormal;
	public Vector3 PlaceBlock;
	public int NewBlockType;
	public Text HitBlockText;
	public Text PlaceBlockText;
	public int HitTerrainIndex = 0;	// a unique index given to each spawned voxel
	
	public bool DebugRayCamera = false;
	public bool DebugRayMouse = false;

	// Use this for initialization
	void Start () { 
		Cursor.visible = false;
		//TerrainGeneratorScript = terrain.GetComponent("TerrainGenerator") as TerrainGenerator;
	}
	
	// Update is called once per frame
	void Update () {
		//if (Input.GetMouseButtonDown (0))
		//	Debug.Log ("Hitting Left Click");
		//if (Input.GetMouseButtonDown (1))
		//	Debug.Log ("Hitting Right Click");

			//else {	// else if not terrain

			//}
		//}
		/*	if( Physics.Raycast(transform.position, (target.transform.position -
		                                         transform.position).normalized, out hit, distance , layerMask)){
			
			Debug.DrawLine(transform.position,hit.point,Color.red);	// draw a line from the start position to the hit position
			
			
		} else {
			Debug.DrawLine(transform.position,target.transform.position,Color.blue);
		}*/
	}
}
