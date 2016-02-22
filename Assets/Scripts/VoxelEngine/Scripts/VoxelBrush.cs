using UnityEngine;
using System.Collections;

namespace VoxelEngine 
{
	//[RequireComponent(typeof (World))]
	public class VoxelBrush : MonoBehaviour 
	{
		//private World MyWorld;
		[SerializeField] private bool IsDebugMode = false;
		// Brush Settings
		[SerializeField] private Vector3 LastUpdatedPosition;
		[SerializeField] private float VoxelBrushSize = 0;
		[SerializeField] private int VoxelBrushType = 1;
		[SerializeField] private int VoxelBrushRange = 15;
		// Use this for initialization
		void Start () {
			//MyWorld = gameObject.GetComponent<World> ();
		}
		void OnGUI() {
			if (IsDebugMode) 
			{
				GUILayout.Label ("");
				GUILayout.Label ("VoxelBrushSize: " + VoxelBrushSize);
				GUILayout.Label ("VoxelBrushType: " + VoxelBrushType);
				GUILayout.Label ("Last Clicked Position: " + LastUpdatedPosition.ToString());
			}
		} 
		void OnDrawGizmosSelected() {
			Gizmos.color = new Color(1, 1, 1, 1f);
			Gizmos.DrawCube(LastUpdatedPosition+transform.lossyScale/2f, transform.lossyScale);	
		}
	
		// Update is called once per frame
		void Update () {
			/*Debug.DrawLine (Camera.main.gameObject.transform.position, 
			                Camera.main.gameObject.transform.position+Camera.main.gameObject.transform.forward*20f, 
			                Color.blue);*/
			if (Input.GetMouseButtonDown (0)) {
				//LastUpdatedPosition = 
				World.UpdateBlockCamera(VoxelBrushType, VoxelBrushSize, VoxelBrushRange);
			}
			else if (Input.GetMouseButtonDown (1)) 
			{
				//LastUpdatedPosition =
					World.UpdateBlockCamera(0, VoxelBrushSize, VoxelBrushRange);
			}
			if (Input.GetKey (KeyCode.LeftControl)) 
			{
				if (Input.GetAxis ("Mouse ScrollWheel") > 0) {// == 0) { 
					VoxelBrushSize++;
					if (VoxelBrushSize > 8)
						VoxelBrushSize = 8;
				} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {// == 0) { 
					VoxelBrushSize--;
					if (VoxelBrushSize < 0)
						VoxelBrushSize = 0;
				}
			} else {
				if (Input.GetAxis ("Mouse ScrollWheel") > 0) {// == 0) { 
					VoxelBrushType++;
					if (VoxelBrushType > 16)
						VoxelBrushType = 16;
				} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {// == 0) { 
					VoxelBrushType--;
					if (VoxelBrushType < 1)
						VoxelBrushType = 1;
				}
			}
		}
	}
}
