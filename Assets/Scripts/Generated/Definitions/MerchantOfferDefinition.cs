using UnityEngine;

[System.Serializable]
public class MerchantOfferDefinition
{
	[JsonField]
[Tooltip("Relative to other offers from this merchant")]
	public float weighting = 1.0f;
	[JsonField]
[Tooltip("The level at which this offer appears")]
	public int merchantLevel = 0;
	[JsonField]
	public IngredientDefinition[] inputs = new IngredientDefinition[0];
	[JsonField]
	public ItemStackDefinition output = new ItemStackDefinition();
	[JsonField]
	public float merchantXP = 1.0f;
}
