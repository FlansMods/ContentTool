using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class WorkbenchIOSettingDefinition : Element
{
	[JsonField]
	public EWorkbenchInventoryType type = EWorkbenchInventoryType.AllTypes;
	[JsonField]
	public bool allowInput = false;
	[JsonField]
	public bool allowExtract = false;
}
