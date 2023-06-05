using UnityEngine;

[System.Serializable]
public class ClassDefinition : Definition
{
	[JsonField]
	public ItemStackDefinition[] startingItems = new ItemStackDefinition[0];
	[JsonField]
	public string spawnOnEntity = "";
	[JsonField]
	public string playerSkinOverride = "";
	[JsonField]
	public ItemStackDefinition hat = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition chest = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition legs = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition shoes = new ItemStackDefinition();
}
