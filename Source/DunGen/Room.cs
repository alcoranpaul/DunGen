using System;
using System.Collections.Generic;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// Room Script.
/// </summary>
public class Room
{
    public RoomPosition WorldPosition { get; private set; }
    public int Width { get; private set; }
    public int Length { get; private set; }
    public int Height { get; private set; }
    public Actor ModelActor { get; private set; }

    public Room(RoomPosition worldPosition, int width, int height, int length, Actor modelActor)
    {
        WorldPosition = worldPosition;
        Width = width;
        Length = length;
        Height = height;
        ModelActor = modelActor;
    }

    public override string ToString()
    {
        return $"Room at {WorldPosition.X}, {WorldPosition.Z} with dimensions {Width}x{Length}x{Height}";
    }

}

public struct RoomPosition
{
    public int X;
    public int Z;
    public readonly Vector2 Position2D => new Vector2(X, Z);
    public readonly Vector3 Position3D => new Vector3(X, 0, Z);

    public RoomPosition(int x, int z)
    {
        X = x;
        Z = z;
    }

    public RoomPosition(Vector3 position)
    {
        X = (int)position.X;
        Z = (int)position.Z;
    }

}
