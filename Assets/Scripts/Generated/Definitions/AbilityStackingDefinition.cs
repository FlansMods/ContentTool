using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityStackingDefinition : Element
{
	[JsonField]
	public string stackingKey = "";
	[JsonField]
	public int maxStacks = 4;
	[JsonField]
	public bool decayAllAtOnce = false;
	[JsonField]
	public FloatStatDefinition decayTime = new FloatStatDefinition();
	[JsonField]
	public FloatStatDefinition intensity = new FloatStatDefinition();
}
