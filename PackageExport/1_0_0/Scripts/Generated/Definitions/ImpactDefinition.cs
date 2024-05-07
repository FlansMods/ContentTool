using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ImpactDefinition : Element
{
	[JsonField]
	public EAbilityTarget targetType = EAbilityTarget.ShotEntity;
	[JsonField]
	public AbilityEffectDefinition[] impactEffects = new AbilityEffectDefinition[0];
}
