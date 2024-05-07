using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class TriggerConditionDefinition : Element
{
	[JsonField]
	public ETriggerConditionType conditionType = ETriggerConditionType.CheckActionGroupPath;
	[JsonField]
	public string[] allowedValues = new string[0];
}
