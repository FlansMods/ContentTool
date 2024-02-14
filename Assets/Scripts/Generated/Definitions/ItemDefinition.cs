using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemDefinition : Element
{
	[JsonField]
	public int maxStackSize = 64;
	[JsonField(AssetPathHint = "tags/items/")]
	public ResourceLocation[] tags = new ResourceLocation[0];
}
