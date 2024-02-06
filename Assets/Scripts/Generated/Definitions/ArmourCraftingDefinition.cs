using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ArmourCraftingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public ItemCollectionDefinition craftableArmour = new ItemCollectionDefinition();
	[JsonField]
	public int FECostPerCraft = 0;
}
