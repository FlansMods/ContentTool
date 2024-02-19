using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    north, south, east, west, up, down
}

public static class Directions 
{
	public static Direction[] VALUES = new Direction[]
	{
		Direction.north, Direction.south, Direction.east, Direction.west, Direction.up, Direction.down
	};

	public static Vector3 UAxis(this Direction dir)
	{
		switch (dir)
		{
			case Direction.north: return Vector3.right;
			case Direction.south: return Vector3.right;
			case Direction.east: return Vector3.forward;
			case Direction.west: return Vector3.forward;
			case Direction.up: return Vector3.right;
			case Direction.down: return Vector3.right;
			default: return Vector3.zero;
		}
	}

	public static Vector3 VAxis(this Direction dir)
	{
		switch (dir)
		{
			case Direction.north: return Vector3.up;
			case Direction.south: return Vector3.up;
			case Direction.east: return Vector3.up;
			case Direction.west: return Vector3.up;
			case Direction.up: return Vector3.forward;
			case Direction.down: return Vector3.forward;
			default: return Vector3.zero;
		}
	}

	public static Vector3 Normal(this Direction dir)
    {
        switch(dir)
        {
            case Direction.north: return Vector3.back;
            case Direction.south: return Vector3.forward;
            case Direction.east: return Vector3.right;
            case Direction.west: return Vector3.left;
            case Direction.up: return Vector3.up;
            case Direction.down: return Vector3.down;
            default: return Vector3.zero;
        }
    }
}
