using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH, SOUTH, EAST, WEST, UP, DOWN
}

public static class Directions 
{
	public static Direction[] VALUES = new Direction[]
	{
		Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST, Direction.UP, Direction.DOWN
	};

	public static Vector3 UAxis(this Direction dir)
	{
		switch (dir)
		{
			case Direction.NORTH: return Vector3.right;
			case Direction.SOUTH: return Vector3.right;
			case Direction.EAST: return Vector3.forward;
			case Direction.WEST: return Vector3.forward;
			case Direction.UP: return Vector3.right;
			case Direction.DOWN: return Vector3.right;
			default: return Vector3.zero;
		}
	}

	public static Vector3 VAxis(this Direction dir)
	{
		switch (dir)
		{
			case Direction.NORTH: return Vector3.up;
			case Direction.SOUTH: return Vector3.up;
			case Direction.EAST: return Vector3.up;
			case Direction.WEST: return Vector3.up;
			case Direction.UP: return Vector3.forward;
			case Direction.DOWN: return Vector3.forward;
			default: return Vector3.zero;
		}
	}

	public static Vector3 Normal(this Direction dir)
    {
        switch(dir)
        {
            case Direction.NORTH: return Vector3.back;
            case Direction.SOUTH: return Vector3.forward;
            case Direction.EAST: return Vector3.right;
            case Direction.WEST: return Vector3.left;
            case Direction.UP: return Vector3.up;
            case Direction.DOWN: return Vector3.down;
            default: return Vector3.zero;
        }
    }
}
