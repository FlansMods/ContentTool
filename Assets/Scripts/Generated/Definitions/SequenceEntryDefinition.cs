using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class SequenceEntryDefinition
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
