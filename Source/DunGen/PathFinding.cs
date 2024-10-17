using System;
using System.Collections.Generic;
using GridSystem;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// PathFinding Script.
/// </summary>
public class PathFinding
{


	public GridSystem<RoomNode> GridSystem { get; private set; }
	private const int MOVE_STRAIGHT_COST = 10;
	private const int MOVE_DIAGONAL_COST = 14;



	public PathFinding(Vector2 dimension, float unitScale = 1)
	{
		GridSystem = new GridSystem<RoomNode>(dimension, unitScale, (GridSystem<RoomNode> gridSystem, GridPosition gridPosition) => { return new RoomNode(gridSystem, gridPosition); });

	}

	public PathFinding(int dimension, float unitScale = 1)
	{
		GridSystem = new GridSystem<RoomNode>(new Vector2(dimension), unitScale, (GridSystem<RoomNode> gridSystem, GridPosition gridPosition) => { return new RoomNode(gridSystem, gridPosition); });

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

	private void ToggleNodeWalkable(GridPosition position, bool flag)
	{
		if (!GridSystem.IsPositionValid(position)) return;
		GetNode<RoomNode>(position).SetWalkable(flag);
	}

	public BoundingBox GetBoundingBox()
	{
		return GridSystem.GetBoundingBox();
	}

	public void SpawnDebugObjects(Prefab debugGridPrefab)
	{
		GridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end)
	{
		return FindPath<RoomNode>(start, end);
	}

	public List<GridPosition> FindPath<T>(GridPosition start, GridPosition end) where T : PathNode<T>
	{
		List<PathNode<T>> openList = new List<PathNode<T>>(); // Nodes to be evaluated
		List<PathNode<T>> closedList = new List<PathNode<T>>(); // Already visited nodes

		// Add Start node to the open list
		PathNode<T> startNode = GetNode<T>(start);
		PathNode<T> endNode = GetNode<T>(end);
		// Debug.Log($"OLD Start: {startNode.GridPosition} End: {endNode.GridPosition}");
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
		// Debug.Log($" NEW Start: {startNode.GridPosition} End: {endNode.GridPosition}");
		DebugDraw.DrawSphere(new BoundingSphere(GridSystem.GetWorldPosition(startNode.GridPosition), 15f), Color.DarkRed, 60f);
		Vector3 asd = GridSystem.GetWorldPosition(endNode.GridPosition);
		asd.Y += 100f;
		DebugDraw.DrawSphere(new BoundingSphere(asd, 15f), Color.Azure, 60f);


		// Initialize path nodes
		for (int x = 0; x < GridSystem.Dimension.X; x++)
		{
			for (int z = 0; z < GridSystem.Dimension.Y; z++)
			{
				GridPosition pos = new GridPosition(x, z);
				PathNode<T> pathNode = GetNode<T>(pos);
				pathNode.SetGCost(int.MaxValue);
				pathNode.SetHCost(0);
				pathNode.SetPreviousNode(null);
			}
		}

		startNode.SetGCost(0);
		startNode.SetHCost(CalculateDistance(start, end));

		while (openList.Count > 0)
		{
			PathNode<T> currentNode = GetLowestFCostNode(openList);

			// If the current node is the end node, return the path
			if (currentNode == endNode)
			{
				return CalculatePath(endNode);
			}
			openList.Remove(currentNode);
			closedList.Add(currentNode);

			foreach (PathNode<T> neighbor in GetNeighborNodes(currentNode))
			{
				if (closedList.Contains(neighbor)) continue;

				if (!neighbor.IsWalkable)
				{
					neighbor.NodeType = NodeType.Hallway;
					closedList.Add(neighbor);
					continue;
				}

				// Cost from the start node to the current node
				// WhatIf: Convert this into a delegate
				int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode.GridPosition, neighbor.GridPosition);
				if (currentNode.NodeType == NodeType.Hallway)
				{
					tentativeGCost = -5;
				}


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

	private PathNode<T> FindNearestWalkableNode<T>(PathNode<T> node, int searchRadius = 10) where T : PathNode<T>
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
					PathNode<T> neighborNode = GetNode<T>(newPos);

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





	private List<PathNode<T>> GetNeighborNodes<T>(PathNode<T> node) where T : PathNode<T>
	{
		List<PathNode<T>> neighboringNodes = new List<PathNode<T>>();

		GridPosition position = node.GridPosition;

		if (GridSystem.IsPositionXValid(position.X - 1))
		{

			neighboringNodes.Add(GetNode<T>(position.X - 1, position.Z)); // Left
																		  // if (GridSystem.IsPositionZValid(position.Z - 1))
																		  // 	neighboringNodes.Add(GetNode(position.X - 1, position.Z - 1)); // Down Left
																		  // if (GridSystem.IsPositionZValid(position.Z + 1))
																		  // 	neighboringNodes.Add(GetNode(position.X - 1, position.Z + 1)); // Up Left
		}


		if (GridSystem.IsPositionXValid(position.X + 1))
		{
			neighboringNodes.Add(GetNode<T>(position.X + 1, position.Z)); // Right
																		  // if (GridSystem.IsPositionZValid(position.Z - 1))
																		  // 	neighboringNodes.Add(GetNode(position.X + 1, position.Z - 1)); // Down Right
																		  // if (GridSystem.IsPositionZValid(position.Z + 1))
																		  // 	neighboringNodes.Add(GetNode(position.X + 1, position.Z + 1)); // Up Right	
		}

		if (GridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode<T>(position.X, position.Z - 1)); // Down

		if (GridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode<T>(position.X, position.Z + 1)); // Up

		string neighbors = "";
		foreach (PathNode<T> n in neighboringNodes)
		{
			neighbors += n.GridPosition + " ";
		}
		return neighboringNodes;
	}

	public PathNode<T> GetNode<T>(int x, int z) where T : PathNode<T>
	{
		GridPosition position = new(x, z);
		return GetNode<T>(position);
	}

	public PathNode<T> GetNode<T>(GridPosition position) where T : PathNode<T>
	{
		if (!GridSystem.IsPositionValid(position)) return null;
		return GridSystem.GetGridObject(position) as T;
	}

	private List<GridPosition> CalculatePath<T>(PathNode<T> endNode) where T : PathNode<T>
	{
		List<PathNode<T>> path = [endNode];

		PathNode<T> currentNode = endNode; // Starting from the end node
		while (currentNode.PreviousNode != null)
		{
			path.Add(currentNode.PreviousNode);
			currentNode = currentNode.PreviousNode;
		}

		path.Reverse();

		List<GridPosition> gridPath = new List<GridPosition>();
		foreach (PathNode<T> node in path)
		{
			gridPath.Add(node.GridPosition);
		}

		return gridPath;
	}

	private PathNode<T> GetLowestFCostNode<T>(List<PathNode<T>> openList) where T : PathNode<T>
	{
		PathNode<T> lowestFCostNode = openList[0];
		for (int i = 1; i < openList.Count; i++)
		{
			if (openList[i].FCost < lowestFCostNode.FCost)
				lowestFCostNode = openList[i];
		}
		return lowestFCostNode;
	}

	public int CalculateDistance(GridPosition a, GridPosition b)
	{
		GridPosition gridPosDistance = a - b;
		int xDistance = Math.Abs(gridPosDistance.X);
		int zDistance = Math.Abs(gridPosDistance.Z);
		int remaining = Math.Abs(xDistance - zDistance);
		// MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + (remaining * MOVE_STRAIGHT_COST);
		return xDistance + zDistance;
	}
}
