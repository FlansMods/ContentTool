using UnityEngine;

[System.Serializable]
public class VehicleMovementDefinition
{
	[JsonField]
	public string name = "default";
	[JsonField]
	public EVehicleMovementType type = EVehicleMovementType.Car;
	[JsonField]
	public WheelDefinition[] wheels = new WheelDefinition[0];
	[JsonField]
	public PropellerDefinition[] propellers = new PropellerDefinition[0];
	[JsonField]
	public LegsDefinition[] legs = new LegsDefinition[0];
	[JsonField]
	public ArmDefinition[] arms = new ArmDefinition[0];
	[JsonField]
	public float maxAcceleration = 1.0f;
	[JsonField]
	public float maxDecceleration = -1.0f;
	[JsonField]
	public float maxSpeed = 100.0f;
	[JsonField]
	public float liftForce = 10.0f;
}
