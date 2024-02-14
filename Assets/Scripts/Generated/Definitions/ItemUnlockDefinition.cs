using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemUnlockDefinition : Element
{
	[JsonField]
	public string name;
	[JsonField]
	public ItemStackDefinition[] items = new ItemStackDefinition[0];
}
