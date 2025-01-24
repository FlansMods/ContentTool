using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutAttachmentModifierDefinition : Element
{
	[JsonField]
	public int inventorySlot = 0;
	[JsonField]
	public ItemStackDefinition attachmentItem = new ItemStackDefinition();
}
