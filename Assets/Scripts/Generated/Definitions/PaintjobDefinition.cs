using UnityEngine;

[System.Serializable]
public class PaintjobDefinition
{
	[JsonField]
	public string textureName = "";
	[JsonField]
	public int paintBucketsRequired = 1;
	[JsonField]
[Tooltip("If non-empty, this will lock cosmetic content based on an entitlement")]
	public string entitlementKey = "";
}
