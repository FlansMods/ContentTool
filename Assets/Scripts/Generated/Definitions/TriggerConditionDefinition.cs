using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class TriggerConditionDefinition
{
	[JsonField]
	public ETriggerConditionType conditionType = ETriggerConditionType.AlwaysAllowed;
	[JsonField]
	public string[] allowedValues = new string[0];
}
