﻿using System;
using System.Collections.Generic;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// DungeonGenSettings Script.
/// </summary>
public class DungeonGenSettings
{
	public int MaxRooms = 10;
	public float Size = 50f;
	[HideInEditor] public BoundingBox BoundingBox;
	public DebugSettings DebugSetting;
	public RoomSettings RoomSetting = new RoomSettings();

	public class DebugSettings
	{
		public MaterialBase Material;
		public Prefab DebugGridPrefab;
		public Prefab PathfindingDebugPrefab;
		public Prefab RoomPrefab;
		public Prefab HallwayFloorPrefab;
		public Prefab RoomFloorPrefab;
		public Prefab RoomDoorFloorPrefab;
		public WallSettings WallPrefab = new WallSettings();
		public DoorSettings DoorPrefab = new DoorSettings();

	}

	public class RoomSettings
	{
		public bool IsRandomDimension = true;
		public Vector2 WidthDimension = new Vector2(5, 10);
		public Vector2 HeightDimension = new Vector2(1, 2);
		public Vector2 LengthDimension = new Vector2(5, 10);
	}

	public class WallSettings
	{
		public Prefab NPrefab;
		public Prefab SPrefab;
		public Prefab EPrefab;
		public Prefab WPrefab;
	}

	public class DoorSettings
	{
		public Prefab NPrefab;
		public Prefab SPrefab;
		public Prefab EPrefab;
		public Prefab WPrefab;
	}
}
