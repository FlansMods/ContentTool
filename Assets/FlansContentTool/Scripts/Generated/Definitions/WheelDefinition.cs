using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class WheelDefinition : Element
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 visualOffset = Vector3.zero;
	[JsonField]
	public Vector3 physicsOffset = Vector3.zero;
	[JsonField]
	public float springStrength = 1.0f;
	[JsonField]
	public float stepHeight = 1.0f;
	[JsonField]
	public float gravityScale = 1.0f;
	[JsonField]
	public float maxForwardTorque = 1.0f;
	[JsonField]
	public float maxReverseTorque = 1.0f;
	[JsonField]
	public float radius = 0.25f;
	[JsonField]
	public bool floatOnWater = false;
	[JsonField]
	public float buoyancy = 1.0f;
	[JsonField]
	[Tooltip("Roughly how many seconds it takes for player changes to torque to be applied")]
	public float torqueResponsiveness = 0.1f;
	[JsonField]
	[Tooltip("Roughly how many seconds it takes for player changes to yaw (steering) to be applied")]
	public float yawResponsiveness = 0.1f;
	[JsonField]
	public EControlLogicHint[] controlHints = new EControlLogicHint[0];
}
