using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ArmourCraftingDefinition : Element
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public ItemCollectionDefinition craftableArmour = new ItemCollectionDefinition();
	[JsonField]
	public int FECostPerCraft = 0;
}
