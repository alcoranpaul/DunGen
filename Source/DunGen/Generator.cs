﻿using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace DunGen;

/// <summary>
/// Generator Script.
/// </summary>
public class Generator
{
	public static Generator Instance { get; private set; }
	public PathFinding Pathfinding { get; private set; }
	public DungeonGenSettings Settings { get; private set; }
	public List<Room> Rooms;
	private Actor DungeonGenActor;
	private const string ACTOR_NAME = "DungeonGenActor";
	public DungeonGenState State { get; private set; }
	public GeneratorState GeneratorState { get; private set; }

	public Generator()
	{
		// Debug.Log("Generator Constructor");
		if (Instance == null)
			Instance = this;

		var settings = Engine.GetCustomSettings("DunGenSettings");
		if (!settings) Debug.LogError("DunGen does not exists");

		Settings = settings.CreateInstance<DungeonGenSettings>();
		State = DungeonGenState.None;
		GeneratorState = GeneratorState.None;

		Rooms = new List<Room>();
	}

	public void GenerateFinalDungeon()
	{
		// Spawn an empty actor to hold the dungeon
		DungeonGenActor = Level.FindActor(ACTOR_NAME);
		if (DungeonGenActor == null)
		{
			DungeonGenActor = new EmptyActor();
			DungeonGenActor.Name = ACTOR_NAME;
			Level.SpawnActor(DungeonGenActor);

		}

		// Debug.Log("Generating dungeon...");
		ChangeState(DungeonGenState.Generating);

		// Setup Grid plus Pathfinding
		GeneratePathfinding();

		DestroyDungeon();
		GenerateRoomData();
		GenerateHallwayPaths();

		ChangeGeneratorState(GeneratorState.None);
		ChangeState(DungeonGenState.None);
		// Debug.Log("Dungeon generation complete");

	}
	public void SpawnDebugDungeon()
	{
		Pathfinding.SpawnDebugObjects(Settings.DebugSetting.PathfindingDebugPrefab);
	}
	public void SetupActor()
	{
		DungeonGenActor = Level.FindActor(ACTOR_NAME);
		if (DungeonGenActor == null)
		{
			DungeonGenActor = new EmptyActor();
			DungeonGenActor.Name = ACTOR_NAME;
			Level.SpawnActor(DungeonGenActor);

		}
	}

	public void GeneratePathfinding()
	{
		// Setup Grid plus Pathfinding
		Pathfinding = new PathFinding(new Vector2(Settings.Size, Settings.Size));
		Settings.BoundingBox = Pathfinding.GetBoundingBox();
		// Debug.Log($"GeneratePathfinding Pathfinding null: {Pathfinding == null}");
	}

	public void DestroyDungeon()
	{
		ChangeState(DungeonGenState.Destroying);
		if (Rooms.Count > 0)
		{
			// Iterate through each room and set it to null
			for (int i = 0; i < Rooms.Count; i++)
			{

				GridSystem.GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(Rooms[i].WorldPosition.Position3D);
				Pathfinding.ToggleNeighborWalkable(gridPos, Rooms[i].Width, Rooms[i].Length, true);
				Rooms[i] = null;  // Set the room reference to null
			}

			// Now clear the list itself
			Rooms.Clear();  // Remove all items from the list
		}

		// If Actor has children, destroy them
		if (DungeonGenActor != null && DungeonGenActor.ChildrenCount > 0)
		{
			DungeonGenActor.DestroyChildren();
		}

	}

	public void GenerateRoomData()
	{
		// Debug.Log($"Pathfinding null: {Pathfinding == null}");
		ChangeGeneratorState(GeneratorState.SpawningRooms);
		for (int i = 0; i < Settings.MaxRooms; i++)
		{
			GenerateRoom(out Room newRoom);
			Rooms.Add(newRoom);
		}
	}

