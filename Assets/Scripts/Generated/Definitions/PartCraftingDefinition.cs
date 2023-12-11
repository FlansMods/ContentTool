using UnityEngine;

[System.Serializable]
public class PartCraftingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public int inputSlots = 8;
	[JsonField]
	public int outputSlots = 8;
	[JsonField]
[Tooltip("In seconds")]
	public float timePerCraft = 1.0f;
	[JsonField]
	public float FECostPerCraft = 0.0f;
	[JsonField]
	public string[] partsByName = new string[0];
	[JsonField]
	public string[] partsByTag = new string[0];
	[JsonField]
	public TieredIngredientDefinition[] partsByTier = new TieredIngredientDefinition[0];
}
