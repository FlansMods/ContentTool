using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutSkinModifierDefinition : Element
{
	[JsonField]
	public ResourceLocation skinID = InvalidLocation;
	[JsonField]
	public bool applyToPlayer = false;
	[JsonField]
	public int applyToSlot = 0;
}
