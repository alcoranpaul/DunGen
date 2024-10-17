using System;
using System.Collections.Generic;
using GridSystem;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// PathFinding Script.
/// </summary>
public class PathFinding<T> where T : PathNode<T>
{
	public GridSystem<T> GridSystem { get; private set; }
	public delegate void TentativeGCostDelegate(ref int tentativeGCost, T node);


	public PathFinding(Vector2 dimension, Func<GridSystem<T>, GridPosition, T> createGridObject, float unitScale = 1)
	{
		GridSystem = new GridSystem<T>(dimension, unitScale, createGridObject);

	}

	public PathFinding(int dimension, Func<GridSystem<T>, GridPosition, T> createGridObject, float unitScale = 1)
	{
		GridSystem = new GridSystem<T>(new Vector2(dimension), unitScale, createGridObject);

	}

	public void ToggleNeighborWalkable(GridPosition basePosition, int Width, int Length, bool flag)
	{
		List<GridPosition> positions = GetNeighborhood(basePosition, Width, Length);

		foreach (GridPosition pos in positions)
			ToggleNodeWalkable(pos, flag);
	}

	public List<GridPosition> GetNeighborhood(GridPosition basePosition, int Width, int Length)
	{
		List<GridPosition> positions = new List<GridPosition>();
		int gridWidth = GridSystem.ToGridSize(Width);
		int gridLength = GridSystem.ToGridSize(Length);

		int widthOffset = gridWidth / 2;
		int lengthOffset = gridLength / 2;


		for (int i = 0; i < gridWidth; i++)
		{
			for (int j = 0; j < gridLength; j++)
			{
				GridPosition pos = new GridPosition(basePosition.X - widthOffset + i, basePosition.Z - lengthOffset + j);
				positions.Add(pos);
			}
		}

		return positions;
	}


	public BoundingBox GetBoundingBox()
	{
		return GridSystem.GetBoundingBox();
	}

	public void SpawnDebugObjects(Prefab debugGridPrefab)
	{
		GridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public T GetNode(int x, int z)
	{
		GridPosition position = new(x, z);
		return GetNode(position);
	}

	public T GetNode(GridPosition position)
	{
		if (!GridSystem.IsPositionValid(position)) return null;
		return GridSystem.GetGridObject(position);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end, TentativeGCostDelegate GCostDelegate = null)
	{
		List<T> openList = new List<T>(); // Nodes to be evaluated
		List<T> closedList = new List<T>(); // Already visited nodes

		// Add Start node to the open list
		T startNode = GetNode(start);
		T endNode = GetNode(end);

		// Check if start or end node is not walkable
		if (!startNode.IsWalkable)
		{
			startNode = FindNearestWalkableNode(startNode);
			if (startNode == null)
			{
				Debug.Log("No walkable starting node found.");
				return null;
			}
		}

		if (!endNode.IsWalkable)
		{
			endNode = FindNearestWalkableNode(endNode);
			if (endNode == null)
			{
				Debug.Log("No walkable ending node found.");
				return null;
			}
		}

		openList.Add(startNode);

		DebugDraw.DrawSphere(new BoundingSphere(GridSystem.GetWorldPosition(startNode.GridPosition), 15f), Color.DarkRed, 60f);

		Vector3 asd = GridSystem.GetWorldPosition(endNode.GridPosition);
		asd.Y += 100f;

		DebugDraw.DrawSphere(new BoundingSphere(asd, 15f), Color.Azure, 60f);

		int dimensionX = (int)GridSystem.Dimension.X;
		int dimensionY = (int)GridSystem.Dimension.Y;

		// Initialize path nodes 
		// WhatIf: Convert into Parallel Processing
		for (int x = 0; x < dimensionX; x++)
		{
			for (int z = 0; z < dimensionY; z++)
			{
				GridPosition pos = new GridPosition(x, z);
				T pathNode = GetNode(pos);
				pathNode.SetGCost(int.MaxValue);
				pathNode.SetHCost(0);
				pathNode.SetPreviousNode(null);
			}
		}

		startNode.SetGCost(0);
		startNode.SetHCost(CalculateDistance(start, end));

		while (openList.Count > 0)
		{
			T currentNode = GetLowestFCostNode(openList);

			// If the current node is the end node, return the path
			if (currentNode == endNode)
			{
				return CalculatePath(endNode);
			}
			openList.Remove(currentNode);
			closedList.Add(currentNode);

			foreach (T neighbor in GetNeighborNodes(currentNode))
			{
				if (closedList.Contains(neighbor)) continue;

				if (!neighbor.IsWalkable)
				{
					closedList.Add(neighbor);
					continue;
				}

				// Cost from the start node to the current node
				// WhatIf: Convert this into a delegate
				int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode.GridPosition, neighbor.GridPosition);

				GCostDelegate?.Invoke(ref tentativeGCost, neighbor);


				if (tentativeGCost < neighbor.GCost)  // If the new path is shorter
				{
					// Update the neighbor node
					neighbor.SetPreviousNode(currentNode);
					neighbor.SetGCost(tentativeGCost);
					neighbor.SetHCost(CalculateDistance(neighbor.GridPosition, end));

					if (!openList.Contains(neighbor))
						openList.Add(neighbor);
				}
			}

		}

		// No path found
		Debug.Log("No path found");
		return null;
	}

