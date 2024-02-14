using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class MerchantOfferDefinition : Element
{
	[JsonField]
	[Tooltip("Relative to other offers from this merchant")]
	public float weighting = 1.0f;
	[JsonField]
	[Tooltip("The level at which this offer appears")]
	public int merchantLevel = 0;
	[JsonField]
	public ItemStackDefinition[] inputs = new ItemStackDefinition[0];
	[JsonField]
	public ItemStackDefinition output = new ItemStackDefinition();
	[JsonField]
	public int maxUses = 1;
	[JsonField]
	public int merchantXP = 1;
	[JsonField]
	public float priceMultiplier = 1.0f;
	[JsonField]
	public int demand = 0;
}
