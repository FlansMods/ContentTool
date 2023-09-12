using UnityEngine;

[System.Serializable]
public class EnergyBlockDefinition
{
	[JsonField]
	public int maxFE = 0;
	[JsonField]
	public int acceptFEPerTick = 0;
	[JsonField]
	public int disperseFEPerTick = 0;
	[JsonField]
	public int numBatterySlots = 0;
	[JsonField]
	public int batterySlotStackSize = 1;
	[JsonField]
[Tooltip("In millibuckets")]
	public int liquidFuelStorage = 0;
	[JsonField]
	public string liquidFuelFilter = "";
	[JsonField]
	public int liquidFEPerMillibucket = 0;
	[JsonField]
	public int numSolidFuelSlots = 0;
	[JsonField]
	public string solidFuelFilter = "";
	[JsonField]
	public int solidFEPerFuelTime = 0;
}
