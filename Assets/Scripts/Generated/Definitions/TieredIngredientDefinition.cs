using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class TieredIngredientDefinition
{
	[JsonField]
[Tooltip("Tag will be of the format 'flansmod:items/wing'")]
	public string tag;
	[JsonField]
	public int minMaterialTier = 1;
	[JsonField]
	public int maxMaterialTier = 5;
	[JsonField]
	public EMaterialType[] allowedMaterials = new EMaterialType[] { EMaterialType.Misc };
}
