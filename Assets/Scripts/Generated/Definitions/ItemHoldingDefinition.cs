using UnityEngine;

[System.Serializable]
public class ItemHoldingDefinition
{
	[JsonField]
	public ItemHoldingSlotDefinition[] slots = new ItemHoldingSlotDefinition[0];
	[JsonField]
	public string allow = "";
	[JsonField]
	public int maxStackSize = 64;
}
