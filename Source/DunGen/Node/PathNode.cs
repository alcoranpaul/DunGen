using System;
using System.Collections.Generic;
using GridSystem;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// PathNode Script.
/// </summary>
public enum NodeType
{
	Hallway,
	Room,
	Other
}

public interface IPathNode : IGridObject
{
	public int GCost { get; }
	public int HCost { get; }
	public int FCost { get; }
	public bool IsWalkable { get; }
	public event EventHandler OnDataChanged;
}

public class PathNode<T> : GridObject<T>, IPathNode where T : PathNode<T>
{
	public int GCost { get; private set; }
	public int HCost { get; private set; }
	public int FCost { get; private set; }

	public PathNode<T> PreviousNode { get; private set; }
	public bool IsWalkable { get; private set; }

	public event EventHandler OnDataChanged;
	public NodeType NodeType { get; set; }  // New property to define node type


	public PathNode(GridSystem<T> gridSystem, GridPosition gridPosition) : base(gridSystem, gridPosition)
	{
		GCost = -1;
		HCost = -1;
		FCost = -1;
		IsWalkable = true;
	}

	public void SetWalkable(bool flag)
	{
		IsWalkable = flag;
		OnDataChanged?.Invoke(this, EventArgs.Empty);
	}

	public void SetGCost(int gCost)
	{
		GCost = gCost;
		CalculateFCost();
	}

	public void SetHCost(int hCost)
	{
		HCost = hCost;
		CalculateFCost();
	}

	private void CalculateFCost()
	{
		FCost = GCost + HCost;
		OnDataChanged?.Invoke(this, EventArgs.Empty);
	}

	public void SetPreviousNode(PathNode<T> previousNode)
	{
		PreviousNode = previousNode;
	}

	public bool IsOccupied()
	{
		Vector3 pos = GridSystem.GetWorldPosition(GridPosition);
		pos.Y -= 100f;

		DebugDraw.DrawSphere(new BoundingSphere(pos, 5f), Color.Red, 10f);
		if (Physics.RayCastAll(pos, Vector3.Up, out RayCastHit[] hits, 100f))
		{
			foreach (RayCastHit hit in hits)
			{
				if (hit.Collider.HasTag("Pathfinding.Obstacle"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string ToString()
	{
		return GridPosition.ToString();
	}
}