using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class VehiclePartDefinition : Element
{
	[JsonField]
	public string partName = "default";
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 localPosition = Vector3.zero;
	[JsonField]
	public Vector3 localEulerAngles = Vector3.zero;
	[JsonField]
	public DamageablePartDefinition damage = new DamageablePartDefinition();
	[JsonField]
	public SeatDefinition[] seats = new SeatDefinition[0];
	[JsonField]
	public MountedGunDefinition[] guns = new MountedGunDefinition[0];
	[JsonField]
	public WheelDefinition[] wheels = new WheelDefinition[0];
	[JsonField]
	public PropellerDefinition[] propellers = new PropellerDefinition[0];
	[JsonField]
	public LegsDefinition[] legs = new LegsDefinition[0];
	[JsonField]
	public ArmDefinition[] arms = new ArmDefinition[0];
	[JsonField]
	public ArticulatedPartDefinition articulation = new ArticulatedPartDefinition();
}
