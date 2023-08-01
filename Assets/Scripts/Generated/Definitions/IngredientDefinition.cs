using UnityEngine;

[System.Serializable]
public class IngredientDefinition
{
	[JsonField]
	public int count = 1;
	[JsonField]
	public bool compareItemName = true;
	[JsonField]
	public string itemName = "minecraft:air";
	[JsonField]
	public bool compareDamage = false;
	[JsonField]
	public int minAllowedDamage = 0;
	[JsonField]
	public int maxAllowedDamage = 0;
	[JsonField]
	public bool compareItemTags = false;
	[JsonField]
	public string[] requiredTags = new string[0];
	[JsonField]
	public string[] materialTags = new string[0];
	[JsonField]
	public string[] disallowedTags = new string[0];
	[JsonField]
	public bool compareNBT = false;
	[JsonField]
	public string[] requiredNBT = new string[0];
	[JsonField]
	public string[] disallowedNBT = new string[0];
}
