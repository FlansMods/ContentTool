using UnityEngine;

[System.Serializable]
public class TieredIngredientDefinition
{
	[JsonField]
[Tooltip("Tag will be of the format 'flansmod:items/wing'")]
	public string tag;
	[JsonField]
	public int[] allowedTiers = new int[] { 1, 2, 3 };
	[JsonField]
	public EMaterialType[] allowedMaterials = new EMaterialType[] { EMaterialType.Misc };
}
