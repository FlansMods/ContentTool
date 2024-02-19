using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;
using System.Collections.Generic;

[System.Serializable]
public class ModifierDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField]
	[FormerlySerializedAs("Stat")]
	public string stat = "";
	[JsonField]
	[FormerlySerializedAs("ApplyFilters")]
	public string[] matchGroupPaths = new string[0];
	[JsonField]
	public StatAccumulatorDefinition[] accumulators = new StatAccumulatorDefinition[0];
	[JsonField]
	[FormerlySerializedAs("SetValue")]
	public string setValue = "";





	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		if (accumulators.Length == 0)
		{
			List<StatAccumulatorDefinition> accums = new List<StatAccumulatorDefinition>();
			if (!float.IsNaN(_add) && _add != 0.0f)
			{
				accums.Add(new StatAccumulatorDefinition()
				{
					value = _add,
					operation = EAccumulationOperation.BaseAdd,
				});
			}
			if(!float.IsNaN(_mul) && _mul != 1.0f)
			{
				accums.Add(new StatAccumulatorDefinition()
				{
					value = 100f * (_mul - 1.0f),
					operation = EAccumulationOperation.StackablePercentage,
				});
			}
			accumulators = accums.ToArray();
		}
	}
	[FormerlySerializedAs("Add")]
	[HideInInspector]
	public float _add = float.NaN;
	[FormerlySerializedAs("Multiply")]
	[HideInInspector]
	public float _mul = float.NaN;
}
