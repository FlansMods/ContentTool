using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/PartDefinition")]
public class PartDefinition : Definition
{
	[JsonField]
	public bool canPlaceInMachiningTable = false;
	[JsonField]
	public bool canPlaceInModificationTable = false;
	[JsonField]
	public string[] compatiblityTags = new string[] { "mecha", "groundVehicle", "plane" };
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public EngineDefinition engine = new EngineDefinition();
	[JsonField]
	public int materialTier = 0;
	[JsonField]
	public EMaterialType materialType = EMaterialType.Misc;
}
