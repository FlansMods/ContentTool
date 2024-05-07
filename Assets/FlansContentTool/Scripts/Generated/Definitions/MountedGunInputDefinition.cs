using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class MountedGunInputDefinition : Element
{
	[JsonField]
	public string gunName = "";
	[JsonField]
	[Tooltip("If not set to toggle, it is instead only pressed for as long as you hold it")]
	public bool toggle = false;
}
