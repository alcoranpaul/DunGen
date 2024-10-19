﻿using System;
using System.Collections.Generic;
using System.Linq;

using FlaxEngine;


namespace DunGen;

/// <summary>
/// Resposible for spawning models in the scene based of on the data generated by <see cref="DataGenerator"/> 
/// </summary>
public class ModelGenerator
{
	public static ModelGenerator Instance { get; private set; }
	public DungeonGenSettings Settings { get; private set; }

	private const string ACTOR_NAME = "DungeonGenActor";
	private readonly DataGenerator dataGenerator;
	private Actor dungeonGenActor;

	public ModelGenerator(DataGenerator dataGenerator)
	{
		if (Instance == null)
			Instance = this;

		GetSettings();
		this.dataGenerator = dataGenerator;
		SetupActor();


	}

	public void GetSettings()
	{
		var settings = Engine.GetCustomSettings("DunGenSettings");
		if (!settings) Debug.LogError("DunGenSettings does not exists in Engine Custom Settings");

		Settings = settings.CreateInstance<DungeonGenSettings>();
	}

	public void SpawnModels()
	{
		DestroyDungeon();
		SpawnFloors();
		// SpawnRooms();
		SpawnDebugNodes();
		// Spawn floor-walls
		SpawnRoomWalls();
		// Spawn doors
		// Should defined by rules

		// https://www.youtube.com/watch?v=YJkfqEtJyzM&list=PL6MURe5By90mRPke0_vkxaqu0lRfeI8zl&index=5&t=295s
		// https://www.youtube.com/watch?v=FvXfukJwqOQ&list=PLcRSafycjWFfEPbSSjGMNY-goOZTuBPMW&index=5
		// https://www.youtube.com/watch?v=PhLcNhK9aro&t=477s
	}

	private void SetupActor()
	{
		dungeonGenActor = Level.FindActor(ACTOR_NAME);
		if (dungeonGenActor == null)
		{
			dungeonGenActor = new EmptyActor();
			dungeonGenActor.Name = ACTOR_NAME;
			Level.SpawnActor(dungeonGenActor);

		}
	}

	private void SpawnDebugNodes()
	{
		foreach (var node in dataGenerator.NodeObjects)
		{
			Vector3 pos = dataGenerator.ToVector3(node);
			Color color;
			switch (node.NodeType)
			{
				case RoomNode.RoomType.Room:
					color = Color.Red;
					break;
				case RoomNode.RoomType.Floor:
					color = Color.Blue;
					break;
				case RoomNode.RoomType.Hallway:
					color = Color.Green;
					break;
				case RoomNode.RoomType.RoomDoor:
					color = Color.Yellow;
					break;
				default:
					color = Color.White;

					break;
			}
			// DebugDraw.DrawText($"{node.NodeType}", pos, color, 8, 60f);
		}
	}

	private void SpawnFloors()
	{
		foreach (var node in dataGenerator.NodeObjects)
		{
			Vector3 pos = dataGenerator.ToVector3(node);

			Actor floor = null;
			switch (node.NodeType)
			{
				case RoomNode.RoomType.Room:
					floor = PrefabManager.SpawnPrefab(Settings.DebugSetting.RoomFloorPrefab, pos, Quaternion.Identity);

					break;
				case RoomNode.RoomType.Floor:
					floor = PrefabManager.SpawnPrefab(Settings.DebugSetting.HallwayFloorPrefab, pos, Quaternion.Identity);
					SpawnFloorWalls(node.GridPosition);
					break;
				case RoomNode.RoomType.RoomDoor:
					floor = PrefabManager.SpawnPrefab(Settings.DebugSetting.RoomDoorFloorPrefab, pos, Quaternion.Identity);
					break;
				default:

					break;
			}

			if (floor != null)
				floor.Parent = dungeonGenActor;
		}


	}

