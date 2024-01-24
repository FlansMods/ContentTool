using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ArmourCraftingEntryDefinition
{
	[JsonField]
	public IngredientDefinition[] ingredients = new IngredientDefinition[0];
	[JsonField]
	public ItemStackDefinition output = new ItemStackDefinition();
}
