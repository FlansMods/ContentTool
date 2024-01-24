using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class RawIngredientDefinition
{
	[JsonField]
[Tooltip("1 = nugget, 9 = ingot, 81 = block or similar ratios for other materials")]
	public int amount;
	[JsonField]
	public string materialName;
}
