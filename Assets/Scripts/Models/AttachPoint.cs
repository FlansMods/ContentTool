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
}

