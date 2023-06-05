using UnityEngine;

[System.Serializable]
public class ItemUnlockDefinition
{
	[JsonField]
	public string name;
	[JsonField]
	public ItemStackDefinition[] items = new ItemStackDefinition[0];
}
