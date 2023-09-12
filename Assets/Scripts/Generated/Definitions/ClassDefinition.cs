using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/ClassDefinition")]
public class ClassDefinition : Definition
{
	[JsonField]
[Tooltip("These will be inserted into the inventory in order. You could add stacks of air if you want to space things out")]
	public ItemStackDefinition[] startingItems = new ItemStackDefinition[0];
	[JsonField]
[Tooltip("Leave empty for no spawning. Use standard minecraft entity tag formatting")]
	public string spawnOnEntity = "";
	[JsonField]
[Tooltip("Leave blank to disable. Renders the player with this skin instead of their own")]
	public string playerSkinOverride = "";
	[JsonField]
[Tooltip("Leave this as empty to take from the team settings")]
	public ItemStackDefinition hat = new ItemStackDefinition();
	[JsonField]
[Tooltip("Leave this as empty to take from the team settings")]
	public ItemStackDefinition chest = new ItemStackDefinition();
	[JsonField]
[Tooltip("Leave this as empty to take from the team settings")]
	public ItemStackDefinition legs = new ItemStackDefinition();
	[JsonField]
[Tooltip("Leave this as empty to take from the team settings")]
	public ItemStackDefinition shoes = new ItemStackDefinition();
}