	private T FindNearestWalkableNode(T node, int searchRadius = 10)
	{
		for (int radius = 1; radius <= searchRadius; radius++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				for (int z = -radius; z <= radius; z++)
				{
					// Skip diagonal nodes: only check when either X or Z offset is 0
					if (Math.Abs(x) != 0 && Math.Abs(z) != 0) continue;

					GridPosition newPos = new GridPosition(node.GridPosition.X + x, node.GridPosition.Z + z);

					// Skip if the position is outside the grid bounds
					if (!GridSystem.IsPositionValid(newPos)) continue;

					// Get the neighbor node
					T neighborNode = GetNode(newPos);

					// If the neighbor is walkable, return it
					if (neighborNode != null && neighborNode.IsWalkable)
					{
						return neighborNode;
					}
				}
			}
		}

		return null; // No walkable node found within the search radius
	}


	private List<T> GetNeighborNodes(T node)
	{
		List<T> neighboringNodes = new List<T>();

		GridPosition position = node.GridPosition;

		if (GridSystem.IsPositionXValid(position.X - 1))
			neighboringNodes.Add(GetNode(position.X - 1, position.Z)); // Left


		if (GridSystem.IsPositionXValid(position.X + 1))
			neighboringNodes.Add(GetNode(position.X + 1, position.Z)); // Right

		if (GridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X, position.Z - 1)); // Down

		if (GridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X, position.Z + 1)); // Up

		return neighboringNodes;
	}


	private List<GridPosition> CalculatePath(T endNode)
	{
		List<T> path = [endNode];

		T currentNode = endNode; // Starting from the end node
		while (currentNode.PreviousNode != null)
		{
			path.Add(currentNode.PreviousNode);
			currentNode = currentNode.PreviousNode;
		}

		path.Reverse();

		List<GridPosition> gridPath = new List<GridPosition>();
		foreach (T node in path)
		{
			gridPath.Add(node.GridPosition);
		}

		return gridPath;
	}

	private T GetLowestFCostNode(List<T> openList)
	{
		T lowestFCostNode = openList[0];
		for (int i = 1; i < openList.Count; i++)
		{
			if (openList[i].FCost < lowestFCostNode.FCost)
				lowestFCostNode = openList[i];
		}
		return lowestFCostNode;
	}

	private int CalculateDistance(GridPosition a, GridPosition b)
	{
		// TODO: Implement a better heuristic
		GridPosition gridPosDistance = a - b;
		int xDistance = Math.Abs(gridPosDistance.X);
		int zDistance = Math.Abs(gridPosDistance.Z);
		int remaining = Math.Abs(xDistance - zDistance);

		return xDistance + zDistance;
	}

	private void ToggleNodeWalkable(GridPosition position, bool flag)
	{
		if (!GridSystem.IsPositionValid(position)) return;
		GetNode(position)?.SetWalkable(flag);
	}
}
