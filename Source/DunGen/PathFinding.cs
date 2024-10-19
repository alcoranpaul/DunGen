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
	private readonly GridSystem<T> gridSystem;
	public delegate void TentativeGCostDelegate(ref int tentativeGCost, T node);
	public T[,] GridObjects => gridSystem.GridObjects;

	public PathFinding(Vector2 dimension, Func<GridSystem<T>, GridPosition, T> createGridObject, float unitScale = 1)
	{
		gridSystem = new GridSystem<T>(dimension, unitScale, createGridObject);

	}

	public PathFinding(int dimension, Func<GridSystem<T>, GridPosition, T> createGridObject, float unitScale = 1)
	{
		gridSystem = new GridSystem<T>(new Vector2(dimension), unitScale, createGridObject);

	}

	/// <summary>
	/// Toggles the neighboring nodes based of off <paramref name="Width"/> and <paramref name="Length"/>
	/// </summary>
	/// <param name="basePosition"></param>
	/// <param name="Width"></param>
	/// <param name="Length"></param>
	/// <param name="flag"></param>
	public void ToggleNeighborWalkable(GridPosition basePosition, int Width, int Length, bool flag)
	{
		List<GridPosition> positions = GetNeighborhood(basePosition, Width, Length);

		foreach (GridPosition pos in positions)
			ToggleNodeWalkable(pos, flag);
	}

	/// <summary>
	/// Returns a list of <see cref="GridPosition"/> (in all 8 directions) based of off <paramref name="Width"/> and <paramref name="Length"/>
	/// </summary>
	/// <param name="basePosition"></param>
	/// <param name="Width"></param>
	/// <param name="Length"></param>
	/// <returns></returns>
	public List<GridPosition> GetNeighborhood(GridPosition basePosition, int Width, int Length)
	{
		List<GridPosition> positions = new List<GridPosition>();
		int gridWidth = gridSystem.ToGridSize(Width);
		int gridLength = gridSystem.ToGridSize(Length);

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


	/// <summary>
	/// Returns a list of <see cref="GridPosition"/> representing the immediate neighbors (1-unit grid radius) around the base position.
	/// </summary>
	/// <param name="basePosition"></param>
	/// <returns></returns>
	public List<GridPosition> GetNeighborhood(GridPosition basePosition)
	{
		List<GridPosition> positions = new List<GridPosition>();

		// Define the relative positions for the 8 possible neighbors
		int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
		int[] dz = { -1, -1, -1, 0, 0, 1, 1, 1 };

		for (int i = 0; i < 8; i++)
		{
			GridPosition pos = new GridPosition(basePosition.X + dx[i], basePosition.Z + dz[i]);
			if (gridSystem.IsPositionValid(pos))
			{
				positions.Add(pos);
			}
		}

		return positions;
	}

	/// <summary>
	/// Returns a list of <see cref="GridPosition"/> representing the outer nodes (most edges) based on <paramref name="Width"/> and <paramref name="Length"/>
	/// </summary>
	/// <param name="basePosition">The base position around which the outer nodes are calculated.</param>
	/// <param name="Width">The width of the grid area.</param>
	/// <param name="Length">The length of the grid area.</param>
	/// <returns>A list of <see cref="GridPosition"/> representing the outer nodes.</returns>
	public List<GridPosition> GetOuterNodes(GridPosition basePosition, int Width, int Length)
	{
		// Initialize the list to hold the positions of the outer nodes
		List<GridPosition> positions = new List<GridPosition>();

		// Convert the width and length to grid sizes
		int gridWidth = gridSystem.ToGridSize(Width);
		int gridLength = gridSystem.ToGridSize(Length);

		// Calculate the offsets to center the grid around the base position
		int widthOffset = gridWidth / 2;
		int lengthOffset = gridLength / 2;

		if (gridSystem.Dimension.X - gridWidth < 0) gridWidth = (int)gridSystem.Dimension.X;
		if (gridSystem.Dimension.Y - gridLength < 0) gridLength = (int)gridSystem.Dimension.Y;
		// Iterate through the grid dimensions
		for (int i = 0; i < gridWidth; i++)
		{
			for (int j = 0; j < gridLength; j++)
			{
				// Include only the outer nodes (first and last rows and columns)
				if (i == 0 || i == gridWidth - 1 || j == 0 || j == gridLength - 1)
				{
					int x = basePosition.X - widthOffset + i;
					int z = basePosition.Z - lengthOffset + j;
					if (x < 0) x = 0;
					else if (x >= gridSystem.Dimension.X) x = (int)gridSystem.Dimension.X - 1;
					if (z < 0) z = 0;
					else if (z >= gridSystem.Dimension.Y) z = (int)gridSystem.Dimension.Y - 1;

					// Calculate the grid position based on the base position and offsets
					GridPosition pos = new GridPosition(x, z);

					// Check if the position is valid within the grid system
					if (!gridSystem.IsPositionValid(pos)) continue;

					// Add the valid outer node position to the list
					positions.Add(pos);
				}
			}
		}

		// Return the list of outer node positions
		return positions;
	}

	/// <summary>
	/// Returns a list of <see cref="GridPosition"/> representing the inner nodes excluding the outer nodes based on <paramref name="Width"/> and <paramref name="Length"/>
	/// </summary>
	/// <param name="basePosition"></param>
	/// <param name="Width"></param>
	/// <param name="Length"></param>
	/// <returns></returns>
	public List<GridPosition> GetInnerNodes(GridPosition basePosition, int Width, int Length)
	{
		List<GridPosition> positions = new List<GridPosition>();
		int gridWidth = gridSystem.ToGridSize(Width);
		int gridLength = gridSystem.ToGridSize(Length);

		int widthOffset = gridWidth / 2;
		int lengthOffset = gridLength / 2;

		if (gridSystem.Dimension.X - gridWidth < 0) gridWidth = (int)gridSystem.Dimension.X;
		if (gridSystem.Dimension.Y - gridLength < 0) gridLength = (int)gridSystem.Dimension.Y;
		for (int i = 1; i < gridWidth - 1; i++)
		{
			for (int j = 1; j < gridLength - 1; j++)
			{
				int x = basePosition.X - widthOffset + i;
				int z = basePosition.Z - lengthOffset + j;
				if (x < 0) x = 0;
				else if (x >= gridSystem.Dimension.X) x = (int)gridSystem.Dimension.X - 1;
				if (z < 0) z = 0;
				else if (z >= gridSystem.Dimension.Y) z = (int)gridSystem.Dimension.Y - 1;

				GridPosition pos = new GridPosition(basePosition.X - widthOffset + i, basePosition.Z - lengthOffset + j);
				positions.Add(pos);
			}
		}

		return positions;
	}


	public BoundingBox GetBoundingBox()
	{
		return gridSystem.GetBoundingBox();
	}

	public void SpawnDebugObjects(Prefab debugGridPrefab)
	{
		gridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public T GetNode(int x, int z)
	{
		GridPosition position = new(x, z);
		return GetNode(position);
	}

	public T GetNode(GridPosition position)
	{
		if (!gridSystem.IsPositionValid(position)) return null;
		return gridSystem.GetGridObject(position);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end, out T startNode, out T endNode, TentativeGCostDelegate GCostDelegate = null)
	{
		List<T> openList = new List<T>(); // Nodes to be evaluated
		List<T> closedList = new List<T>(); // Already visited nodes

		// Add Start node to the open list
		startNode = GetNode(start);
		endNode = GetNode(end);

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

		Vector3 asd = gridSystem.GetWorldPosition(endNode.GridPosition);
		asd.Y += 100f;

		// TODO: Enabled by boolean
		DebugDraw.DrawSphere(new BoundingSphere(asd, 15f), Color.Azure, 60f);

		int dimensionX = (int)gridSystem.Dimension.X;
		int dimensionY = (int)gridSystem.Dimension.Y;

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

			foreach (T neighbor in GetCardinalNodes(currentNode))
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
					if (!gridSystem.IsPositionValid(newPos)) continue;

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


	/// <summary>
	/// Returns a list of neighboring nodes in the cardinal directions (up, down, left, right)
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public List<T> GetCardinalNodes(T node)
	{
		List<T> neighboringNodes = new List<T>();

		GridPosition position = node.GridPosition;

		if (gridSystem.IsPositionXValid(position.X - 1))
			neighboringNodes.Add(GetNode(position.X - 1, position.Z)); // Left

		if (gridSystem.IsPositionXValid(position.X + 1))
			neighboringNodes.Add(GetNode(position.X + 1, position.Z)); // Right

		if (gridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X, position.Z - 1)); // Down

		if (gridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X, position.Z + 1)); // Up

		return neighboringNodes;
	}
	/// <summary>
	/// Returns a list of neighboring nodes in the edge directions (North-East, North-West, South-West, South-East)
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public List<T> GetCornerNodes(T node)
	{
		List<T> neighboringNodes = new List<T>();

		GridPosition position = node.GridPosition;

		// Check North-East
		if (gridSystem.IsPositionXValid(position.X + 1) && gridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X + 1, position.Z + 1));

		// Check North-West
		if (gridSystem.IsPositionXValid(position.X - 1) && gridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X - 1, position.Z + 1));

		// Check South-West
		if (gridSystem.IsPositionXValid(position.X - 1) && gridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X - 1, position.Z - 1));

		// Check South-East
		if (gridSystem.IsPositionXValid(position.X + 1) && gridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X + 1, position.Z - 1));

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
		if (!gridSystem.IsPositionValid(position)) return;
		GetNode(position)?.SetWalkable(flag);
	}

	public GridPosition GetGridPosition(Vector3 position)
	{
		return gridSystem.GetGridPosition(position);
	}

	public Vector3 GetWorldPosition(GridPosition position)
	{
		return gridSystem.GetWorldPosition(position);
	}

	public Vector3 GetWorldPosition(Vector3 position)
	{
		return gridSystem.GetWorldPosition(position);
	}

	public float GetHalfUnitScale()
	{
		return gridSystem.UnitScale / 2;
	}

	public int[] GetDirectionX() => gridSystem.DirectionX;
	public int[] GetDirectionZ() => gridSystem.DirectionY;


}
