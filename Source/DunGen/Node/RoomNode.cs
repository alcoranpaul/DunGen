using System;
using System.Collections.Generic;
using FlaxEngine;


namespace DunGen;

/// <summary>
/// RoomNode Script.
/// </summary>
public class RoomNode : PathNode<RoomNode>
{
	public RoomType NodeType { get; private set; }  // New property to define node type
	public RoomNode(GridSystem.GridSystem<RoomNode> gridSystem, GridSystem.GridPosition gridPosition) : base(gridSystem, gridPosition)
	{
		SetToOther();
	}

	public void SetToHallway()
	{
		NodeType = RoomType.Hallway;
	}

	public void SetToRoom()
	{
		NodeType = RoomType.Room;
	}

	public void SetToOther()
	{
		NodeType = RoomType.Other;
	}

	public enum RoomType
	{
		Hallway,
		Room,
		Other
	}


	public static void CalculateTentativeGCost(ref int tentativeGCost, RoomNode node)
	{
		switch (node.NodeType)
		{
			case RoomType.Hallway:
				tentativeGCost -= 5;
				break;
			case RoomType.Room:
				tentativeGCost += 10;
				break;
			case RoomType.Other:
				tentativeGCost += 5;
				break;
		}
	}

}