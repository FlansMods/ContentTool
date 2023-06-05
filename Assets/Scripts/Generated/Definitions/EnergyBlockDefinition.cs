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
}
