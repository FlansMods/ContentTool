using UnityEngine;

[System.Serializable]
public class VehicleDefinition : Definition
{
	[JsonField]
	public SeatDefinition[] seats = new SeatDefinition[0];
	[JsonField]
	public ArticulatedPartDefinition[] articulatedParts = new ArticulatedPartDefinition[0];
	[JsonField]
	public MountedGunDefinition[] guns = new MountedGunDefinition[0];
	[JsonField]
	public VehicleMovementDefinition[] movementModes = new VehicleMovementDefinition[0];
	[JsonField]
	public Vector3 restingEulerAngles = Vector3.zero;
	[JsonField]
	public EngineDefinition defaultEngine = new EngineDefinition();
	[JsonField]
	public bool useAABBCollisionOnly = false;
	[JsonField]
	public Vector3 aabbSize = Vector3.zero;
	[JsonField]
	public int CargoSlots = 0;
	[JsonField]
	public bool CanAccessMenusWhileMoving = true;
}
