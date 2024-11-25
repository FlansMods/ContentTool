using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class PropellerDefinition : Element
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 visualOffset = Vector3.zero;
	[JsonField]
	public Vector3 forceOffset = Vector3.zero;
}
