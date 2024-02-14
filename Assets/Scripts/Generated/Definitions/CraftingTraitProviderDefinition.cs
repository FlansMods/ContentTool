using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class CraftingTraitProviderDefinition : Element
{
	[JsonField(AssetPathHint = "traits/")]
	public ResourceLocation trait = InvalidLocation;
	[JsonField]
	public int level = 1;
}
