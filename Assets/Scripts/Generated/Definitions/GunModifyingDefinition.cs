using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class GunModifyingDefinition : Element
{
	[JsonField]
	public bool isActive = false;
	[JsonField(AssetPathHint = "attachments/")]
	[Tooltip("Disallows certain mods, but only if size > 0")]
	public ResourceLocation[] disallowedMods = new ResourceLocation[0];
	[JsonField(AssetPathHint = "attachments/")]
	[Tooltip("Allows only certain mods if set. If size == 0, nothing will be applied")]
	public ResourceLocation[] allowedMods = new ResourceLocation[0];
	[JsonField]
	public int FECostPerModify = 0;
}
