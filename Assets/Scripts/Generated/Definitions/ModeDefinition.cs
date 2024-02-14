using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ModeDefinition : Element
{
	[JsonField]
	public string key = "mode";
	[JsonField]
	public string[] values = new string[] { "on", "off" };
	[JsonField]
	public string defaultValue = "off";
}
