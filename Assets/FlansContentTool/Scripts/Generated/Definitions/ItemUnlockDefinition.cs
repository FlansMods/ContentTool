using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemUnlockDefinition : Element
{
	[JsonField]
	public string name;
	[JsonField]
	public ItemStackDefinition[] items = new ItemStackDefinition[0];
}
