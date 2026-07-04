using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool baseWalkable;
    public bool walkable;
    public bool hasTower;

    public Vector3 worldPosition;
	public int gridX;
	public int gridY;
	public int movementPenalty;

    public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;

	public Node(bool _walkable, Vector3 _worldPosition, int gridX, int gridY, int _penalty)
	{
        walkable = _walkable;
        baseWalkable = _walkable;

        worldPosition = _worldPosition;
		this.gridX = gridX;
		this.gridY = gridY;
		this.movementPenalty = _penalty;

		hasTower = false;
    }

    public void RefreshWalkable()
    {
        walkable = baseWalkable && !hasTower;
    }

    public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get { return heapIndex; }
		set { heapIndex = value; }
	}

	public int CompareTo(Node other)
	{
		int compare = fCost.CompareTo(other.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(other.hCost);
		}
		return -compare;
	}
}
