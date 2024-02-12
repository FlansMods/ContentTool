using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class CraftingTraitProviderDefinition
{
	[JsonField]
	public ResourceLocation trait = InvalidLocation;
	[JsonField]
	public int level = 1;
}
