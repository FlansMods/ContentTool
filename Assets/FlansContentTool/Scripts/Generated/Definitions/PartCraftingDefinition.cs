using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class PartCraftingDefinition : Element
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
	public ItemCollectionDefinition craftableParts = new ItemCollectionDefinition();
}
