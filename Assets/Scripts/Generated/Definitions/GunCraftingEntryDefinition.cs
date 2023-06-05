using UnityEngine;

[System.Serializable]
public class GunCraftingEntryDefinition
{
	[JsonField]
	public ItemStackDefinition[] outputs = new ItemStackDefinition[0];
	[JsonField]
	public IngredientDefinition[] ingredients = new IngredientDefinition[0];
}
