using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class CollisionPointDefinition : Element
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 offset = Vector3.zero;
}
