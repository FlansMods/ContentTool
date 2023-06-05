using UnityEngine;

[System.Serializable]
public class ItemStackDefinition
{
	[JsonField]
	public string item = "minecraft:air";
	[JsonField]
	public int count = 0;
	[JsonField]
	public int damage = 0;
	[JsonField]
	public string tags = "{}";
}
