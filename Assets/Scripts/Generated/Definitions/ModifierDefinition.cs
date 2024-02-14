using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ModifierDefinition : Element
{
	[JsonField]
	public string stat = "";
	[JsonField]
	public string[] matchGroupPaths = new string[0];
	[JsonField]
	public StatAccumulatorDefinition[] accumulators = new StatAccumulatorDefinition[0];
	[JsonField]
	public string setValue = "";
}
