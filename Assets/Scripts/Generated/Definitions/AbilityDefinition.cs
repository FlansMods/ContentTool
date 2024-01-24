using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/AbilityDefinition")]
public class AbilityDefinition : Definition
{
	[JsonField]
	public int maxLevel = 5;
	[JsonField]
	public EAbilityTrigger startTrigger = EAbilityTrigger.Instant;
	[JsonField]
	public EAbilityTrigger endTrigger = EAbilityTrigger.Instant;
	[JsonField]
	public string[] triggerConditions = new string[0];
	[JsonField]
	public EAbilityTarget targetType = EAbilityTarget.Shooter;
	[JsonField]
	public EAbilityEffect effectType = EAbilityEffect.Nothing;
	[JsonField]
	public string[] effectParameters = new string[0];
	[JsonField]
[Tooltip("The modifiers to add when the effect is active")]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public float baseIntensity = 1.0f;
	[JsonField]
	public float addIntensityPerLevel = 1.0f;
	[JsonField]
[Tooltip("This is applied separately, multiplying intensity by (mulIntensityPerAttachment * numAttachments). So 0.33 would give 2x (1+0.33*3) intensity with 3 attachments")]
	public float mulIntensityPerAttachment = 0.0f;
	[JsonField]
[Tooltip("This is applied separately, multiplying intensity by 1 + (mulIntensityForFullMag * #num-bullets/#mag-size). So 0.5 would give 1.5x intensity with a full mag, or 1.25x with half full")]
	public float mulIntensityForFullMag = 0.0f;
	[JsonField]
[Tooltip("If true, you are instead passing in 'mag emptiness'")]
	public bool invertMagFullness = false;
	[JsonField]
	public float baseDuration = 1.0f;
	[JsonField]
	public float extraDurationPerLevel = 1.0f;
	[JsonField]
	public bool stackAmount = false;
	[JsonField]
	public float maxAmount = 1.0f;
	[JsonField]
	public bool stackDuration = false;
	[JsonField]
	public float maxDuration = 1.0f;
}
