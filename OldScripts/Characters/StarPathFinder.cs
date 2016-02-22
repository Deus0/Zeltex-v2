using UnityEngine;
using System.Collections.Generic;


//[System.Serializable]
public class GridSystem0 {
	public bool IsObstacle = false;

	//public int Index;
	public GridSystem0() {

	}
};

[System.Serializable]
public class GridSystem1 {
	public List<GridSystem0> Data;
	public GridSystem1() {
		Data = new List<GridSystem0> ();
	}
};
[System.Serializable]
public class GridSystem2 {
	public List<GridSystem1> Data;
	public GridSystem2() {
		Data = new List<GridSystem1> ();
	}
};

//Stores the block data
//Used to get data about it
[System.Serializable]
public class GridSystem {
	public List<GridSystem2> Data;
	public Vector3 Size = new Vector3(16,16,16);

	public GridSystem() {
		Data = new List<GridSystem2> ();
		InitilizeData ();
	}
	public void ClearData() {
		Data.Clear ();
	}
	public void InitilizeData() {
		ClearData ();
		for (int i = 0; i < Size.x; i++) 
		{
			GridSystem2 NewBlock2 = new GridSystem2(); 
			for (int j = 0; j < Size.y; j++) 
			{
				GridSystem1 NewBlock1 = new GridSystem1();
				for (int k = 0; k < Size.z; k++) 
				{
					GridSystem0 NewBlock0 = new GridSystem0();
					NewBlock1.Data.Add(NewBlock0);
				}
				NewBlock2.Data.Add(NewBlock1);
			}
			Data.Add(NewBlock2);
		}
	}

	public GridSystem0 GetGridCell(Vector3 Position) {
		if (IsPositionInBlockBoundaries (Position)) {
			return Data [(int)(Position.x)].Data [(int)(Position.y)].Data [(int)(Position.z)];
		} else {
			// should get it from world class instead
			return new GridSystem0 ();
		}
	}
	public bool IsPositionInBlockBoundaries(Vector3 Position) {
		if (Position.x >= 0 && Position.x < Data.Count)
			if (Position.y >= 0 && Position.y < Data [(int)(Position.x)].Data.Count)
			if (Position.z >= 0 && Position.z < Data [(int)(Position.x)].Data [(int)(Position.y)].Data.Count) {
				return true;
			}
		return false;
	}
};

public class StarNode 
{
	// the position that our node is in
	public Vector3 Position;
	// G = the movement cost to move from the starting point A to a given square on the grid, following the path generated to get there
	// It will cost 1 point for horizontal move, and 1 point for vertical, this can be tweaked to say make it cost more to move vertically, if i want to favour one or the other
	// it can also be expanded upon for chess moves, like the horse movement - one forward and diagnol
	// using 1.4f for diagnol movements
	public float ScoreG = 0f; 
	// H = the estimated movement cost to move from that given square on the grid to the final destination, point B. 
	// This is often referred to as the heuristic, which can be a bit confusing. The reason why it is called that is because it is a guess. We really don’t know the actual distance until we find the path, because all sorts of things can be in the way (walls, water, etc.). 
	// You are given one way to calculate H in this tutorial, but there are many others that you can find in other articles on the web.
	public float ScoreH = 0f;

	public StarNode NextNode = null;	// set to null if end of path

	// the default parent constructor
	public StarNode(Vector3 NewPosition, Vector3 TargetPosition) {
		Position = NewPosition;
		CalculateScoreH (TargetPosition);
	}
	// the constructor for child nodes
	public StarNode(Vector3 NewPosition, StarNode ParentNode, Vector3 TargetPosition) {
		Position = NewPosition;
		ScoreG = ParentNode.ScoreG;
		// Test this later, by giving more score to down movements
		//Vector3 MovementDistance = Position - ParentNode.Position;
		ScoreG ++;	// add a higher score for movement
		CalculateScoreH (TargetPosition);
	}
	
	List<StarNode> PossibleNextNodes = new List<StarNode> ();
	// Checks the surrounding nodes for the lowest FScore
	// TargetPosition is used to calculate H Score inside the constructor
	public StarNode AddNextNode(float Movement, Vector3 TargetPosition) 
	{
		StarNode NodeRight = new StarNode (Position + new Vector3 (Movement, 0, 0), this, TargetPosition);
		StarNode NodeLeft = new StarNode (Position + new Vector3 (-Movement, 0, 0), this, TargetPosition);
		StarNode NodeForward = new StarNode (Position + new Vector3 (0, 0, Movement), this, TargetPosition);
		StarNode NodeBack = new StarNode (Position + new Vector3 (0, 0, -Movement), this, TargetPosition);

		// Put a check here, if Position of node is equal to an obstacle, put its score as Math.Infinity so it never becomes the node

		/*Debug.LogError ("NodeRight: " + NodeRight.Position + " - Score: " + NodeRight.ScoreF().ToString());
		Debug.LogError ("NodeLeft: " + NodeLeft.Position + " - Score: " + NodeLeft.ScoreF().ToString());
		Debug.LogError ("NodeForward: " + NodeForward.Position + " - Score: " + NodeForward.ScoreF().ToString());
		Debug.LogError ("NodeBack: " + NodeBack.Position + " - Score: " + NodeBack.ScoreF().ToString());
		*/
		PossibleNextNodes.Clear ();
		NextNode = NodeRight;
		PossibleNextNodes.Add (NextNode);
		AddNextNode (NextNode, NodeLeft);
		AddNextNode (NextNode, NodeForward);
		AddNextNode (NextNode, NodeBack);

		NextNode = PossibleNextNodes [Random.Range (0,PossibleNextNodes.Count-1)];
		PossibleNextNodes.Clear ();
		return NextNode;
	}

