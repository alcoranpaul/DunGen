using System;
using System.Collections.Generic;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// RoomData Script.
/// </summary>
public class RoomSettings
{
	public RoomPrefab RoomPrefabs;

	public class RoomPrefab
	{
		public Prefab Room;
		public Prefab Floor;
	}
}


