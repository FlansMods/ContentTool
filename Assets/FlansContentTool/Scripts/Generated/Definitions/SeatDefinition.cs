using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class SeatDefinition : Element
{
	[JsonField]
	public Vector3 offsetFromAttachPoint = Vector3.zero;
	[JsonField]
	public float minYaw = -360f;
	[JsonField]
	public float maxYaw = 360f;
	[JsonField]
	public float minPitch = -90f;
	[JsonField]
	public float maxPitch = 90f;
	[JsonField]
	public bool gyroStabilised = false;
	[JsonField]
	public VehicleControlOptionDefinition[] controllerOptions = new VehicleControlOptionDefinition[0];
	[JsonField]
	public InputDefinition[] inputs = new InputDefinition[0];
}
