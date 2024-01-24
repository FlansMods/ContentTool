using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class GunCraftingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public string[] craftsByName = new string[0];
	[JsonField]
	public string[] craftsByTag = new string[0];
	[JsonField]
	public int maxSlots = 8;
	[JsonField]
	public int FECostPerCraft = 0;
}
