using System;
using System.Collections.Generic;
using FlaxEngine;

namespace GridSystem;

/// <summary>
/// Represents an object that exists within a grid system. This generic class binds the object to a specific grid system and assigns it a position within that grid.
/// <para>It is designed to work with any type that inherits from <see cref="GridObject{T}"/>, allowing for flexible and reusable grid-based systems.</para>
/// </summary>
/// <typeparam name="T">The type of the grid object, constrained to inherit from <see cref="GridObject{T}"/>.</typeparam>
public class GridObject<T> : IGridObject where T : GridObject<T>
{
	public GridSystem<T> GridSystem { get; private set; }
	public GridPosition GridPosition { get; private set; }

	public GridObject(GridSystem<T> gridSystem, GridPosition gridPosition)
	{
		GridSystem = gridSystem;
		GridPosition = gridPosition;
	}
}

/// <summary>
/// Defines the properties required for an object to be part of a grid system. Each grid object must have a position within the grid.
/// </summary>
public interface IGridObject
{
	public GridPosition GridPosition { get; }
}