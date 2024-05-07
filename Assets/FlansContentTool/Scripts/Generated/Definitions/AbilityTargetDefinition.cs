using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class AbilityTargetDefinition : Element
{
	[JsonField]
	public EAbilityTarget targetType = EAbilityTarget.Shooter;
	[JsonField]
	public ResourceLocation[] matchIDs = new ResourceLocation[0];
	[JsonField(AssetPathHint = "tags/")]
	public ResourceLocation[] matchTags = new ResourceLocation[0];
}
