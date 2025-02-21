using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class EngineDefinition : Element
{
	[JsonField]
	public float maxAcceleration = 1.0f;
	[JsonField]
	public float maxDeceleration = 1.0f;
	[JsonField]
	public float maxSpeed = 1.0f;
	[JsonField]
	public EFuelType fuelType = EFuelType.Creative;
	[JsonField]
	public float fuelConsumptionIdle = 0.0f;
	[JsonField]
	public float fuelConsumptionFull = 1.0f;
	[JsonField]
	public int solidFuelSlots = 0;
	[JsonField]
	[Tooltip("In millibuckets")]
	public int liquidFuelCapacity = 1000;
	[JsonField]
	public int batterySlots = 0;
	[JsonField]
	public int FECapacity = 0;
}
