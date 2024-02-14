using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemStackDefinition : Element
{
	[JsonField]
	public string item = "minecraft:air";
	[JsonField]
	public int count = 1;
	[JsonField]
	public int damage = 0;
	[JsonField]
	public string tags = "{}";
}
