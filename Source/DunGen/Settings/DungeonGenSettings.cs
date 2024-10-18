using System;
using System.Collections.Generic;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// DungeonGenSettings Script.
/// </summary>
public class DungeonGenSettings
{
	public int MaxRooms = 10;
	public float Size = 10f;
	[HideInEditor] public BoundingBox BoundingBox;
	public DebugSettings DebugSetting;

	public class DebugSettings
	{
		public MaterialBase Material;
		public Prefab DebugGridPrefab;
		public Prefab PathfindingDebugPrefab;
		public Prefab RoomPrefab;
		public Prefab HallwayFloorPrefab;
		public Prefab RoomFloorPrefab;
		public Prefab RoomDoorFloorPrefab;
	}
}
