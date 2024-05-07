using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ArticulatedPartDefinition : Element
{
	[JsonField]
	public string partName = "";
	[JsonField]
	public string attachedToPart = "body";
	[JsonField]
	public float minParameter = 0f;
	[JsonField]
	public float maxParameter = 1f;
	[JsonField]
	public float startParameter = 0f;
	[JsonField]
	public bool cyclic = false;
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
	[Tooltip("How fast does the part turn? Set to 0 or negative to make it move at player look speed.")]
	public float rotateSpeed = 1.0f;
}
