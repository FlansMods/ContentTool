using UnityEngine;

[System.Serializable]
public class IngredientDefinition
{
	[JsonField]
	public string itemName = "minecraft:air";
	[JsonField]
	public int count = 1;
	[JsonField]
	public bool compareDamage = false;
	[JsonField]
	public int damage = 0;
	[JsonField]
	public bool compareNBT = false;
	[JsonField]
	public string[] requiredTags = new string[0];
	[JsonField]
	public string[] disallowedTags = new string[0];
	[JsonField]
	public bool checkCapabilities = false;
	[JsonField]
	public string[] requiredCapabilities = new string[0];
	[JsonField]
	public string[] disallowedCapabilites = new string[0];
}
