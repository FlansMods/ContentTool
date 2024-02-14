using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class FloatStatDefinition : Element
{
	[JsonField]
	public float baseValue = 0.0f;
	[JsonField]
	public StatAccumulatorDefinition[] additional = new StatAccumulatorDefinition[0];
}
