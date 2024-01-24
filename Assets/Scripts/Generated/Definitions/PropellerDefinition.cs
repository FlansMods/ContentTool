using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class PropellerDefinition
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 visualOffset = Vector3.zero;
	[JsonField]
	public Vector3 forceOffset = Vector3.zero;
}
