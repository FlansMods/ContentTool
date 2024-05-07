using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class SequenceEntryDefinition : Element
{
	[JsonField]
	public int tick = 0;
	[JsonField]
	public ESmoothSetting entry = ESmoothSetting.linear;
	[JsonField]
	public ESmoothSetting exit = ESmoothSetting.linear;
	[JsonField]
	public string frame = "";
}
