using UnityEngine;

[System.Serializable]
public class WorkbenchSideDefinition
{
	[JsonField]
	public Direction side;
	[JsonField]
	public int EUInputPerTick = 0;
	[JsonField]
	public int EUOutputPerTick = 0;
	[JsonField]
	public bool workbenchCanUseInventoriesOnSide = false;
	[JsonField]
	public bool acceptItems = false;
	[JsonField]
	public string acceptFilter = "";
	[JsonField]
	public bool outputItems = false;
	[JsonField]
	public string outputFilter = "";
}
