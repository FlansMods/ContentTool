using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/VehicleDefinition")]
public class VehicleDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public SeatDefinition[] seats = new SeatDefinition[0];
	[JsonField]
	public ArticulatedPartDefinition[] articulatedParts = new ArticulatedPartDefinition[0];
	[JsonField]
	public MountedGunDefinition[] guns = new MountedGunDefinition[0];
	[JsonField]
	public DamageablePartDefinition[] damageables = new DamageablePartDefinition[0];
	[JsonField]
	public VehiclePhysicsDefinition physics = new VehiclePhysicsDefinition();
	[JsonField]
	public VehicleControlOptionDefinition[] controllers = new VehicleControlOptionDefinition[0];
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
