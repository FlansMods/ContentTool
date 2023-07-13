using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/ToolDefinition")]
public class ToolDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] secondaryActions = new ActionDefinition[0];
	[JsonField]
	public bool hasDurability = false;
	[JsonField]
	public int maxDurability = 0;
	[JsonField]
	public bool destroyWhenBroken = false;
	[JsonField]
	public bool usesPower = false;
	[JsonField]
	public int internalFEStorage = 0;
	[JsonField]
	public int primaryFEUsage = 0;
	[JsonField]
	public int secondaryFEUsage = 0;
	[JsonField]
	public float spendFEOnFailRatio = 0.0f;
	[JsonField]
	public int foodValue = 0;
}
