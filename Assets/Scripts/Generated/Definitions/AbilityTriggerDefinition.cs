using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityTriggerDefinition : Element
{
	[JsonField]
	public EAbilityTrigger triggerType = EAbilityTrigger.Instant;
	[JsonField]
	public TriggerConditionDefinition[] triggerConditions = new TriggerConditionDefinition[0];
}
