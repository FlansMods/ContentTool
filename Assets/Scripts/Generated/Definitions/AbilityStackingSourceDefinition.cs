using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityStackingSourceDefinition
{
	[JsonField]
	public EOperationType operation = EOperationType.Add;
	[JsonField]
	public float value = 0.0f;
	[JsonField]
	public EStackingType multiplyPer = EStackingType.One;
}
