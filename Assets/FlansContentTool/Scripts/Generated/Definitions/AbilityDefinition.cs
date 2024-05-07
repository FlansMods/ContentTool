using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class AbilityDefinition : Element
{
	[JsonField]
	public AbilityTriggerDefinition[] startTriggers = new AbilityTriggerDefinition[0];
	[JsonField]
	public AbilityTriggerDefinition[] endTriggers = new AbilityTriggerDefinition[0];
	[JsonField]
	public AbilityTargetDefinition[] targets = new AbilityTargetDefinition[0];
	[JsonField]
	public AbilityEffectDefinition[] effects = new AbilityEffectDefinition[0];
	[JsonField]
	public AbilityStackingDefinition stacking = new AbilityStackingDefinition();
}
