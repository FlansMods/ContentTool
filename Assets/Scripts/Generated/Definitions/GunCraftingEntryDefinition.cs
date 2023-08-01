using UnityEngine;

[System.Serializable]
public class GunCraftingEntryDefinition
{
	[JsonField]
	public ItemStackDefinition[] outputs = new ItemStackDefinition[0];
	[JsonField]
	public RecipePartDefinition[] parts = new RecipePartDefinition[0];
}
