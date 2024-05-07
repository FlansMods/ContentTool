using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class VehiclePhysicsDefinition : Element
{
	[JsonField]
	public Vector3 restingEulerAngles = Vector3.zero;
	[JsonField]
	[Tooltip("How much speed to lose per second")]
	public float drag = 0.05f;
	[JsonField]
	[Tooltip("Legacy only, use new axis bindings")]
	public float maxThrottle = 1.0f;
	[JsonField]
	public WheelDefinition[] wheels = new WheelDefinition[0];
	[JsonField]
	public PropellerDefinition[] propellers = new PropellerDefinition[0];
	[JsonField]
	public LegsDefinition[] legs = new LegsDefinition[0];
	[JsonField]
	public ArmDefinition[] arms = new ArmDefinition[0];
	[JsonField]
	public CollisionPointDefinition[] collisionPoints = new CollisionPointDefinition[0];
}
