using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class WheelDefinition : Element
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 visualOffset = Vector3.zero;
	[JsonField]
	public Vector3 physicsOffset = Vector3.zero;
	[JsonField]
	public float springStrength = 1.0f;
	[JsonField]
	public float stepHeight = 1.0f;
}