	private void SpawnFloorWalls(GridSystem.GridPosition gridPos)
	{
		// Cardinal directions: North, East, South, West
		int[] dx = dataGenerator.DirectionX;
		int[] dz = dataGenerator.DirectionZ;
		for (int i = 0; i < 4; i++)
		{
			GridSystem.GridPosition neighborPos = new GridSystem.GridPosition(gridPos.X + dx[i], gridPos.Z + dz[i]);
			var neighborNode = dataGenerator.GetNode(neighborPos);

			bool valid = neighborNode == null || (neighborNode.NodeType != RoomNode.RoomType.Floor && neighborNode.NodeType != RoomNode.RoomType.RoomDoor);
			if (valid)
			{
				Vector3 pos = dataGenerator.ToVector3(gridPos);
				Actor wall = null;
				switch (i)
				{
					case 0:
						wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.NPrefab, pos, Quaternion.Identity);
						break;
					case 1:
						wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.EPrefab, pos, Quaternion.Identity);
						break;
					case 2:
						wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.SPrefab, pos, Quaternion.Identity);
						break;
					case 3:
						wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.WPrefab, pos, Quaternion.Identity);
						break;
				}
				if (wall != null)
					wall.Parent = dungeonGenActor;
			}
		}

	}

	private void SpawnRoomWalls()
	{
		foreach (Room room in dataGenerator.Rooms)
		{
			SpawnOuterModels(room);
		}
	}

	private void SpawnOuterModels(Room room)
	{
		var outerNodePositions = room.OuterNodesPosition;
		foreach (var position in outerNodePositions)
		{
			NodeRoomType nType = CalculateRoomNodeType(position);

			RoomNode.RoomType[] inValid = [RoomNode.RoomType.Room, RoomNode.RoomType.RoomDoor];
			bool isNotRelatedToRoom(RoomNode room) { return room == null || !inValid.Contains(room.NodeType); }

			// Spawn the appropriate wall or door
			Actor wall = null;
			switch (nType)
			{
				case NodeRoomType.Cardinal:
					wall = SpawnRoomWall(position, isNotRelatedToRoom);
					break;
				case NodeRoomType.Door:
					wall = SpawnRoomDoor(position, (RoomNode room) => { return room != null && !inValid.Contains(room.NodeType); });
					if (wall != null)
						wall.Parent = dungeonGenActor;
					wall = SpawnRoomWall(position,
					 (RoomNode room) => { return room == null; });
					break;
				case NodeRoomType.Corner:
					wall = SpawnCornerRoomWall(position);
					break;
				default:
					break;
			}

			if (wall != null)
				wall.Parent = dungeonGenActor;

			Vector3 pos = dataGenerator.ToVector3(position);
			pos.Y += 20f;
			DebugDraw.DrawText($"{nType}", pos, Color.LightGreen, 8, 60f);
		}
	}

	private Actor SpawnRoomWall(GridSystem.GridPosition gridPos, Func<RoomNode, bool> isValidRoomType)
	{
		CardinalDirection dir = CalculateDirection(gridPos, isValidRoomType, out Vector3 pos);

		switch (dir)
		{
			case CardinalDirection.North:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.NPrefab, pos, Quaternion.Identity);
			case CardinalDirection.East:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.EPrefab, pos, Quaternion.Identity);
			case CardinalDirection.South:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.SPrefab, pos, Quaternion.Identity);
			case CardinalDirection.West:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.WPrefab, pos, Quaternion.Identity);
			default:
				return null;
		}
	}

	private CardinalDirection CalculateDirection(GridSystem.GridPosition gridPos, Func<RoomNode, bool> isValidRoomType, out Vector3 worldPos)
	{
		worldPos = dataGenerator.ToVector3(gridPos);
		return GetSingleCardinalDirection(gridPos, isValidRoomType);
	}

	private Actor SpawnRoomDoor(GridSystem.GridPosition gridPos, Func<RoomNode, bool> isValidRoomType)
	{
		// Node that is null or not a room/room-door from the direction of the gridPos
		CardinalDirection dir = CalculateDirection(gridPos, isValidRoomType, out Vector3 pos);

		switch (dir)
		{
			case CardinalDirection.North:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.DoorPrefab.NPrefab, pos, Quaternion.Identity);
			case CardinalDirection.East:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.DoorPrefab.EPrefab, pos, Quaternion.Identity);
			case CardinalDirection.South:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.DoorPrefab.SPrefab, pos, Quaternion.Identity);
			case CardinalDirection.West:
				return PrefabManager.SpawnPrefab(Settings.DebugSetting.DoorPrefab.WPrefab, pos, Quaternion.Identity);
			default:
				return null;
		}
	}

	private Actor SpawnCornerRoomWall(GridSystem.GridPosition gridPos)
	{
		CornerDirection cornerDir = GetCornerDirection(gridPos);
		Vector3 pos = dataGenerator.ToVector3(gridPos);
		Actor wall = null;

		switch (cornerDir)
		{
			case CornerDirection.NorthEast:
				DebugDraw.DrawText("NorthEast", pos, Color.LightGreen, 8, 60f);
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.SPrefab, pos, Quaternion.Identity);
				wall.Parent = dungeonGenActor;
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.WPrefab, pos, Quaternion.Identity);
				break;
			case CornerDirection.NorthWest:
				DebugDraw.DrawText("NorthWest", pos, Color.LightBlue, 8, 60f);
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.SPrefab, pos, Quaternion.Identity);
				wall.Parent = dungeonGenActor;
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.EPrefab, pos, Quaternion.Identity);
				break;
			case CornerDirection.SouthEast:
				DebugDraw.DrawText("SouthEast", pos, Color.Linen, 8, 60f);
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.NPrefab, pos, Quaternion.Identity);
				wall.Parent = dungeonGenActor;
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.WPrefab, pos, Quaternion.Identity);
				break;
			case CornerDirection.SouthWest:
				DebugDraw.DrawText("SouthWest", pos, Color.LightGoldenrodYellow, 8, 60f);
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.NPrefab, pos, Quaternion.Identity);
				wall.Parent = dungeonGenActor;
				wall = PrefabManager.SpawnPrefab(Settings.DebugSetting.WallPrefab.EPrefab, pos, Quaternion.Identity);
				break;
			default:
				break;
		}
		return wall;
	}

	private CornerDirection GetCornerDirection(GridSystem.GridPosition pos)
	{
		// Cardinal directions: North, East, South, West
		int[] dx = dataGenerator.DirectionX;
		int[] dz = dataGenerator.DirectionZ;

		// Check for North
		GridSystem.GridPosition northPos = new GridSystem.GridPosition(pos.X + dx[0], pos.Z + dz[0]);
		RoomNode northNode = dataGenerator.GetNode(northPos);

		// Check for East
		GridSystem.GridPosition eastPos = new GridSystem.GridPosition(pos.X + dx[1], pos.Z + dz[1]);
		RoomNode eastNode = dataGenerator.GetNode(eastPos);

		// Determine if the north and east nodes are valid
		bool northValid = northNode != null && (northNode.NodeType == RoomNode.RoomType.Room || northNode.NodeType == RoomNode.RoomType.RoomDoor);
		bool eastValid = eastNode != null && (eastNode.NodeType == RoomNode.RoomType.Room || eastNode.NodeType == RoomNode.RoomType.RoomDoor);

		if (!northValid)
		{
			// North is not valid
			return eastValid ? CornerDirection.SouthEast : CornerDirection.SouthWest;
		}
		else
		{
			// North is valid
			return eastValid ? CornerDirection.NorthEast : CornerDirection.NorthWest;
		}
	}

	private CardinalDirection GetSingleCardinalDirection(GridSystem.GridPosition pos, Func<RoomNode, bool> isValidRoomType)
	{
		// Cardinal directions: North, East, South, West
		int[] dx = dataGenerator.DirectionX;
		int[] dz = dataGenerator.DirectionZ;
		CardinalDirection[] directions = { CardinalDirection.North, CardinalDirection.East, CardinalDirection.South, CardinalDirection.West };

		for (int i = 0; i < directions.Length; i++)
		{
			GridSystem.GridPosition neighborPos = new GridSystem.GridPosition(pos.X + dx[i], pos.Z + dz[i]);
			var neighborNode = dataGenerator.GetNode(neighborPos);

			if (isValidRoomType(neighborNode))
			{
				return directions[i];
			}
		}

		return CardinalDirection.None;
	}


	private NodeRoomType CalculateRoomNodeType(GridSystem.GridPosition pos)
	{
		// If room-door then return door
		if (dataGenerator.GetNode(pos).NodeType == RoomNode.RoomType.RoomDoor) return NodeRoomType.Door;

		// else - Check the node's neightbor
		var neighborhood = dataGenerator.GetNeighborhood(pos);
		List<GridSystem.GridPosition> validNodes = new List<GridSystem.GridPosition>();
		foreach (var node in neighborhood)
		{
			// Get the Node
			RoomNode room = dataGenerator.GetNode(node);
			if (room.NodeType != RoomNode.RoomType.Room && room.NodeType != RoomNode.RoomType.RoomDoor) continue;

			// Check for Cardinal and Cornered Edges
			if (room.NodeType == RoomNode.RoomType.Room || room.NodeType == RoomNode.RoomType.RoomDoor)
				validNodes.Add(node);

		}

		NodeRoomType nodeType = NodeRoomType.Cardinal;
		if (validNodes.Count == 3) nodeType = NodeRoomType.Corner;
		return nodeType;
	}

	private void SpawnPremadeRooms() // For premade rooms
	{
		foreach (Room room in dataGenerator.Rooms)
		{
			Vector3 pos = room.RoomPosition.Position3D;
			Actor childModel = PrefabManager.SpawnPrefab(Settings.DebugSetting.RoomPrefab, pos, Quaternion.
			Identity);

			childModel.Parent = dungeonGenActor;
			childModel.Scale = new Vector3(room.Width, room.Height, room.Length);
			StaticModel model = childModel as StaticModel;
			model.SetMaterial(0, Settings.DebugSetting.Material);
		}

	}

	public void DestroyDungeon()
	{
		// If Actor has children, destroy them
		if (dungeonGenActor != null && dungeonGenActor.ChildrenCount > 0)
		{
			dungeonGenActor.DestroyChildren();
		}

	}

	private enum CardinalDirection
	{
		North,
		East,
		South,
		West,
		None
	}

	private enum CornerDirection
	{
		NorthEast,
		SouthEast,
		SouthWest,
		NorthWest
	}

	private enum NodeRoomType
	{
		/// <summary>
		/// North, East, South, West
		/// </summary>
		Cardinal,
		/// <summary>
		/// North-East, South-East, South-West, North-West
		/// </summary>
		Corner,

		Door
	}
}
