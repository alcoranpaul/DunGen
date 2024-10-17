
using System;
using System.Diagnostics;
using FlaxEngine;
using GridSystem;

namespace DunGen;

/// <summary>Value-type enumerator for traversing a <see cref="Coord3D"/> row-first
/// <code>
/// // Equivalent to the following nested loops
/// for(Coord3D c = offset; c.d2 &lt; offset.d2 + size.d2; c.d2++) {
///     for(c.d1 = offset.d1; c.d1 &lt; offset.d1 + size.d1; c.d1++) {
///         for(c.d0 = offset.d0; c.d0 &lt; offset.d0 + size.d0; c.d0++) {
///             yield return c;
///         }
///     }
/// }
/// </code>
/// </summary>
public struct Coord3DRowMajorCubeIterator { /* bla bla bla */ }
public interface IPathNode : IGridObject
{
	public int GCost { get; }
	public int HCost { get; }
	public int FCost { get; }
	public bool IsWalkable { get; }
	public event EventHandler OnDataChanged;
}

/// <summary>
/// Represents a single node in a grid-based pathfinding system, implementing the <see cref="IPathNode"/> interface. 
/// <para>This class extends <see cref="GridObject{T}"/> and manages essential pathfinding properties such as GCost (movement cost from the start node), 
/// HCost (estimated movement cost to the destination), and FCost (combined GCost and HCost).</para>
/// <para>Each node can track its predecessor in the path (PreviousNode) and determine whether it is walkable (IsWalkable), making it suitable for use in algorithms like A*.</para>
/// </summary>
/// <typeparam name="T">The node type, constrained to inherit from <see cref="PathNode{T}"/>.</typeparam>
public class PathNode<T> : GridObject<T>, IPathNode where T : PathNode<T>
{
	public int GCost { get; private set; }
	public int HCost { get; private set; }
	public int FCost { get; private set; }

	public T PreviousNode { get; private set; }
	public bool IsWalkable { get; private set; }

	public event EventHandler OnDataChanged;
	// public event EventHandler Walksable



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

	public void SetPreviousNode(T previousNode)
	{
		PreviousNode = previousNode;
	}


	// public void 


	public override string ToString()
	{
		return GridPosition.ToString();
	}
}