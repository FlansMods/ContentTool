using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemHoldingSlotDefinition
{
	[JsonField]
	public string name = "";
	[JsonField]
	public int stackSize = 1;
}
