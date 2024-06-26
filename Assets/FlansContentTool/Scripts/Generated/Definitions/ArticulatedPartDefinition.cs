using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ArticulatedPartDefinition : Element
{
	[JsonField]
	public bool active = false;
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
	public SoundDefinition traverseSound = new SoundDefinition();
	[JsonField]
	[Tooltip("If set true, this turret has to line up its yaw, then its pitch, one at a time")]
	public bool traveseIndependently = false;
	[JsonField]
	[Tooltip("If non-empty, the turret will try to align itself to this seat's look vector")]
	public string followSeatAtPath = "";
	[JsonField]
	public bool lockSeatToGunAngles = false;
	[JsonField]
	[Tooltip("How fast does the part turn? Set to 0 or negative to make it move at player look speed.")]
	public float rotateSpeed = 1.0f;
}
