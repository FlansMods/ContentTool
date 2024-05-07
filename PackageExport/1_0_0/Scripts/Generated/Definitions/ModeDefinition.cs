using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

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
