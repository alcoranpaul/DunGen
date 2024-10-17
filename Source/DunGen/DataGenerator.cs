using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace DunGen;

/// <summary>
/// Generates the dungeon data by performing the following steps:
/// The DataGenerator class is responsible for generating and calculating the data for the dungeon
/// </summary>
public class DataGenerator // WhatIf: use dependency injection rather than Singleton
{
	public static DataGenerator Instance { get; private set; }
	public PathFinding<RoomNode> Pathfinding { get; private set; }
	public DungeonGenSettings Settings { get; private set; }
	public List<Room> Rooms { get; private set; }
	public HashSet<GridSystem.GridPosition> Paths { get; private set; }

	public DungeonGenState State { get; private set; }
	public GeneratorState GeneratorState { get; private set; }



	public DataGenerator()
	{
		if (Instance == null)
			Instance = this;

		var settings = Engine.GetCustomSettings("DunGenSettings");
		if (!settings) Debug.LogError("DunGenSettings does not exists in Engine Custom Settings");

		Settings = settings.CreateInstance<DungeonGenSettings>();
		State = DungeonGenState.None;
		GeneratorState = GeneratorState.None;

		Rooms = new List<Room>();
		Paths = new HashSet<GridSystem.GridPosition>();
	}

	/// <summary>
	/// Generates the dungeon data by setting up the grid and pathfinding, destroying existing data,
	/// generating room data, and creating hallway paths. Updates the dungeon generation state
	/// throughout the process.
	/// </summary>
	public void GenerateDungeonData()
	{
		ChangeState(DungeonGenState.Generating);

		GeneratePathfinding();

		DestroyData();
		GenerateRoomData();
		GenerateHallwayPaths();

		ChangeGeneratorState(GeneratorState.None);
		ChangeState(DungeonGenState.None);

	}

	// TODO: Remove Debug methods, probably in another class for the editor like DataDebugger
	/// <summary>
	///	Spawn Debug objects for the grid system
	/// </summary>
	public void SpawnGridDebugDungeon()
	{
		if (Pathfinding == null) return;
		Pathfinding.SpawnDebugObjects(Settings.DebugSetting.DebugGridPrefab);
	}

	/// <summary>
	/// Spawn Debug objects for the pathfinding grid
	/// </summary>
	public void SpawnPathGridDebugDungeon()
	{
		if (Pathfinding == null) return;
		Pathfinding.SpawnDebugObjects(Settings.DebugSetting.PathfindingDebugPrefab);
	}

	/// <summary>
	/// Generate the pathfinding grid. Data is taken from Custom settings(<see cref="DungeonGenSettings"/>) found in Content/GameSettings
	/// </summary>
	private void GeneratePathfinding()
	{
		Pathfinding = new PathFinding<RoomNode>(new Vector2(Settings.Size, Settings.Size), (GridSystem.GridSystem<RoomNode> GridSystem, GridSystem.GridPosition gridPosition) => { return new RoomNode(GridSystem, gridPosition); });

		Settings.BoundingBox = Pathfinding.GetBoundingBox();// WhatIf: remove bounding from from settings and just call the method
	}

	/// <summary>
	/// Destroys the existing data by setting the rooms to null and clearing the paths
	/// </summary>
	public void DestroyData()
	{
		if (Rooms?.Count > 0)
		{
			// Iterate through each room and set it to null
			for (int i = 0; i < Rooms.Count; i++)
			{
				GridSystem.GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(Rooms[i].RoomPosition.Position3D);

				// Toggle neighborhood from Room.NeighborNodes	 
				Pathfinding.ToggleNeighborWalkable(gridPos, Rooms[i].Width, Rooms[i].Length, true);
			}
			Rooms?.Clear();
		}

		Paths?.Clear();

	}

	/// <summary>
	/// Generates the room data by creating rooms and setting the neighbor nodes for each room
	/// </summary>
	private void GenerateRoomData()
	{
		ChangeGeneratorState(GeneratorState.SpawningRooms);
		for (int i = 0; i < Settings.MaxRooms; i++)
		{
			GenerateRoom(out Room newRoom);
			Rooms.Add(newRoom);
		}
	}

	/// <summary>
	/// Generates a room by creating a random room size and position, and setting the neighbor nodes for the room
	/// </summary>
	/// <param name="newRoom"></param>
	private void GenerateRoom(out Room newRoom)
	{
		Random rand = new Random();

		// TODO: Have a setting for randomized rooms or predefined rooms
		int Width = rand.Next(2, 5);
		int Height = rand.Next(1, 2);
		int Length = rand.Next(2, 5);

		bool isPositionValid = FindValidRoomPosition(Width, Height, Length, out Vector3 position);

		if (!isPositionValid) Debug.LogError("No valid position found for room"); // Generate another room? until room count has reached max

		GridSystem.GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(position);
		Pathfinding.ToggleNeighborWalkable(gridPos, Width, Length, false); // Set nodes to unwalkable

		Vector3 worldPos = Pathfinding.GridSystem.GetWorldPosition(gridPos);
		RoomPosition roomPosition = new RoomPosition(worldPos);
		newRoom = new Room(roomPosition, Width, Height, Length);
		newRoom.SetNeighborNodes(Pathfinding.GetNeighborhood(gridPos, Width, Length));
		return;
	}

