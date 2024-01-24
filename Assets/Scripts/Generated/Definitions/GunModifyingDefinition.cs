using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class GunModifyingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
[Tooltip("Disallows certain mods, but only if size > 0")]
	public string[] disallowedMods = new string[0];
	[JsonField]
[Tooltip("Allows only certain mods if set. If size == 0, nothing will be applied")]
	public string[] allowedMods = new string[0];
	[JsonField]
	public int FECostPerModify = 0;
}
