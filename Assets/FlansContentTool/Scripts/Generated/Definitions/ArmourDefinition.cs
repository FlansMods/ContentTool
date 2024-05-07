using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/ArmourDefinition")]
public class ArmourDefinition : Definition
{
	[JsonField]
	public bool hasDurability = false;
	[JsonField]
	public int maxDurability = 0;
	[JsonField]
	public EArmourType armourType = EArmourType.Chest;
	[JsonField]
	public int armourToughness = 0;
	[JsonField]
	public int damageReduction = 0;
	[JsonField]
	public bool enchantable = false;
	[JsonField]
	public int enchantability = 0;
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public PaintableDefinition paints = new PaintableDefinition();
	[JsonField]
	[Tooltip("If you want this armour to have mode toggles or other inputs, put them here")]
	public HandlerDefinition[] inputHandlers = new HandlerDefinition[0];
	[JsonField]
	[Tooltip("These are triggered actions that fire when certain conditions are met")]
	public AbilityDefinition[] staticAbilities = new AbilityDefinition[0];
	[JsonField]
	public CraftingTraitProviderDefinition[] traitProviders = new CraftingTraitProviderDefinition[0];
	[JsonField]
	public ModeDefinition[] modes = new ModeDefinition[0];
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public string[] immunities = new string[0];
	[JsonField]
	public string animationSet = "";
	[JsonField]
	public string armourTextureName = "";
	[JsonField]
	public bool nightVision = false;
	[JsonField]
	public string screenOverlay = "";
}
