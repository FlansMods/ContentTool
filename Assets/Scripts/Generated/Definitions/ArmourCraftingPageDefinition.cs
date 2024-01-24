using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ArmourCraftingPageDefinition
{
	[JsonField]
	public string name = "";
	[JsonField]
	public ArmourCraftingEntryDefinition[] entries = new ArmourCraftingEntryDefinition[0];
}
