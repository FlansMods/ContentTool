using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityEffectDefinition : Element
{
	[JsonField]
	public EAbilityEffect effectType = EAbilityEffect.Nothing;
	[JsonField]
	[Tooltip("The modifiers to add when the effect is active")]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
}