	private void GenerateRoom(out Room newRoom)
	{
		Random rand = new Random();


		int Width = rand.Next(2, 5);
		int Height = rand.Next(1, 2);
		int Length = rand.Next(2, 5);

		bool isPositionValid = FindValidRoomPosition(Width, Height, Length, out Vector3 position);

		if (!isPositionValid) Debug.LogError("No valid position found for room"); // Generate another room? until room count has reached max

		Actor childModel = PrefabManager.SpawnPrefab(Settings.DebugSetting.RoomPrefab, position, Quaternion.
		Identity);

		childModel.Parent = DungeonGenActor;
		childModel.Scale = new Vector3(Width, Height, Length);
		StaticModel model = childModel as StaticModel;
		model.SetMaterial(0, Settings.DebugSetting.Material);

		GridSystem.GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(position);
		Pathfinding.ToggleNeighborWalkable(gridPos, Width, Length, false);

		Vector3 worldPos = Pathfinding.GridSystem.GetWorldPosition(gridPos);
		RoomPosition roomPosition = new RoomPosition(worldPos);
		newRoom = new Room(roomPosition, Width, Height, Length, childModel);

		return;
	}

	private bool FindValidRoomPosition(int Width, int Height, int Length, out Vector3 _position)
	{

		return FindValidRoomPosition(Width, Height, Length, 5, out _position);
	}

	private bool FindValidRoomPosition(int Width, int Height, int Length, int maxAttemps, out Vector3 _position)
	{
		if (maxAttemps <= 0)
		{
			_position = Vector3.Zero;
			return false;
		}

		// Create a random generator
		Random rnd = new Random();

		// Generate a random position inside the bounding box
		Vector3 position = new Vector3(
			rnd.Next((int)Settings.BoundingBox.Minimum.X, (int)Settings.BoundingBox.Maximum.X),
			0, // Keep Y constant at 0 for floor placement
			rnd.Next((int)Settings.BoundingBox.Minimum.Z, (int)Settings.BoundingBox.Maximum.Z)
		);

		// Check neighbors including base position
		List<GridSystem.GridPosition> neightborhood = Pathfinding.GetNeighborhood(Pathfinding.GridSystem.GetGridPosition(position), Width, Length);
		bool canOccupySpace = true;
		foreach (var neighbor in neightborhood)
		{
			PathNode<RoomNode> node = Pathfinding.GetNode<RoomNode>(neighbor);

			// Debug.Log($"Checking neighbor at {neighbor} with node is not null: {node != null}");
			// Debug.Log($"Checking neighbor at {neighbor} with node is not null: {node != null}");

			if (node == null) continue;
			// Debug.Log($"Node is: {node}");
			if (!node.IsWalkable)
			{
				canOccupySpace = false;
				break;
			}
		}


		// If there is no hit, set the position
		if (canOccupySpace)
		{
			_position = Pathfinding.GridSystem.GetConvertedWorldPosition(position);
			return true;
		}

		return FindValidRoomPosition(Width, Height, Length, --maxAttemps, out _position);

	}

	public void GenerateHallwayPaths()
	{
		ChangeGeneratorState(GeneratorState.GeneratingPaths);
		float debugTime = 60f;
		List<DelaunayTriangulation.Point> points = new List<DelaunayTriangulation.Point>();
		HashSet<DelaunayTriangulation.Edge> edges = CreateDelaunayTriangulation(points);

		List<DelaunayTriangulation.Edge> hallwayPaths = CalculatePaths(debugTime, points, edges);

		// Set Node type to hallway nodes
		foreach (var edge in hallwayPaths)
		{
			GridSystem.GridPosition startingPos = Pathfinding.GridSystem.GetGridPosition(edge.A.VPoint);
			GridSystem.GridPosition end = Pathfinding.GridSystem.GetGridPosition(edge.B.VPoint);

			Pathfinding.GetNode<RoomNode>(startingPos).NodeType = NodeType.Hallway;
			Pathfinding.GetNode<RoomNode>(end).NodeType = NodeType.Hallway;
		}

		foreach (var edge in hallwayPaths)
		{
			GridSystem.GridPosition startingPos = Pathfinding.GridSystem.GetGridPosition(edge.A.VPoint);
			GridSystem.GridPosition end = Pathfinding.GridSystem.GetGridPosition(edge.B.VPoint);
			// Debug.Log($"Path from {startingPos} to {end}");
			List<GridSystem.GridPosition> paths = Pathfinding.FindPath(startingPos, end);
			if (paths == null) continue;
			string pathString = $"Path from {paths[0]} to {paths[^1]}: ";
			for (int i = 0; i < paths.Count; i++)  // Notice we're using paths.Count, not paths.Count - 1
			{
				GridSystem.GridPosition currentPos = paths[i];

				// Append the current position to pathString
				pathString += currentPos.ToString();

				// Add a separator for readability (but don't add it after the last position)
				if (i < paths.Count - 1)
				{
					pathString += " -> ";
				}

				// Spawn a single floor at each path point
				Actor floor = PrefabManager.SpawnPrefab(Settings.DebugSetting.FloorPrefab, Pathfinding.GridSystem.GetWorldPosition(currentPos), Quaternion.Identity);
				floor.Parent = DungeonGenActor;

				// Draw the line if there is a next point in the path
				if (i < paths.Count - 1)
				{
					DebugDraw.DrawLine(
						Pathfinding.GridSystem.GetWorldPosition(paths[i]),
						Pathfinding.GridSystem.GetWorldPosition(paths[i + 1]),
						Color.Red,
						60f
					);
				}
			}
			Debug.Log(pathString);
		}
	}

