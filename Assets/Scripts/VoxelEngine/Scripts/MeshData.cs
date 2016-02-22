using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine {
	[System.Serializable]
	public class MeshData 
	{
		[SerializeField] public List<Vector3> Verticies = new List<Vector3>();
		[SerializeField] public List<int> Triangles = new List<int>();
		[SerializeField] public List<Vector2> TextureCoordinates = new List<Vector2>();
		[SerializeField] public List<Color32> Colors = new List<Color32>();
		
		public MeshData() { }

		public bool HasData() {
			return (Verticies.Count > 0);
		}
		public void AddToVertex(Vector3 Addition) 
		{
			for (int i = 0; i < Verticies.Count; i++) 
			{
				Verticies[i] += (Addition);
			}
		}
		public Vector3[] GetVerticies() {
			return Verticies.ToArray ();
		}
		public Vector2[] GetTextureCoordinates() {
			return TextureCoordinates.ToArray ();
		}
		public int[] GetTriangles() {
			return Triangles.ToArray ();
		}
		public List<Color32> GetColors() {
			return Colors;
		}
		public void Clear() {
			Verticies.Clear ();
			Triangles.Clear ();
			TextureCoordinates.Clear ();
			Colors.Clear ();
		}
		public void Add(MeshData NewMesh) {
			int TrianglesBuffer = Verticies.Count;
			for (int i = 0; i < NewMesh.Verticies.Count; i++) {
				Verticies.Add(NewMesh.Verticies[i]);
			}
			for (int i = 0; i < NewMesh.Triangles.Count; i++) {
				Triangles.Add(TrianglesBuffer+NewMesh.Triangles[i]);
			}
			for (int i = 0; i < NewMesh.TextureCoordinates.Count; i++) {
				TextureCoordinates.Add(NewMesh.TextureCoordinates[i]);
			}
			for (int i = 0; i < NewMesh.Colors.Count; i++) {
				Colors.Add(NewMesh.Colors[i]);
			}
		}

		public void AddTriangle(Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3) 
		{
			Verticies.Add (Vertex1);
			Verticies.Add (Vertex2);
			Verticies.Add (Vertex3);
			Triangles.Add(Verticies.Count - 3);
			Triangles.Add(Verticies.Count - 2);
			Triangles.Add(Verticies.Count - 1);
		}
		
		public void AddQuadUVs(int VoxelIndex, int TileResolution) 
		{
			float TotalTiles = TileResolution * TileResolution;
			float TileSize = 1f/((float)TileResolution);
			
			float Buffer = 1f/(TileResolution * 16f);	// one pixel is the buffer
			//Debug.LogError ("Buffer: " + Buffer);
			float PosX = (VoxelIndex % TileResolution)*TileSize;
			float PosY = (VoxelIndex / TileResolution)*TileSize;
			TextureCoordinates.Add (new Vector2 (PosX+Buffer, 
			                                     PosY+Buffer));
			TextureCoordinates.Add (new Vector2 (PosX+TileSize-Buffer, 
			                                     PosY+Buffer));
			TextureCoordinates.Add (new Vector2 (PosX+TileSize-Buffer, 
			                                     PosY+TileSize-Buffer));
			TextureCoordinates.Add (new Vector2 (PosX+Buffer, 
			                                     PosY+TileSize-Buffer));
		}

		public void AddQuadUVs() 
		{
			TextureCoordinates.Add (new Vector2 (0, 0));
			TextureCoordinates.Add (new Vector2 (1, 0));
			TextureCoordinates.Add (new Vector2 (1, 1));
			TextureCoordinates.Add (new Vector2 (0, 1));
		}
		public void AddQuadColours(byte Brightness) {
			for (int i = 0; i < 4; i++)
				AddColor (new Color32 (Brightness, Brightness, Brightness, 255));
		}

		public void AddQuadTriangles()
		{
			Triangles.Add(Verticies.Count - 4);
			Triangles.Add(Verticies.Count - 3);
			Triangles.Add(Verticies.Count - 2);
			
			Triangles.Add(Verticies.Count - 4);
			Triangles.Add(Verticies.Count - 2);
			Triangles.Add(Verticies.Count - 1);
		}

		public void AddColor(Color32 NewColor) {
			Colors.Add (NewColor);
		}

		public void AddVertex(Vector3 NewVertex)
		{
			Verticies.Add(NewVertex);
		}
		
		public void AddTriangle(int tri)
		{
			Triangles.Add(tri);
		}
	}
}