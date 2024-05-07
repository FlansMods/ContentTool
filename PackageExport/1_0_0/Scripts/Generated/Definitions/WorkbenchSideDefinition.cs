using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class WorkbenchSideDefinition : Element
{
	[JsonField]
	public Direction side;
	[JsonField]
	public int EUInputPerTick = 0;
	[JsonField]
	public int EUOutputPerTick = 0;
	[JsonField]
	public WorkbenchIOSettingDefinition[] ioSettings = new WorkbenchIOSettingDefinition[0];
	[JsonField]
	[Tooltip("To-Do, potential option to directly access neighbour inventories when crafting")]
	public bool workbenchCanUseInventoriesOnSide = false;
}
