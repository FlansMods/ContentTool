using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class DamageablePartDefinition : Element
{
	[JsonField]
	public string partName = "body";
	[JsonField]
	public float maxHealth = 100;
	[JsonField]
	public float armourToughness = 1;
	[JsonField]
	public Vector3 hitboxCenter = Vector3.zero;
	[JsonField]
	public Vector3 hitboxHalfExtents = new Vector3(1f, 1f, 1f);
}
