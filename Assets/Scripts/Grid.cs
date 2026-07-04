using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public bool displayGridGizmos;
	public Transform player;

	public LayerMask unwalkableMask;
	public Vector2 gridWolrdSize = Vector2.zero;
	public float nodeRadius = 0.5f;
	Node[,] grid;

	public List<Node> path;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	private void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWolrdSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWolrdSize.y / nodeDiameter);
		CreateGrid();
	}

	public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	private void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];

		Vector3 worldBottmLeft = transform.position -
			Vector3.right * gridWolrdSize.x / 2 -
			Vector3.forward * gridWolrdSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for(int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottmLeft +
					Vector3.right * (x * nodeDiameter + nodeRadius) +
					Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbors(Node node)
	{
		List<Node> neighbors = new List<Node>();
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;
				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbors.Add(grid[checkX, checkY]);
				}
			}
		}
		return neighbors;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWolrdSize.x / 2) / gridWolrdSize.x;
		float percentY = (worldPosition.z + gridWolrdSize.y / 2) / gridWolrdSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);
		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWolrdSize.x, 1, gridWolrdSize.y));
		
		if (grid != null && displayGridGizmos)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}
	}
}