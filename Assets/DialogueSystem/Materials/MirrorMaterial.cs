using UnityEngine;
using System.Collections;

public class MirrorMaterial : MonoBehaviour {
	private RenderTexture renderTexture;
	public int renderTextureSize = 512;
	// Use this for initialization
	void Start () {  
		/*if( !RenderTexture.enabled ) {
			Debug.LogError("Render textures are not available. Disabling mirror script");
			enabled = false;
			return;
		}*/
		
		renderTexture = new RenderTexture( renderTextureSize, renderTextureSize, 16 );
		renderTexture.isPowerOfTwo = true;
		GameObject CameraObject = new GameObject ();
		CameraObject.transform.SetParent (transform, false);
		CameraObject.transform.localEulerAngles = new Vector3 (-90, 0, 0);
		Camera cam = CameraObject.AddComponent<Camera>();
		Camera mainCam = Camera.main;
		cam.targetTexture = renderTexture;
		cam.clearFlags = mainCam.clearFlags;
		cam.cullingMask = mainCam.cullingMask;
		cam.backgroundColor = mainCam.backgroundColor;
		cam.nearClipPlane = mainCam.nearClipPlane;
		cam.farClipPlane = mainCam.farClipPlane;
		cam.fieldOfView = mainCam.fieldOfView;
		
		gameObject.GetComponent<MeshRenderer> ().material.mainTexture = renderTexture;	//SetTexture("_Texture", renderTexture);
		
		//ReflectionRenderTexture reflScript = gameObject.GetComponent(ReflectionRenderTexture);
		//reflScript.m_ReflectUpperSide = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
