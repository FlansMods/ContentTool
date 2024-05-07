using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/CraftingTraitDefinition")]
public class CraftingTraitDefinition : Definition
{
	[JsonField]
	public int maxLevel = 5;
	[JsonField]
	public AbilityDefinition[] abilities = new AbilityDefinition[0];
}
