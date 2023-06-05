using UnityEngine;

[System.Serializable]
public class GunCraftingPageDefinition
{
	[JsonField]
	public GunCraftingEntryDefinition[] entries = new GunCraftingEntryDefinition[0];
}
