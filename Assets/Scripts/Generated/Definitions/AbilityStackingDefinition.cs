using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityStackingDefinition
{
	[JsonField]
	public string stackingKey = "";
	[JsonField]
	public int maxStacks = 4;
	[JsonField]
	public bool decayAllAtOnce = false;
	[JsonField]
	public AbilityStackingSourceDefinition[] decayTime = new AbilityStackingSourceDefinition[0];
	[JsonField]
	public AbilityStackingSourceDefinition[] intensity = new AbilityStackingSourceDefinition[0];
}
