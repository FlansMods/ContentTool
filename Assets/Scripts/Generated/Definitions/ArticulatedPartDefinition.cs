using UnityEngine;

[System.Serializable]
public class ArticulatedPartDefinition
{
	[JsonField]
	public string partName = "";
	[JsonField]
	public string attachedToPart = "body";
	[JsonField]
	public float minYaw = 0f;
	[JsonField]
	public float maxYaw = 0f;
	[JsonField]
	public float minPitch = 0f;
	[JsonField]
	public float maxPitch = 0f;
	[JsonField]
	public float minRoll = 0f;
	[JsonField]
	public float maxRoll = 0f;
	[JsonField]
	public Vector3 minOffset = Vector3.zero;
	[JsonField]
	public Vector3 maxOffset = Vector3.zero;
	[JsonField]
	public float rotateSpeed = 1.0f;
}
