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
	public float mass = 10.0f;
	[JsonField]
	[Tooltip("Legacy only, use new axis bindings")]
	public float maxThrottle = 1.0f;
	[JsonField]
	public CollisionPointDefinition[] collisionPoints = new CollisionPointDefinition[0];
}
