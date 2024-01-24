using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class MountedGunInputDefinition
{
	[JsonField]
	public string gunName = "";
	[JsonField]
[Tooltip("If not set to toggle, it is instead only pressed for as long as you hold it")]
	public bool toggle = false;
}