	// checks both nodes to see which one is closer to the target position using the FScore
	void AddNextNode(StarNode PossibleNode1, StarNode PossibleNode2) {
		/*if (PossibleNode1.ScoreF () < PossibleNode2.ScoreF ()) {
			//NextNode = PossibleNode1;
			//PossibleNextNodes.Clear();
			//PossibleNextNodes.Add (NextNode);
		} 
		else */
		if (PossibleNode1.ScoreF () == PossibleNode2.ScoreF ()) {
			PossibleNextNodes.Add (PossibleNode2);
		}
		else if (PossibleNode1.ScoreF () < PossibleNode2.ScoreF ()){
			NextNode = PossibleNode2;
			PossibleNextNodes.Add (NextNode);
		}
	}

	// Manhattan method for calculating H
	// calculate the total number of squares moved horizontally and vertically to reach the target square from the current square
	//	While ignoring diagonal movement, and ignoring any obstacles that may be in the way
	public void CalculateScoreH(Vector3 TargetPosition) 
	{
		ScoreH = Mathf.Abs (TargetPosition.x - Position.x);
		ScoreH += Mathf.Abs (TargetPosition.y - Position.y);
		ScoreH += Mathf.Abs (TargetPosition.z - Position.z);
	}

	public float ScoreF() {
		return ScoreG + ScoreH;
	}
};

//	Our path is generated by repeatedly going through our open list and choosing the square with the lowest F score
public class StarPathFinder 
{
	public bool IsDiagnolMovement = true;
	public bool IsDiagnolMovementThroughObstacles = false;		// maybe for ghosts that travel through walls, make sure i check if the blocks are obstacles here
	GridSystem MyGrid = new GridSystem ();
	List<StarNode> DifferentPaths = new List<StarNode>();

	public StarPathFinder() {}

	public List<Vector3> FindPath(Vector3 Begin, Vector3 End) {

		Begin.x = Mathf.FloorToInt (Begin.x);
		Begin.y = Mathf.FloorToInt (Begin.y);
		Begin.z = Mathf.FloorToInt (Begin.z);
		End.x = Mathf.FloorToInt (End.x);
		End.y = Mathf.FloorToInt (End.y);
		End.z = Mathf.FloorToInt (End.z);

		Debug.LogError ("Finding Path between: " + Begin.ToString () + " : To : " + End.ToString () + " : " + Time.time);
		Debug.LogError ("Path is a distance of: " + (Begin - End).ToString ());

		List<Vector3> PathPoints = new List<Vector3> ();
		if (Begin == End) {
			PathPoints.Add (Begin);
			return PathPoints;
		}

		StarNode MyPath = CalculatePath (Begin, End);
		
		if (MyPath == null) {
			Debug.LogError("Path was not found.");
			PathPoints.Add (End);
		} else {
			Debug.LogError ("Found path!");
			while (MyPath.NextNode != null) {
				PathPoints.Add (MyPath.Position);
				MyPath = MyPath.NextNode;	// iterate to next node
			}
			PathPoints.Add (MyPath.Position);	// as the while loop ends when next node == null, it cuts off just before the end point
			Debug.LogError ("Path was: " + PathPoints.Count + " in Size.");
		}
		return PathPoints;
	}
	public StarNode CalculatePath(Vector3 Begin, Vector3 End) {
		StarNode BeginNode = new StarNode (Begin, End);
		StarNode CurrentNode = BeginNode;

		for (int i = 0; i < 1000; i++) 
		{
			//Debug.LogError ("NodePosition: " + CurrentNode.Position + " - Score: " + CurrentNode.ScoreF().ToString());
			CurrentNode = CurrentNode.AddNextNode(1, End);
			//Debug.LogError ("NextNodePosition: " + CurrentNode.Position + " - Score: " + CurrentNode.ScoreF().ToString());

			// if Found a path return it
			if (CurrentNode.Position == End)
				return BeginNode;
			//i = 1000;	// debug break
			//Debug.Break();
		}
		return null;
	}
};                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 