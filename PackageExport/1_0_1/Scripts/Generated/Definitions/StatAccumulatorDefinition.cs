using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class StatAccumulatorDefinition : Element
{
	[JsonField]
	public EAccumulationOperation operation = EAccumulationOperation.BaseAdd;
	[JsonField]
	public float value = 0.0f;
	[JsonField]
	public EAccumulationSource[] multiplyPer = new EAccumulationSource[0];
}