	// Helper method to check if a path already exists in the uniquePaths list
	private bool ContainsPath(List<List<GridSystem.GridPosition>> uniquePaths, List<GridSystem.GridPosition> newPath)
	{
		foreach (var existingPath in uniquePaths)
		{
			if (ArePathsEqual(existingPath, newPath))
			{
				return true;  // Path is already in the list
			}
		}
		return false;  // Path is unique
	}

	// Helper method to compare two lists of GridPosition
	private bool ArePathsEqual(List<GridSystem.GridPosition> path1, List<GridSystem.GridPosition> path2)
	{
		if (path1.Count != path2.Count) return false;

		for (int i = 0; i < path1.Count; i++)
		{
			if (!path1[i].Equals(path2[i])) return false;
		}

		return true;  // Paths are the same
	}

	private List<DelaunayTriangulation.Edge> CalculatePaths(float debugTime, List<DelaunayTriangulation.Point> points, HashSet<DelaunayTriangulation.Edge> edges)
	{
		// Debug.Log("Calculating MST ...");
		List<Prim.Edge> weightedEdges = new List<Prim.Edge>();
		foreach (var edge in edges)
		{
			Prim.Edge e = new Prim.Edge(edge.A, edge.B);
			weightedEdges.Add(e);
		}

		List<DelaunayTriangulation.Edge> finalPaths = Prim.MinimumSpanningTree(weightedEdges, points[0]);

		// Debug.Log($"Adding more paths into MST ..." + finalPaths.Count);
		// Add more edges to the MST
		foreach (var edge in edges)
		{
			if (finalPaths.Contains(edge)) continue;

			float rand = Random.Shared.NextFloat();
			if (rand < 0.451f)
				finalPaths.Add(edge);
		}

		DelaunayTriangulation.Edge.DebugEdges(finalPaths, Color.DarkBlue, 40f);

		return finalPaths;
	}

	private HashSet<DelaunayTriangulation.Edge> CreateDelaunayTriangulation(List<DelaunayTriangulation.Point> points)
	{
		foreach (var room in Rooms)
		{
			DelaunayTriangulation.Point point = new DelaunayTriangulation.Point(room.WorldPosition.X, room.WorldPosition.Z);
			points.Add(point);
		}
		DelaunayTriangulation delaunay = DelaunayTriangulation.Triangulate(points);

		return delaunay.Edges;
	}

	private void ChangeState(DungeonGenState state)
	{
		State = state;
	}

	private void ChangeGeneratorState(GeneratorState state)
	{
		GeneratorState = state;
	}


}
public enum DungeonGenState
{
	/// <summary>
	/// Nothing or Success
	/// </summary>
	None,
	Idle,
	Generating,
	Destroying,
	Failed
}

public enum GeneratorState
{
	/// <summary>
	/// Nothing or Success
	/// </summary>
	None,
	SpawningRooms,
	GeneratingPaths,
	Failed
}