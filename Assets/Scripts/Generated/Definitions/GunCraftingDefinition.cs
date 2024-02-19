using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class GunCraftingDefinition : Element
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public ItemCollectionDefinition craftableGuns = new ItemCollectionDefinition();
	[JsonField]
	public int maxSlots = 8;
	[JsonField]
	public int FECostPerCraft = 0;
}
