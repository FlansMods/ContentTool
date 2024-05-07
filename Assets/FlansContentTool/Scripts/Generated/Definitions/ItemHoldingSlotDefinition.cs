using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemHoldingSlotDefinition : Element
{
	[JsonField]
	public string name = "";
	[JsonField]
	public int stackSize = 1;
}
