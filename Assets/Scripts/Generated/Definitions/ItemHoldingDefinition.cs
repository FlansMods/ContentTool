using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemHoldingDefinition : Element
{
	[JsonField]
	public ItemHoldingSlotDefinition[] slots = new ItemHoldingSlotDefinition[0];
	[JsonField]
	public string allow = "";
	[JsonField]
	public int maxStackSize = 64;
}
