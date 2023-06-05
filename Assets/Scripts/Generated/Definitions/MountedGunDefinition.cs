using UnityEngine;

[System.Serializable]
public class MountedGunDefinition
{
	[JsonField]
	public string name = "default";
	[JsonField]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	public Vector3 recoil = Vector3.zero;
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public Vector3 shootPointOffset = Vector3.zero;
	[JsonField]
	public float minYaw = -360f;
	[JsonField]
	public float maxYaw = 360f;
	[JsonField]
	public float minPitch = -90;
	[JsonField]
	public float maxPitch = 90f;
	[JsonField]
	public float aimingSpeed = 1.0f;
	[JsonField]
	public bool lockSeatToGunAngles = false;
	[JsonField]
	public bool traveseIndependently = false;
	[JsonField]
	public SoundDefinition yawSound = new SoundDefinition();
	[JsonField]
	public SoundDefinition pitchSound = new SoundDefinition();
}
