using UnityEngine;

[System.Serializable]
public class RecipePartDefinition
{
	[JsonField]
	public ERecipePart part = ERecipePart.misc;
	[JsonField]
	public TieredIngredientDefinition[] tieredIngredients = new TieredIngredientDefinition[0];
	[JsonField]
	public IngredientDefinition[] additionalIngredients = new IngredientDefinition[0];
}
