using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttachPoint
{
	public string name = "";
	public string attachedTo = "";
	public Vector3 position = Vector3.zero;

	public AttachPoint()
	{

	}
	public AttachPoint(string name, string attachedTo, Vector3 position)
	{
		this.name = name;
		this.attachedTo = attachedTo;
		this.position = position;
	}

	public Vector3 GuessDirection()
	{
		switch (name)
		{
			case "grip": return Vector3.down;
			case "barrel": return Vector3.right;
			case "scope": return Vector3.up;
			case "sights": return Vector3.up;
			case "stock": return Vector3.left;
			case "eye_line": return Vector3.right;
		}
		return Vector3.right;
	}

	public Color GetDebugColour()
	{
		switch(name)
		{
			case "grip": return Color.magenta;
			case "barrel": return Color.blue;
			case "scope": return Color.green;
			case "sights": return Color.green;
			case "stock": return Color.yellow;
			case "eye_line": return Color.cyan;
		}
		return Color.red;
	}
}

