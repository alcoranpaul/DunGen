using System;
using System.Collections.Generic;
using FlaxEngine;


namespace DunGen;

/// <summary>
/// RoomNode Script.
/// </summary>
public class RoomNode : PathNode<RoomNode>
{
    public RoomNode(GridSystem.GridSystem<RoomNode> gridSystem, GridSystem.GridPosition gridPosition) : base(gridSystem, gridPosition)
    {
    }

}