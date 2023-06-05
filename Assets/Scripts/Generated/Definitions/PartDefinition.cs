using UnityEngine;

[System.Serializable]
public class PartDefinition : Definition
{
	[JsonField]
	public bool canPlaceInMachiningTable = false;
	[JsonField]
	public bool canPlaceInModificationTable = false;
	[JsonField]
	public int maxStackSize = 64;
	[JsonField]
	public bool triggersApocalypse = false;
	[JsonField]
	public string[] compatiblityTags = new string[] { "mecha", "groundVehicle", "plane" };
	[JsonField]
	public EngineDefinition engine = new EngineDefinition();
}
