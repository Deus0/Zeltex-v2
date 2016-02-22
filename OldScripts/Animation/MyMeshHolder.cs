using UnityEngine;
using System.Collections;

namespace OldCode {
public class MyMeshHolder : MonoBehaviour {
	public BlockStructure MyBlockStructure;
	public MyMesh MeshData = new MyMesh();
	public BlockMesher MyBlockMesher = new BlockMesher ();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		MyBlockMesher.Update ();
		if (MyBlockStructure.MyBlocks.HasUpdated) {
			MyBlockMesher.UpdateMeshesOnThread (MeshData, MyBlockStructure.MyBlocks);
			MyBlockStructure.MyBlocks.HasUpdated = false;
		}
		if (MyBlockMesher.CanUpdateMesh) {	// has finished generating mesh data
			UpdateMesh();
			MyBlockMesher.CanUpdateMesh = false;
			MyBlockMesher.IsUpdatingMesh = false;
		}
	}
	public void UpdateMesh() {
		if (gameObject.GetComponent<SkinnedMeshRenderer> ()) {
			//gameObject.GetComponent<CustomBones>().MySkinnedMesh.GetSkinnedDataFromMesh();
			//gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh = new Mesh ();
			if (gameObject.GetComponent<MeshCollider>() == null)
				gameObject.AddComponent<MeshCollider>();
			MeshData.UpdateMeshWithData (gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh, gameObject.GetComponent<MeshCollider> (), MyBlockStructure.MyBlocks.IsCentred);
		}
	}
}
}