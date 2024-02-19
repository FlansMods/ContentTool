using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LegsDefinition : Element
{
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 visualOffset = Vector3.zero;
	[JsonField]
	public Vector3 physicsOffset = Vector3.zero;
	[JsonField]
	public float stepHeight = 1.0f;
	[JsonField]
	public float jumpHeight = 1.0f;
	[JsonField]
	public float jumpVelocity = 1.0f;
	[JsonField]
	public float rotateSpeed = 10.0f;
	[JsonField]
	[Tooltip("Once the player turns beyond this limit, legs engage and rotate")]
	public float bodyMinYaw = -90f;
	[JsonField]
	[Tooltip("Once the player turns beyond this limit, legs engage and rotate")]
	public float bodyMaxYaw = 90f;
	[JsonField]
	public float legLength = 1.0f;
	[JsonField]
	public float negateFallDamageRatio = 0.0f;
	[JsonField]
	public float transferFallDamageIntoEnvironmentRatio = 0.0f;
}
