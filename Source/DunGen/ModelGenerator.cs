﻿using System;
using System.Collections.Generic;

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

		var settings = Engine.GetCustomSettings("DunGenSettings");
		if (!settings) Debug.LogError("DunGenSettings does not exists in Engine Custom Settings");

		Settings = settings.CreateInstance<DungeonGenSettings>();
		this.dataGenerator = dataGenerator;
		SetupActor();


	}

	public void SpawnModels()
	{
		DestroyDungeon();
		SpawnFloors();
		SpawnRooms();
		SpawnDebugNodes();
		// Spawn floor-walls
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
				default:
					color = Color.White;

					break;
			}
			DebugDraw.DrawText($"{node.NodeType}", pos, color, 8, 60f);
		}
	}

	private void SpawnFloors()
	{

		foreach (GridSystem.GridPosition nodePos in dataGenerator.Paths)
		{
			Vector3 pos = dataGenerator.ToVector3(nodePos);
			Actor floor = PrefabManager.SpawnPrefab(Settings.DebugSetting.FloorPrefab, pos, Quaternion.Identity);
			floor.Parent = dungeonGenActor;


			// DebugDraw.DrawText($"Floor Node", pos, Color.Blue, 8, 60f);

		}
	}

	private void SpawnRooms()
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

			Debug.Log($"Room: {room}... NeightborCount: {room.NeighborNodes.Count}");
			// foreach (var neighborGridPos in room.NeighborNodes)
			// {
			// 	// BoundingSphere boundingSphere = new BoundingSphere(dataGenerator.Pathfinding.GridSystem.GetWorldPosition(neighborGridPos), 15f);
			// 	// DebugDraw.DrawSphere(boundingSphere, Color.Red, 60f);

			// 	DebugDraw.DrawText($"Room Node", dataGenerator.Pathfinding.GridSystem.GetWorldPosition(neighborGridPos), Color.Red, 8, 60f);
			// }
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
}
