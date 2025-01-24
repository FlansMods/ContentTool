using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutItemModifierDefinition : Element
{
	[JsonField]
	public int inventorySlot = 0;
	[JsonField]
	public ItemStackDefinition item = new ItemStackDefinition();
}