	/// <summary>
	/// Finds a valid room position by generating a random position and checking if the room can occupy the space
	/// </summary>
	/// <param name="Width"></param>
	/// <param name="Height"></param>
	/// <param name="Length"></param>
	/// <param name="_position"></param>
	/// <returns></returns>
	private bool FindValidRoomPosition(int Width, int Height, int Length, out Vector3 _position)
	{
		return FindValidRoomPosition(Width, Height, Length, 5, out _position); // TODO: Set max attempts to a setting
	}

	/// <summary>
	/// Recursive function to find a valid room position by generating a random position and checking if the room can occupy the space
	/// </summary>
	/// <param name="Width"></param>
	/// <param name="Height"></param>
	/// <param name="Length"></param>
	/// <param name="maxAttemps"></param>
	/// <param name="_position"></param>
	/// <returns></returns>
	private bool FindValidRoomPosition(int Width, int Height, int Length, int maxAttemps, out Vector3 _position)
	{
		if (maxAttemps <= 0)
		{
			_position = Vector3.Zero;
			return false;
		}

		// WhatIf: have an option for random position or predefined position - Say Room A needs to be near Room C
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
		foreach (var neighbor in neightborhood) // Cheacks the neighborhood for walkable nodes based off of Width and Length
		{
			RoomNode node = Pathfinding.GetNode(neighbor);

			if (node == null) continue;
			if (!node.IsWalkable)
			{
				canOccupySpace = false;
				break;
			}
		}


		// If can occupy the space or all neighbors are walkable then return with the position
		if (canOccupySpace)
		{
			_position = Pathfinding.GridSystem.GetConvertedWorldPosition(position);
			return true;
		}

		return FindValidRoomPosition(Width, Height, Length, --maxAttemps, out _position);

	}

	/// <summary>
	/// Generates the hallway paths by creating a Delaunay triangulation, calculating the minimum spanning tree, and adding more paths to the tree
	/// </summary>
	private void GenerateHallwayPaths()
	{
		ChangeGeneratorState(GeneratorState.GeneratingPaths);
		float debugTime = 60f;
		List<DelaunayTriangulation.Point> points = new List<DelaunayTriangulation.Point>();
		HashSet<DelaunayTriangulation.Edge> edges = CreateDelaunayTriangulation(points);
		HashSet<DelaunayTriangulation.Edge> HallwayPaths = CalculatePaths(debugTime, points, edges);


		foreach (var edge in HallwayPaths)
		{
			GridSystem.GridPosition startingPos = Pathfinding.GridSystem.GetGridPosition(edge.A.VPoint);
			GridSystem.GridPosition end = Pathfinding.GridSystem.GetGridPosition(edge.B.VPoint);

			// Set Node type to hallway nodes
			Pathfinding.GetNode(startingPos).SetToHallway();
			Pathfinding.GetNode(end).SetToHallway();

			// Debug.Log($"Path from {startingPos} to {end}");
			List<GridSystem.GridPosition> paths = Pathfinding.FindPath(
				startingPos,
				end,
				new PathFinding<RoomNode>.TentativeGCostDelegate(RoomNode.CalculateTentativeGCost));

			if (paths == null) continue;

			for (int i = 0; i < paths.Count; i++)  // Notice we're using paths.Count, not paths.Count - 1
			{
				GridSystem.GridPosition currentPos = paths[i];
				Paths.Add(currentPos);
			}
		}
	}

	/// <summary>
	///	Calculates the paths by creating a minimum spanning tree from the Delaunay triangulation and adding more paths to the tree via a random chance
	/// </summary>
	/// <param name="debugTime"></param>
	/// <param name="points"></param>
	/// <param name="edges"></param>
	/// <returns></returns>
	private HashSet<DelaunayTriangulation.Edge> CalculatePaths(float debugTime, List<DelaunayTriangulation.Point> points, HashSet<DelaunayTriangulation.Edge> edges)
	{
		// Debug.Log("Calculating MST ...");
		List<Prim.Edge> weightedEdges = new List<Prim.Edge>();
		foreach (var edge in edges)
		{
			Prim.Edge e = new Prim.Edge(edge.A, edge.B);
			weightedEdges.Add(e);
		}

		HashSet<DelaunayTriangulation.Edge> finalPaths = Prim.MinimumSpanningTree(weightedEdges, points[0]);

		// Add more edges to the MST
		foreach (var edge in edges)
		{
			if (finalPaths.Contains(edge)) continue;

			float rand = Random.Shared.NextFloat();
			if (rand < 0.451f) // TODO: Set random chance to a setting
				finalPaths.Add(edge);
		}
		List<DelaunayTriangulation.Edge> listPaths = finalPaths.ToList();

		// TODO: use a bool to toggle debug draw
		DelaunayTriangulation.Edge.DebugEdges(listPaths, Color.DarkBlue, 40f);

		return finalPaths;
	}

	/// <summary>
	/// Creates a Delaunay triangulation by adding points to the triangulation
	/// </summary>
	/// <param name="points"></param>
	/// <returns></returns>
	private HashSet<DelaunayTriangulation.Edge> CreateDelaunayTriangulation(List<DelaunayTriangulation.Point> points)
	{
		foreach (var room in Rooms)
		{
			DelaunayTriangulation.Point point = new DelaunayTriangulation.Point(room.RoomPosition.X, room.RoomPosition.Z);
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