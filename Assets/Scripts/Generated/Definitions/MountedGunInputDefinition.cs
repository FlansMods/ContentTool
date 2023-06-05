using UnityEngine;

[System.Serializable]
public class MountedGunInputDefinition
{
	[JsonField]
	public string gunName = "";
	[JsonField]
	public bool toggle = false;
}
