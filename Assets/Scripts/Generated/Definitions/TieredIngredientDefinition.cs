using UnityEngine;

[System.Serializable]
public class TieredIngredientDefinition
{
	[JsonField]
	public string tag;
	[JsonField]
	public int[] allowedTiers = new int[] { 1, 2, 3 };
	[JsonField]
	public EMaterialType[] allowedMaterials = new EMaterialType[] { EMaterialType.Misc };
}
