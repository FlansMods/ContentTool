using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ArmourCraftingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public ArmourCraftingPageDefinition[] pages = new ArmourCraftingPageDefinition[0];
	[JsonField]
	public int FECostPerCraft = 0;
}
