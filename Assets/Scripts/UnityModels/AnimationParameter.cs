using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationParameter
{
	public string key = "";
	public bool isVec3 = false;
	public float floatValue = 0.0f;
	public Vector3 vec3Value = Vector3.zero;

	public AnimationParameter()
	{
		
	}
	public AnimationParameter(string key, bool isVec3, float floatValue, Vector3 vec3Value)
	{
		this.key = key;
		this.isVec3 = isVec3;
		this.floatValue = floatValue;
		this.vec3Value = vec3Value;
	}
}
