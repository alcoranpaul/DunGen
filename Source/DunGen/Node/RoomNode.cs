using System;
using System.Collections.Generic;
using FlaxEngine;


namespace DunGen;

/// <summary>
/// RoomNode Script.
/// </summary>
public class RoomNode : PathNode<RoomNode>
{
	public RoomType NodeType { get; private set; }

	public RoomNode(GridSystem.GridSystem<RoomNode> gridSystem, GridSystem.GridPosition gridPosition) : base(gridSystem, gridPosition)
	{
		SetToNone();
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

	public void SetToFloor()
	{
		NodeType = RoomType.Floor;
	}

	public void SetToRoorDoor()
	{
		NodeType = RoomType.RoomDoor;
	}

	private void SetToNone()
	{
		NodeType = RoomType.None;
	}

	public enum RoomType
	{
		Hallway,
		Room,
		Floor,
		RoomDoor,
		Other,
		None
	}

	public override string ToString()
	{
		return $"{base.ToString()}";
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