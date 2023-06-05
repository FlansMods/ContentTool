using UnityEngine;

[System.Serializable]
public class GunCraftingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public GunCraftingPageDefinition[] pages = new GunCraftingPageDefinition[0];
	[JsonField]
	public int FECostPerCraft = 0;
}
