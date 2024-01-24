using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemUnlockDefinition
{
	[JsonField]
	public string name;
	[JsonField]
	public ItemStackDefinition[] items = new ItemStackDefinition[0];
}
