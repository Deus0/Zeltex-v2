using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace OldCode {
[System.Serializable]
public class GRIDCELL {
	public List<Vector3> points = new List<Vector3>();
	public List<float> weights = new List<float>();

	// each grid cell has 4 corners with different weightings
	public GRIDCELL() {	
		for (int i = 0; i < 8; i++) {
			points.Add(new Vector3(0, 0, 0));
			weights.Add(0.0f);
		}
	}
};

[System.Serializable]
public class TRIANGLE {
	public List<Vector3> position = new List<Vector3>();
	public List<Vector3> tex = new List<Vector3>();
	public List<Color32> colors = new List<Color32>();
	public List<Vector3> normals = new List<Vector3>();
	public Vector3 normal = new Vector3(0, 1, 0);
	
	public TRIANGLE() {
		for (int i = 0; i < 3; i++) {
			position.Add(new Vector3());
			normals.Add(new Vector3());
			tex.Add(new Vector3());
			colors.Add(new Color32());
			colors[i] = new Color32(255, 255, 255, 255);
		}
	}
	
	public TRIANGLE(int pie, TRIANGLE t) {
		normal = new Vector3(t.normal.x,t.normal.y, t.normal.z);
		for (int i = 0; i < 3; i++) {
			position.Add(new Vector3());
			normals.Add(new Vector3());
			tex.Add(new Vector3());
			colors.Add(new Color32());
			position[i] = new Vector3(t.position[i].x,t.position[i].y, t.position[i].z);
			colors[i] = new Color32(255,255,255,255);
		}
	}
	
	// No idea whats happening here
	public void calcnormal(bool invertnormals) {
		Vector3 nv = new Vector3();
		Vector3 v1 = new Vector3();
		Vector3 v2 = new Vector3();
		v1 = new Vector3((position[1].x - position[0].x), (position[1].y - position[0].y),
		             (position[1].z - position[0].z));
		v2 = new Vector3((position[2].x - position[0].x), (position[2].y - position[0].y),
		             (position[2].z - position[0].z));
		
		nv.x = (v1.y * v2.z) - (v1.z * v2.y);
		nv.y = (v1.z * v2.x) - (v1.x * v2.z);
		nv.z = (v1.x * v2.y) - (v1.y * v2.x);
		if (!invertnormals) {
			normal.x = nv.x;
			normal.y = nv.y;
			normal.z = nv.z;
		}
		else {
			normal.x = -nv.x;
			normal.y = -nv.y;
			normal.z = -nv.z;
		}
		normals[0] = normal;
		normals[1] = normal;
		normals[2] = normal;
		
		v1 = new Vector3((position[0].x - position[1].x), (position[0].y - position[1].y),(position[0].z - position[1].z));
		v2 = new Vector3((position[2].x - position[1].x), (position[2].y - position[1].y),(position[2].z - position[1].z));
		
		nv.x = (v1.y * v2.z) - (v1.z * v2.y);
		nv.y = (v1.z * v2.x) - (v1.x * v2.z);
		nv.z = (v1.x * v2.y) - (v1.y * v2.x);
		if (!invertnormals) {
			normal.x = nv.x;
			normal.y = nv.y;
			normal.z = nv.z;
		}
		else {
			normal.x = -nv.x;
			normal.y = -nv.y;
			normal.z = -nv.z;
		}
		normals[1] = normal;
		
		v1 = new Vector3((position[0].x - position[2].x), (position[0].y - position[2].y), (position[0].z - position[2].z)); 
		v2 = new Vector3((position[1].x - position[2].x), (position[1].y - position[2].y), (position[1].z - position[2].z));
		
		nv.x = (v1.y * v2.z) - (v1.z * v2.y);
		nv.y = (v1.z * v2.x) - (v1.x * v2.z);
		nv.z = (v1.x * v2.y) - (v1.y * v2.x);
		if (!invertnormals) {
			normal.x = nv.x;
			normal.y = nv.y;
			normal.z = nv.z;
		}
		else {
			normal.x = -nv.x;
			normal.y = -nv.y;
			normal.z = -nv.z;
		}
		normals[2] = normal;
	}
};


[System.Serializable]
public class MarchingCubes {
	
	//static int edgeTable[256] = new int[256] { // 256
	static int[] edgeTable = new int[256] {
		0x0, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c, 0x80c, 0x905, 0xa0f,
		0xb06, 0xc0a, 0xd03, 0xe09, 0xf00, 0x190, 0x99, 0x393, 0x29a,
		0x596, 0x49f, 0x795, 0x69c, 0x99c, 0x895, 0xb9f, 0xa96, 0xd9a,
		0xc93, 0xf99, 0xe90, 0x230, 0x339, 0x33, 0x13a, 0x636, 0x73f,
		0x435, 0x53c, 0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39,
		0xd30, 0x3a0, 0x2a9, 0x1a3, 0xaa, 0x7a6, 0x6af, 0x5a5, 0x4ac,
		0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0, 0x460,
		0x569, 0x663, 0x76a, 0x66, 0x16f, 0x265, 0x36c, 0xc6c, 0xd65,
		0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60, 0x5f0, 0x4f9, 0x7f3,
		0x6fa, 0x1f6, 0xff, 0x3f5, 0x2fc, 0xdfc, 0xcf5, 0xfff, 0xef6,
		0x9fa, 0x8f3, 0xbf9, 0xaf0, 0x650, 0x759, 0x453, 0x55a, 0x256,
		0x35f, 0x55, 0x15c, 0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53,
		0x859, 0x950, 0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5,
		0xcc, 0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
		0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0xcc,
		0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0, 0x950, 0x859,
		0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c, 0x15c, 0x55, 0x35f,
		0x256, 0x55a, 0x453, 0x759, 0x650, 0xaf0, 0xbf9, 0x8f3, 0x9fa,
		0xef6, 0xfff, 0xcf5, 0xdfc, 0x2fc, 0x3f5, 0xff, 0x1f6, 0x6fa,
		0x7f3, 0x4f9, 0x5f0, 0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f,
		0xd65, 0xc6c, 0x36c, 0x265, 0x16f, 0x66, 0x76a, 0x663, 0x569,
		0x460, 0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
		0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa, 0x1a3, 0x2a9, 0x3a0, 0xd30,
		0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c, 0x53c, 0x435,
		0x73f, 0x636, 0x13a, 0x33, 0x339, 0x230, 0xe90, 0xf99, 0xc93,
		0xd9a, 0xa96, 0xb9f, 0x895, 0x99c, 0x69c, 0x795, 0x49f, 0x596,
		0x29a, 0x393, 0x99, 0x190, 0xf00, 0xe09, 0xd03, 0xc0a, 0xb06,
		0xa0f, 0x905, 0x80c, 0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203,
		0x109, 0x0
	};
	
	static int[][] triTable = new int[256][] {
		new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
		new int[] {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
		new int[] {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
		new int[] {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
		new int[] {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
		new int[] {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
		new int[] {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
		new int[] {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
		new int[] {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
		new int[] {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
		new int[] {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
		new int[] {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
		new int[] {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
		new int[] {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
		new int[] {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
		new int[] {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
		new int[] {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
		new int[] {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
		new int[] {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
		new int[] {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
		new int[] {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
		new int[] {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
		new int[] {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
		new int[] {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
		new int[] {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
		new int[] {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
		new int[] {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
		new int[] {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
		new int[] {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
		new int[] {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
		new int[] {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
		new int[] {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
		new int[] {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
		new int[] {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
		new int[] {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
		new int[] {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
		new int[] {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
		new int[] {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
		new int[] {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
		new int[] {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
		new int[] {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
		new int[] {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
		new int[] {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
		new int[] {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
		new int[] {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
		new int[] {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
		new int[] {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
		new int[] {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
		new int[] {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
		new int[] {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
		new int[] {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
		new int[] {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
		new int[] {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
		new int[] {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
		new int[] {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
		new int[] {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
		new int[] {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
		new int[] {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
		new int[] {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
		new int[] {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
		new int[] {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
		new int[] {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
		new int[] {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
		new int[] {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
		new int[] {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
		new int[] {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
		new int[] {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
		new int[] {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
		new int[] {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
		new int[] {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
		new int[] {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
		new int[] {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
		new int[] {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
		new int[] {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
		new int[] {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
		new int[] {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
		new int[] {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
		new int[] {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
		new int[] {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
		new int[] {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
		new int[] {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
		new int[] {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
		new int[] {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
		new int[] {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
		new int[] {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
		new int[] {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
		new int[] {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
		new int[] {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
		new int[] {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
		new int[] {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
		new int[] {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
		new int[] {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
		new int[] {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
		new int[] {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
		new int[] {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
		new int[] {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
		new int[] {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
		new int[] {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
		new int[] {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
		new int[] {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
		new int[] {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
		new int[] {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
		new int[] {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
		new int[] {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
		new int[] {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
		new int[] {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
		new int[] {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
		new int[] {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
		new int[] {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
		new int[] {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
		new int[] {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
	};

	public MarchingCubes() {}

	//MarchingCubes(Vector3 MaxSize, int resolution_, List<float> data);
	public List<TRIANGLE> triangles = new List<TRIANGLE>();
	public List<TRIANGLE> trilist = new List<TRIANGLE>();
	List<float> data = new List<float> ();
	public float cubeIsoVal = 0.01f;
	public float isolevel;
	public GRIDCELL grid;
	public Vector3 worlddim, worldstride, worldcenter, datastride;
	public float resolution = 32;
	public Vector3 CubeSize;
	public Vector3 WorldSize;
	public Vector3 gridSize;
	public bool closesides = true;
	
	public MarchingCubes(Vector3 MaxSize, int resolution_) {
		resolution = resolution_;
		gridSize = new Vector3(resolution, resolution, resolution);
		//gxgy = resolution * resolution;
		//numxyz = resolution * resolution * resolution;	// totalNumberOfPoints
		worlddim = MaxSize;
		worldstride = new Vector3(gridSize.x / MaxSize.x, gridSize.y / MaxSize.y, gridSize.z / MaxSize.z);
		datastride = new Vector3(MaxSize.x / gridSize.x, MaxSize.y / gridSize.y, MaxSize.z / gridSize.z);
		worldcenter = new Vector3(MaxSize.x / 2.0f, MaxSize.y / 2.0f, MaxSize.z / 2.0f);
		WorldSize = new Vector3(resolution, resolution, resolution);
		for (int i = 0; i < 5; i++) {
			triangles.Add(new TRIANGLE());
		}
		//isolevel = 0.0161f;
		grid = new GRIDCELL();
	}
	
	// the equations are the key
	// iso value is the value per 
	public Vector3 VertexInterp(float isolevel, Vector3 p1, Vector3 p2, float valp1, float valp2) {
		float mu;
		Vector3 position = new Vector3();
		
		if (Mathf.Abs(isolevel - valp1) < 0.00001)
			return(p1);
		if (Mathf.Abs(isolevel - valp2) < 0.00001)
			return(p2);
		if (Mathf.Abs(valp1 - valp2) < 0.00001)
			return(p1);
		mu = (isolevel - valp1) / (valp2 - valp1);
		position.x = p1.x + mu * (p2.x - p1.x);
		position.y = p1.y + mu * (p2.y - p1.y);
		position.z = p1.z + mu * (p2.z - p1.z);
		
		return(position);
	}
	public float GetData(int DataIndex) {
		if (DataIndex >= 0 && DataIndex < data.Count)
			return data [DataIndex];
		return 0;
	}
	// inserted into this, should be something like all the surrounding cubes
	public void PolygoniseCube(int i, int j, int k, int BlockType) {
		Vector3 BlockSize = new Vector3(1, 1, 1);
		grid.weights[0] = GetData(Mathf.RoundToInt(i + j * WorldSize.x + k * WorldSize.x * WorldSize.x)); // i j k position in grid
		grid.weights[1] = GetData(Mathf.RoundToInt(i + 1 + j * WorldSize.x + k  * WorldSize.x * WorldSize.x));
		grid.weights[2] = GetData(Mathf.RoundToInt(i + 1 + (j + 1) * WorldSize.x + k  * WorldSize.x * WorldSize.x));
		grid.weights[3] = GetData(Mathf.RoundToInt(i + (j + 1) * WorldSize.x + k * WorldSize.x * WorldSize.x));
		grid.weights[4] = GetData(Mathf.RoundToInt(i + j * WorldSize.x + (k + 1) * WorldSize.x * WorldSize.x));
		grid.weights[5] = GetData(Mathf.RoundToInt(i + 1 + j * WorldSize.x + (k + 1) * WorldSize.x * WorldSize.x));
		grid.weights[6] = GetData(Mathf.RoundToInt(i + 1 + (j + 1) * WorldSize.x + (k + 1) * WorldSize.x * WorldSize.x));
		grid.weights[7] = GetData(Mathf.RoundToInt(i + (j + 1) * WorldSize.x + (k + 1)  * WorldSize.x * WorldSize.x));
		// points of a cube
		grid.points[0] = new Vector3 (i * BlockSize.x, 		j * BlockSize.y, 		k * BlockSize.z);
		grid.points[1] = new Vector3 ((i + 1) * BlockSize.x,  	j * BlockSize.y, 		k * BlockSize.z);
		grid.points[2] = new Vector3 ((i + 1) * BlockSize.x,	(j + 1) * BlockSize.y , k * BlockSize.z);
		grid.points[3] = new Vector3 (i * BlockSize.x, 			(j + 1) * BlockSize.y,  k * BlockSize.z);
		grid.points[4] = new Vector3 (i * BlockSize.x,			j * BlockSize.y , 		(k + 1) * BlockSize.z);
		grid.points[5] = new Vector3 ((i + 1) * BlockSize.x, 	j * BlockSize.y, 		(k + 1) * BlockSize.z);
		grid.points[6] = new Vector3 ((i + 1) * BlockSize.x,	(j + 1) * BlockSize.y , (k + 1) * BlockSize.z);
		grid.points[7] = new Vector3 (i * BlockSize.x, 			(j + 1) * BlockSize.y, 	(k + 1) * BlockSize.z);

		// ^^ p - points of the cube
		// val - iso value of the points -> depends on the grid values -> maybe I can try inputting these into the function and base them off my blockData
		int TriangleCount = Polygonise(isolevel);// , triangles);
		
		for (int z = 0; z < TriangleCount; z++) {
			triangles[z].colors[0] = new Color32(255, 255, 255, (byte)BlockType);
			triangles[z].colors[1] = new Color32(255, 255, 255, (byte)BlockType);
			triangles[z].colors[2] = new Color32(255, 255, 255, (byte)BlockType);
			trilist.Add(triangles[z]);
		}
	}
	
	//List<float> MarchingCubesData = new List<float>();
	// the outter edges of the data file will be the other chunks blocks, but they won't be rendered
	public void PolygonizeData(Blocks MyBlocks, Vector3 WorldSize_) {
		data.Clear ();
		for (int i = 0; i < (MyBlocks.Size.x)*(MyBlocks.Size.y)*(MyBlocks.Size.z); i++)
			data.Add(0.0f);
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				Vector3 loc = new Vector3 (i, j, k);
				//Debug.Log ("Added cube " + MyBlocks.GetBlockType(loc).ToString());
				if (MyBlocks.GetBlockType(loc) != 0)  {	// if no mesh create a mes
					AddCube (loc, new Vector3 (1, 1, 1));
					//Debug.Log ("Added cube " + loc.ToString());
				}
			}

		trilist.Clear();
		CubeSize = WorldSize_;
		WorldSize = WorldSize_;
		int BlockType = 1;
		gridSize = WorldSize;
		//numxyz = WorldSize.x*WorldSize.y*WorldSize.z;
		worlddim = WorldSize;
		worldstride = new Vector3(gridSize.x / WorldSize.x, gridSize.y / WorldSize.y, gridSize.z / WorldSize.z);
		datastride = new Vector3(WorldSize_.x / gridSize.x, WorldSize_.y / gridSize.y, WorldSize_.z / gridSize.z);
		worldcenter = new Vector3(WorldSize.x / 2, WorldSize.x / 2, WorldSize.x / 2);
		
		for (int i = 0; i < MyBlocks.Size.x; i++) {
			for (int j = 0; j < MyBlocks.Size.y; j++) {
				for (int k = 0; k < MyBlocks.Size.z; k++) {
					//BlockType = NewBlockData[i].Data[j].Data[k].BlockType;// this doesn't work as the blocks don't perfec
					PolygoniseCube(i,j,k,BlockType);
				}
			}
		}
	}


	// as triangles doesnt pass by reference, it doesnt get saved
	public int Polygonise(float isolevel){//, List<TRIANGLE> triangles) {
		//return TriangleCount;
		int i;// , ntriang;
		int cubeindex;
		Vector3[] vertlist = new Vector3[12];
		/*
	* Determine the index into the edge table which tells us which vertices
	* are inside of the surface
	*/

		cubeindex = 0;
		if (grid.weights[0] < isolevel) {
			cubeindex |= 1;
		}
		if (grid.weights[1] < isolevel) {
			cubeindex |= 2;
		}
		if (grid.weights[2] < isolevel) {
			cubeindex |= 4;
		}
		if (grid.weights[3] < isolevel) {
			cubeindex |= 8;
		}
		if (grid.weights[4] < isolevel) {
			cubeindex |= 16;
		}
		if (grid.weights[5] < isolevel) {
			cubeindex |= 32;
		}
		if (grid.weights[6] < isolevel) {
			cubeindex |= 64;
		}
		if (grid.weights[7] < isolevel) {
			cubeindex |= 128;
		}
		
		// if Cube is entirely in/out of the surface
		if (edgeTable[cubeindex] == 0) {
			return (0);
		}
		
		//Find the vertices where the surface intersects the cube
		// int temp = edgeTable[cubeindex] & 1;
		if ((edgeTable[cubeindex] & 1) != 0) {
			vertlist[0] = VertexInterp(isolevel, grid.points[0], grid.points[1],
			                           grid.weights[0], grid.weights[1]);
		}
		if ((edgeTable[cubeindex] & 2) != 0) {
			vertlist[1] = VertexInterp(isolevel, grid.points[1], grid.points[2],
			                           grid.weights[1], grid.weights[2]);
		}
		if ((edgeTable[cubeindex] & 4) != 0) {
			vertlist[2] = VertexInterp(isolevel, grid.points[2], grid.points[3],
			                           grid.weights[2], grid.weights[3]);
		}
		if ((edgeTable[cubeindex] & 8) != 0) {
			vertlist[3] = VertexInterp(isolevel, grid.points[3], grid.points[0],
			                           grid.weights[3], grid.weights[0]);
		}
		if ((edgeTable[cubeindex] & 16) != 0) {
			vertlist[4] = VertexInterp(isolevel, grid.points[4], grid.points[5],
			                           grid.weights[4], grid.weights[5]);
		}
		if ((edgeTable[cubeindex] & 32) != 0) {
			vertlist[5] = VertexInterp(isolevel, grid.points[5], grid.points[6],
			                           grid.weights[5], grid.weights[6]);
		}
		if ((edgeTable[cubeindex] & 64) != 0) {
			vertlist[6] = VertexInterp(isolevel, grid.points[6], grid.points[7],
			                           grid.weights[6], grid.weights[7]);
		}
		if ((edgeTable[cubeindex] & 128) != 0) {
			vertlist[7] = VertexInterp(isolevel, grid.points[7], grid.points[4],
			                           grid.weights[7], grid.weights[4]);
		}
		if ((edgeTable[cubeindex] & 256) != 0) {
			vertlist[8] = VertexInterp(isolevel, grid.points[0], grid.points[4],
			                           grid.weights[0], grid.weights[4]);
		}
		if ((edgeTable[cubeindex] & 512) != 0) {
			vertlist[9] = VertexInterp(isolevel, grid.points[1], grid.points[5],
			                           grid.weights[1], grid.weights[5]);
		}
		if ((edgeTable[cubeindex] & 1024) != 0) {
			vertlist[10] = VertexInterp(isolevel, grid.points[2], grid.points[6],
			                            grid.weights[2], grid.weights[6]);
		}
		if ((edgeTable[cubeindex] & 2048) != 0) {
			vertlist[11] = VertexInterp(isolevel, grid.points[3], grid.points[7],
			                            grid.weights[3], grid.weights[7]);
		}
		/* Create the triangle */
		int TriangleCount = 0;
		for (i = 0; triTable[cubeindex][i] != -1; i += 3) {
			triangles[TriangleCount].position[0] = vertlist[triTable[cubeindex][i + 2]];
			triangles[TriangleCount].position[1] = vertlist[triTable[cubeindex][i + 1]];
			triangles[TriangleCount].position[2] = vertlist[triTable[cubeindex][i]];
			TriangleCount++;
		}
		
		return (TriangleCount);
	}
	
	public void AddCube(Vector3 Position, Vector3 Dimensions) {
		float val = cubeIsoVal;	// isovalue or w/e
		// replace this getindex
		Vector3 centre = new Vector3((int)(Position.x * worldstride.x), (int)(Position.y * worldstride.y), (int)(Position.z * worldstride.z));
		
		if (closesides) {
			if (centre.x < 1) {
				centre.x = 1;
			}
			if (centre.y < 1) {
				centre.y = 1;
			}
			if (centre.z < 1) {
				centre.z = 1;
			}
			if (centre.x >= gridSize.x - 1) {
				centre.x = gridSize.x - 2;
			}
			if (centre.y >= gridSize.y - 1) {
				centre.y = gridSize.y - 2;
			}
			if (centre.z >= gridSize.z - 1) {
				centre.z = gridSize.z - 2;
			}
		}
		else {
			if (centre.x < 0) {
				centre.x = 0;
			}
			if (centre.y < 0) {
				centre.y = 0;
			}
			if (centre.z < 0) {
				centre.z = 0;
			}
			if (centre.x >= gridSize.x) {
				centre.x = gridSize.x - 1;
			}
			if (centre.y >= gridSize.y) {
				centre.y = gridSize.y - 1;
			}
			if (centre.z >= gridSize.z) {
				centre.z = gridSize.z - 1;
			}
		}
		Vector3 dimension = new Vector3((int)(Dimensions.x * worldstride.x),
		                            (int)(Dimensions.y * worldstride.y),
		                            (int)(Dimensions.z * worldstride.z));
		if (dimension.x < 2) {
			dimension.x = 2;
		}
		if (dimension.y < 2) {
			dimension.y = 2;
		}
		if (dimension.z < 2) {
			dimension.z = 2;
		}
		Vector3 height = new Vector3(dimension.x / 2, dimension.y / 2, dimension.z / 2);
		Vector3 size = new Vector3(centre.x - height.x, centre.y - height.y, centre.z - height.z);
		
		if (size.x < 1) {
			size.x = 1;
		}
		if (size.y < 1) {
			size.y = 1;
		}
		if (size.z < 1) {
			size.z = 1;
		}
		Vector3 total = new Vector3(Mathf.Min((int)(centre.x + height.x), (int)gridSize.x - 1),
		                            Mathf.Min((int)(centre.y + height.y), (int)gridSize.y - 1),
		                            Mathf.Min((int)(centre.z + height.z), (int)gridSize.z - 1));
		
		//Vector3 total = new Vector3(1, 1, 1);
		//Vector3 size = new Vector3(0, 0, 0);
		// println("adding cube: "+val+" "+sz+" "+tz+" "+dimz);
		int index = 0;
		for (float i = size.x; i < total.x; i++) {
			for (float j = size.y; j < total.y; j++) {
				for (float k = size.z; k < total.z; k++) {
					index = Mathf.RoundToInt(i + j * gridSize.x + k * gridSize.x * gridSize.y);
					if (index >= 0 && index < data.Count)
						data[index] += val;
				}
			}
		}
	}
	
	public void ClearData() {
		for (int i = 0; i < data.Count; i++) {
			data[i] = 0.0f;
		}
		trilist.Clear();
	}

}
}